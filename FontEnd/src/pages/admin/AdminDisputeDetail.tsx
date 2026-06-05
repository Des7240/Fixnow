import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ChevronLeft, CheckCircle, XCircle, DollarSign, Image as ImageIcon } from 'lucide-react';
import { message, Modal, Input } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { getImageUrl } from '../../utils/constants';
import { clsx } from 'clsx';

interface DisputeDetail {
  id: string;
  bookingId: string;
  customerId: string;
  customerName: string;
  workerId: string;
  workerName: string;
  reason: string;
  status: string;
  createdAt: string;
  evidences: Array<{
    id: string;
    fileUrl: string;
    createdAt: string;
  }>;
  refunds: Array<{
    id: string;
    amount: number;
    status: string;
    createdAt: string;
  }>;
}

export default function AdminDisputeDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [dispute, setDispute] = useState<DisputeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  const [isRefundModalOpen, setIsRefundModalOpen] = useState(false);
  const [refundAmount, setRefundAmount] = useState('');

  useEffect(() => {
    fetchDispute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const fetchDispute = async () => {
    try {
      setLoading(true);
      const res = await axiosInstance.get(`/disputes/${id}`);
      setDispute(res.data);
    } catch (err) {
      message.error('Không thể tải chi tiết khiếu nại');
      navigate('/admin/disputes');
    } finally {
      setLoading(false);
    }
  };

  const handleCloseDispute = async () => {
    try {
      setActionLoading(true);
      await axiosInstance.post(`/disputes/admin/${id}/close`);
      message.success('Đã đóng khiếu nại thành công');
      fetchDispute();
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi đóng khiếu nại');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRefund = async () => {
    const amount = parseInt(refundAmount);
    if (isNaN(amount) || amount <= 0) {
      message.error('Số tiền hoàn trả không hợp lệ');
      return;
    }

    try {
      setActionLoading(true);
      await axiosInstance.post(`/disputes/admin/${id}/refund`, {
        amount,
        refundType: 'PARTIAL' // MVP simply uses PARTIAL
      });
      message.success('Đã tạo lệnh hoàn tiền thành công! Tiền sẽ được tự động trừ từ ví thợ.');
      setIsRefundModalOpen(false);
      setRefundAmount('');
      fetchDispute();
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi tạo hoàn tiền');
    } finally {
      setActionLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'OPEN': return 'bg-red-100 text-red-600';
      case 'INVESTIGATING': return 'bg-orange-100 text-orange-600';
      case 'RESOLVED': return 'bg-green-100 text-green-600';
      case 'REFUNDED': return 'bg-blue-100 text-blue-600';
      case 'CLOSED': return 'bg-gray-100 text-gray-600';
      default: return 'bg-gray-100 text-gray-600';
    }
  };

  if (loading) {
    return (
      <div className="p-8 flex justify-center items-center h-full">
        <span className="w-10 h-10 border-4 border-orange-500 border-t-transparent rounded-full animate-spin inline-block"></span>
      </div>
    );
  }

  if (!dispute) return null;

  return (
    <div className="p-8">
      <div className="flex items-center gap-4 mb-8">
        <button onClick={() => navigate('/admin/disputes')} className="p-2 -ml-2 rounded-full hover:bg-gray-200 transition-colors">
          <ChevronLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-2xl font-bold text-gray-900">Chi tiết Khiếu nại</h1>
        <span className={clsx("px-3 py-1 rounded-full text-xs font-bold ml-auto", getStatusColor(dispute.status))}>
          {dispute.status}
        </span>
      </div>

      <div className="grid grid-cols-3 gap-6">
        {/* Left Column: Details */}
        <div className="col-span-2 space-y-6">
          <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-200">
            <h2 className="text-lg font-bold text-gray-900 mb-4">Thông tin chung</h2>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-gray-500 mb-1">Mã khiếu nại</p>
                <p className="font-semibold text-gray-900">#{dispute.id}</p>
              </div>
              <div>
                <p className="text-gray-500 mb-1">Mã đơn hàng</p>
                <p className="font-semibold text-blue-600 cursor-pointer hover:underline">#{dispute.bookingId}</p>
              </div>
              <div>
                <p className="text-gray-500 mb-1">Người khiếu nại (Customer)</p>
                <p className="font-semibold text-gray-900">{dispute.customerName}</p>
                <p className="text-[10px] text-gray-400">{dispute.customerId}</p>
              </div>
              <div>
                <p className="text-gray-500 mb-1">Người bị khiếu nại (Worker)</p>
                <p className="font-semibold text-gray-900">{dispute.workerName}</p>
                <p className="text-[10px] text-gray-400">{dispute.workerId}</p>
              </div>
            </div>
            
            <div className="mt-6">
              <p className="text-gray-500 mb-2">Lý do khiếu nại</p>
              <div className="bg-red-50 text-red-900 p-4 rounded-xl border border-red-100">
                {dispute.reason}
              </div>
            </div>
          </div>

          <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-200">
            <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <ImageIcon className="w-5 h-5 text-gray-400" /> Bằng chứng đính kèm
            </h2>
            {dispute.evidences.length === 0 ? (
              <p className="text-gray-500 italic">Không có bằng chứng nào được cung cấp.</p>
            ) : (
              <div className="grid grid-cols-3 gap-4">
                {dispute.evidences.map((ev) => (
                  <a key={ev.id} href={getImageUrl(ev.fileUrl)} target="_blank" rel="noreferrer" className="block relative group aspect-square rounded-xl overflow-hidden border border-gray-200 bg-gray-50">
                    <img 
                      src={getImageUrl(ev.fileUrl)} 
                      alt="Evidence" 
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" 
                    />
                    <div className="absolute inset-0 bg-black/50 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                      <span className="text-white text-sm font-medium">Xem ảnh lớn</span>
                    </div>
                  </a>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right Column: Actions & Refunds */}
        <div className="space-y-6">
          <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-200">
            <h2 className="text-lg font-bold text-gray-900 mb-4">Phán quyết</h2>
            
            {dispute.status === 'CLOSED' || dispute.status === 'REFUNDED' ? (
              <div className="text-center py-6 bg-gray-50 rounded-2xl border border-gray-100">
                <CheckCircle className="w-12 h-12 text-gray-400 mx-auto mb-2" />
                <p className="text-gray-500 font-medium">Khiếu nại này đã được đóng</p>
              </div>
            ) : (
              <div className="space-y-3">
                <button 
                  onClick={() => setIsRefundModalOpen(true)}
                  disabled={actionLoading}
                  className="w-full py-3 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-xl shadow-lg shadow-orange-500/20 transition-colors flex justify-center items-center gap-2"
                >
                  <DollarSign className="w-5 h-5" /> Hoàn tiền (Trừ từ Ví Thợ)
                </button>
                <button 
                  onClick={handleCloseDispute}
                  disabled={actionLoading}
                  className="w-full py-3 bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-bold rounded-xl transition-colors flex justify-center items-center gap-2"
                >
                  <XCircle className="w-5 h-5" /> Đóng khiếu nại (Không hoàn tiền)
                </button>
              </div>
            )}
          </div>

          {dispute.refunds.length > 0 && (
            <div className="bg-white rounded-3xl p-6 shadow-sm border border-gray-200">
              <h2 className="text-lg font-bold text-gray-900 mb-4">Lịch sử Hoàn tiền</h2>
              <div className="space-y-3">
                {dispute.refunds.map(refund => (
                  <div key={refund.id} className="p-4 bg-orange-50 rounded-xl border border-orange-100">
                    <div className="flex justify-between items-center mb-1">
                      <span className="font-bold text-gray-900">{refund.amount.toLocaleString('vi-VN')} đ</span>
                      <span className="text-[10px] font-bold px-2 py-1 bg-orange-200 text-orange-800 rounded-md">
                        {refund.status}
                      </span>
                    </div>
                    <p className="text-xs text-gray-500">
                      {new Date(refund.createdAt).toLocaleString('vi-VN')}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      <Modal
        title="Tạo lệnh Hoàn tiền"
        open={isRefundModalOpen}
        onCancel={() => setIsRefundModalOpen(false)}
        footer={null}
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-4 space-y-4">
          <div className="bg-orange-50 text-orange-800 p-4 rounded-xl text-sm mb-4">
            Lưu ý: Hành động này sẽ <strong>tự động trừ tiền từ ví của Thợ</strong>. Vui lòng đảm bảo bạn đã kiểm tra kỹ các bằng chứng trước khi phán quyết.
          </div>
          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Số tiền hoàn (VND)</label>
            <Input
              type="number"
              value={refundAmount}
              onChange={e => setRefundAmount(e.target.value)}
              placeholder="Ví dụ: 150000"
              className="py-3 px-4 rounded-xl font-bold text-lg"
            />
          </div>
          <button
            onClick={handleRefund}
            disabled={actionLoading}
            className="w-full py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20 disabled:opacity-50 flex justify-center items-center"
          >
            {actionLoading ? <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span> : 'Xác nhận trừ tiền & Hoàn trả'}
          </button>
        </div>
      </Modal>
    </div>
  );
}
