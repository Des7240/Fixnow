import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, MapPin, Clock, Search, Filter, Wrench, ChevronRight, Loader2 } from 'lucide-react';
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
}

export default function NearbyJobs() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [jobs, setJobs] = useState<OpenJob[]>([]);
  const [locating, setLocating] = useState(false);

  useEffect(() => {
    handleRefresh();
  }, []);

  const handleRefresh = () => {
    setLocating(true);
    if (!navigator.geolocation) {
      message.error('Trình duyệt không hỗ trợ lấy vị trí.');
      fetchJobs(21.0285, 105.8048); // Fallback
      setLocating(false);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        fetchJobs(position.coords.latitude, position.coords.longitude);
        setLocating(false);
      },
      (error) => {
        console.error(error);
        message.error('Không thể lấy vị trí. Dùng vị trí mặc định.');
        fetchJobs(21.0285, 105.8048);
        setLocating(false);
      }
    );
  };

  const fetchJobs = async (lat: number, lng: number) => {
    setLoading(true);
    try {
      const res = await axiosInstance.get(`/open-jobs/nearby?lat=${lat}&lng=${lng}`);
      setJobs(res.data);
    } catch (err) {
      message.error('Không thể tải danh sách công việc');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white px-4 py-4 flex items-center justify-between shadow-sm sticky top-0 z-20">
        <div className="flex items-center gap-4">
            <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
                <ArrowLeft className="w-6 h-6 text-gray-700" />
            </button>
            <h1 className="text-lg font-bold text-gray-900">Việc mới quanh đây</h1>
        </div>
        <button 
            onClick={handleRefresh}
            disabled={locating}
            className="p-2 text-orange-600 font-semibold text-sm"
        >
            {locating ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Làm mới'}
        </button>
      </div>

      <div className="flex-1 p-4 space-y-4">
        {/* Search Bar Placeholder */}
        <div className="relative">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input 
            type="text" 
            placeholder="Tìm kiếm công việc..."
            className="w-full bg-white border-none rounded-2xl py-4 pl-12 pr-4 shadow-sm focus:ring-2 focus:ring-orange-500/50"
          />
          <button className="absolute right-4 top-1/2 -translate-y-1/2 p-2 bg-gray-50 rounded-xl">
            <Filter className="w-4 h-4 text-gray-600" />
          </button>
        </div>

        {loading ? (
            <div className="flex flex-col items-center justify-center py-20 gap-4">
                <Loader2 className="w-10 h-10 text-orange-500 animate-spin" />
                <p className="text-gray-500 font-medium">Đang quét tìm việc mới...</p>
            </div>
        ) : jobs.length === 0 ? (
            <div className="text-center py-20 space-y-4 bg-white rounded-3xl border border-dashed border-gray-200">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto">
                    <Wrench className="w-10 h-10 text-gray-300" />
                </div>
                <div>
                    <h3 className="text-gray-900 font-bold">Chưa có việc nào gần đây</h3>
                    <p className="text-gray-500 text-sm max-w-[200px] mx-auto">Hãy thử thay đổi vị trí hoặc chờ chút nhé!</p>
                </div>
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
                            <span className="text-xs text-gray-400 flex items-center gap-1">
                                <Clock className="w-3 h-3" /> {new Date(job.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                            </span>
                        </div>
                        <h3 className="text-gray-900 font-bold text-lg mb-2 group-hover:text-orange-600 transition-colors">{job.title}</h3>
                        <p className="text-gray-500 text-sm line-clamp-2 mb-4 leading-relaxed">{job.description}</p>
                        
                        <div className="flex items-center justify-between pt-4 border-t border-gray-50">
                            <div className="flex items-center gap-1.5 text-gray-600">
                                <MapPin className="w-4 h-4 text-orange-500" />
                                <span className="text-xs font-medium truncate max-w-[150px]">{job.address}</span>
                            </div>
                            <div className="flex items-center gap-3">
                                <div className="text-right">
                                    <p className="text-[10px] text-gray-400 font-bold uppercase">Báo giá</p>
                                    <p className="text-sm font-bold text-gray-900">{job.offerCount}</p>
                                </div>
                                <div className="p-2 bg-gray-50 rounded-full group-hover:bg-orange-500 group-hover:text-white transition-all">
                                    <ChevronRight className="w-5 h-5" />
                                </div>
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
