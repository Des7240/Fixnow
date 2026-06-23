import * as Location from 'expo-location';
import * as TaskManager from 'expo-task-manager';
import axiosClient from '../api/axiosClient';

const LOCATION_TASK_NAME = 'background-location-task';

// Define the background task
TaskManager.defineTask(LOCATION_TASK_NAME, async ({ data, error }) => {
  if (error) {
    console.error('Error in background location task:', error);
    return;
  }
  if (data) {
    const { locations } = data as { locations: Location.LocationObject[] };
    const latestLocation = locations[0];
    
    try {
      // Call Backend API to update location
      await axiosClient.put('/workers/location', {
        lat: latestLocation.coords.latitude,
        lng: latestLocation.coords.longitude
      });
      console.log('Location updated in background', latestLocation.coords.latitude, latestLocation.coords.longitude);
    } catch (err) {
      console.error('Failed to update location to backend', err);
    }
  }
});

export const startBackgroundLocationTracking = async () => {
  try {
    const { status: foregroundStatus } = await Location.requestForegroundPermissionsAsync();
    if (foregroundStatus !== 'granted') {
      console.log('Foreground location permission denied');
      return;
    }

    const { status: backgroundStatus } = await Location.requestBackgroundPermissionsAsync();
    if (backgroundStatus !== 'granted') {
      console.log('Background location permission denied');
      return;
    }

    await Location.startLocationUpdatesAsync(LOCATION_TASK_NAME, {
      accuracy: Location.Accuracy.Balanced,
      timeInterval: 60000, // Update every 1 minute
      distanceInterval: 100, // Or every 100 meters
      showsBackgroundLocationIndicator: true,
      foregroundService: {
        notificationTitle: "Fixnow Thợ",
        notificationBody: "Đang cập nhật vị trí của bạn để nhận việc",
        notificationColor: "#1a73e8",
      }
    });
    console.log('Started background location tracking');
  } catch (error) {
    console.error('Failed to start background tracking', error);
  }
};

export const stopBackgroundLocationTracking = async () => {
  try {
    const isRegistered = await TaskManager.isTaskRegisteredAsync(LOCATION_TASK_NAME);
    if (isRegistered) {
      await Location.stopLocationUpdatesAsync(LOCATION_TASK_NAME);
      console.log('Stopped background location tracking');
    }
  } catch (error) {
    console.error('Failed to stop background tracking', error);
  }
};
