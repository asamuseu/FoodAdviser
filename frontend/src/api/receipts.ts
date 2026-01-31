import { ApiClient } from './http';
import type { ReceiptDto } from './dtos';

export type { ReceiptDto };

export class ReceiptsApi {
  private readonly client = new ApiClient();

  upload(file: File, signal?: AbortSignal): Promise<ReceiptDto> {
    const form = new FormData();
    form.append('File', file);
    return this.client.postForm<ReceiptDto>('/api/Receipts/upload', form, signal);
  }

  recent(signal?: AbortSignal): Promise<ReceiptDto[]> {
    return this.client.get<ReceiptDto[]>('/api/Receipts/recent', signal);
  }

  // OpenAPI marks this endpoint as a stub with unspecified payload.
  analyze(payload: unknown, signal?: AbortSignal): Promise<void> {
    return this.client.postJson<void>('/api/Receipts/analyze', payload, signal);
  }
}
