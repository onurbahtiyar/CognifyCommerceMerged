export interface ExpenseCategoryDto {
    expenseCategoryId: number;
    name: string;
    description: string;
}

export interface ExpenseDto {
    expenseId: number;
    expenseCategoryId: number;
    expenseCategoryName: string;
    description: string;
    amount: number;
    expenseDate: Date;
    notes: string;
    createdDate: Date;
}

export interface ExpenseAddDto {
    expenseCategoryId: number | null;
    description: string;
    amount: number;
    expenseDate: string; // Tarihi ISO formatında göndermek daha güvenilirdir
    notes?: string;
}

export interface ExpenseUpdateDto {
    expenseId: number;
    expenseCategoryId: number;
    description: string;
    amount: number;
    expenseDate: string;
    notes?: string;
}