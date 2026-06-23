import React, { useEffect, useState } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import LoginScreen from './src/screens/LoginScreen';
import MainTabNavigator from './src/navigation/MainTabNavigator';
import JobDetailScreen from './src/screens/JobDetailScreen';
import JobProcessScreen from './src/screens/JobProcessScreen';
import * as SecureStore from 'expo-secure-store';
import { authApi } from './src/api/authApi';

import { useAuthStore } from './src/stores/useAuthStore';
import { ActivityIndicator, View } from 'react-native';

const Stack = createNativeStackNavigator();

export default function App() {
  const user = useAuthStore((state) => state.user);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    const bootstrapAsync = async () => {
      try {
        const token = await SecureStore.getItemAsync('accessToken');
        if (token) {
          // Verify token and fetch user profile
          const res = await authApi.getProfile();
          if (res.data) {
            useAuthStore.getState().setUser(res.data);
          }
        }
      } catch (e) {
        console.log('No token or token expired');
        await SecureStore.deleteItemAsync('accessToken');
        await SecureStore.deleteItemAsync('refreshToken');
      } finally {
        setIsReady(true);
      }
    };

    bootstrapAsync();
  }, []);

  if (!isReady) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" color="#1a73e8" />
      </View>
    );
  }

  return (
    <SafeAreaProvider>
      <NavigationContainer>
        <StatusBar style="auto" />
        <Stack.Navigator>
          {user ? (
            <>
              <Stack.Screen 
                name="MainTabs" 
                component={MainTabNavigator} 
                options={{ headerShown: false }} 
              />
              <Stack.Screen 
                name="JobDetail" 
                component={JobDetailScreen} 
                options={{ headerShown: false }} 
              />
              <Stack.Screen 
                name="JobProcess" 
                component={JobProcessScreen} 
                options={{ headerShown: false }} 
              />
            </>
          ) : (
            <Stack.Screen 
              name="Login" 
              component={LoginScreen} 
              options={{ headerShown: false }} 
            />
          )}
        </Stack.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}
