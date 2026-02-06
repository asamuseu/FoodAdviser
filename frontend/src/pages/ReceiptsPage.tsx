import { useMemo, useState, useRef, type ChangeEvent, type DragEvent } from 'react';
import { ReceiptsApi } from '../api/receipts';
import type { ReceiptModel, ReceiptLineItemModel } from '../api/models';
import { useAuth } from '../contexts';

type UploadState =
  | { status: 'idle' }
  | { status: 'uploading' }
  | { status: 'error'; message: string }
  | { status: 'success'; receipt: ReceiptModel };

export default function ReceiptsPage() {
  const { apiClient } = useAuth();
  const api = useMemo(() => new ReceiptsApi(apiClient), [apiClient]);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [state, setState] = useState<UploadState>({ status: 'idle' });
  const [dragOver, setDragOver] = useState(false);

  function handleFileSelect(selectedFile: File | null) {
    if (!selectedFile) return;

    // Validate file type
    if (!['image/png', 'image/jpeg'].includes(selectedFile.type)) {
      setState({ status: 'error', message: 'Only PNG or JPEG images are allowed.' });
      return;
    }

    setFile(selectedFile);
    setState({ status: 'idle' });

    // Create preview URL
    const url = URL.createObjectURL(selectedFile);
    setPreview(url);
  }

  function onFileChange(e: ChangeEvent<HTMLInputElement>) {
    const selectedFile = e.target.files?.[0] ?? null;
    handleFileSelect(selectedFile);
  }

  function onDragOver(e: DragEvent) {
    e.preventDefault();
    setDragOver(true);
  }

  function onDragLeave(e: DragEvent) {
    e.preventDefault();
    setDragOver(false);
  }

  function onDrop(e: DragEvent) {
    e.preventDefault();
    setDragOver(false);
    const droppedFile = e.dataTransfer.files[0] ?? null;
    handleFileSelect(droppedFile);
  }

  async function onUpload() {
    if (!file) return;

    setState({ status: 'uploading' });

    try {
      const receipt = await api.upload(file);
      setState({ status: 'success', receipt });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to upload receipt.';
      setState({ status: 'error', message });
    }
  }

  function onReset() {
    setFile(null);
    setPreview(null);
    setState({ status: 'idle' });
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  }

  function formatCurrency(value: number): string {
    return value.toLocaleString(undefined, { style: 'currency', currency: 'USD' });
  }

  const isError = state.status === 'error';
  const errorId = isError ? 'receipt-error' : undefined;

  return (
    <div className="container">
      <h1 className="header">Upload Receipt</h1>
      <p className="muted">Upload a receipt image (PNG or JPEG) to extract purchased products.</p>

      {/* Upload area */}
      {state.status !== 'success' && (
        <div
          className={`drop-zone ${dragOver ? 'drop-zone--active' : ''} ${state.status === 'uploading' ? 'drop-zone--disabled' : ''} ${isError ? 'drop-zone--error' : ''}`}
          onDragOver={onDragOver}
          onDragLeave={onDragLeave}
          onDrop={onDrop}
          onClick={() => state.status !== 'uploading' && fileInputRef.current?.click()}
          aria-invalid={isError}
          aria-describedby={errorId}
        >
          <input
            ref={fileInputRef}
            type="file"
            accept="image/png,image/jpeg"
            onChange={onFileChange}
            style={{ display: 'none' }}
            aria-describedby={errorId}
          />
          {state.status === 'uploading' ? (
            <div className="drop-zone__content">
              <div className="spinner" />
              <p>Analyzing receipt…</p>
            </div>
          ) : preview ? (
            <img src={preview} alt="Receipt preview" className="receipt-preview" />
          ) : (
            <div className="drop-zone__content">
              <span className="drop-zone__icon">📄</span>
              <p>Drag & drop a receipt image here</p>
              <p className="muted">or click to select a file</p>
            </div>
          )}
        </div>
      )}

      {/* File info and actions */}
      {file && state.status !== 'success' && (
        <div className="row row--space">
          <span className="muted">
            Selected: <strong>{file.name}</strong> ({(file.size / 1024).toFixed(1)} KB)
          </span>
          <div className="row">
            <button onClick={onReset} className="secondary">
              Clear
            </button>
            <button onClick={onUpload} disabled={state.status === 'uploading'}>
              {state.status === 'uploading' ? 'Uploading…' : 'Upload & Analyze'}
            </button>
          </div>
        </div>
      )}

      {/* Error state */}
      {state.status === 'error' && (
        <div id={errorId} className="alert-error" role="alert">
          {state.message}
        </div>
      )}

      {/* Success state - show extracted products */}
      {state.status === 'success' && (
        <div className="receipt-result">
          <div className="row row--space">
            <h2>Extracted Products</h2>
            <button onClick={onReset}>Upload Another</button>
          </div>

          {/* Keep showing the receipt image */}
          {preview && (
            <div className="receipt-image-container">
              <img src={preview} alt="Uploaded receipt" className="receipt-preview" />
            </div>
          )}

          {state.receipt.items.length === 0 ? (
            <p className="muted">No products were extracted from this receipt.</p>
          ) : (
            <>
              <div className="table" role="table" aria-label="Extracted receipt items">
                <div className="thead thead--4col" role="row">
                  <span role="columnheader">Product</span>
                  <span role="columnheader" className="right">Quantity</span>
                  <span role="columnheader">Unit</span>
                  <span role="columnheader" className="right">Price</span>
                </div>
                {state.receipt.items.map((item: ReceiptLineItemModel, index: number) => (
                  <div className="trow trow--4col" role="row" key={index}>
                    <span role="cell">{item.name}</span>
                    <span role="cell" className="right">{item.quantity}</span>
                    <span role="cell" className="muted">{item.unit ?? '—'}</span>
                    <span role="cell" className="right">{formatCurrency(item.price)}</span>
                  </div>
                ))}
              </div>

              <div className="receipt-total">
                <strong>Total:</strong> {formatCurrency(state.receipt.total)}
              </div>

              <p className="muted" style={{ marginTop: '12px' }}>
                Receipt ID: {state.receipt.id}<br />
                Processed: {new Date(state.receipt.createdAt).toLocaleString()}
              </p>
            </>
          )}
        </div>
      )}
    </div>
  );
}
