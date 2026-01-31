import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { InventoryApi } from '../api/inventory';
import type { CreateFoodItemDto, FoodItemDto } from '../api/dtos';

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: FoodItemDto[] };

const emptyForm: CreateFoodItemDto = {
  name: '',
  quantity: 1,
  unit: '',
  expiresAt: null,
};

export default function InventoryPage() {
  const api = useMemo(() => new InventoryApi(), []);
  const [state, setState] = useState<LoadState>({ status: 'loading' });
  const [saving, setSaving] = useState(false);

  // Form state
  const [form, setForm] = useState<CreateFoodItemDto>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);

  async function loadInventory() {
    setState({ status: 'loading' });
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
      .then((items) => setState({ status: 'success', data: items }))
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
    setShowForm(true);
  }

  function openEditForm(item: FoodItemDto) {
    setForm({
      name: item.name,
      quantity: item.quantity,
      unit: item.unit,
      expiresAt: item.expiresAt ?? null,
    });
    setEditingId(item.id);
    setShowForm(true);
  }

  function closeForm() {
    setForm(emptyForm);
    setEditingId(null);
    setShowForm(false);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (!form.name.trim() || !form.unit.trim()) return;

    setSaving(true);
    try {
      if (editingId) {
        // Update existing item
        const updatedItem: FoodItemDto = {
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
      alert(message);
    } finally {
      setSaving(false);
    }
  }

  async function onDelete(item: FoodItemDto) {
    if (!confirm(`Delete "${item.name}"?`)) return;

    setSaving(true);
    try {
      await api.remove(item.id);
      await loadInventory();
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to delete item.';
      alert(message);
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
        <p className="error">Error: {state.message}</p>
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

      <p className="muted">{items.length} product(s) in stock</p>

      {/* Add/Edit Form Modal */}
      {showForm && (
        <div className="modal-overlay" onClick={closeForm}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{editingId ? 'Edit Product' : 'Add Product'}</h2>
            <form onSubmit={onSubmit} className="form">
              <label className="form-field">
                <span>Name</span>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="Product name"
                  required
                  autoFocus
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
                  onClick={() => onDelete(item)}
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
