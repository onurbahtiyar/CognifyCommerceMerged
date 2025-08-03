export interface NotificationDto {
    notificationId: number;
    topic: string;
    type: 'Info' | 'Warning' | 'Critical' | 'Success';
    message: string;
    relatedEntityId: number | null;
    url: string;
    createdDate: string;
    isRead: boolean;
}