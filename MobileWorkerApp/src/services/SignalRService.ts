import * as signalR from '@microsoft/signalr';
import * as SecureStore from 'expo-secure-store';

import { Alert } from 'react-native';

const BASE_URL = 'https://fixnow-api-009b.onrender.com';

class SignalRService {
  private notificationConnection: signalR.HubConnection | null = null;
  private chatConnection: signalR.HubConnection | null = null;

  async startConnections() {
    const token = await SecureStore.getItemAsync('accessToken');
    if (!token) return;

    // Notification Hub
    this.notificationConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/notification`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.notificationConnection.on('ReceiveNotification', (notification) => {
      console.log('Received notification:', notification);
      Alert.alert(notification.title || 'Thông báo', notification.message || notification.content || 'Bạn có thông báo mới!');
    });

    this.notificationConnection.on('ReceiveBookingMatch', (booking) => {
      console.log('Received Booking Match:', booking);
      Alert.alert('CÓ ĐƠN TÌM THỢ NGAY!', 'Khách hàng vừa tạo một đơn khẩn cấp gần bạn. Vui lòng kiểm tra Trang Chủ để nhận đơn ngay!');
    });

    try {
      await this.notificationConnection.start();
      console.log('Notification Hub connected');
    } catch (err) {
      console.error('Error connecting to Notification Hub', err);
    }

    // Chat Hub
    this.chatConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/chatHub`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.chatConnection.on('ReceiveMessage', (user, message) => {
      console.log(`Received message from ${user}: ${message}`);
      // TODO: Dispatch to chat store
    });

    try {
      await this.chatConnection.start();
      console.log('Chat Hub connected');
    } catch (err) {
      console.error('Error connecting to Chat Hub', err);
    }
  }

  async stopConnections() {
    if (this.notificationConnection) {
      await this.notificationConnection.stop();
      this.notificationConnection = null;
    }
    if (this.chatConnection) {
      await this.chatConnection.stop();
      this.chatConnection = null;
    }
  }

  // Helper method to send chat message
  async sendMessage(bookingId: string, message: string) {
    if (this.chatConnection && this.chatConnection.state === signalR.HubConnectionState.Connected) {
      await this.chatConnection.invoke('SendMessage', bookingId, message);
    } else {
      console.error('Chat Hub is not connected');
    }
  }
}

export default new SignalRService();
