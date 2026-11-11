export interface Transfer {
  id: number;
  userId: number;
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  transferDate: string;
  description?: string;
}
