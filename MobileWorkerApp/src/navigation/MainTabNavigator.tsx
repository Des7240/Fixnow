import React from 'react';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';

import HomeScreen from '../screens/HomeScreen';
import MarketplaceScreen from '../screens/MarketplaceScreen';
import MyJobsScreen from '../screens/MyJobsScreen';
import ProfileScreen from '../screens/ProfileScreen';

const Tab = createBottomTabNavigator();

export default function MainTabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={{
        tabBarActiveTintColor: '#1a73e8',
        tabBarInactiveTintColor: '#666',
        tabBarStyle: {
          borderTopWidth: 1,
          borderTopColor: '#eee',
          elevation: 0,
          shadowOpacity: 0,
          minHeight: 60,
          paddingTop: 8,
        },
        headerShown: false,
      }}
    >
      <Tab.Screen 
        name="HomeTab" 
        component={HomeScreen} 
        options={{
          title: 'Trang chủ',
          tabBarIcon: ({ color, size }) => <Ionicons name="home-outline" color={color} size={size} />
        }} 
      />
      <Tab.Screen 
        name="MarketplaceTab" 
        component={MarketplaceScreen} 
        options={{
          title: 'Chợ Việc',
          tabBarIcon: ({ color, size }) => <Ionicons name="search-outline" color={color} size={size} />
        }} 
      />
      <Tab.Screen 
        name="MyJobsTab" 
        component={MyJobsScreen} 
        options={{
          title: 'Đơn Việc',
          tabBarIcon: ({ color, size }) => <Ionicons name="briefcase-outline" color={color} size={size} />
        }} 
      />
      <Tab.Screen 
        name="ProfileTab" 
        component={ProfileScreen} 
        options={{
          title: 'Tài khoản',
          tabBarIcon: ({ color, size }) => <Ionicons name="person-outline" color={color} size={size} />
        }} 
      />
    </Tab.Navigator>
  );
}
