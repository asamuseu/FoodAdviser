import { ApiClient } from './http';
import type { ReceiptModel } from './models';

export type { ReceiptModel };

export class ReceiptsApi {
  private readonly client: ApiClient;

  constructor(client?: ApiClient) {
    this.client = client ?? new ApiClient();
  }

  upload(file: File, signal?: AbortSignal): Promise<ReceiptModel> {
    const form = new FormData();
    form.append('File', file);
    return this.client.postForm<ReceiptModel>('/api/Receipts/upload', form, signal);
  }

  recent(signal?: AbortSignal): Promise<ReceiptModel[]> {
    return this.client.get<ReceiptModel[]>('/api/Receipts/recent', signal);
  }

  // OpenAPI marks this endpoint as a stub with unspecified payload.
  analyze(payload: unknown, signal?: AbortSignal): Promise<void> {
    return this.client.postJson<void>('/api/Receipts/analyze', payload, signal);
  }
}
