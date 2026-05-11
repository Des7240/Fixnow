import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Star, Clock, DollarSign, CheckCircle, Loader2, Wrench, User, ChevronRight, Shield } from 'lucide-react';
import { message, Modal, Badge } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { API_BASE_URL } from '../../utils/constants';

interface Offer {
  id: string;
  workerId: string;
  workerName: string;
  workerAvatar?: string;
  estimatedPrice: number;
  analysis: string;
  estimatedArrivalMinutes: number;
  estimatedRepairTimeMinutes: number;
  warrantyDays?: number;
  workerRating: number;
  workerCompletedJobs: number;
  workerScore: number;
}

interface OpenJob {
  id: string;
  title: string;
  status: string;
}

export default function ViewOffers() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [job, setJob] = useState<OpenJob | null>(null);
  const [selecting, setSelecting] = useState<string | null>(null);

  useEffect(() => {
    fetchData();
  }, [id]);

  const fetchData = async () => {
    try {
      const [jobRes, offersRes] = await Promise.all([
        axiosInstance.get(`/open-jobs/${id}`),
        axiosInstance.get(`/open-jobs/${id}/offers`)
      ]);
      setJob(jobRes.data);
      setOffers(offersRes.data);
    } catch (err) {
      message.error('Không thể tải dữ liệu');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectWorker = (offer: Offer) => {
    Modal.confirm({
        title: 'Xác nhận chọn thợ',
        content: `Bạn có chắc chắn muốn chọn thợ ${offer.workerName} cho công việc này không?`,
        okText: 'Xác nhận',
        cancelText: 'Hủy',
        onOk: async () => {
            setSelecting(offer.id);
            try {
                await axiosInstance.post(`/open-jobs/${id}/select-worker`, { offerId: offer.id });
                message.success('Đã chọn thợ thành công! Bạn có thể bắt đầu chat với thợ.');
                navigate('/customer/bookings'); // Usually redirect to the newly created booking
            } catch (err: any) {
                message.error(err?.response?.data?.message || 'Lỗi khi chọn thợ');
            } finally {
                setSelecting(null);
            }
        }
    });
  };

  const handleRejectOffer = (offer: Offer) => {
    Modal.confirm({
        title: 'Từ chối báo giá',
        content: `Bạn có chắc chắn muốn từ chối báo giá của ${offer.workerName}?`,
        okText: 'Từ chối',
        okType: 'danger',
        cancelText: 'Hủy',
        onOk: async () => {
            try {
                await axiosInstance.post(`/open-jobs/offers/${offer.id}/reject`);
                message.success('Đã từ chối báo giá');
                fetchData();
            } catch (err) {
                message.error('Lỗi khi từ chối báo giá');
            }
        }
    });
  };

  if (loading) return (
    <div className="h-screen flex items-center justify-center">
        <Loader2 className="w-10 h-10 text-orange-500 animate-spin" />
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white px-4 py-4 flex items-center gap-4 shadow-sm sticky top-0 z-20">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
          <ArrowLeft className="w-6 h-6 text-gray-700" />
        </button>
        <div>
            <h1 className="text-lg font-bold text-gray-900">Danh sách báo giá</h1>
            <p className="text-xs text-gray-500 font-medium line-clamp-1">{job?.title}</p>
        </div>
      </div>

      <div className="flex-1 p-4 space-y-4">
        {offers.length === 0 ? (
            <div className="text-center py-20 bg-white rounded-3xl border border-dashed border-gray-200">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Clock className="w-10 h-10 text-gray-300" />
                </div>
                <h3 className="text-gray-900 font-bold">Chưa có thợ nào báo giá</h3>
                <p className="text-gray-500 text-sm max-w-[200px] mx-auto mt-2">Thông thường thợ sẽ báo giá trong vòng 15-30 phút.</p>
            </div>
        ) : (
            <div className="space-y-4">
                <p className="text-xs font-bold text-gray-400 uppercase ml-1">Đã nhận {offers.length} báo giá</p>
                {offers.map((offer) => (
                    <div key={offer.id} className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
                        <div className="p-5">
                            <div className="flex justify-between items-start mb-4">
                                <div className="flex items-center gap-3">
                                    <div className="w-12 h-12 bg-orange-100 rounded-2xl flex items-center justify-center overflow-hidden">
                                        {offer.workerAvatar ? (
                                            <img src={`${API_BASE_URL}${offer.workerAvatar}`} alt="avatar" className="w-full h-full object-cover" />
                                        ) : (
                                            <User className="w-6 h-6 text-orange-600" />
                                        )}
                                    </div>
                                    <div>
                                        <div className="flex items-center gap-2">
                                            <h4 className="font-bold text-gray-900">{offer.workerName}</h4>
                                            <Badge count={`${offer.workerScore.toFixed(0)}đ`} style={{ backgroundColor: '#f59e0b', fontSize: '10px' }} />
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <div className="flex items-center text-orange-500">
                                                <Star className="w-3 h-3 fill-current" />
                                                <span className="text-xs font-bold ml-1">{offer.workerRating.toFixed(1)}</span>
                                            </div>
                                            <span className="text-[10px] text-gray-400 font-bold">•</span>
                                            <span className="text-[10px] text-gray-500 font-bold uppercase">{offer.workerCompletedJobs} việc đã làm</span>
                                        </div>
                                    </div>
                                </div>
                                <div className="text-right">
                                    <p className="text-[10px] text-gray-400 font-bold uppercase">Báo giá</p>
                                    <p className="text-lg font-black text-orange-600">
                                        {offer.estimatedPrice.toLocaleString()}đ
                                    </p>
                                </div>
                            </div>

                            <div className="bg-gray-50 p-4 rounded-2xl mb-4">
                                <p className="text-xs font-bold text-gray-400 uppercase mb-2 flex items-center gap-1">
                                    <Wrench className="w-3 h-3" /> Phân tích thợ
                                </p>
                                <p className="text-sm text-gray-700 leading-relaxed italic">
                                    "{offer.analysis}"
                                </p>
                            </div>

                            <div className="flex items-center gap-6 mb-5 ml-1 flex-wrap">
                                <div className="flex items-center gap-2">
                                    <Clock className="w-4 h-4 text-gray-400" />
                                    <div>
                                        <p className="text-[10px] text-gray-400 font-bold uppercase">Thời gian đến</p>
                                        <p className="text-xs font-bold text-gray-900">{offer.estimatedArrivalMinutes} phút</p>
                                    </div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Wrench className="w-4 h-4 text-gray-400" />
                                    <div>
                                        <p className="text-[10px] text-gray-400 font-bold uppercase">Sửa trong</p>
                                        <p className="text-xs font-bold text-gray-900">{offer.estimatedRepairTimeMinutes} phút</p>
                                    </div>
                                </div>
                                {offer.warrantyDays && (
                                    <div className="flex items-center gap-2">
                                        <Shield className="w-4 h-4 text-blue-500" />
                                        <div>
                                            <p className="text-[10px] text-gray-400 font-bold uppercase">Bảo hành</p>
                                            <p className="text-xs font-bold text-blue-600">{offer.warrantyDays} ngày</p>
                                        </div>
                                    </div>
                                )}
                            </div>

                            <div className="flex gap-3">
                                <button 
                                    onClick={() => handleRejectOffer(offer)}
                                    disabled={selecting === offer.id}
                                    className="flex-1 py-3 bg-red-50 text-red-600 text-sm font-bold rounded-xl hover:bg-red-100 transition-colors"
                                >
                                    Từ chối
                                </button>
                                <button 
                                    onClick={() => handleSelectWorker(offer)}
                                    disabled={selecting === offer.id}
                                    className="flex-[2] py-3 bg-gray-900 text-white text-sm font-bold rounded-xl hover:bg-black transition-colors shadow-lg shadow-gray-900/20 flex items-center justify-center gap-2"
                                >
                                    {selecting === offer.id ? <Loader2 className="w-4 h-4 animate-spin" /> : <CheckCircle className="w-4 h-4" />}
                                    Chọn thợ này
                                </button>
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        )}
      </div>
    </div>
  );
}
