import { useState, useEffect } from 'react';
import { Bell, Check, Clock, Info } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';
import { clsx } from 'clsx';
import { useNavigate } from 'react-router-dom';
import { useNotificationStore } from '../../stores/notificationStore';
import { useAuthStore } from '../../stores/authStore';

interface Notification {
  id: string;
  title: string;
  content: string;
  type: string;
  isRead: boolean;
  referenceId?: string;
  createdAt: string;
}

export default function NotificationsList() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { fetchUnreadCount } = useNotificationStore();
  const { user } = useAuthStore();

  useEffect(() => {
    fetchNotifications();
  }, []);

  const fetchNotifications = async () => {
    try {
      const res = await axiosInstance.get('/notifications');
      setNotifications(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const markAsRead = async (id: string) => {
    try {
      await axiosInstance.patch(`/notifications/${id}/read`);
      setNotifications(prev => 
        prev.map(n => n.id === id ? { ...n, isRead: true } : n)
      );
      fetchUnreadCount(); // Sync badge count
    } catch (err) {
      console.error(err);
    }
  };

  const markAllAsRead = async () => {
    try {
      await axiosInstance.patch('/notifications/read-all');
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      fetchUnreadCount(); // Sync badge count
    } catch (err) {
      console.error(err);
    }
  };

  const handleNotificationClick = (notification: Notification) => {
    if (!notification.isRead) {
      markAsRead(notification.id);
    }
    
    // Navigate based on type and referenceId
    if (notification.referenceId) {
      if (notification.type === 'CHAT_MESSAGE') {
        navigate(`/bookings/${notification.referenceId}/chat`);
        return;
      }

      if (notification.type === 'NEW_OPEN_JOB') {
        navigate(`/worker/open-jobs/${notification.referenceId}`);
        return;
      }

      if (notification.type === 'NEW_WORKER_OFFER') {
        navigate(`/customer/open-jobs/${notification.referenceId}/offers`);
        return;
      }

      if (notification.type === 'KYC_APPROVED' || notification.type === 'KYC_REJECTED') {
        navigate(`/worker/kyc`);
        return;
      }

      if (notification.type === 'SKILL_APPROVED' || notification.type === 'SKILL_REJECTED') {
        navigate(`/worker/profile`);
        return;
      }

      if (notification.type === 'DISPUTE_CREATED' || notification.type === 'DISPUTE_RESOLVED') {
        const prefix = user?.role?.toUpperCase() === 'ADMIN' ? '/admin' : user?.role?.toUpperCase() === 'WORKER' ? '/worker/bookings' : '/customer/bookings';
        navigate(`${prefix}/${notification.referenceId}/dispute`);
        return;
      }

      const rolePrefix = user?.role?.toUpperCase() === 'WORKER' ? '/worker' : '/customer';
      navigate(`${rolePrefix}/bookings/${notification.referenceId}`);
    }
  };

  return (
    <div className="min-h-full bg-gray-50 flex flex-col">
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm z-10 sticky top-0 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Thông báo</h1>
        {notifications.some(n => !n.isRead) && (
          <button 
            onClick={markAllAsRead}
            className="text-sm font-semibold text-orange-500 hover:text-orange-600 flex items-center gap-1"
          >
            <Check className="w-4 h-4" /> Đọc tất cả
          </button>
        )}
      </div>

      <div className="flex-1 p-4 pb-24 overflow-y-auto">
        {loading ? (
          <div className="flex justify-center mt-10">
            <span className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></span>
          </div>
        ) : notifications.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-gray-400 mt-20">
            <Bell className="w-16 h-16 mb-4 opacity-20" />
            <p>Bạn không có thông báo nào</p>
          </div>
        ) : (
          <div className="space-y-3">
            {notifications.map((n) => (
              <div 
                key={n.id}
                onClick={() => handleNotificationClick(n)}
                className={clsx(
                  "p-4 rounded-2xl border transition-all cursor-pointer relative",
                  n.isRead 
                    ? "bg-white border-gray-100 opacity-70" 
                    : "bg-white border-orange-100 shadow-sm ring-1 ring-orange-50"
                )}
              >
                {!n.isRead && (
                  <span className="absolute top-4 right-4 w-2 h-2 bg-orange-500 rounded-full"></span>
                )}
                <div className="flex gap-4">
                  <div className={clsx(
                    "w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0",
                    n.isRead ? "bg-gray-100 text-gray-400" : "bg-orange-100 text-orange-600"
                  )}>
                    {n.type.includes('BOOKING') ? <Clock className="w-5 h-5" /> : <Info className="w-5 h-5" />}
                  </div>
                  <div className="flex-1">
                    <h3 className={clsx("font-bold text-sm", n.isRead ? "text-gray-700" : "text-gray-900")}>
                      {n.title}
                    </h3>
                    <p className="text-xs text-gray-500 mt-1 line-clamp-2">{n.content}</p>
                    <span className="text-[10px] text-gray-400 mt-2 block">
                      {new Date(n.createdAt).toLocaleString('vi-VN')}
                    </span>
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
