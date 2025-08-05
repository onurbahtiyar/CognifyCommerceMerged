export interface ChatSessionDto {
  sessionId: string;
  title: string;
  createdAt: string;
}

export interface ChatMessageDto {
  messageId: number;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
  isDatabaseQuery: boolean;
  relatedSql?: string;
  chartType?: string;
}