export interface Message {

    id: number;
    senderId: number;
    senderKnownAs: string;
    senderPhotoUrl: string;
    recipientId: number;
    recipientKnownAs: string;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
    recipientPhotoUrl: string;
}
