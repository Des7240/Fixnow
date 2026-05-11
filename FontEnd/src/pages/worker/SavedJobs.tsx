import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, MapPin, Wrench, ChevronRight, Loader2, BookmarkCheck, DollarSign } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';

interface OpenJob {
  id: string;
  title: string;
  description: string;
  address: string;
  serviceName: string;
  createdAt: string;
  offerCount: number;
  minBudget?: number;
  maxBudget?: number;
}

export default function SavedJobs() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [jobs, setJobs] = useState<OpenJob[]>([]);

  useEffect(() => {
    fetchSavedJobs();
  }, []);

  const fetchSavedJobs = async () => {
    setLoading(true);
    try {
      const res = await axiosInstance.get('/open-jobs/saved');
      setJobs(res.data);
    } catch (err) {
      message.error('Không thể tải danh sách công việc đã lưu');
    } finally {
      setLoading(false);
    }
  };

  const unsaveJob = async (e: React.MouseEvent, jobId: string) => {
    e.stopPropagation();
    try {
      await axiosInstance.delete(`/open-jobs/${jobId}/save`);
      setJobs(jobs.filter(j => j.id !== jobId));
      message.success('Đã bỏ lưu công việc');
    } catch (err) {
      message.error('Lỗi khi bỏ lưu');
    }
  };

  const formatCurrency = (val?: number) => {
    if (val === undefined || val === null || val === 0) return null;
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
  };

  const getBudgetDisplay = (min?: number, max?: number) => {
    const minFmt = formatCurrency(min);
    const maxFmt = formatCurrency(max);

    if (minFmt && maxFmt) return `${minFmt} - ${maxFmt}`;
    if (minFmt) return `Từ ${minFmt}`;
    if (maxFmt) return `Đến ${maxFmt}`;
    return 'Thỏa thuận';
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <div className="bg-white px-4 py-4 flex items-center gap-4 shadow-sm sticky top-0 z-20">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
          <ArrowLeft className="w-6 h-6 text-gray-700" />
        </button>
        <h1 className="text-lg font-bold text-gray-900">Công việc đã lưu</h1>
      </div>

      <div className="flex-1 p-4">
        {loading ? (
            <div className="flex flex-col items-center justify-center py-20 gap-4">
                <Loader2 className="w-10 h-10 text-orange-500 animate-spin" />
                <p className="text-gray-500 font-medium">Đang tải...</p>
            </div>
        ) : jobs.length === 0 ? (
            <div className="text-center py-20 space-y-4 bg-white rounded-3xl border border-dashed border-gray-200">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto">
                    <BookmarkCheck className="w-10 h-10 text-gray-300" />
                </div>
                <div>
                    <h3 className="text-gray-900 font-bold">Chưa có việc nào được lưu</h3>
                    <p className="text-gray-500 text-sm max-w-[200px] mx-auto">Lưu lại những công việc bạn quan tâm để xem sau nhé!</p>
                </div>
                <button 
                    onClick={() => navigate('/worker/nearby-jobs')}
                    className="text-orange-600 font-bold text-sm"
                >
                    Khám phá chợ việc ngay
                </button>
            </div>
        ) : (
            <div className="space-y-4">
                {jobs.map((job) => (
                    <div 
                        key={job.id}
                        onClick={() => navigate(`/worker/open-jobs/${job.id}`)}
                        className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100 hover:border-orange-200 transition-all cursor-pointer group"
                    >
                        <div className="flex justify-between items-start mb-3">
                            <span className="bg-orange-100 text-orange-600 text-[10px] font-bold px-2 py-1 rounded-full uppercase tracking-wider">
                                {job.serviceName}
                            </span>
                            <button 
                                onClick={(e) => unsaveJob(e, job.id)}
                                className="p-2 rounded-full text-orange-500 bg-orange-50"
                            >
                                <BookmarkCheck className="w-5 h-5" />
                            </button>
                        </div>
                        <h3 className="text-gray-900 font-bold text-lg mb-2 group-hover:text-orange-600 transition-colors">{job.title}</h3>
                        <p className="text-gray-500 text-sm line-clamp-2 mb-4 leading-relaxed">{job.description}</p>
                        
                        <div className="flex items-center gap-2 mb-4">
                            <div className="flex items-center gap-1 bg-green-50 text-green-700 px-3 py-1.5 rounded-xl border border-green-100">
                                <DollarSign className="w-4 h-4" />
                                <span className="text-sm font-bold">
                                    {getBudgetDisplay(job.minBudget, job.maxBudget)}
                                </span>
                            </div>
                        </div>

                        <div className="flex items-center justify-between pt-4 border-t border-gray-50">
                            <div className="flex items-center gap-1.5 text-gray-600">
                                <MapPin className="w-4 h-4 text-orange-500" />
                                <span className="text-xs font-medium truncate max-w-[150px]">{job.address}</span>
                            </div>
                            <div className="p-2 bg-gray-50 rounded-full group-hover:bg-orange-500 group-hover:text-white transition-all">
                                <ChevronRight className="w-5 h-5" />
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
