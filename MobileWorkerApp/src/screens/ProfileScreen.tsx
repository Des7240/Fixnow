import React, { useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuthStore } from '../stores/useAuthStore';
import { useWorkerStore } from '../stores/useWorkerStore';
import { workerApi } from '../api/workerApi';
import * as SecureStore from 'expo-secure-store';

export default function ProfileScreen({ navigation }: any) {
  const { user, logout } = useAuthStore();
  const { profile, wallet, setProfile, setWallet } = useWorkerStore();

  useEffect(() => {
    fetchProfileData();
  }, []);

  const fetchProfileData = async () => {
    try {
      const resProfile = await workerApi.getProfile();
      if (resProfile.data) setProfile(resProfile.data);

      const resWallet = await workerApi.getWallet();
      if (resWallet.data) setWallet(resWallet.data);
    } catch (error) {
      console.log('Failed to fetch profile', error);
    }
  };

  const handleLogout = async () => {
    await SecureStore.deleteItemAsync('accessToken');
    await SecureStore.deleteItemAsync('refreshToken');
    logout();
    navigation.replace('Login');
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <View style={styles.avatarPlaceholder}>
          <Text style={styles.avatarText}>{user?.fullName?.[0] || '?'}</Text>
        </View>
        <Text style={styles.name}>{user?.fullName || 'Người Dùng'}</Text>
        <Text style={styles.phone}>{user?.email || 'Chưa cập nhật'}</Text>
        
        {profile && (
          <View style={styles.statsContainer}>
            <View style={styles.statBox}>
              <Text style={styles.statValue}>{profile.averageRating}⭐</Text>
              <Text style={styles.statLabel}>Đánh giá</Text>
            </View>
            <View style={styles.statBox}>
              <Text style={styles.statValue}>{profile.totalJobs}</Text>
              <Text style={styles.statLabel}>Việc đã làm</Text>
            </View>
          </View>
        )}
      </View>

      <View style={styles.walletCard}>
        <Text style={styles.walletTitle}>Số dư Ví</Text>
        <Text style={styles.walletBalance}>
          {wallet ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(wallet.balance) : '0 đ'}
        </Text>
        <TouchableOpacity style={styles.withdrawButton}>
          <Text style={styles.withdrawButtonText}>Rút Tiền</Text>
        </TouchableOpacity>
      </View>

      <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
        <Text style={styles.logoutButtonText}>Đăng xuất</Text>
      </TouchableOpacity>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  header: { alignItems: 'center', padding: 32, backgroundColor: '#fff', marginBottom: 16 },
  avatarPlaceholder: { width: 80, height: 80, borderRadius: 40, backgroundColor: '#1a73e8', marginBottom: 16, justifyContent: 'center', alignItems: 'center' },
  avatarText: { fontSize: 32, color: '#fff', fontWeight: 'bold' },
  name: { fontSize: 20, fontWeight: 'bold', color: '#333' },
  phone: { fontSize: 14, color: '#666', marginTop: 4 },
  statsContainer: { flexDirection: 'row', marginTop: 24, paddingTop: 16, borderTopWidth: 1, borderTopColor: '#eee', width: '100%' },
  statBox: { flex: 1, alignItems: 'center' },
  statValue: { fontSize: 18, fontWeight: 'bold', color: '#333' },
  statLabel: { fontSize: 12, color: '#666', marginTop: 4 },
  walletCard: { backgroundColor: '#fff', padding: 20, marginHorizontal: 16, borderRadius: 12, alignItems: 'center', shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.1, shadowRadius: 4, elevation: 3 },
  walletTitle: { fontSize: 16, color: '#666' },
  walletBalance: { fontSize: 32, fontWeight: 'bold', color: '#4CAF50', marginVertical: 12 },
  withdrawButton: { backgroundColor: '#e8f0fe', paddingHorizontal: 24, paddingVertical: 10, borderRadius: 8 },
  withdrawButtonText: { color: '#1a73e8', fontWeight: 'bold' },
  logoutButton: { marginTop: 'auto', marginBottom: 32, marginHorizontal: 16, backgroundColor: '#ffebee', padding: 16, borderRadius: 8, alignItems: 'center' },
  logoutButtonText: { color: '#f44336', fontWeight: 'bold', fontSize: 16 },
});
