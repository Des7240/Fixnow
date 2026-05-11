import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, MapPin, Clock, Search, Filter, Wrench, ChevronRight, Loader2, Bookmark, BookmarkCheck, DollarSign, AlertCircle } from 'lucide-react';
import { message, Badge } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { useMarketplaceStore } from '../../stores/marketplaceStore';
import MarketplaceFilters from './MarketplaceFilters';

export default function NearbyJobs() {
  const navigate = useNavigate();
  const { jobs, loading, filters, setFilters, fetchJobs, saveJob, unsaveJob } = useMarketplaceStore();
  
  const [locating, setLocating] = useState(false);
  const [filterModal, setFilterModal] = useState(false);
  const [services, setServices] = useState([]);
  const [userLocation, setUserLocation] = useState<{lat: number, lng: number} | null>(null);

  useEffect(() => {
    fetchServices();
    handleRefresh();
  }, []);

  const fetchServices = async () => {
    try {
      const res = await axiosInstance.get('/services');
      setServices(res.data);
    } catch (err) {
      console.error('Failed to fetch services', err);
    }
  };

  const handleRefresh = () => {
    setLocating(true);
    if (!navigator.geolocation) {
      message.error('Trình duyệt không hỗ trợ lấy vị trí.');
      const fallback = { lat: 21.0285, lng: 105.8048 };
      setUserLocation(fallback);
      fetchJobs(fallback.lat, fallback.lng);
      setLocating(false);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        const loc = { lat: position.coords.latitude, lng: position.coords.longitude };
        setUserLocation(loc);
        fetchJobs(loc.lat, loc.lng);
        setLocating(false);
      },
      (error) => {
        console.error(error);
        message.error('Không thể lấy vị trí. Dùng vị trí mặc định.');
        const fallback = { lat: 21.0285, lng: 105.8048 };
        setUserLocation(fallback);
        fetchJobs(fallback.lat, fallback.lng);
        setLocating(false);
      }
    );
  };

  const onApplyFilters = () => {
    if (userLocation) {
      fetchJobs(userLocation.lat, userLocation.lng);
    }
  };

  const toggleSave = (e: React.MouseEvent, jobId: string, isSaved: boolean) => {
    e.stopPropagation();
    if (isSaved) {
      unsaveJob(jobId);
      message.success('Đã bỏ lưu công việc');
    } else {
      saveJob(jobId);
      message.success('Đã lưu công việc');
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

  const getUrgencyTag = (level?: string) => {
    switch (level) {
      case 'URGENT':
        return (
          <div className="absolute top-0 right-0">
            <div className="bg-orange-500 text-white text-[8px] font-black px-4 py-1 rotate-45 translate-x-3 -translate-y-1 shadow-sm uppercase">
              Gấp
            </div>
          </div>
        );
      case 'CRITICAL':
        return (
          <div className="absolute top-0 right-0">
            <div className="bg-red-600 text-white text-[8px] font-black px-4 py-1 rotate-45 translate-x-3 -translate-y-1 shadow-sm uppercase">
              Rất Gấp
            </div>
          </div>
        );
      default:
        return null;
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
            <h1 className="text-lg font-bold text-gray-900">Chợ việc FixNow</h1>
        </div>
        <div className="flex items-center gap-2">
            <button 
                onClick={() => navigate('/worker/saved-jobs')}
                className="p-2 text-gray-600 hover:bg-gray-100 rounded-full"
            >
                <Bookmark className="w-5 h-5" />
            </button>
            <button 
                onClick={handleRefresh}
                disabled={locating}
                className="p-2 text-orange-600 font-semibold text-sm"
            >
                {locating ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Làm mới'}
            </button>
        </div>
      </div>

      <div className="flex-1 p-4 space-y-4 pb-20">
        {/* Search Bar & Filter Toggle */}
        <div className="flex gap-2">
            <div className="relative flex-1">
                <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input 
                    type="text" 
                    placeholder="Tìm theo tên, mô tả..."
                    className="w-full bg-white border-none rounded-2xl py-4 pl-12 pr-4 shadow-sm focus:ring-2 focus:ring-orange-500/50"
                />
            </div>
            <button 
                onClick={() => setFilterModal(true)}
                className={`p-4 rounded-2xl shadow-sm transition-all ${
                    (filters.serviceIds.length > 0 || filters.minBudget || filters.urgencyLevel) 
                    ? 'bg-orange-500 text-white' 
                    : 'bg-white text-gray-600'
                }`}
            >
                <Filter className="w-6 h-6" />
            </button>
        </div>

        {/* Quick Radius Filter Scroll */}
        <div className="flex gap-2 overflow-x-auto pb-2 -mx-4 px-4 no-scrollbar">
            {[2, 5, 10, 20, 50].map((r) => (
                <button
                    key={r}
                    onClick={() => { setFilters({ radius: r }); onApplyFilters(); }}
                    className={`whitespace-nowrap px-4 py-2 rounded-full text-xs font-bold transition-all ${
                        filters.radius === r 
                        ? 'bg-orange-500 text-white shadow-md' 
                        : 'bg-white text-gray-500 border border-gray-100'
                    }`}
                >
                    {r}km
                </button>
            ))}
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
                    <h3 className="text-gray-900 font-bold">Chưa tìm thấy việc phù hợp</h3>
                    <p className="text-gray-500 text-sm max-w-[200px] mx-auto">Hãy thử mở rộng bán kính hoặc đổi bộ lọc nhé!</p>
                </div>
                <button 
                    onClick={() => { setFilters({ radius: 20, serviceIds: [], urgencyLevel: undefined }); onApplyFilters(); }}
                    className="text-orange-600 font-bold text-sm"
                >
                    Xem tất cả việc trong 20km
                </button>
            </div>
        ) : (
            <div className="space-y-4">
                {jobs.map((job) => (
                    <div 
                        key={job.id}
                        onClick={() => navigate(`/worker/open-jobs/${job.id}`)}
                        className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100 hover:border-orange-200 transition-all cursor-pointer group relative overflow-hidden"
                    >
                        {/* Urgency Badge */}
                        {getUrgencyTag(job.urgencyLevel)}

                        <div className="flex justify-between items-start mb-3">
                            <div className="flex items-center gap-2">
                                <span className="bg-gray-100 text-gray-600 text-[10px] font-bold px-2 py-1 rounded-full uppercase tracking-wider">
                                    {job.serviceName}
                                </span>
                                {job.distanceKm !== undefined && (
                                    <span className="text-orange-600 text-[10px] font-bold flex items-center gap-0.5">
                                        <MapPin className="w-3 h-3" /> {job.distanceKm.toFixed(1)}km
                                    </span>
                                )}
                            </div>
                            <button 
                                onClick={(e) => toggleSave(e, job.id, !!job.isSaved)}
                                className={`p-2 rounded-full transition-all ${job.isSaved ? 'text-orange-500 bg-orange-50' : 'text-gray-300 hover:bg-gray-100'}`}
                            >
                                {job.isSaved ? <BookmarkCheck className="w-5 h-5" /> : <Bookmark className="w-5 h-5" />}
                            </button>
                        </div>

                        <h3 className="text-gray-900 font-bold text-lg mb-2 group-hover:text-orange-600 transition-colors leading-tight">{job.title}</h3>
                        <p className="text-gray-500 text-sm line-clamp-2 mb-4 leading-relaxed">{job.description}</p>
                        
                        {/* Price Tag */}
                        <div className="flex items-center gap-2 mb-4">
                            <div className="flex items-center gap-1 bg-green-50 text-green-700 px-3 py-1.5 rounded-xl border border-green-100">
                                <DollarSign className="w-4 h-4" />
                                <span className="text-sm font-bold">
                                    {getBudgetDisplay(job.minBudget, job.maxBudget)}
                                </span>
                            </div>
                            {job.urgencyLevel && job.urgencyLevel !== 'NORMAL' && (
                                <div className={`flex items-center gap-1 px-2 py-1 rounded-lg text-[10px] font-bold uppercase ${
                                    job.urgencyLevel === 'CRITICAL' ? 'bg-red-100 text-red-600' : 'bg-orange-100 text-orange-600'
                                }`}>
                                    <AlertCircle className="w-3 h-3" />
                                    {job.urgencyLevel === 'CRITICAL' ? 'Ưu tiên cao' : 'Cần gấp'}
                                </div>
                            )}
                        </div>

                        <div className="flex items-center justify-between pt-4 border-t border-gray-50">
                            <div className="flex items-center gap-1.5 text-gray-500 max-w-[60%]">
                                <MapPin className="w-4 h-4 text-gray-400 flex-shrink-0" />
                                <span className="text-xs font-medium truncate">{job.address}</span>
                            </div>
                            <div className="flex items-center gap-4">
                                <div className="text-right">
                                    <p className="text-[10px] text-gray-400 font-bold uppercase tracking-tighter">Báo giá</p>
                                    <p className="text-sm font-black text-gray-900">{job.offerCount}</p>
                                </div>
                                <div className="p-2 bg-gray-50 rounded-full group-hover:bg-orange-500 group-hover:text-white transition-all shadow-sm">
                                    <ChevronRight className="w-5 h-5" />
                                </div>
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        )}
      </div>

      {/* Filter Modal */}
      <MarketplaceFilters 
        open={filterModal}
        onClose={() => setFilterModal(false)}
        services={services}
        filters={filters}
        setFilters={setFilters}
        onApply={onApplyFilters}
      />
    </div>
  );
}
