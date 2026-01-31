import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { InventoryApi } from './api/inventory';
import { ReceiptsApi } from './api/receipts';
import { RecipesApi } from './api/recipes';
import type { CreateFoodItemDto, FoodItemDto } from './api/inventory';
import type { ReceiptDto } from './api/receipts';
import type {
  ConfirmRecipesResponseDto,
  DishType,
  GenerateRecipesRequestDto,
  RecipeDto,
} from './api/recipes';

type AsyncState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: T };

function formatDateTime(value?: string | null): string {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

export default function App() {
  const inventoryApi = useMemo(() => new InventoryApi(), []);
  const receiptsApi = useMemo(() => new ReceiptsApi(), []);
  const recipesApi = useMemo(() => new RecipesApi(), []);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [inventoryState, setInventoryState] = useState<AsyncState<FoodItemDto[]>>({
    status: 'idle',
  });
  const [mutating, setMutating] = useState(false);

  const [createForm, setCreateForm] = useState<CreateFoodItemDto>({
    name: '',
    quantity: 1,
    unit: '',
    expiresAt: null,
  });

  const [receiptFile, setReceiptFile] = useState<File | null>(null);
  const [uploadState, setUploadState] = useState<AsyncState<ReceiptDto>>({ status: 'idle' });
  const [recentReceiptsState, setRecentReceiptsState] = useState<AsyncState<unknown>>({
    status: 'idle',
  });

  const [generateForm, setGenerateForm] = useState<GenerateRecipesRequestDto>({
    dishType: 0 as DishType,
    numberOfPersons: 2,
  });
  const [generatedRecipesState, setGeneratedRecipesState] = useState<AsyncState<RecipeDto[]>>({
    status: 'idle',
  });
  const [selectedRecipeIds, setSelectedRecipeIds] = useState<string[]>([]);
  const [confirmState, setConfirmState] = useState<AsyncState<ConfirmRecipesResponseDto>>({
    status: 'idle',
  });

  useEffect(() => {
    const controller = new AbortController();
    setInventoryState({ status: 'loading' });

    inventoryApi
      .list({ page, pageSize, signal: controller.signal })
      .then((items) => setInventoryState({ status: 'success', data: items }))
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return;
        const message = error instanceof Error ? error.message : 'Failed to load inventory.';
        setInventoryState({ status: 'error', message });
      });

    return () => controller.abort();
  }, [inventoryApi, page, pageSize]);

  async function refresh() {
    setInventoryState({ status: 'loading' });
    try {
      const items = await inventoryApi.list({ page, pageSize });
      setInventoryState({ status: 'success', data: items });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to load inventory.';
      setInventoryState({ status: 'error', message });
    }
  }

  async function onCreateSubmit(event: FormEvent) {
    event.preventDefault();
    if (!createForm.name.trim() || !createForm.unit.trim()) return;

    setMutating(true);
    try {
      await inventoryApi.create(createForm);
      setCreateForm({ name: '', quantity: 1, unit: '', expiresAt: null });
      await refresh();
    } finally {
      setMutating(false);
    }
  }

  async function onDelete(id: string) {
    setMutating(true);
    try {
      await inventoryApi.remove(id);
      await refresh();
    } finally {
      setMutating(false);
    }
  }

  async function onUploadReceipt(event: FormEvent) {
    event.preventDefault();
    if (!receiptFile) return;

    setMutating(true);
    setUploadState({ status: 'loading' });
    try {
      const receipt = await receiptsApi.upload(receiptFile);
      setUploadState({ status: 'success', data: receipt });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to upload receipt.';
      setUploadState({ status: 'error', message });
    } finally {
      setMutating(false);
    }
  }

  async function onLoadRecentReceipts() {
    setMutating(true);
    setRecentReceiptsState({ status: 'loading' });
    try {
      const result = await receiptsApi.recent();
      setRecentReceiptsState({ status: 'success', data: result });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to load recent receipts.';
      setRecentReceiptsState({ status: 'error', message });
    } finally {
      setMutating(false);
    }
  }

  async function onGenerateRecipes(event: FormEvent) {
    event.preventDefault();
    setMutating(true);
    setGeneratedRecipesState({ status: 'loading' });
    setConfirmState({ status: 'idle' });
    try {
      const recipes = await recipesApi.generate(generateForm);
      setGeneratedRecipesState({ status: 'success', data: recipes });
      setSelectedRecipeIds([]);
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to generate recipes.';
      setGeneratedRecipesState({ status: 'error', message });
    } finally {
      setMutating(false);
    }
  }

  async function onConfirmRecipes() {
    if (selectedRecipeIds.length === 0) return;
    setMutating(true);
    setConfirmState({ status: 'loading' });
    try {
      const response = await recipesApi.confirm({ recipeIds: selectedRecipeIds });
      setConfirmState({ status: 'success', data: response });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to confirm recipes.';
      setConfirmState({ status: 'error', message });
    } finally {
      setMutating(false);
    }
  }

  return (
    <div className="container">
      <header className="header">
        <h1>FoodManager</h1>
        <p className="muted">
          API: <code>{import.meta.env.VITE_API_BASE_URL ?? '(not set)'}</code>
        </p>
      </header>

      <section className="card">
        <h2>Inventory</h2>

        <form className="row" onSubmit={onCreateSubmit}>
          <input
            aria-label="Name"
            placeholder="Name"
            value={createForm.name}
            onChange={(e) =>
              setCreateForm((prev: CreateFoodItemDto) => ({ ...prev, name: e.target.value }))
            }
          />
          <input
            aria-label="Quantity"
            type="number"
            min={0.01}
            step={0.01}
            value={createForm.quantity ?? 1}
            onChange={(e) =>
              setCreateForm((prev: CreateFoodItemDto) => ({
                ...prev,
                quantity: Number(e.target.value),
              }))
            }
          />
          <input
            aria-label="Unit"
            placeholder="Unit (e.g. pcs, g)"
            value={createForm.unit}
            onChange={(e) =>
              setCreateForm((prev: CreateFoodItemDto) => ({ ...prev, unit: e.target.value }))
            }
          />
          <input
            aria-label="ExpiresAt"
            type="datetime-local"
            value={createForm.expiresAt ? createForm.expiresAt.slice(0, 16) : ''}
            onChange={(e) =>
              setCreateForm((prev: CreateFoodItemDto) => ({
                ...prev,
                expiresAt: e.target.value ? new Date(e.target.value).toISOString() : null,
              }))
            }
          />
          <button disabled={mutating || !createForm.name.trim() || !createForm.unit.trim()}>
            Add
          </button>
        </form>

        <div className="row row--space">
          <div className="row">
            <label className="row">
              <span className="muted">Page</span>
              <input
                aria-label="Page"
                type="number"
                min={1}
                value={page}
                onChange={(e) => setPage(Math.max(1, Number(e.target.value)))}
                style={{ width: 90 }}
              />
            </label>
            <label className="row">
              <span className="muted">Page size</span>
              <input
                aria-label="Page size"
                type="number"
                min={1}
                max={200}
                value={pageSize}
                onChange={(e) => setPageSize(Math.max(1, Number(e.target.value)))}
                style={{ width: 110 }}
              />
            </label>
          </div>
          <button className="secondary" onClick={refresh} disabled={mutating}>
            Refresh
          </button>
        </div>

        {inventoryState.status === 'loading' && <p>Loading…</p>}
        {inventoryState.status === 'error' && (
          <p className="error">{inventoryState.message}</p>
        )}
        {inventoryState.status === 'success' && (
          <div className="table">
            <div className="thead">
              <div>Name</div>
              <div className="right">Qty</div>
              <div>Unit</div>
              <div>Expires</div>
              <div className="right">Actions</div>
            </div>
            {inventoryState.data.length === 0 ? (
              <div className="trow">
                <div className="muted">No items.</div>
              </div>
            ) : (
              inventoryState.data.map((item) => (
                <div key={item.id ?? `${item.name ?? 'item'}-${item.unit ?? ''}-${item.quantity}`} className="trow">
                  <div>{item.name ?? ''}</div>
                  <div className="right">{item.quantity}</div>
                  <div>{item.unit ?? ''}</div>
                  <div>{formatDateTime(item.expiresAt)}</div>
                  <div className="right">
                    <button
                      className="danger"
                      onClick={() => item.id && onDelete(item.id)}
                      disabled={mutating || !item.id}
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </section>

      <section className="card" style={{ marginTop: 16 }}>
        <h2>Receipts</h2>
        <form className="row" onSubmit={onUploadReceipt}>
          <input
            aria-label="Receipt file"
            type="file"
            accept="image/*,application/pdf"
            onChange={(e) => setReceiptFile(e.target.files?.item(0) ?? null)}
          />
          <button disabled={mutating || !receiptFile}>Upload</button>
          <button
            type="button"
            className="secondary"
            onClick={onLoadRecentReceipts}
            disabled={mutating}
          >
            Load recent
          </button>
        </form>

        {uploadState.status === 'loading' && <p>Uploading…</p>}
        {uploadState.status === 'error' && <p className="error">{uploadState.message}</p>}
        {uploadState.status === 'success' && (
          <pre className="pre">{JSON.stringify(uploadState.data, null, 2)}</pre>
        )}

        {recentReceiptsState.status === 'loading' && <p>Loading recent…</p>}
        {recentReceiptsState.status === 'error' && (
          <p className="error">{recentReceiptsState.message}</p>
        )}
        {recentReceiptsState.status === 'success' && (
          <pre className="pre">{JSON.stringify(recentReceiptsState.data, null, 2)}</pre>
        )}
      </section>

      <section className="card" style={{ marginTop: 16 }}>
        <h2>Recipes</h2>

        <form className="row" onSubmit={onGenerateRecipes}>
          <label className="row">
            <span className="muted">Dish type</span>
            <select
              aria-label="Dish type"
              value={generateForm.dishType}
              onChange={(e) =>
                setGenerateForm((prev: GenerateRecipesRequestDto) => ({
                  ...prev,
                  dishType: Number(e.target.value) as DishType,
                }))
              }
            >
              <option value={0}>0</option>
              <option value={1}>1</option>
              <option value={2}>2</option>
              <option value={3}>3</option>
              <option value={4}>4</option>
              <option value={5}>5</option>
            </select>
          </label>

          <label className="row">
            <span className="muted">Persons</span>
            <input
              aria-label="Number of persons"
              type="number"
              min={1}
              max={20}
              value={generateForm.numberOfPersons}
              onChange={(e) =>
                setGenerateForm((prev: GenerateRecipesRequestDto) => ({
                  ...prev,
                  numberOfPersons: Number(e.target.value),
                }))
              }
              style={{ width: 100 }}
            />
          </label>
          <button disabled={mutating}>Generate</button>
        </form>

        {generatedRecipesState.status === 'loading' && <p>Generating…</p>}
        {generatedRecipesState.status === 'error' && (
          <p className="error">{generatedRecipesState.message}</p>
        )}
        {generatedRecipesState.status === 'success' && (
          <>
            {generatedRecipesState.data.length === 0 ? (
              <p className="muted">No recipes returned.</p>
            ) : (
              <div className="list">
                {generatedRecipesState.data.map((recipe) => {
                  const id = recipe.id;
                  if (!id) return null;
                  const checked = selectedRecipeIds.includes(id);
                  return (
                    <label key={id} className="listItem">
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={(e) => {
                          const next = e.target.checked
                            ? [...selectedRecipeIds, id]
                            : selectedRecipeIds.filter((x) => x !== id);
                          setSelectedRecipeIds(next);
                        }}
                      />
                      <span>
                        <strong>{recipe.title ?? '(untitled)'}</strong>
                        <span className="muted"> — id: {id}</span>
                      </span>
                    </label>
                  );
                })}
              </div>
            )}

            <div className="row row--space">
              <div className="muted">Selected: {selectedRecipeIds.length}</div>
              <button
                className="secondary"
                type="button"
                disabled={mutating || selectedRecipeIds.length === 0}
                onClick={onConfirmRecipes}
              >
                Confirm selected
              </button>
            </div>
          </>
        )}

        {confirmState.status === 'loading' && <p>Confirming…</p>}
        {confirmState.status === 'error' && <p className="error">{confirmState.message}</p>}
        {confirmState.status === 'success' && (
          <pre className="pre">{JSON.stringify(confirmState.data, null, 2)}</pre>
        )}
      </section>
    </div>
  );
}

