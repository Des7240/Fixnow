import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { jobApi } from '../api/jobApi';
import * as Location from 'expo-location';
import { Ionicons } from '@expo/vector-icons';

export default function MarketplaceScreen({ navigation }: any) {
  const [jobs, setJobs] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchJobs = async () => {
    try {
      let { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        setLoading(false);
        setRefreshing(false);
        return;
      }

      let loc = await Location.getCurrentPositionAsync({});
      const response = await jobApi.getMarketplaceJobs(loc.coords.latitude, loc.coords.longitude, 50); // Bán kính 50km
      setJobs(response.data);
    } catch (error) {
      console.log('Error fetching jobs:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchJobs();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    fetchJobs();
  };

  const renderJobItem = ({ item }: { item: any }) => (
    <View style={styles.jobCard}>
      <View style={styles.jobHeader}>
        <Text style={styles.jobTitle}>{item.title}</Text>
        <Text style={styles.jobPrice}>
          {item.minBudget?.toLocaleString('vi-VN')} đ - {item.maxBudget?.toLocaleString('vi-VN')} đ
        </Text>
      </View>
      <Text style={styles.serviceName}>{item.serviceName}</Text>
      <View style={styles.locationContainer}>
        <Ionicons name="location-outline" size={16} color="#666" />
        <Text style={styles.distanceText}>{(item.distanceKm || 0).toFixed(1)} km - {item.address}</Text>
      </View>
      <Text style={styles.description} numberOfLines={2}>{item.description}</Text>
      
      <TouchableOpacity 
        style={styles.button}
        onPress={() => navigation.navigate('JobDetail', { jobId: item.id })}
      >
        <Text style={styles.buttonText}>Báo giá ngay</Text>
      </TouchableOpacity>
    </View>
  );

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Chợ Việc Làm</Text>
        <Text style={styles.headerSubtitle}>Các đơn khách hàng đang gọi thợ quanh đây</Text>
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color="#1a73e8" />
        </View>
      ) : (
        <FlatList
          data={jobs}
          renderItem={renderJobItem}
          keyExtractor={item => item.id}
          contentContainerStyle={styles.listContainer}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
          ListEmptyComponent={
            <View style={styles.emptyContainer}>
              <Ionicons name="sad-outline" size={64} color="#ccc" />
              <Text style={styles.emptyText}>Hiện chưa có công việc nào quanh đây</Text>
            </View>
          }
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  header: {
    backgroundColor: '#1a73e8',
    padding: 20,
    paddingTop: 10,
  },
  headerTitle: { fontSize: 24, fontWeight: 'bold', color: '#fff' },
  headerSubtitle: { fontSize: 14, color: '#e8f0fe', marginTop: 4 },
  listContainer: { padding: 16 },
  jobCard: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  jobHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 8,
  },
  jobTitle: { fontSize: 16, fontWeight: 'bold', color: '#333', flex: 1, marginRight: 8 },
  jobPrice: { fontSize: 15, fontWeight: 'bold', color: '#4CAF50' },
  serviceName: { fontSize: 13, color: '#1a73e8', marginBottom: 8, fontWeight: '500' },
  locationContainer: { flexDirection: 'row', alignItems: 'center', marginBottom: 8 },
  distanceText: { fontSize: 14, color: '#666', marginLeft: 4, flex: 1 },
  description: { fontSize: 14, color: '#444', marginBottom: 16, lineHeight: 20 },
  button: {
    backgroundColor: '#e8f0fe',
    paddingVertical: 12,
    borderRadius: 8,
    alignItems: 'center',
  },
  buttonText: { color: '#1a73e8', fontSize: 14, fontWeight: 'bold' },
  emptyContainer: { alignItems: 'center', marginTop: 60 },
  emptyText: { color: '#666', fontSize: 16, marginTop: 16 },
});
