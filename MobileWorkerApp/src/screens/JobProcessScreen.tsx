import React, { useEffect, useState, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, Alert, Linking, Platform, Image, Modal, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { bookingApi } from '../api/bookingApi';
import { quotationApi } from '../api/quotationApi';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';

export default function JobProcessScreen({ route, navigation }: any) {
  const { bookingId } = route.params;
  const [booking, setBooking] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(false);

  // Quotation State
  const [showQuoteModal, setShowQuoteModal] = useState(false);
  const [quoteItems, setQuoteItems] = useState([{ itemName: '', quantity: '1', unitPrice: '' }]);
  const [quoteNote, setQuoteNote] = useState('');
  const [submittingQuote, setSubmittingQuote] = useState(false);

  const fetchBookingDetails = async () => {
    try {
      const response = await bookingApi.getBookingDetails(bookingId);
      setBooking(response.data);
    } catch (error) {
      Alert.alert('Lỗi', 'Không thể lấy thông tin đơn hàng');
      navigation.goBack();
    } finally {
      setLoading(false);
    }
  };

  useFocusEffect(
    useCallback(() => {
      fetchBookingDetails();
    }, [bookingId])
  );

  const handleUpdateStatus = async (newStatus: string) => {
    setUpdating(true);
    try {
      await bookingApi.updateStatus(bookingId, newStatus);
      Alert.alert('Thành công', 'Đã cập nhật trạng thái đơn hàng');
      fetchBookingDetails();
    } catch (error: any) {
      Alert.alert('Lỗi', error.response?.data?.message || 'Không thể cập nhật trạng thái');
    } finally {
      setUpdating(false);
    }
  };

  const handleSubmitQuote = async () => {
    const items = quoteItems.map(i => ({
      itemName: i.itemName,
      quantity: Number(i.quantity),
      unitPrice: Number(i.unitPrice)
    }));

    if (items.some(i => !i.itemName || isNaN(i.unitPrice) || i.unitPrice <= 0 || isNaN(i.quantity) || i.quantity <= 0)) {
      Alert.alert('Lỗi', 'Vui lòng nhập đầy đủ thông tin hợp lệ cho các hạng mục.');
      return;
    }

    setSubmittingQuote(true);
    try {
      await quotationApi.createQuotation({ bookingId, items, note: quoteNote });
      Alert.alert('Thành công', 'Đã gửi báo giá cho khách hàng.');
      setShowQuoteModal(false);
      fetchBookingDetails(); // Refresh to get QUOTED status
    } catch (error: any) {
      Alert.alert('Lỗi', error.response?.data?.message || 'Không thể gửi báo giá');
    } finally {
      setSubmittingQuote(false);
    }
  };

  const addQuoteItem = () => {
    setQuoteItems([...quoteItems, { itemName: '', quantity: '1', unitPrice: '' }]);
  };

  const removeQuoteItem = (index: number) => {
    const newItems = [...quoteItems];
    newItems.splice(index, 1);
    setQuoteItems(newItems);
  };

  const updateQuoteItem = (index: number, field: string, value: string) => {
    const newItems = [...quoteItems];
    (newItems[index] as any)[field] = value;
    setQuoteItems(newItems);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'PENDING': return '#FFA000';
      case 'MATCHING': return '#FFA000';
      case 'ASSIGNED': return '#1a73e8';
      case 'ON_THE_WAY': return '#8E24AA';
      case 'INSPECTING': return '#F57C00';
      case 'QUOTED': return '#E65100';
      case 'QUOTE_APPROVED': return '#4CAF50';
      case 'WORKING': return '#00ACC1';
      case 'COMPLETED': return '#4CAF50';
      case 'CANCELLED': return '#F44336';
      default: return '#666';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'PENDING': return 'Đang chờ xác nhận';
      case 'MATCHING': return 'Đang tìm thợ';
      case 'ASSIGNED': return 'Đã nhận đơn';
      case 'ON_THE_WAY': return 'Đang di chuyển tới';
      case 'INSPECTING': return 'Đang khảo sát';
      case 'QUOTED': return 'Đã báo giá - chờ duyệt';
      case 'QUOTE_APPROVED': return 'Đã duyệt báo giá';
      case 'WORKING': return 'Đang thực hiện';
      case 'COMPLETED': return 'Đã hoàn thành';
      case 'CANCELLED': return 'Đã hủy';
      default: return status;
    }
  };

  const openMap = () => {
    if (!booking) return;
    const url = Platform.select({
      ios: `maps:0,0?q=${booking.latitude},${booking.longitude}`,
      android: `geo:0,0?q=${booking.latitude},${booking.longitude}(${booking.address})`
    });
    if (url) Linking.openURL(url);
  };

  const callCustomer = () => {
    if (!booking?.customer?.phoneNumber) return;
    Linking.openURL(`tel:${booking.customer.phoneNumber}`);
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a73e8" />
      </View>
    );
  }

  if (!booking) return null;

  const totalQuoteAmount = quoteItems.reduce((sum, item) => sum + (Number(item.quantity) * Number(item.unitPrice) || 0), 0);

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backButton}>
          <Ionicons name="arrow-back" size={24} color="#333" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Chi tiết đơn việc</Text>
        <View style={{ width: 24 }}>
          {/* Lịch sử đơn (Timeline) Button placeholder, can add navigation to a timeline screen later */}
          <Ionicons name="time-outline" size={24} color="#1a73e8" onPress={() => Alert.alert('Lịch sử đơn', 'Tính năng đang được phát triển')} />
        </View>
      </View>

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {/* Status Badge */}
        <View style={[styles.statusBanner, { backgroundColor: getStatusColor(booking.status) + '15' }]}>
          <Ionicons name="information-circle-outline" size={24} color={getStatusColor(booking.status)} />
          <Text style={[styles.statusBannerText, { color: getStatusColor(booking.status) }]}>
            Trạng thái: {getStatusText(booking.status)}
          </Text>
        </View>

        {/* Customer Info */}
        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Thông tin khách hàng</Text>
          <View style={styles.customerRow}>
            <View style={styles.avatar}>
              <Text style={styles.avatarText}>{booking.customer?.fullName?.[0] || 'K'}</Text>
            </View>
            <View style={styles.customerInfo}>
              <Text style={styles.customerName}>{booking.customer?.fullName}</Text>
              <Text style={styles.customerPhone}>{booking.customer?.phoneNumber}</Text>
            </View>
            <View style={styles.actionButtons}>
              <TouchableOpacity style={styles.actionBtn} onPress={callCustomer}>
                <Ionicons name="call" size={20} color="#1a73e8" />
              </TouchableOpacity>
              <TouchableOpacity style={styles.actionBtn} onPress={() => Alert.alert('Chat', 'Tính năng đang phát triển')}>
                <Ionicons name="chatbubble-ellipses" size={20} color="#1a73e8" />
              </TouchableOpacity>
            </View>
          </View>
        </View>

        {/* Job Info */}
        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Thông tin công việc</Text>
          <Text style={styles.serviceName}>{booking.service?.name}</Text>
          
          <View style={styles.infoRow}>
            <Ionicons name="location" size={20} color="#666" />
            <Text style={styles.infoText}>{booking.address}</Text>
            <TouchableOpacity onPress={openMap} style={styles.mapBtn}>
              <Text style={styles.mapBtnText}>Bản đồ</Text>
            </TouchableOpacity>
          </View>

          <View style={styles.infoRow}>
            <Ionicons name="time" size={20} color="#666" />
            <Text style={styles.infoText}>
              Lịch hẹn: {new Date(booking.scheduledStartTime || booking.createdAt).toLocaleString('vi-VN')}
            </Text>
          </View>

          <View style={styles.infoRow}>
            <Ionicons name="cash" size={20} color="#4CAF50" />
            <Text style={styles.priceText}>
              Giá thỏa thuận: {booking.finalPrice ? booking.finalPrice.toLocaleString('vi-VN') + ' đ' : 'Chưa chốt'}
            </Text>
          </View>

          <View style={styles.divider} />
          <Text style={styles.subTitle}>Mô tả chi tiết:</Text>
          <Text style={styles.descriptionText}>{booking.notes || booking.description || 'Không có ghi chú thêm.'}</Text>

          {booking.fileUrls && booking.fileUrls.length > 0 && (
            <>
              <View style={styles.divider} />
              <Text style={styles.subTitle}>Hình ảnh đính kèm:</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageScroll}>
                {booking.fileUrls.map((url: string, index: number) => (
                  <Image key={index} source={{ uri: url }} style={styles.attachedImage} />
                ))}
              </ScrollView>
            </>
          )}
        </View>
      </ScrollView>

      {/* Action Footer */}
      <View style={styles.footer}>
        {booking.status === 'ASSIGNED' && (
          <TouchableOpacity 
            style={styles.primaryBtn} 
            onPress={() => handleUpdateStatus('ON_THE_WAY')}
            disabled={updating}
          >
            {updating ? <ActivityIndicator color="#fff"/> : <Text style={styles.primaryBtnText}>Bắt đầu di chuyển</Text>}
          </TouchableOpacity>
        )}
        
        {booking.status === 'ON_THE_WAY' && (
          <TouchableOpacity 
            style={[styles.primaryBtn, { backgroundColor: '#8E24AA' }]} 
            onPress={() => handleUpdateStatus('INSPECTING')}
            disabled={updating}
          >
            {updating ? <ActivityIndicator color="#fff"/> : <Text style={styles.primaryBtnText}>Đã tới nơi</Text>}
          </TouchableOpacity>
        )}

        {booking.status === 'INSPECTING' && (
          <TouchableOpacity 
            style={[styles.primaryBtn, { backgroundColor: '#F57C00' }]} 
            onPress={() => setShowQuoteModal(true)}
            disabled={updating}
          >
            <Text style={styles.primaryBtnText}>Tạo báo giá chính thức</Text>
          </TouchableOpacity>
        )}

        {booking.status === 'QUOTED' && (
          <View style={[styles.primaryBtn, { backgroundColor: '#ccc' }]}>
            <Text style={styles.primaryBtnText}>Đang chờ khách duyệt báo giá...</Text>
          </View>
        )}

        {booking.status === 'WORKING' && (
          <TouchableOpacity 
            style={[styles.primaryBtn, { backgroundColor: '#00ACC1' }]} 
            onPress={() => handleUpdateStatus('COMPLETED')}
            disabled={updating}
          >
            {updating ? <ActivityIndicator color="#fff"/> : <Text style={styles.primaryBtnText}>Hoàn thành công việc</Text>}
          </TouchableOpacity>
        )}

        {(booking.status === 'PENDING' || booking.status === 'ASSIGNED') && (
          <TouchableOpacity 
            style={styles.cancelBtn} 
            onPress={() => handleUpdateStatus('CANCELLED')}
            disabled={updating}
          >
            <Text style={styles.cancelBtnText}>Hủy đơn</Text>
          </TouchableOpacity>
        )}
      </View>

      {/* Modal Tạo Báo Giá */}
      <Modal
        visible={showQuoteModal}
        animationType="slide"
        transparent={true}
        onRequestClose={() => setShowQuoteModal(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <View style={styles.modalHeader}>
              <Text style={styles.modalTitle}>Tạo Báo Giá Chính Thức</Text>
              <TouchableOpacity onPress={() => setShowQuoteModal(false)}>
                <Ionicons name="close" size={24} color="#666" />
              </TouchableOpacity>
            </View>

            <ScrollView style={styles.modalBody}>
              <Text style={styles.modalSubtitle}>Thêm các hạng mục (Vật tư, Công thợ...)</Text>
              
              {quoteItems.map((item, index) => (
                <View key={index} style={styles.quoteItemContainer}>
                  <View style={styles.quoteItemHeader}>
                    <Text style={{fontWeight: 'bold', color: '#555'}}>Hạng mục {index + 1}</Text>
                    {quoteItems.length > 1 && (
                      <TouchableOpacity onPress={() => removeQuoteItem(index)}>
                        <Ionicons name="trash" size={20} color="#F44336" />
                      </TouchableOpacity>
                    )}
                  </View>
                  
                  <TextInput
                    style={styles.input}
                    placeholder="Tên hạng mục (vd: Công thay dây)"
                    value={item.itemName}
                    onChangeText={(v) => updateQuoteItem(index, 'itemName', v)}
                  />
                  <View style={{ flexDirection: 'row', gap: 10 }}>
                    <TextInput
                      style={[styles.input, { flex: 1 }]}
                      placeholder="SL"
                      keyboardType="numeric"
                      value={item.quantity}
                      onChangeText={(v) => updateQuoteItem(index, 'quantity', v)}
                    />
                    <TextInput
                      style={[styles.input, { flex: 3 }]}
                      placeholder="Đơn giá (VNĐ)"
                      keyboardType="numeric"
                      value={item.unitPrice}
                      onChangeText={(v) => updateQuoteItem(index, 'unitPrice', v)}
                    />
                  </View>
                </View>
              ))}

              <TouchableOpacity style={styles.addQuoteItemBtn} onPress={addQuoteItem}>
                <Ionicons name="add-circle-outline" size={20} color="#1a73e8" />
                <Text style={styles.addQuoteItemText}>Thêm hạng mục</Text>
              </TouchableOpacity>

              <View style={styles.divider} />
              
              <TextInput
                style={[styles.input, { height: 80, textAlignVertical: 'top' }]}
                placeholder="Ghi chú thêm cho khách hàng..."
                multiline
                value={quoteNote}
                onChangeText={setQuoteNote}
              />

              <View style={styles.totalContainer}>
                <Text style={styles.totalText}>Tổng cộng:</Text>
                <Text style={styles.totalAmount}>{totalQuoteAmount.toLocaleString('vi-VN')} đ</Text>
              </View>
            </ScrollView>

            <View style={styles.modalFooter}>
              <TouchableOpacity 
                style={[styles.primaryBtn, { marginBottom: 0 }]} 
                onPress={handleSubmitQuote}
                disabled={submittingQuote}
              >
                {submittingQuote ? <ActivityIndicator color="#fff" /> : <Text style={styles.primaryBtnText}>Tạo Báo Giá</Text>}
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>

    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  header: {
    backgroundColor: '#fff',
    padding: 16,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    borderBottomWidth: 1,
    borderBottomColor: '#eee'
  },
  backButton: { padding: 4 },
  headerTitle: { fontSize: 18, fontWeight: 'bold', color: '#333' },
  scrollContent: { padding: 16, paddingBottom: 100 },
  statusBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 16,
    borderRadius: 8,
    marginBottom: 16
  },
  statusBannerText: { fontSize: 16, fontWeight: 'bold', marginLeft: 12 },
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
  sectionTitle: { fontSize: 16, fontWeight: 'bold', color: '#333', marginBottom: 16, borderBottomWidth: 1, borderBottomColor: '#eee', paddingBottom: 8 },
  customerRow: { flexDirection: 'row', alignItems: 'center' },
  avatar: { width: 50, height: 50, borderRadius: 25, backgroundColor: '#e8f0fe', justifyContent: 'center', alignItems: 'center', marginRight: 12 },
  avatarText: { fontSize: 20, fontWeight: 'bold', color: '#1a73e8' },
  customerInfo: { flex: 1 },
  customerName: { fontSize: 16, fontWeight: 'bold', color: '#333', marginBottom: 4 },
  customerPhone: { fontSize: 14, color: '#666' },
  actionButtons: { flexDirection: 'row' },
  actionBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: '#e8f0fe', justifyContent: 'center', alignItems: 'center', marginLeft: 8 },
  serviceName: { fontSize: 18, fontWeight: 'bold', color: '#1a73e8', marginBottom: 12 },
  infoRow: { flexDirection: 'row', alignItems: 'center', marginBottom: 12 },
  infoText: { fontSize: 15, color: '#444', marginLeft: 12, flex: 1 },
  priceText: { fontSize: 16, fontWeight: 'bold', color: '#4CAF50', marginLeft: 12, flex: 1 },
  mapBtn: { paddingHorizontal: 12, paddingVertical: 6, backgroundColor: '#f0f0f0', borderRadius: 16 },
  mapBtnText: { fontSize: 12, color: '#333', fontWeight: 'bold' },
  divider: { height: 1, backgroundColor: '#eee', marginVertical: 12 },
  subTitle: { fontSize: 14, fontWeight: 'bold', color: '#666', marginBottom: 8 },
  descriptionText: { fontSize: 15, color: '#333', lineHeight: 22 },
  imageScroll: { marginTop: 8, paddingVertical: 4 },
  attachedImage: { width: 120, height: 120, borderRadius: 8, marginRight: 12, backgroundColor: '#eee', borderWidth: 1, borderColor: '#eee' },
  footer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    backgroundColor: '#fff',
    padding: 16,
    borderTopWidth: 1,
    borderTopColor: '#eee',
  },
  primaryBtn: {
    backgroundColor: '#1a73e8',
    paddingVertical: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginBottom: 8,
  },
  primaryBtnText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
  cancelBtn: {
    paddingVertical: 12,
    alignItems: 'center',
  },
  cancelBtnText: { color: '#F44336', fontSize: 16, fontWeight: 'bold' },

  // Modal Styles
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modalContent: { backgroundColor: '#fff', borderTopLeftRadius: 20, borderTopRightRadius: 20, height: '85%' },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 20, borderBottomWidth: 1, borderBottomColor: '#eee' },
  modalTitle: { fontSize: 18, fontWeight: 'bold', color: '#333' },
  modalBody: { padding: 20 },
  modalSubtitle: { fontSize: 14, color: '#666', marginBottom: 16 },
  quoteItemContainer: { backgroundColor: '#f9f9f9', padding: 12, borderRadius: 8, marginBottom: 12, borderWidth: 1, borderColor: '#eee' },
  quoteItemHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 8 },
  input: { backgroundColor: '#fff', borderWidth: 1, borderColor: '#ddd', borderRadius: 8, padding: 12, marginBottom: 8, fontSize: 15 },
  addQuoteItemBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', padding: 12, borderStyle: 'dashed', borderWidth: 1, borderColor: '#1a73e8', borderRadius: 8, marginTop: 8 },
  addQuoteItemText: { color: '#1a73e8', fontWeight: 'bold', marginLeft: 8 },
  totalContainer: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: 16, padding: 16, backgroundColor: '#e8f0fe', borderRadius: 8 },
  totalText: { fontSize: 16, fontWeight: 'bold', color: '#333' },
  totalAmount: { fontSize: 18, fontWeight: 'bold', color: '#1a73e8' },
  modalFooter: { padding: 20, borderTopWidth: 1, borderTopColor: '#eee', backgroundColor: '#fff' }
});
