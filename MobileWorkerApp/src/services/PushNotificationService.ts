import * as Notifications from 'expo-notifications';
import * as Device from 'expo-device';
import Constants from 'expo-constants';
import { Platform } from 'react-native';
import axiosClient from '../api/axiosClient';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
    shouldShowBanner: true,
    shouldShowList: true,
  }),
});

export async function registerForPushNotificationsAsync() {
  let token;

  if (Platform.OS === 'android') {
    await Notifications.setNotificationChannelAsync('default', {
      name: 'default',
      importance: Notifications.AndroidImportance.MAX,
      vibrationPattern: [0, 250, 250, 250],
      lightColor: '#FF231F7C',
    });
  }

  if (Device.isDevice) {
    const { status: existingStatus } = await Notifications.getPermissionsAsync();
    let finalStatus = existingStatus;
    if (existingStatus !== 'granted') {
      const { status } = await Notifications.requestPermissionsAsync();
      finalStatus = status;
    }
    if (finalStatus !== 'granted') {
      console.log('Failed to get push token for push notification!');
      return;
    }
    
    // Using Expo's Push Token service
    try {
      /*
      // Check if we are running in Expo Go. If so, skip remote push token because SDK 53+ removed it.
      if (Constants.executionEnvironment === 'storeClient') {
        console.log('Skipping push token registration in Expo Go.');
        return null;
      }

      const projectId =
        Constants?.expoConfig?.extra?.eas?.projectId ?? Constants?.easConfig?.projectId ?? 'dummy-project-id';
      
      token = (
        await Notifications.getExpoPushTokenAsync({
          projectId,
        })
      ).data;
      
      console.log('Push Token:', token);
      
      // Send token to backend
      await axiosClient.put('/workers/fcm-token', { token });
      */
      console.log('Push Token fetch disabled in development');
    } catch (e) {
      console.log('Error registering push token:', e);
    }
  } else {
    console.log('Must use physical device for Push Notifications');
  }

  return token;
}
