import React, { useEffect, useState, useCallback } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, Switch, ActivityIndicator, RefreshControl, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import * as Location from 'expo-location';
import { useFocusEffect } from '@react-navigation/native';
import { startBackgroundLocationTracking, stopBackgroundLocationTracking } from '../services/LocationService';
import { registerForPushNotificationsAsync } from '../services/PushNotificationService';
import { useAuthStore } from '../stores/useAuthStore';
import { jobApi } from '../api/jobApi';
import { workerApi } from '../api/workerApi';
import { bookingApi } from '../api/bookingApi';
import { Ionicons } from '@expo/vector-icons';

import SignalRService from '../services/SignalRService';

export default function HomeScreen({ navigation }: any) {
  const user = useAuthStore((state) => state.user);
  const [isOnline, setIsOnline] = useState(false);
  const [location, setLocation] = useState<Location.LocationObject | null>(null);
  const [openJobs, setOpenJobs] = useState<any[]>([]);
  const [urgentBookings, setUrgentBookings] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [accepting, setAccepting] = useState(false);

  const fetchJobs = async () => {
    try {
      let { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') return;

      let loc = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.High
      });
      setLocation(loc);

      const [jobsRes, urgentRes] = await Promise.all([
        jobApi.getMarketplaceJobs(loc.coords.latitude, loc.coords.longitude, 20),
        bookingApi.getMatchingBookings().catch(() => ({ data: [] }))
      ]);
      // Chỉ lấy 3 việc gần nhất để hiển thị ở trang chủ
      setOpenJobs(jobsRes.data.slice(0, 3));
      setUrgentBookings(urgentRes.data || []);
    } catch (error) {
      console.log('Error fetching jobs for Home:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    (async () => {
      // Đăng ký Push Notification
      await registerForPushNotificationsAsync();
      
      // Bật kết nối SignalR để nhận Popup thông báo real-time
      await SignalRService.startConnections();

      // Fetch profile to sync online status
      try {
        const res = await workerApi.getProfile();
        const profile = res.data;
        if (profile?.availabilityStatus === 'ONLINE') {
          setIsOnline(true);
          await startBackgroundLocationTracking();
        }
      } catch (err) {
        console.log('Error fetching profile for availability status', err);
      }
    })();

    // Cleanup khi thoát app
    return () => {
      SignalRService.stopConnections();
    };
  }, []);

  useFocusEffect(
    useCallback(() => {
      fetchJobs();
    }, [])
  );

  const onRefresh = () => {
    setRefreshing(true);
    fetchJobs();
  };

  const handleToggleOnline = async (value: boolean) => {
    try {
      await workerApi.updateAvailability(value ? 'ONLINE' : 'OFFLINE');
      setIsOnline(value);
      if (value) {
        await startBackgroundLocationTracking();
      } else {
        await stopBackgroundLocationTracking();
      }
    } catch (error) {
      console.log('Error toggling online status', error);
      Alert.alert('Lỗi', 'Không thể cập nhật trạng thái hoạt động.');
    }
  };

  const handleAcceptUrgent = async (bookingId: string) => {
    if (accepting) return;
    setAccepting(true);
    try {
      await bookingApi.acceptBooking(bookingId);
      Alert.alert('Thành công', 'Nhận đơn thành công! Hãy di chuyển tới vị trí khách hàng.');
      navigation.navigate('JobProcess', { bookingId });
    } catch (error: any) {
      Alert.alert('Lỗi', error.response?.data?.message || 'Không thể nhận đơn này. Có thể đã bị thợ khác nhận.');
      fetchJobs();
    } finally {
      setAccepting(false);
    }
  };

  const renderUrgentItem = ({ item }: { item: any }) => (
    <View style={styles.urgentCard}>
      <View style={styles.urgentHeader}>
        <Ionicons name="warning" size={20} color="#F44336" />
        <Text style={styles.urgentTitle}>ĐƠN KHẨN CẤP ĐANG CHỜ BẠN</Text>
      </View>
      <Text style={styles.urgentServiceName}>{item.service?.name}</Text>
      <View style={styles.locationContainer}>
        <Ionicons name="location-outline" size={16} color="#666" />
        <Text style={styles.distanceText}>{item.address}</Text>
      </View>
      {item.description && <Text style={styles.description} numberOfLines={2}>{item.description}</Text>}
      
      <View style={{ flexDirection: 'row', marginTop: 8 }}>
        <TouchableOpacity 
          style={[styles.urgentButton, { flex: 1, marginRight: 8, marginTop: 0, backgroundColor: '#fff', borderWidth: 1, borderColor: '#F44336' }]}
          onPress={() => navigation.navigate('JobDetail', { jobId: item.id, isUrgent: true })}
        >
          <Text style={[styles.urgentButtonText, { color: '#F44336' }]}>XEM CHI TIẾT</Text>
        </TouchableOpacity>
        <TouchableOpacity 
          style={[styles.urgentButton, { flex: 1, marginTop: 0 }]}
          onPress={() => handleAcceptUrgent(item.id)}
          disabled={accepting}
        >
          {accepting ? <ActivityIndicator color="#fff" /> : <Text style={styles.urgentButtonText}>NHẬN NGAY</Text>}
        </TouchableOpacity>
      </View>
    </View>
  );

  const renderJobItem = ({ item }: any) => (
    <View style={styles.jobCard}>
      <Text style={styles.jobTitle}>{item.title}</Text>
      <View style={styles.jobDetails}>
        <Text style={styles.jobDistance}>📍 {(item.distanceKm || 0).toFixed(1)} km</Text>
        <Text style={styles.jobPrice}>
          {item.minBudget?.toLocaleString('vi-VN')} đ - {item.maxBudget?.toLocaleString('vi-VN')} đ
        </Text>
      </View>
      <TouchableOpacity 
        style={styles.acceptButton}
        onPress={() => navigation.navigate('JobDetail', { jobId: item.id })}
      >
        <Text style={styles.acceptButtonText}>Xem chi tiết</Text>
      </TouchableOpacity>
    </View>
  );

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <View style={styles.headerLeft}>
          <Text style={styles.greeting}>Xin chào, {user?.fullName || 'Thợ'}!</Text>
          {location && (
            <Text style={styles.locationText}>
              Vị trí: {location.coords.latitude.toFixed(4)}, {location.coords.longitude.toFixed(4)}
            </Text>
          )}
        </View>
        <View style={styles.statusContainer}>
          <Text style={[styles.statusText, { color: isOnline ? '#4CAF50' : '#666' }]}>
            {isOnline ? 'Đang trực' : 'Nghỉ ngơi'}
          </Text>
          <Switch
            value={isOnline}
            onValueChange={handleToggleOnline}
            trackColor={{ false: '#767577', true: '#81b0ff' }}
            thumbColor={isOnline ? '#1a73e8' : '#f4f3f4'}
          />
        </View>
      </View>

      <View style={styles.content}>
        <View style={styles.sectionHeader}>
          <Text style={styles.sectionTitle}>Công việc quanh đây</Text>
          <TouchableOpacity onPress={() => navigation.navigate('MarketplaceTab')}>
            <Text style={styles.seeAllText}>Xem tất cả</Text>
          </TouchableOpacity>
        </View>

        {loading ? (
          <ActivityIndicator size="large" color="#1a73e8" style={{ marginTop: 40 }} />
        ) : (
          <FlatList
            data={openJobs}
            renderItem={renderJobItem}
            keyExtractor={item => item.id}
            contentContainerStyle={styles.listContainer}
            refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
            ListHeaderComponent={
              urgentBookings.length > 0 ? (
                <View style={styles.urgentSection}>
                  {urgentBookings.map(item => <React.Fragment key={item.id}>{renderUrgentItem({ item })}</React.Fragment>)}
                </View>
              ) : null
            }
            ListEmptyComponent={
              <Text style={{ textAlign: 'center', color: '#666', marginTop: 20 }}>
                Không có công việc nào gần bạn lúc này.
              </Text>
            }
          />
        )}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  header: {
    backgroundColor: '#fff',
    padding: 20,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  headerLeft: {
    flex: 1,
  },
  greeting: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
  },
  locationText: {
    fontSize: 12,
    color: '#666',
    marginTop: 4,
  },
  statusContainer: {
    alignItems: 'center',
    marginLeft: 16,
  },
  statusText: {
    fontSize: 12,
    marginBottom: 4,
    fontWeight: 'bold',
  },
  content: {
    flex: 1,
    padding: 16,
  },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
  },
  seeAllText: {
    fontSize: 14,
    color: '#1a73e8',
    fontWeight: 'bold',
  },
  listContainer: {
    paddingBottom: 20,
  },
  jobCard: {
    backgroundColor: '#fff',
    padding: 16,
    borderRadius: 8,
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  urgentSection: {
    marginBottom: 16,
  },
  urgentCard: {
    backgroundColor: '#FFF4F4',
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    elevation: 3,
    shadowColor: '#F44336',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.2,
    shadowRadius: 4,
    borderWidth: 1,
    borderColor: '#FFCDD2',
  },
  urgentHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
  },
  urgentTitle: { fontSize: 16, fontWeight: 'bold', color: '#F44336', marginLeft: 8 },
  urgentServiceName: { fontSize: 18, color: '#D32F2F', marginBottom: 8, fontWeight: 'bold' },
  urgentButton: {
    backgroundColor: '#F44336',
    paddingVertical: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 8,
  },
  urgentButtonText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
  locationContainer: { flexDirection: 'row', alignItems: 'center', marginBottom: 8 },
  distanceText: { fontSize: 14, color: '#666', marginLeft: 4, flex: 1 },
  description: { fontSize: 14, color: '#444', marginBottom: 16, lineHeight: 20 },
  jobTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 8,
  },
  jobDetails: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 16,
  },
  jobDistance: {
    fontSize: 14,
    color: '#666',
  },
  jobPrice: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#4CAF50',
  },
  acceptButton: {
    backgroundColor: '#e8f0fe',
    paddingVertical: 10,
    borderRadius: 6,
    alignItems: 'center',
  },
  acceptButtonText: {
    color: '#1a73e8',
    fontWeight: 'bold',
    fontSize: 14,
  },
});
