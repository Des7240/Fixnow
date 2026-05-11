import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Clock, MapPin, ChevronRight, MessageSquare, AlertCircle, Plus } from 'lucide-react';
import { Tag, Empty, Button, Skeleton } from 'antd';
import { useOpenJobStore } from '../../stores/openJobStore';
import moment from 'moment';

const getStatusColor = (status: string) => {
  switch (status) {
    case 'OPEN': return 'green';
    case 'RECEIVING_OFFERS': return 'orange';
    case 'WORKER_SELECTED': return 'blue';
    case 'BOOKING_CREATED': return 'cyan';
    case 'EXPIRED': return 'red';
    case 'CLOSED': return 'default';
    default: return 'default';
  }
};

const getStatusLabel = (status: string) => {
  switch (status) {
    case 'OPEN': return 'Đang mở';
    case 'RECEIVING_OFFERS': return 'Đang nhận báo giá';
    case 'WORKER_SELECTED': return 'Đã chọn thợ';
    case 'BOOKING_CREATED': return 'Đã tạo đơn';
    case 'EXPIRED': return 'Hết hạn';
    case 'CLOSED': return 'Đã đóng';
    default: return status;
  }
};

export default function MyOpenJobs() {
  const navigate = useNavigate();
  const { myJobs, loading, fetchMyJobs } = useOpenJobStore();

  useEffect(() => {
    fetchMyJobs();
  }, [fetchMyJobs]);

  return (
    <div className="h-screen bg-gray-50 flex flex-col">
      <div className="bg-white px-4 py-4 flex items-center justify-between shadow-sm z-10">
        <div className="flex items-center gap-4">
          <button onClick={() => navigate('/customer/home')} className="p-2 -ml-2 rounded-full hover:bg-gray-100">
            <ArrowLeft className="w-6 h-6 text-gray-700" />
          </button>
          <h1 className="text-lg font-bold text-gray-900">Việc đã đăng</h1>
        </div>
        <button 
          onClick={() => navigate('/customer/create-open-job')}
          className="p-2 bg-orange-500 rounded-xl text-white shadow-lg shadow-orange-500/20"
        >
          <Plus className="w-5 h-5" />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {loading ? (
          Array(3).fill(0).map((_, i) => (
            <div key={i} className="bg-white p-4 rounded-3xl shadow-sm space-y-3">
              <Skeleton active paragraph={{ rows: 2 }} />
            </div>
          ))
        ) : myJobs.length === 0 ? (
          <div className="h-full flex flex-col items-center justify-center text-center p-8">
            <Empty description="Bạn chưa đăng tin nào" />
            <Button 
                type="primary" 
                size="large" 
                onClick={() => navigate('/customer/create-open-job')}
                className="mt-4 bg-orange-500 border-none rounded-xl h-12 px-8 font-bold"
            >
                Đăng tin ngay
            </Button>
          </div>
        ) : (
          myJobs.map((job) => (
            <div 
              key={job.id} 
              onClick={() => navigate(`/customer/open-jobs/${job.id}/offers`)}
              className="bg-white p-5 rounded-3xl shadow-sm border border-gray-100 active:scale-95 transition-transform"
            >
              <div className="flex justify-between items-start mb-3">
                <Tag color={getStatusColor(job.status)} className="rounded-full px-3 font-bold">
                  {getStatusLabel(job.status)}
                </Tag>
                <span className="text-[10px] text-gray-400 font-bold uppercase tracking-wider">
                  {moment(job.createdAt).fromNow()}
                </span>
              </div>

              <h3 className="text-base font-bold text-gray-900 mb-1 line-clamp-1">{job.title}</h3>
              <div className="flex items-center gap-1 text-gray-500 text-xs mb-3">
                <MapPin className="w-3 h-3" />
                <span className="line-clamp-1">{job.address}</span>
              </div>

              <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-50">
                <div className="flex items-center gap-4">
                  <div className="flex flex-col">
                    <span className="text-[10px] text-gray-400 font-bold uppercase">Báo giá</span>
                    <span className="text-sm font-bold text-orange-600">{job.offerCount} thợ</span>
                  </div>
                  <div className="flex flex-col">
                    <span className="text-[10px] text-gray-400 font-bold uppercase">Ngân sách</span>
                    <span className="text-sm font-bold text-gray-700">
                      {job.minBudget ? `${job.minBudget.toLocaleString()}đ` : 'Thỏa thuận'}
                    </span>
                  </div>
                </div>
                <ChevronRight className="w-5 h-5 text-gray-300" />
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
