import type { Guid, IsoDateTime } from './common';

export interface ReceiptLineItemDto {
  name: string;
  quantity: number;
  unit?: string | null;
  price: number;
}

export interface ReceiptDto {
  id: Guid;
  createdAt: IsoDateTime;
  items: ReceiptLineItemDto[];
  total: number;
}
