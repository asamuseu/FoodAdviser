import type { Guid, IsoDateTime } from './common';

export interface ReceiptLineItemModel {
  name: string;
  quantity: number;
  unit?: string | null;
  price: number;
}

export interface ReceiptModel {
  id: Guid;
  createdAt: IsoDateTime;
  items: ReceiptLineItemModel[];
  total: number;
}
