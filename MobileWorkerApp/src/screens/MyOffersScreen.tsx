import React, { useEffect, useState, useCallback } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { jobApi } from '../api/jobApi';
import { useFocusEffect } from '@react-navigation/native';
import { Ionicons } from '@expo/vector-icons';

export default function MyOffersScreen({ navigation }: any) {
  const [offers, setOffers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchOffers = async () => {
    try {
      const response = await jobApi.getMyOffers();
      setOffers(response.data);
    } catch (error) {
      console.log('Error fetching offers:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useFocusEffect(
    useCallback(() => {
      fetchOffers();
    }, [])
  );

  const onRefresh = () => {
    setRefreshing(true);
    fetchOffers();
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'SUBMITTED': return '#FFA000';
      case 'ACCEPTED': return '#4CAF50';
      case 'REJECTED': return '#F44336';
      case 'BOOKING_CREATED': return '#1a73e8';
      default: return '#666';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'SUBMITTED': return 'Đang chờ duyệt';
      case 'ACCEPTED': return 'Đã được chọn';
      case 'REJECTED': return 'Bị từ chối';
      case 'BOOKING_CREATED': return 'Đã tạo đơn';
      default: return status;
    }
  };

  const renderOfferItem = ({ item }: { item: any }) => (
    <TouchableOpacity 
      style={styles.card}
      onPress={() => navigation.navigate('JobDetail', { jobId: item.openJobId })}
    >
      <View style={styles.headerRow}>
        <Text style={styles.jobTitle} numberOfLines={2}>{item.jobTitle || 'Công việc'}</Text>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) + '20' }]}>
          <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>
            {getStatusText(item.status)}
          </Text>
        </View>
      </View>
      
      <View style={styles.infoRow}>
        <Ionicons name="cash-outline" size={16} color="#666" />
        <Text style={styles.priceText}>
          Báo giá: {item.estimatedPrice?.toLocaleString('vi-VN')} đ
        </Text>
      </View>

      <View style={styles.infoRow}>
        <Ionicons name="time-outline" size={16} color="#666" />
        <Text style={styles.infoText}>
          Thời gian đến: {item.estimatedArrivalMinutes} phút
        </Text>
      </View>
      
      {item.analysis ? (
        <Text style={styles.analysis} numberOfLines={2}>Ghi chú: {item.analysis}</Text>
      ) : null}
    </TouchableOpacity>
  );

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={{ padding: 4, marginRight: 12 }}>
          <Ionicons name="arrow-back" size={24} color="#1a73e8" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Báo Giá Của Tôi</Text>
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color="#1a73e8" />
        </View>
      ) : (
        <FlatList
          data={offers}
          renderItem={renderOfferItem}
          keyExtractor={item => item.id}
          contentContainerStyle={styles.listContainer}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
          ListEmptyComponent={
            <View style={styles.emptyContainer}>
              <Ionicons name="document-text-outline" size={64} color="#ccc" />
              <Text style={styles.emptyText}>Bạn chưa gửi báo giá nào</Text>
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
    backgroundColor: '#fff',
    padding: 20,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  headerTitle: { fontSize: 20, fontWeight: 'bold', color: '#1a73e8' },
  listContainer: { padding: 16 },
  card: {
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
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 12,
  },
  jobTitle: { fontSize: 16, fontWeight: 'bold', color: '#333', flex: 1, marginRight: 8 },
  statusBadge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 4,
  },
  statusText: { fontSize: 12, fontWeight: 'bold' },
  infoRow: { flexDirection: 'row', alignItems: 'center', marginBottom: 8 },
  infoText: { fontSize: 14, color: '#666', marginLeft: 8, flex: 1 },
  priceText: { fontSize: 14, fontWeight: 'bold', color: '#4CAF50', marginLeft: 8 },
  analysis: { fontSize: 14, color: '#666', marginTop: 4, fontStyle: 'italic' },
  emptyContainer: { alignItems: 'center', marginTop: 60 },
  emptyText: { color: '#666', fontSize: 16, marginTop: 16 },
});
