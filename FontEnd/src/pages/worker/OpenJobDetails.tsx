import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, MapPin, Clock, FileText, Wrench, Shield, Send, Loader2, DollarSign, AlertCircle } from 'lucide-react';
import { message, Modal } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { API_BASE_URL } from '../../utils/constants';

interface OpenJob {
  id: string;
  title: string;
  description: string;
  address: string;
  serviceName: string;
  createdAt: string;
  fileUrls: string[];
  minBudget?: number;
  maxBudget?: number;
  urgencyLevel?: string;
  navigationUrl?: string;
}

export default function OpenJobDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [job, setJob] = useState<OpenJob | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [offerModal, setOfferModal] = useState(false);
  
  // Offer Form
  const [price, setPrice] = useState('');
  const [analysis, setAnalysis] = useState('');
  const [eta, setEta] = useState('30');
  const [repairTime, setRepairTime] = useState('60');
  const [warranty, setWarranty] = useState('30');

  useEffect(() => {
    fetchJobDetails();
  }, [id]);

  const fetchJobDetails = async () => {
    try {
      const res = await axiosInstance.get(`/open-jobs/${id}`);
      setJob(res.data);
    } catch (err) {
      message.error('Không thể tải chi tiết công việc');
      navigate(-1);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitOffer = async () => {
    if (!price || !analysis) {
        message.warning('Vui lòng điền đầy đủ giá dự kiến và phân tích tình trạng');
        return;
    }

    setSubmitting(true);
    try {
      await axiosInstance.post(`/open-jobs/${id}/offers`, {
        estimatedPrice: parseFloat(price),
        analysis,
        estimatedArrivalMinutes: parseInt(eta),
        estimatedRepairTimeMinutes: parseInt(repairTime),
        warrantyDays: parseInt(warranty)
      });
      message.success('Gửi báo giá thành công!');
      setOfferModal(false);
      navigate('/worker');
    } catch (err: any) {
      message.error(err?.response?.data?.message || 'Lỗi khi gửi báo giá');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return (
    <div className="h-screen flex items-center justify-center">
        <Loader2 className="w-10 h-10 text-orange-500 animate-spin" />
    </div>
  );

  if (!job) return null;

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col pb-24">
      {/* Header */}
      <div className="bg-white px-4 py-4 flex items-center gap-4 shadow-sm sticky top-0 z-20">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
          <ArrowLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-lg font-bold text-gray-900">Chi tiết công việc</h1>
      </div>

      <div className="p-4 space-y-6 max-w-md mx-auto w-full">
        {/* Basic Info */}
        <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4">
                <span className="bg-orange-100 text-orange-600 text-[10px] font-bold px-2 py-1 rounded-full uppercase tracking-wider">
                    {job.serviceName}
                </span>
                <span className="text-xs text-gray-400">
                    Đăng lúc: {new Date(job.createdAt).toLocaleString()}
                </span>
            </div>
            <h2 className="text-xl font-bold text-gray-900 mb-4">{job.title}</h2>
            
            <div className="flex flex-wrap gap-2 mb-4">
                {(job.minBudget || job.maxBudget) ? (
                    <div className="flex items-center gap-1.5 bg-green-50 text-green-700 px-3 py-1.5 rounded-xl border border-green-100">
                        <DollarSign className="w-4 h-4" />
                        <span className="text-sm font-bold">
                            {job.minBudget ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(job.minBudget) : ''}
                            {job.minBudget && job.maxBudget ? ' - ' : ''}
                            {job.maxBudget ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(job.maxBudget) : (job.minBudget ? '' : 'Thỏa thuận')}
                            {!job.maxBudget && job.minBudget ? ' (Min)' : ''}
                        </span>
                    </div>
                ) : (
                    <div className="flex items-center gap-1.5 bg-gray-50 text-gray-600 px-3 py-1.5 rounded-xl border border-gray-100">
                        <DollarSign className="w-4 h-4" />
                        <span className="text-sm font-bold">Thỏa thuận</span>
                    </div>
                )}
                {job.urgencyLevel && job.urgencyLevel !== 'NORMAL' && (
                    <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl border ${
                        job.urgencyLevel === 'CRITICAL' ? 'bg-red-50 text-red-700 border-red-100' : 'bg-orange-50 text-orange-700 border-orange-100'
                    }`}>
                        <AlertCircle className="w-4 h-4" />
                        <span className="text-sm font-bold uppercase">Ưu tiên: {job.urgencyLevel === 'URGENT' ? 'Gấp' : 'Rất gấp'}</span>
                    </div>
                )}
            </div>

            <div className="space-y-3">
                <div className="flex items-start gap-3 text-gray-600">
                    <MapPin className="w-5 h-5 text-orange-500 flex-shrink-0 mt-1" />
                    <div className="flex-1">
                        <span className="text-sm font-medium block">{job.address}</span>
                        {job.navigationUrl && (
                          <button 
                            onClick={() => window.open(job.navigationUrl, '_blank')}
                            className="mt-2 flex items-center gap-1.5 text-xs font-bold text-blue-600 bg-blue-50 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition-all w-fit"
                          >
                            <MapPin className="w-3.5 h-3.5" /> Dẫn đường tới đây
                          </button>
                        )}
                    </div>
                </div>
            </div>
        </div>

        {/* Description */}
        <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
            <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
                <FileText className="w-5 h-5 text-orange-500" />
                <h3>Mô tả tình trạng</h3>
            </div>
            <p className="text-gray-600 text-sm leading-relaxed whitespace-pre-wrap">
                {job.description}
            </p>
        </div>

        {/* Attachments */}
        {job.fileUrls && job.fileUrls.length > 0 && (
            <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
                <div className="flex items-center gap-2 mb-4 text-gray-900 font-bold">
                    <Wrench className="w-5 h-5 text-orange-500" />
                    <h3>Hình ảnh hiện trường</h3>
                </div>
                <div className="grid grid-cols-2 gap-3">
                    {job.fileUrls.map((url, i) => (
                        <img 
                            key={i} 
                            src={`${API_BASE_URL}${url}`} 
                            alt="attachment" 
                            className="w-full aspect-video object-cover rounded-xl"
                        />
                    ))}
                </div>
            </div>
        )}

        {/* Safety Tip */}
        <div className="bg-blue-50 p-4 rounded-2xl flex gap-3 text-blue-800">
            <Shield className="w-6 h-6 text-blue-500 flex-shrink-0" />
            <p className="text-xs font-medium leading-relaxed">
                Vui lòng phân tích kỹ tình trạng dựa trên mô tả và hình ảnh để đưa ra báo giá chính xác nhất. Giá có thể điều chỉnh sau khi khảo sát thực tế (nếu cần).
            </p>
        </div>
      </div>

      {/* Floating Action Button */}
      <div className="fixed bottom-0 left-0 right-0 p-4 bg-gradient-to-t from-white via-white to-transparent">
        <button
          onClick={() => setOfferModal(true)}
          className="w-full max-w-md mx-auto block py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-xl shadow-gray-900/20 transition-all flex items-center justify-center gap-2"
        >
          <Send className="w-5 h-5" /> Gửi báo giá ngay
        </button>
      </div>

      {/* Offer Modal */}
      <Modal
        open={offerModal}
        onCancel={() => setOfferModal(false)}
        footer={null}
        closable={false}
        centered
        bodyStyle={{ padding: 0 }}
        width={400}
      >
        <div className="p-6 space-y-6">
            <div className="text-center">
                <h3 className="text-xl font-bold text-gray-900">Gửi báo giá thợ</h3>
                <p className="text-gray-500 text-sm">Điền thông tin để khách hàng tin tưởng chọn bạn</p>
            </div>

            <div className="space-y-4">
                <div>
                    <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Giá dự kiến (VNĐ)</label>
                    <div className="relative">
                        <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                        <input 
                            type="number" 
                            value={price}
                            onChange={(e) => setPrice(e.target.value)}
                            placeholder="Ví dụ: 200000"
                            className="w-full bg-gray-50 rounded-xl border-none py-4 pl-12 pr-4 text-lg font-bold text-gray-900 focus:ring-2 focus:ring-orange-500/50"
                        />
                    </div>
                </div>

                <div>
                    <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Phân tích của bạn</label>
                    <textarea 
                        value={analysis}
                        onChange={(e) => setAnalysis(e.target.value)}
                        placeholder="Ví dụ: Dựa trên ảnh, tôi đoán bồn cầu bị hỏng phao, cần thay mới..."
                        rows={4}
                        className="w-full bg-gray-50 rounded-xl border-none px-4 py-3 text-sm text-gray-900 focus:ring-2 focus:ring-orange-500/50 resize-none"
                    />
                </div>

                <div>
                    <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Thời gian có mặt (phút)</label>
                    <select 
                        value={eta}
                        onChange={(e) => setEta(e.target.value)}
                        className="w-full bg-gray-50 rounded-xl border-none px-4 py-4 text-sm font-bold text-gray-900 focus:ring-2 focus:ring-orange-500/50"
                    >
                        <option value="15">15 phút</option>
                        <option value="30">30 phút</option>
                        <option value="45">45 phút</option>
                        <option value="60">1 tiếng</option>
                    </select>
                </div>

                <div className="flex gap-3">
                    <div className="flex-1">
                        <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Sửa trong (phút)</label>
                        <input 
                            type="number" 
                            value={repairTime}
                            onChange={(e) => setRepairTime(e.target.value)}
                            placeholder="60"
                            className="w-full bg-gray-50 rounded-xl border-none py-3 px-4 text-sm font-bold text-gray-900 focus:ring-2 focus:ring-orange-500/50"
                        />
                    </div>
                    <div className="flex-1">
                        <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Bảo hành (ngày)</label>
                        <input 
                            type="number" 
                            value={warranty}
                            onChange={(e) => setWarranty(e.target.value)}
                            placeholder="30"
                            className="w-full bg-gray-50 rounded-xl border-none py-3 px-4 text-sm font-bold text-gray-900 focus:ring-2 focus:ring-orange-500/50"
                        />
                    </div>
                </div>
            </div>

            <div className="flex gap-3">
                <button 
                    onClick={() => setOfferModal(false)}
                    className="flex-1 py-4 bg-gray-100 text-gray-700 font-bold rounded-2xl hover:bg-gray-200"
                >
                    Hủy
                </button>
                <button 
                    onClick={handleSubmitOffer}
                    disabled={submitting}
                    className="flex-3 py-4 bg-orange-500 text-white font-bold rounded-2xl hover:bg-orange-600 disabled:opacity-50 flex items-center justify-center gap-2"
                >
                    {submitting && <Loader2 className="w-5 h-5 animate-spin" />}
                    Xác nhận gửi
                </button>
            </div>
        </div>
      </Modal>
    </div>
  );
}
