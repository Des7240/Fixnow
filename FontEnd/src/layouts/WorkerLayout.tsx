import { useEffect, useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Briefcase, ListOrdered, FileBadge, User, Bell, Wallet } from 'lucide-react';
import { clsx } from 'clsx';
import { useNotificationStore } from '../stores/notificationStore';
import { useSignalR } from '../signalr/SignalRContext';
import { message } from 'antd';
import axiosInstance from '../utils/axiosInstance';

export default function WorkerLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { unreadCount, fetchUnreadCount } = useNotificationStore();
  const { connection } = useSignalR();
  const [newJob, setNewJob] = useState<any>(null);

  useEffect(() => {
    fetchUnreadCount();
  }, [fetchUnreadCount]);

  useEffect(() => {
    // Fetch existing matching jobs when layout mounts
    const fetchMatchingJobs = async () => {
      try {
        const res = await axiosInstance.get('/bookings/matching');
        if (res.data && res.data.length > 0) {
          setNewJob(res.data[0]);
        }
      } catch (err) {
        console.error('Lỗi lấy danh sách đơn matching:', err);
      }
    };

    fetchMatchingJobs();
  }, []);

  useEffect(() => {
    if (connection) {
      connection.on('ReceiveBookingMatch', (data) => {
        message.info('Có đơn hàng mới phù hợp với bạn!');
        setNewJob(data);
      });

      return () => {
        connection.off('ReceiveBookingMatch');
      };
    }
  }, [connection]);

  const acceptJob = () => {
    if (newJob) {
      navigate(`/worker/bookings/${newJob.id || newJob.bookingId}`);
      setNewJob(null);
    }
  };

  const navItems = [
    { path: '/worker', icon: Briefcase, label: 'Trang chủ' },
    { path: '/worker/bookings', icon: ListOrdered, label: 'Đơn hàng' },
    { path: '/worker/wallet', icon: Wallet, label: 'Ví tiền' },
    { path: '/worker/notifications', icon: Bell, label: 'Thông báo', badge: unreadCount },
    { path: '/worker/profile', icon: User, label: 'Hồ sơ' },
  ];

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Main Content Area */}
      <div className="flex-1 overflow-y-auto overflow-x-hidden">
        <Outlet />
      </div>

      {/* Bottom Navigation */}
      <div className="bg-white border-t border-gray-100 px-6 py-3 pb-safe shadow-[0_-5px_20px_rgba(0,0,0,0.03)] z-50">
        <div className="flex justify-between items-center max-w-md mx-auto">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.path || 
              (item.path !== '/worker' && location.pathname.startsWith(item.path));

            return (
              <button
                key={item.path}
                onClick={() => navigate(item.path)}
                className={clsx(
                  'flex flex-col items-center gap-1 transition-colors duration-200',
                  isActive ? 'text-orange-500' : 'text-gray-400 hover:text-gray-600'
                )}
              >
                <div className={clsx(
                  'p-1.5 rounded-xl transition-all duration-300 relative',
                  isActive ? 'bg-orange-50' : 'bg-transparent'
                )}>
                  <Icon className={clsx('w-6 h-6', isActive ? 'stroke-[2.5px]' : 'stroke-2')} />
                  {item.badge && item.badge > 0 && (
                    <span className="absolute top-1 right-1 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center ring-2 ring-white">
                      {item.badge > 9 ? '9+' : item.badge}
                    </span>
                  )}
                </div>
                <span className={clsx(
                  'text-[10px] font-semibold',
                  isActive ? 'text-orange-500' : 'text-gray-500'
                )}>
                  {item.label}
                </span>
              </button>
            );
          })}
        </div>
      </div>

      {/* New Job Modal Overlay (Global for Worker) */}
      {newJob && (
        <div className="fixed inset-0 bg-black/60 z-[9999] flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl p-6 max-w-sm w-full shadow-2xl animate-in fade-in zoom-in duration-300">
            <div className="w-16 h-16 bg-green-100 text-green-600 rounded-full flex items-center justify-center mx-auto mb-4">
              <Briefcase className="w-8 h-8 animate-bounce" />
            </div>
            <h3 className="text-xl font-bold text-gray-900 mb-2">Đơn mới vừa đến!</h3>
            <p className="text-gray-600 text-sm mb-6">Có khách hàng ở gần bạn đang cần sửa chữa ngay.</p>
            
            <div className="flex gap-3">
              <button 
                onClick={() => setNewJob(null)}
                className="flex-1 py-3 bg-gray-100 text-gray-700 font-bold rounded-xl"
              >
                Bỏ qua
              </button>
              <button 
                onClick={acceptJob}
                className="flex-1 py-3 bg-orange-500 text-white font-bold rounded-xl shadow-lg shadow-orange-500/30"
              >
                Xem chi tiết
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
