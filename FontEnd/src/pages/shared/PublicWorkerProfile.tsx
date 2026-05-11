import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { User, Briefcase, Wrench, Star, MessageSquare, ArrowLeft, Loader2, Clock, CheckCircle } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { clsx } from 'clsx';

interface Review {
  id: string;
  customerName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

interface WorkerProfile {
    userId: string;
    fullName: string;
    email: string;
    bio?: string;
    experienceYears: number;
    averageRating: number;
    totalJobs: number;
    skills: {
        serviceId: string;
        serviceName: string;
        status: string;
    }[];
}

export default function PublicWorkerProfile() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState<WorkerProfile | null>(null);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [ratingSummary, setRatingSummary] = useState({ averageRating: 0, totalReviews: 0 });

  useEffect(() => {
    if (id) {
        fetchData();
    }
  }, [id]);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [profileRes, summaryRes, reviewsRes] = await Promise.all([
        axiosInstance.get(`/workers/${id}/profile`),
        axiosInstance.get(`/reviews/workers/${id}/summary`),
        axiosInstance.get(`/reviews/workers/${id}`)
      ]);
      setProfile(profileRes.data);
      setRatingSummary(summaryRes.data);
      setReviews(reviewsRes.data);
    } catch (err: any) {
      console.error(err);
      message.error('Không thể tải thông tin thợ');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return (
    <div className="h-screen flex items-center justify-center bg-white">
        <Loader2 className="w-10 h-10 text-orange-500 animate-spin" />
    </div>
  );

  if (!profile) return (
    <div className="p-10 text-center">
        <p>Không tìm thấy hồ sơ thợ</p>
        <button onClick={() => navigate(-1)} className="mt-4 text-orange-500 font-bold">Quay lại</button>
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col pb-10">
      {/* Header */}
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm z-20 sticky top-0 flex items-center gap-4">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 hover:bg-gray-100 rounded-full">
          <ArrowLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-xl font-bold text-gray-900">Hồ sơ thợ</h1>
      </div>

      <div className="p-4 space-y-6 max-w-lg mx-auto w-full">
        {/* Profile Card */}
        <div className="bg-white p-8 rounded-[2.5rem] shadow-sm border border-gray-100 flex flex-col items-center">
          <div className="w-24 h-24 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 font-bold text-4xl mb-4">
            {profile.fullName.charAt(0)}
          </div>
          <h2 className="text-2xl font-black text-gray-900">{profile.fullName}</h2>
          <div className="flex items-center gap-2 mt-1">
            <span className="px-3 py-1 bg-orange-50 text-orange-600 text-[10px] font-bold rounded-full uppercase">Thợ chuyên nghiệp</span>
          </div>
          
          <div className="flex items-center gap-10 w-full mt-8 pt-6 border-t border-gray-50">
            <div className="flex-1 text-center">
              <div className="flex items-center justify-center gap-1 text-orange-500 font-black text-xl">
                <Star className="w-5 h-5 fill-current" />
                {ratingSummary.averageRating.toFixed(1)}
              </div>
              <p className="text-[10px] text-gray-400 font-bold uppercase mt-1 tracking-wider">Đánh giá</p>
            </div>
            <div className="w-px h-10 bg-gray-100"></div>
            <div className="flex-1 text-center">
              <div className="text-gray-900 font-black text-xl">
                {ratingSummary.totalReviews}
              </div>
              <p className="text-[10px] text-gray-400 font-bold uppercase mt-1 tracking-wider">Việc đã làm</p>
            </div>
          </div>
        </div>

        {/* Bio & Skills */}
        <div className="space-y-4">
          <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
            <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4">
              <Briefcase className="w-5 h-5 text-orange-500" /> Giới thiệu
            </h3>
            <div className="space-y-4">
              <div className="flex items-center gap-3 bg-gray-50 p-3 rounded-2xl">
                <Clock className="w-5 h-5 text-gray-400" />
                <div>
                    <p className="text-[10px] font-bold text-gray-400 uppercase">Kinh nghiệm</p>
                    <p className="text-sm font-bold text-gray-900">{profile.experienceYears} năm làm nghề</p>
                </div>
              </div>
              <p className="text-sm text-gray-600 leading-relaxed italic">
                "{profile.bio || 'Chưa có mô tả giới thiệu'}"
              </p>
            </div>
          </div>

          <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
            <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4">
              <Wrench className="w-5 h-5 text-orange-500" /> Kỹ năng chuyên môn
            </h3>
            <div className="flex flex-wrap gap-2">
              {profile.skills.filter(s => s.status === 'APPROVED').map(skill => (
                <div key={skill.serviceId} className="px-4 py-2 bg-orange-50 text-orange-700 rounded-xl text-xs font-bold border border-orange-100 flex items-center gap-2">
                  <CheckCircle className="w-3.5 h-3.5" />
                  {skill.serviceName}
                </div>
              ))}
              {profile.skills.filter(s => s.status === 'APPROVED').length === 0 && (
                <p className="text-gray-400 text-sm italic">Chưa có kỹ năng nào được duyệt</p>
              )}
            </div>
          </div>
        </div>

        {/* Reviews */}
        <div>
          <h3 className="flex items-center gap-2 font-bold text-gray-900 mb-4 px-2">
            <MessageSquare className="w-5 h-5 text-orange-500" /> Đánh giá từ khách hàng
          </h3>
          
          {reviews.length === 0 ? (
            <div className="bg-white p-10 rounded-[2.5rem] border border-dashed border-gray-200 text-center text-gray-400">
              <Star className="w-10 h-10 mx-auto mb-2 opacity-20" />
              <p className="text-sm">Chưa có đánh giá nào</p>
            </div>
          ) : (
            <div className="space-y-4">
              {reviews.map(review => (
                <div key={review.id} className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
                  <div className="flex justify-between items-start mb-3">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-gray-50 rounded-2xl flex items-center justify-center text-gray-400 font-bold text-sm">
                        {review.customerName.charAt(0)}
                      </div>
                      <div>
                        <p className="text-sm font-bold text-gray-900">{review.customerName}</p>
                        <p className="text-[10px] text-gray-400 font-medium">{new Date(review.createdAt).toLocaleDateString('vi-VN')}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-0.5 text-orange-500">
                      {[...Array(5)].map((_, i) => (
                        <Star key={i} className={clsx("w-3.5 h-3.5", i < review.rating ? "fill-current" : "text-gray-200")} />
                      ))}
                    </div>
                  </div>
                  <div className="bg-gray-50 p-4 rounded-2xl">
                    <p className="text-sm text-gray-600 italic">"{review.comment}"</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
