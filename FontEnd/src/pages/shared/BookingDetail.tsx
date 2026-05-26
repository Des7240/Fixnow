import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { MapPin, Clock, Phone, CheckCircle, ChevronLeft, Star, AlertCircle, ListOrdered, X, Check, MessageSquare, AlertTriangle, FileText } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';
import { clsx } from 'clsx';
import { message, Modal, Rate, Input, Popconfirm } from 'antd';
import { useAuthStore } from '../../stores/authStore';

interface TimelineEvent {
  id: string;
  oldStatus?: string;
  newStatus: string;
  createdAt: string;
}

interface Booking {
  id: string;
  status: string;
  paymentStatus: string;
  address: string;
  lat: number;
  lng: number;
  description: string;
  createdAt: string;
  customer: {
    id: string;
    fullName: string;
    email: string;
  };
  worker?: {
    id: string;
    fullName: string;
    email: string;
  };
  service: {
    id: string;
    name: string;
  };
  quotations: any[];
  totalAmount?: number;
  navigationUrl?: string;
}

export default function BookingDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [booking, setBooking] = useState<Booking | null>(null);
  const [timeline, setTimeline] = useState<TimelineEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  
  // Review Modal State
  const [isReviewModalOpen, setIsReviewModalOpen] = useState(false);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [submittingReview, setSubmittingReview] = useState(false);

  // Payment Modal State
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<'VNPAY' | 'MOMO' | 'SEPAY'>('VNPAY');
  const [paymentAction, setPaymentAction] = useState<'APPROVE' | 'DIRECT'>('APPROVE');

  // Promo Code State
  const [promoCode, setPromoCode] = useState('');
  const [promoDiscount, setPromoDiscount] = useState<number>(0);
  const [promoError, setPromoError] = useState('');
  const [validatingPromo, setValidatingPromo] = useState(false);
  const [appliedPromo, setAppliedPromo] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      fetchData();
    }
  }, [id]);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [bookingRes, timelineRes] = await Promise.all([
        axiosInstance.get(`/bookings/${id}`),
        axiosInstance.get(`/bookings/${id}/timeline`)
      ]);
      console.log('Booking Data:', bookingRes.data);
      console.log('Current User Role:', user?.role);
      setBooking(bookingRes.data);
      setTimeline(timelineRes.data);
    } catch (err) {
      console.error(err);
      message.error('Không thể tải thông tin đơn hàng');
    } finally {
      setLoading(false);
    }
  };

  const updateStatus = async (newStatus: string) => {
    setActionLoading(true);
    try {
      await axiosInstance.patch(`/bookings/${id}/status`, { status: newStatus });
      message.success('Cập nhật trạng thái thành công');
      fetchData();
    } catch (err) {
      message.error('Lỗi khi cập nhật trạng thái');
    } finally {
      setActionLoading(false);
    }
  };

  const handleAccept = async () => {
    setActionLoading(true);
    try {
      await axiosInstance.post(`/bookings/${id}/accept`);
      message.success('Đã nhận đơn hàng thành công!');
      fetchData();
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi chấp nhận đơn');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    setActionLoading(true);
    try {
      await axiosInstance.post(`/bookings/${id}/reject`);
      message.success('Đã từ chối đơn hàng');
      navigate('/worker');
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi từ chối đơn');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCancel = async () => {
    setActionLoading(true);
    try {
      await axiosInstance.patch(`/bookings/${id}/cancel`);
      message.success('Đã hủy đơn hàng thành công');
      fetchData();
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi hủy đơn hàng');
    } finally {
      setActionLoading(false);
    }
    };

    const handleFinishWork = () => {
    Modal.confirm({
      title: 'Hoàn thành công việc',
      content: 'Bạn có muốn cập nhật lại báo giá cuối cùng dựa trên thực tế sửa chữa không? Nếu có, khách hàng sẽ duyệt lại báo giá trước khi thanh toán.',
      okText: 'Cập nhật báo giá',
      cancelText: 'Hoàn thành ngay',
      onOk: () => {
        navigate(`/worker/bookings/${id}/quotation/create`);
      },
      onCancel: () => {
        updateStatus('COMPLETED');
      }
    });
    };

    const handleReviewSubmit = async () => {    setSubmittingReview(true);
    try {
      await axiosInstance.post('/reviews', {
        bookingId: id,
        rating,
        comment
      });
      message.success('Cảm ơn bạn đã đánh giá!');
      setIsReviewModalOpen(false);
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi gửi đánh giá');
    } finally {
      setSubmittingReview(false);
    }
  };

  const handleApproveQuote = () => {
    setPaymentAction('APPROVE');
    setIsPaymentModalOpen(true);
  };

  const handleDirectPayment = () => {
    setPaymentAction('DIRECT');
    setIsPaymentModalOpen(true);
  };

  const handleValidatePromo = async () => {
    if (!promoCode.trim()) return;
    
    const pendingQuote = booking?.quotations?.find((q: any) => q.status === 'PENDING' || q.status === 'APPROVED');
    const orderValue = pendingQuote?.totalAmount || booking?.totalAmount || 0;

    setValidatingPromo(true);
    setPromoError('');
    try {
      const res = await axiosInstance.post('/promotions/validate', {
        code: promoCode,
        orderValue: orderValue,
        serviceId: booking?.service.id
      });
      
      if (res.data.isValid) {
        setPromoDiscount(res.data.discountAmount);
        setAppliedPromo(promoCode);
        message.success('Áp dụng mã khuyến mãi thành công!');
      } else {
        setPromoError(res.data.errorMessage);
        setAppliedPromo(null);
        setPromoDiscount(0);
      }
    } catch (error) {
      setPromoError('Có lỗi xảy ra khi kiểm tra mã khuyến mãi');
    } finally {
      setValidatingPromo(false);
    }
  };

  const executePayment = async () => {
    setActionLoading(true);
    setIsPaymentModalOpen(false);
    try {
      if (paymentAction === 'APPROVE') {
        const pendingQuote = booking?.quotations?.find((q: any) => q.status === 'PENDING');
        const quotationId = pendingQuote?.id;
        if (!quotationId) {
          message.error('Không tìm thấy báo giá đang chờ duyệt');
          return;
        }
        await axiosInstance.post(`/quotations/${quotationId}/approve`, { promoCode: appliedPromo || null });
      }

      const endpoint = selectedProvider === 'VNPAY' ? '/payments/vnpay' : selectedProvider === 'MOMO' ? '/payments/momo' : '/payments/sepay';
      const paymentRes = await axiosInstance.post(endpoint, {
        bookingId: id,
        provider: selectedProvider
      });
      
      if (paymentRes.data?.paymentUrl) {
        window.location.href = paymentRes.data.paymentUrl;
      } else {
        message.error('Không lấy được URL thanh toán');
        fetchData();
      }
    } catch (err: any) {
      console.error('Lỗi thanh toán:', err);
      message.error(err.response?.data?.message || 'Lỗi khi xử lý thanh toán');
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="h-screen flex items-center justify-center bg-white">
        <span className="w-10 h-10 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></span>
      </div>
    );
  }

  if (!booking) return <div className="p-10 text-center">Không tìm thấy đơn hàng</div>;

  const currentRole = user?.role?.toUpperCase();
  const currentStatus = booking.status?.toUpperCase();
  
  const isCustomer = currentRole === 'CUSTOMER';
  const isWorker = currentRole === 'WORKER';
  
  const activeQuotation = booking.quotations?.find((q: any) => q.status === 'PENDING' || q.status === 'APPROVED');

  return (
    <div className="min-h-screen bg-gray-50 pb-32">
      {/* Header */}
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm sticky top-0 z-20 flex items-center gap-4">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 hover:bg-gray-100 rounded-full transition-all">
          <ChevronLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-xl font-bold text-gray-900">Chi tiết đơn hàng</h1>
      </div>

      <div className="p-4 space-y-4 max-w-lg mx-auto">
        {/* Status Card */}
        <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 text-center">
          <div className="flex gap-2 justify-center mb-3">
            <div className="inline-block px-4 py-1.5 rounded-full bg-orange-50 text-orange-600 text-xs font-bold">
              {currentStatus}
            </div>
            {booking.paymentStatus === 'PAID' && (
              <div className="inline-block px-4 py-1.5 rounded-full bg-green-50 text-green-600 text-xs font-bold">
                ĐÃ THANH TOÁN
              </div>
            )}
            {booking.paymentStatus === 'UNPAID' && (currentStatus === 'WORKING' || currentStatus === 'COMPLETED') && (
              <div className="inline-block px-4 py-1.5 rounded-full bg-red-50 text-red-600 text-xs font-bold">
                CHƯA THANH TOÁN
              </div>
            )}
          </div>
          <h2 className="text-2xl font-black text-gray-900 mb-1">{booking.service.name}</h2>
          <p className="text-sm text-gray-500">Mã đơn: #{booking.id.split('-')[0]}</p>
        </div>

        {/* Info Section */}
        <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 space-y-6">
          <div className="flex gap-4">
            <div className="w-10 h-10 bg-gray-50 rounded-2xl flex items-center justify-center flex-shrink-0 text-orange-500">
              <MapPin className="w-5 h-5" />
            </div>
            <div className="flex-1">
              <p className="text-[10px] font-bold text-gray-400 uppercase tracking-wider">Địa chỉ thực hiện</p>
              <p className="text-sm text-gray-700 font-medium mt-0.5">{booking.address}</p>
              {isWorker && booking.navigationUrl && (
                <button 
                  onClick={() => window.open(booking.navigationUrl, '_blank')}
                  className="mt-2 flex items-center gap-1.5 text-xs font-bold text-blue-600 bg-blue-50 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition-all w-fit"
                >
                  <MapPin className="w-3.5 h-3.5" /> Dẫn đường tới đây
                </button>
              )}
            </div>
          </div>

          <div className="flex gap-4">
            <div className="w-10 h-10 bg-gray-50 rounded-2xl flex items-center justify-center flex-shrink-0 text-orange-500">
              <Clock className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] font-bold text-gray-400 uppercase tracking-wider">Thời gian tạo đơn</p>
              <p className="text-sm text-gray-700 font-medium mt-0.5">
                {new Date(booking.createdAt).toLocaleString('vi-VN')}
              </p>
            </div>
          </div>

          <div className="flex gap-4">
            <div className="w-10 h-10 bg-gray-50 rounded-2xl flex items-center justify-center flex-shrink-0 text-orange-500">
              <AlertCircle className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] font-bold text-gray-400 uppercase tracking-wider">Mô tả công việc</p>
              <p className="text-sm text-gray-700 font-medium mt-0.5">{booking.description || 'Không có mô tả'}</p>
            </div>
          </div>

          <div className="pt-6 border-t border-gray-50 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 font-bold">
                {(isCustomer ? booking.worker?.fullName : booking.customer.fullName)?.charAt(0) || '?'}
              </div>
              <div>
                <p className="text-[10px] font-bold text-gray-400 uppercase tracking-wider">
                  {isCustomer ? 'Người thực hiện' : 'Khách hàng'}
                </p>
                <p className="text-sm text-gray-900 font-bold">
                  {isCustomer ? (booking.worker?.fullName || 'Đang tìm thợ...') : booking.customer.fullName}
                </p>
              </div>
            </div>
            {(isCustomer && booking.worker) || isWorker ? (
              <div className="flex gap-2">
                <button 
                  onClick={() => navigate(`/bookings/${booking.id}/chat`)}
                  className="p-3 bg-orange-50 text-orange-600 rounded-2xl hover:bg-orange-100 transition-all"
                >
                  <MessageSquare className="w-5 h-5" />
                </button>
                <button className="p-3 bg-green-50 text-green-600 rounded-2xl hover:bg-green-100 transition-all">
                  <Phone className="w-5 h-5" />
                </button>
              </div>
            ) : null}
          </div>
        </div>

        {/* Quotation Section */}
        {activeQuotation && (
          <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100">
            <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
               <FileText className="w-5 h-5 text-orange-500" /> Báo giá dịch vụ
            </h3>
            <div className="space-y-3 mb-4">
              {activeQuotation.items?.map((item: any, idx: number) => (
                <div key={idx} className="flex justify-between items-center text-sm">
                  <div>
                    <p className="font-semibold text-gray-800">{item.itemName}</p>
                    <p className="text-gray-500 text-xs">SL: {item.quantity}</p>
                  </div>
                  <p className="font-bold text-gray-900">{(item.unitPrice * item.quantity).toLocaleString('vi-VN')} đ</p>
                </div>
              ))}
            </div>
            <div className="pt-4 border-t border-gray-100 flex justify-between items-center">
              <span className="font-bold text-gray-500">Tổng cộng:</span>
              <span className="text-xl font-black text-orange-600">{activeQuotation.totalAmount?.toLocaleString('vi-VN')} đ</span>
            </div>
            {activeQuotation.note && (
              <p className="mt-4 text-xs text-gray-500 italic bg-gray-50 p-3 rounded-xl border border-gray-100">
                Ghi chú: {activeQuotation.note}
              </p>
            )}
          </div>
        )}

        {/* Timeline Section */}
        <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100">
          <h3 className="text-lg font-bold text-gray-900 mb-6 flex items-center gap-2">
            <ListOrdered className="w-5 h-5 text-orange-500" /> Tiến độ đơn hàng
          </h3>
          
          <div className="space-y-8 relative before:absolute before:inset-0 before:left-[19px] before:w-0.5 before:bg-gray-100">
            {timeline.length === 0 ? (
              <p className="text-sm text-gray-400 italic pl-10">Chưa có cập nhật tiến độ</p>
            ) : (
              timeline.map((event, idx) => (
                <div key={event.id} className="relative flex items-start pl-10 group">
                  <div className={clsx(
                    "absolute left-0 w-10 h-10 rounded-full flex items-center justify-center z-10 border-4 border-white transition-all",
                    idx === timeline.length - 1 ? "bg-orange-500 text-white shadow-lg shadow-orange-200" : "bg-gray-100 text-gray-400"
                  )}>
                    <CheckCircle className="w-5 h-5" />
                  </div>
                  <div>
                    <p className={clsx(
                      "text-sm font-bold",
                      idx === timeline.length - 1 ? "text-orange-600" : "text-gray-700"
                    )}>
                      {event.newStatus}
                    </p>
                    <p className="text-[10px] text-gray-400 font-medium">
                      {new Date(event.createdAt).toLocaleString('vi-VN')}
                    </p>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Action Buttons */}
        <div className="fixed bottom-0 left-0 right-0 p-4 bg-white/95 backdrop-blur-xl border-t border-gray-100 z-[60] shadow-[0_-10px_30px_rgba(0,0,0,0.08)]">
          <div className="max-w-lg mx-auto">
            {isWorker && (
              <div className="flex flex-col gap-3">
                {(currentStatus === 'MATCHING' || currentStatus === 'PENDING') && !booking.worker && (
                  <div className="flex gap-3">
                    <button 
                      onClick={handleReject}
                      disabled={actionLoading}
                      className="flex-1 py-4 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold rounded-2xl transition-all flex items-center justify-center gap-2"
                    >
                      <X className="w-5 h-5" /> Từ chối
                    </button>
                    <button 
                      onClick={handleAccept}
                      disabled={actionLoading}
                      className="flex-[2] py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-xl shadow-orange-500/20 transition-all flex items-center justify-center gap-2"
                    >
                      {actionLoading ? <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span> : <Check className="w-5 h-5" />}
                      Chấp nhận ngay
                    </button>
                  </div>
                )}
                {(currentStatus === 'ASSIGNED' || currentStatus === 'INSPECTING') && (
                  <button 
                    onClick={() => navigate(`/worker/bookings/${booking.id}/quotation/create`)}
                    disabled={actionLoading}
                    className="w-full py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-xl shadow-orange-500/20 transition-all"
                  >
                    Tạo báo giá
                  </button>
                )}
                {currentStatus === 'QUOTED' && (
                  <div className="w-full py-4 bg-gray-100 text-gray-500 font-bold rounded-2xl text-center">
                    Đang chờ khách hàng duyệt báo giá
                  </div>
                )}
                {currentStatus === 'ON_THE_WAY' && (
                  <button 
                    onClick={() => updateStatus('WORKING')}
                    disabled={actionLoading}
                    className="w-full py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-xl shadow-orange-500/20 transition-all"
                  >
                    Đã đến & Bắt đầu làm việc
                  </button>
                )}
                {currentStatus === 'WORKING' && (
                  <button 
                    onClick={handleFinishWork}
                    disabled={actionLoading}
                    className="w-full py-4 bg-green-500 hover:bg-green-600 text-white font-bold rounded-2xl shadow-xl shadow-green-500/20 transition-all"
                  >
                    Hoàn thành công việc
                  </button>
                )}
              </div>
            )}

            {isCustomer && (currentStatus === 'WORKING' || currentStatus === 'COMPLETED') && booking.paymentStatus !== 'PAID' && (
              <button 
                onClick={handleDirectPayment}
                disabled={actionLoading}
                className="w-full py-4 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-xl shadow-blue-500/20 mb-3"
              >
                {actionLoading ? 'Đang xử lý...' : 'Thanh toán ngay'}
              </button>
            )}

            {isCustomer && currentStatus === 'COMPLETED' && (
              <div className="flex flex-col gap-3">
                <button 
                  onClick={() => setIsReviewModalOpen(true)}
                  className="w-full py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-xl shadow-gray-900/20 transition-all flex items-center justify-center gap-2"
                >
                  <Star className="w-5 h-5 text-yellow-400 fill-yellow-400" /> Đánh giá ngay
                </button>
                <button 
                  onClick={() => navigate(`/customer/bookings/${booking.id}/dispute`)}
                  className="w-full py-3 bg-white text-red-500 font-bold rounded-2xl flex items-center justify-center gap-2 hover:bg-red-50"
                >
                  <AlertTriangle className="w-5 h-5" /> Khiếu nại dịch vụ
                </button>
              </div>
            )}

            {isCustomer && currentStatus === 'QUOTED' && (
              <div className="flex gap-3">
                <button 
                  className="flex-1 py-4 bg-gray-100 text-gray-700 font-bold rounded-2xl"
                >
                  Từ chối
                </button>
                <button 
                  onClick={handleApproveQuote}
                  disabled={actionLoading}
                  className="flex-[2] py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-xl shadow-orange-500/20 flex items-center justify-center"
                >
                  {actionLoading ? 'Đang xử lý...' : 'Chấp nhận & Thanh toán'}
                </button>
              </div>
            )}

            {isCustomer && ['PENDING', 'MATCHING', 'ASSIGNED'].includes(currentStatus || '') && (
              <Popconfirm
                title="Hủy đơn hàng?"
                description="Bạn có chắc chắn muốn hủy đơn hàng này không?"
                onConfirm={handleCancel}
                okText="Đồng ý"
                cancelText="Quay lại"
                okButtonProps={{ danger: true }}
              >
                <button 
                  disabled={actionLoading}
                  className="w-full py-4 bg-white border-2 border-red-100 text-red-500 font-bold rounded-2xl hover:bg-red-50 transition-all"
                >
                  Hủy đơn hàng
                </button>
              </Popconfirm>
            )}
          </div>
        </div>
      </div>

      {/* Review Modal */}
      <Modal
        title="Đánh giá dịch vụ"
        open={isReviewModalOpen}
        onCancel={() => setIsReviewModalOpen(false)}
        footer={null}
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-4 flex flex-col items-center gap-6">
          <div className="text-center">
            <p className="text-gray-500 text-sm mb-4">Trải nghiệm của bạn với thợ <strong>{booking.worker?.fullName}</strong> thế nào?</p>
            <Rate 
              value={rating} 
              onChange={setRating} 
              className="text-4xl text-orange-500"
            />
          </div>
          
          <div className="w-full">
            <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Nhận xét của bạn</label>
            <Input.TextArea
              rows={4}
              value={comment}
              onChange={e => setComment(e.target.value)}
              placeholder="Chia sẻ thêm chi tiết về chất lượng dịch vụ..."
              className="rounded-2xl border-gray-100 focus:border-orange-500 focus:ring-orange-500/20"
            />
          </div>

          <button
            onClick={handleReviewSubmit}
            disabled={submittingReview}
            className="w-full py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20 disabled:opacity-50"
          >
            {submittingReview ? 'Đang gửi...' : 'Gửi đánh giá'}
          </button>
        </div>
      </Modal>

      {/* Payment Provider Selection Modal */}
      <Modal
        title="Chọn phương thức thanh toán"
        open={isPaymentModalOpen}
        onCancel={() => setIsPaymentModalOpen(false)}
        footer={null}
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-4 space-y-4">
          <p className="text-gray-500 text-sm text-center mb-6">
            Vui lòng chọn phương thức thanh toán để tiếp tục
          </p>

          {activeQuotation && (
            <div className="bg-gray-50 p-4 rounded-2xl mb-4 border border-gray-100">
               <div className="flex justify-between text-sm mb-2">
                 <span className="text-gray-600">Tổng tiền báo giá:</span>
                 <span className="font-bold">{activeQuotation.totalAmount.toLocaleString('vi-VN')} đ</span>
               </div>
               {appliedPromo && (
                 <div className="flex justify-between text-sm mb-2 text-green-600">
                   <span>Giảm giá (Mã {appliedPromo}):</span>
                   <span className="font-bold">-{promoDiscount.toLocaleString('vi-VN')} đ</span>
                 </div>
               )}
               <div className="flex justify-between text-lg mt-3 pt-3 border-t border-gray-200">
                 <span className="font-bold text-gray-900">Cần thanh toán:</span>
                 <span className="font-black text-orange-600">
                    {Math.max(0, activeQuotation.totalAmount - promoDiscount).toLocaleString('vi-VN')} đ
                 </span>
               </div>
            </div>
          )}

          {isCustomer && (paymentAction === 'APPROVE' || paymentAction === 'DIRECT') && (
            <div className="mb-4">
               <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Mã Khuyến Mãi (Nếu có)</label>
               <div className="flex gap-2">
                 <Input 
                   value={promoCode} 
                   onChange={(e) => setPromoCode(e.target.value.toUpperCase())} 
                   disabled={!!appliedPromo}
                   placeholder="Nhập mã voucher" 
                   className="uppercase rounded-xl"
                 />
                 {!appliedPromo ? (
                    <button 
                      onClick={handleValidatePromo} 
                      disabled={validatingPromo || !promoCode}
                      className="px-4 bg-gray-900 text-white rounded-xl text-sm font-bold hover:bg-black disabled:opacity-50 whitespace-nowrap"
                    >
                      {validatingPromo ? 'Đang kiểm tra...' : 'Áp dụng'}
                    </button>
                 ) : (
                    <button 
                      onClick={() => { setAppliedPromo(null); setPromoDiscount(0); setPromoCode(''); }}
                      className="px-4 bg-red-100 text-red-600 rounded-xl text-sm font-bold hover:bg-red-200 whitespace-nowrap"
                    >
                      Xóa
                    </button>
                 )}
               </div>
               {promoError && <p className="text-xs text-red-500 mt-1">{promoError}</p>}
            </div>
          )}

          <div className="grid grid-cols-1 gap-3">
            <button
              onClick={() => setSelectedProvider('VNPAY')}
              className={clsx(
                "flex items-center gap-4 p-4 rounded-2xl border-2 transition-all text-left",
                selectedProvider === 'VNPAY' ? "border-orange-500 bg-orange-50" : "border-gray-100 hover:border-gray-200"
              )}
            >
              <div className="w-12 h-12 bg-white rounded-xl shadow-sm flex items-center justify-center p-2">
                <img src="https://vnpay.vn/wp-content/uploads/2020/07/Logo-VNPAYQR-update.png" alt="VNPay" className="w-full h-auto object-contain" />
              </div>
              <div>
                <p className="font-bold text-gray-900">VNPay</p>
                <p className="text-xs text-gray-500">Thanh toán qua ứng dụng ngân hàng hoặc thẻ ATM</p>
              </div>
            </button>

            <button
              onClick={() => setSelectedProvider('MOMO')}
              className={clsx(
                "flex items-center gap-4 p-4 rounded-2xl border-2 transition-all text-left",
                selectedProvider === 'MOMO' ? "border-pink-500 bg-pink-50" : "border-gray-100 hover:border-gray-200"
              )}
            >
              <div className="w-12 h-12 bg-white rounded-xl shadow-sm flex items-center justify-center p-2">
                <img src="https://upload.wikimedia.org/wikipedia/vi/f/fe/MoMo_Logo.png" alt="MoMo" className="w-full h-auto object-contain" />
              </div>
              <div>
                <p className="font-bold text-gray-900">Ví MoMo</p>
                <p className="text-xs text-gray-500">Thanh toán nhanh qua ví điện tử MoMo</p>
              </div>
            </button>

            <button
              onClick={() => setSelectedProvider('SEPAY')}
              className={clsx(
                "flex items-center gap-4 p-4 rounded-2xl border-2 transition-all text-left",
                selectedProvider === 'SEPAY' ? "border-blue-500 bg-blue-50" : "border-gray-100 hover:border-gray-200"
              )}
            >
              <div className="w-12 h-12 bg-white rounded-xl shadow-sm flex items-center justify-center p-2">
                <img src="https://sepay.vn/assets/img/logo.svg" alt="SePay" className="w-full h-auto object-contain" />
              </div>
              <div>
                <p className="font-bold text-gray-900">VietQR (SePay)</p>
                <p className="text-xs text-gray-500">Thanh toán bằng ứng dụng ngân hàng qua mã QR</p>
              </div>
            </button>
          </div>

          <button
            onClick={executePayment}
            disabled={actionLoading}
            className="w-full mt-6 py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20"
          >
            {actionLoading ? 'Đang xử lý...' : 'Thanh toán ngay'}
          </button>
        </div>
      </Modal>
    </div>
  );
}
