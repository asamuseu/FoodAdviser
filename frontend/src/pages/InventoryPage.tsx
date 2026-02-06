import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { InventoryApi } from '../api/inventory';
import type { CreateFoodItemModel, FoodItemModel } from '../api/models';
import { useAuth } from '../contexts';

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: FoodItemModel[] };

const emptyForm: CreateFoodItemModel = {
  name: '',
  quantity: 1,
  unit: '',
  expiresAt: null,
};

export default function InventoryPage() {
  const { apiClient } = useAuth();
  const api = useMemo(() => new InventoryApi(apiClient), [apiClient]);
  const [state, setState] = useState<LoadState>({ status: 'loading' });
  const [saving, setSaving] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  // Form state
  const [form, setForm] = useState<CreateFoodItemModel>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<FoodItemModel | null>(null);

  async function loadInventory() {
    setState({ status: 'loading' });
    setActionError(null);
    try {
      const items = await api.list({ page: 1, pageSize: 100 });
      setState({ status: 'success', data: items });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to load inventory.';
      setState({ status: 'error', message });
    }
  }

  useEffect(() => {
    const controller = new AbortController();

    api
      .list({ page: 1, pageSize: 100, signal: controller.signal })
      .then((items) => {
        setState({ status: 'success', data: items });
        setActionError(null);
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return;
        const message = error instanceof Error ? error.message : 'Failed to load inventory.';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, [api]);

  function openAddForm() {
    setForm(emptyForm);
    setEditingId(null);
    setFormError(null);
    setShowForm(true);
  }

  function openEditForm(item: FoodItemModel) {
    setForm({
      name: item.name,
      quantity: item.quantity,
      unit: item.unit,
      expiresAt: item.expiresAt ?? null,
    });
    setEditingId(item.id);
    setFormError(null);
    setShowForm(true);
  }

  function closeForm() {
    setForm(emptyForm);
    setEditingId(null);
    setFormError(null);
    setShowForm(false);
  }

  function openDeleteModal(item: FoodItemModel) {
    setDeleteTarget(item);
  }

  function closeDeleteModal() {
    setDeleteTarget(null);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (!form.name.trim() || !form.unit.trim()) return;

    setSaving(true);
    setFormError(null);
    try {
      if (editingId) {
        // Update existing item
        const updatedItem: FoodItemModel = {
          id: editingId,
          name: form.name,
          quantity: form.quantity,
          unit: form.unit,
          expiresAt: form.expiresAt,
        };
        await api.update(editingId, updatedItem);
      } else {
        // Create new item
        await api.create(form);
      }
      closeForm();
      await loadInventory();
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to save item.';
      setFormError(message);
    } finally {
      setSaving(false);
    }
  }

  async function onDelete(item: FoodItemModel) {
    setSaving(true);
    try {
      await api.remove(item.id);
      await loadInventory();
      closeDeleteModal();
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to delete item.';
      setActionError(message);
    } finally {
      setSaving(false);
    }
  }

  function formatDateForInput(dateStr: string | null | undefined): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return '';
    return date.toISOString().split('T')[0];
  }

  // Loading state
  if (state.status === 'loading') {
    return (
      <div className="container">
        <h1 className="header">Inventory</h1>
        <div className="spinner" />
      </div>
    );
  }

  // Error state
  if (state.status === 'error') {
    return (
      <div className="container">
        <h1 className="header">Inventory</h1>
        <div className="alert-error" role="alert">{state.message}</div>
        <button onClick={loadInventory}>Retry</button>
      </div>
    );
  }

  const items = state.data;

  return (
    <div className="container">
      <div className="row row--space">
        <h1 className="header" style={{ margin: 0 }}>Inventory</h1>
        <button onClick={openAddForm} disabled={saving}>
          + Add Product
        </button>
      </div>

      {actionError && (
        <div className="alert-error" role="alert">{actionError}</div>
      )}

      <p className="muted">{items.length} product(s) in stock</p>

      {/* Add/Edit Form Modal */}
      {showForm && (
        <div className="modal-overlay" onClick={closeForm}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{editingId ? 'Edit Product' : 'Add Product'}</h2>
            <form
              onSubmit={onSubmit}
              className={`form${formError ? ' form--invalid' : ''}`}
              aria-invalid={!!formError}
            >
              {formError && (
                <div className="alert-error" role="alert">
                  {formError}
                </div>
              )}
              <label className="form-field">
                <span>Name</span>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="Product name"
                  required
                  autoFocus
                  aria-invalid={!!formError}
                />
              </label>

              <div className="form-row">
                <label className="form-field">
                  <span>Quantity</span>
                  <input
                    type="number"
                    value={form.quantity}
                    onChange={(e) => setForm({ ...form, quantity: parseFloat(e.target.value) || 0 })}
                    min="0.01"
                    step="0.01"
                    required
                    aria-invalid={!!formError}
                  />
                </label>

                <label className="form-field">
                  <span>Unit</span>
                  <input
                    type="text"
                    value={form.unit}
                    onChange={(e) => setForm({ ...form, unit: e.target.value })}
                    placeholder="kg, pcs, L…"
                    required
                    aria-invalid={!!formError}
                  />
                </label>
              </div>

              <label className="form-field">
                <span>Expires (optional)</span>
                <input
                  type="date"
                  value={formatDateForInput(form.expiresAt)}
                  onChange={(e) =>
                    setForm({ ...form, expiresAt: e.target.value ? new Date(e.target.value).toISOString() : null })
                  }
                  aria-invalid={!!formError}
                />
              </label>

              <div className="form-actions">
                <button type="button" onClick={closeForm} className="secondary" disabled={saving}>
                  Cancel
                </button>
                <button type="submit" disabled={saving}>
                  {saving ? 'Saving…' : editingId ? 'Update' : 'Add'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {deleteTarget && (
        <div className="modal-overlay" onClick={closeDeleteModal}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Delete Product</h2>
            <p>
              Are you sure you want to delete <strong>{deleteTarget.name}</strong>?
            </p>
            <div className="form-actions">
              <button type="button" onClick={closeDeleteModal} className="secondary" disabled={saving}>
                Cancel
              </button>
              <button
                type="button"
                className="danger"
                onClick={() => onDelete(deleteTarget)}
                disabled={saving}
              >
                {saving ? 'Deleting…' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Empty state */}
      {items.length === 0 ? (
        <p className="muted">No products available in your inventory. Add one to get started!</p>
      ) : (
        <div className="table" role="table" aria-label="Inventory items">
          <div className="thead thead--5col" role="row">
            <span role="columnheader">Name</span>
            <span role="columnheader" className="right">Quantity</span>
            <span role="columnheader">Unit</span>
            <span role="columnheader">Expires</span>
            <span role="columnheader" className="right">Actions</span>
          </div>
          {items.map((item) => (
            <div className="trow trow--5col" role="row" key={item.id}>
              <span role="cell">{item.name}</span>
              <span role="cell" className="right">{item.quantity}</span>
              <span role="cell">{item.unit}</span>
              <span role="cell" className="muted">
                {item.expiresAt ? new Date(item.expiresAt).toLocaleDateString() : '—'}
              </span>
              <span role="cell" className="right">
                <button
                  onClick={() => openEditForm(item)}
                  className="btn-icon"
                  title="Edit"
                  disabled={saving}
                >
                  ✏️
                </button>
                <button
                  onClick={() => openDeleteModal(item)}
                  className="btn-icon danger"
                  title="Delete"
                  disabled={saving}
                >
                  🗑️
                </button>
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
