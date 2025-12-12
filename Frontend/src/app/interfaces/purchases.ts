
export interface PurchaseFilter {
  description:string | null;
  transactionStartDate: Date | null;
  transactionEndDate: Date | null;
  minAmount: number | null;
  maxAmount:number | null;
  start: number | null;
  pageSize: number | null;
}

export interface Purchase {
  id: number;
  description: string;
  transactionDatetimeUtc: Date;
  purchaseAmount: number;
  transactionIdentifier: string;
}

export interface PurchaseQueryResult {
  items: Purchase[];
  count: number;
}

export interface PurchaseData {
  description: string;
  purchaseAmount: number;
  transactionDateUtc: Date;
}
