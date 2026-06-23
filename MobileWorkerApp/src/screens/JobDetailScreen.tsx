import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Alert, TextInput, Image } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { jobApi } from '../api/jobApi';
import { bookingApi } from '../api/bookingApi';
import { Ionicons } from '@expo/vector-icons';

export default function JobDetailScreen({ route, navigation }: any) {
  const { jobId, isUrgent } = route.params;
  const [job, setJob] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [amount, setAmount] = useState('');
  const [message, setMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [accepting, setAccepting] = useState(false);

  useEffect(() => {
    const fetchJobDetails = async () => {
      try {
        if (isUrgent) {
          const response = await bookingApi.getBookingDetails(jobId);
          setJob(response.data);
        } else {
          const response = await jobApi.getJobDetails(jobId);
          setJob(response.data);
        }
      } catch (error) {
        Alert.alert('Lỗi', 'Không thể lấy thông tin công việc');
        navigation.goBack();
      } finally {
        setLoading(false);
      }
    };
    fetchJobDetails();
  }, [jobId, isUrgent]);

  const handleSubmitOffer = async () => {
    if (!amount || isNaN(Number(amount))) {
      Alert.alert('Lỗi', 'Vui lòng nhập mức giá báo hợp lệ');
      return;
    }
    setSubmitting(true);
    try {
      await jobApi.submitOffer(jobId, {
        estimatedPrice: Number(amount),
        analysis: message || 'Tôi có thể đến ngay để hỗ trợ bạn.',
        estimatedArrivalMinutes: 30, // Mặc định 30 phút
        estimatedRepairTimeMinutes: 60, // Mặc định 60 phút
      });
      Alert.alert('Thành công', 'Báo giá đã được gửi cho khách hàng');
      navigation.goBack();
    } catch (error: any) {
      Alert.alert('Lỗi', error.response?.data?.message || 'Không thể gửi báo giá');
    } finally {
      setSubmitting(false);
    }
  };

  const handleAcceptUrgent = async () => {
    setAccepting(true);
    try {
      await bookingApi.acceptBooking(jobId);
      Alert.alert('Thành công', 'Nhận đơn thành công! Hãy di chuyển tới vị trí khách hàng.');
      navigation.replace('JobProcess', { bookingId: jobId });
    } catch (error: any) {
      Alert.alert('Lỗi', error.response?.data?.message || 'Không thể nhận đơn này. Có thể đã bị thợ khác nhận.');
      navigation.goBack();
    } finally {
      setAccepting(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a73e8" />
      </View>
    );
  }

  if (!job) return null;

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backButton}>
          <Ionicons name="arrow-back" size={24} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Chi tiết công việc</Text>
      </View>

      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.card}>
          <Text style={styles.jobTitle}>{job.title || job.service?.name || 'Đơn sửa chữa khẩn cấp'}</Text>
          <Text style={styles.serviceName}>{job.serviceName || job.service?.name}</Text>
          
          <View style={styles.row}>
            <Ionicons name="location-outline" size={20} color="#666" />
            <Text style={styles.addressText}>{job.address}</Text>
          </View>
          
          {!isUrgent && (
            <View style={styles.row}>
              <Ionicons name="cash-outline" size={20} color="#4CAF50" />
              <Text style={styles.budgetText}>
                Ngân sách: {job.minBudget?.toLocaleString('vi-VN')} đ - {job.maxBudget?.toLocaleString('vi-VN')} đ
              </Text>
            </View>
          )}

          <View style={styles.divider} />
          <Text style={styles.sectionTitle}>Mô tả chi tiết</Text>
          <Text style={styles.description}>{job.description || 'Không có mô tả chi tiết.'}</Text>

          {job.fileUrls && job.fileUrls.length > 0 && (
            <>
              <View style={styles.divider} />
              <Text style={styles.sectionTitle}>Hình ảnh đính kèm</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageScroll}>
                {job.fileUrls.map((url: string, index: number) => (
                  <Image key={index} source={{ uri: url }} style={styles.attachedImage} />
                ))}
              </ScrollView>
            </>
          )}
        </View>

        {isUrgent ? (
          <View style={styles.card}>
            <Text style={[styles.sectionTitle, { color: '#F44336' }]}>🚨 Đơn Khẩn Cấp</Text>
            <Text style={{ marginBottom: 16, color: '#666', lineHeight: 22 }}>Khách hàng đang cần thợ ngay lập tức. Hãy nhấn nhận đơn nếu bạn có thể di chuyển tới ngay.</Text>
            <TouchableOpacity 
              style={[styles.submitButton, { backgroundColor: '#F44336' }]} 
              onPress={handleAcceptUrgent}
              disabled={accepting}
            >
              {accepting ? (
                <ActivityIndicator color="#fff" />
              ) : (
                <Text style={styles.submitButtonText}>NHẬN ĐƠN NGAY</Text>
              )}
            </TouchableOpacity>
          </View>
        ) : (
          <View style={styles.card}>
            <Text style={styles.sectionTitle}>Gửi Báo Giá</Text>
            
            <Text style={styles.label}>Mức giá đề xuất (VNĐ)</Text>
            <TextInput
              style={styles.input}
              placeholder="VD: 250000"
              keyboardType="numeric"
              value={amount}
              onChangeText={setAmount}
            />
            
            <Text style={styles.label}>Lời nhắn cho khách (Tùy chọn)</Text>
            <TextInput
              style={[styles.input, styles.textArea]}
              placeholder="Ví dụ: Tôi có thể đến ngay trong 15 phút..."
              multiline
              numberOfLines={4}
              value={message}
              onChangeText={setMessage}
            />
            
            <TouchableOpacity 
              style={[styles.submitButton, submitting && styles.buttonDisabled]} 
              onPress={handleSubmitOffer}
              disabled={submitting}
            >
              {submitting ? (
                <ActivityIndicator color="#fff" />
              ) : (
                <Text style={styles.submitButtonText}>Gửi Báo Giá</Text>
              )}
            </TouchableOpacity>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  header: {
    backgroundColor: '#1a73e8',
    padding: 16,
    flexDirection: 'row',
    alignItems: 'center',
  },
  backButton: { marginRight: 16 },
  headerTitle: { fontSize: 20, fontWeight: 'bold', color: '#fff' },
  scrollContent: { padding: 16 },
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
  jobTitle: { fontSize: 20, fontWeight: 'bold', color: '#333', marginBottom: 4 },
  serviceName: { fontSize: 14, color: '#1a73e8', marginBottom: 16, fontWeight: '500' },
  row: { flexDirection: 'row', alignItems: 'center', marginBottom: 12 },
  addressText: { fontSize: 15, color: '#444', marginLeft: 8, flex: 1 },
  budgetText: { fontSize: 15, fontWeight: 'bold', color: '#4CAF50', marginLeft: 8 },
  divider: { height: 1, backgroundColor: '#eee', marginVertical: 16 },
  sectionTitle: { fontSize: 16, fontWeight: 'bold', color: '#333', marginBottom: 12 },
  description: { fontSize: 15, color: '#444', lineHeight: 22 },
  label: { fontSize: 14, color: '#333', marginBottom: 8, fontWeight: '500' },
  input: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
    padding: 12,
    fontSize: 15,
    marginBottom: 16,
    backgroundColor: '#fafafa',
  },
  textArea: { height: 100, textAlignVertical: 'top' },
  submitButton: {
    backgroundColor: '#1a73e8',
    paddingVertical: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 8,
  },
  buttonDisabled: { backgroundColor: '#90b4e8' },
  submitButtonText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
  imageScroll: { marginTop: 8, paddingVertical: 4 },
  attachedImage: { width: 120, height: 120, borderRadius: 8, marginRight: 12, backgroundColor: '#eee', borderWidth: 1, borderColor: '#eee' },
});
