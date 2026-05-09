import { useState, useEffect } from 'react';
import { useAuthStore } from '../../stores/authStore';
import { Switch, message } from 'antd';
import { Power, MapPin, Briefcase } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';
import { clsx } from 'clsx';
import { useSignalR } from '../../signalr/SignalRContext';
import { useNavigate } from 'react-router-dom';

export default function WorkerDashboard() {
  const { user } = useAuthStore();
  const { connection } = useSignalR();
  const navigate = useNavigate();
  const [isOnline, setIsOnline] = useState(false);
  const [locationUpdateActive, setLocationUpdateActive] = useState(false);
  const [newJob, setNewJob] = useState<any>(null);

  useEffect(() => {
    // Fetch initial profile to check status
    const fetchProfile = async () => {
      try {
        const res = await axiosInstance.get('/workers/profile');
        if (res.data.availabilityStatus === 'ONLINE') {
          setIsOnline(true);
        }
      } catch (err) {
        console.log('Chưa có profile, vui lòng tạo profile trước');
      }
    };
    fetchProfile();
  }, []);

  useEffect(() => {
    // Fetch existing matching jobs when component mounts or becomes online
    const fetchMatchingJobs = async () => {
      if (isOnline) {
        try {
          const res = await axiosInstance.get('/bookings/matching');
          if (res.data && res.data.length > 0) {
            // If multiple matching jobs, just show the first one for simplicity in this MVP view
            setNewJob(res.data[0]);
          }
        } catch (err) {
          console.error('Lỗi lấy danh sách đơn matching:', err);
        }
      }
    };

    fetchMatchingJobs();
  }, [isOnline]);

  useEffect(() => {
    let intervalId: ReturnType<typeof setInterval>;

    if (isOnline) {
      // Start GPS tracking
      setLocationUpdateActive(true);
      const updateLocation = () => {
        if (navigator.geolocation) {
          navigator.geolocation.getCurrentPosition(
            async (pos) => {
              try {
                await axiosInstance.put('/workers/location', {
                  lat: pos.coords.latitude,
                  lng: pos.coords.longitude
                });
              } catch (e) {
                console.error('Lỗi cập nhật vị trí');
              }
            },
            (err) => console.warn('Lỗi lấy GPS:', err)
          );
        }
      };

      updateLocation(); // Run immediately
      intervalId = setInterval(updateLocation, 30000); // 30s interval
    } else {
      setLocationUpdateActive(false);
    }

    return () => clearInterval(intervalId);
  }, [isOnline]);

  const handleStatusChange = async (checked: boolean) => {
    try {
      const status = checked ? 'ONLINE' : 'OFFLINE';
      await axiosInstance.patch('/workers/profile/availability', { status });
      setIsOnline(checked);
      message.success(checked ? 'Đã bật trực tuyến, đang chờ đơn!' : 'Đã tắt trực tuyến');
    } catch (err) {
      message.error('Vui lòng cập nhật đủ Kỹ năng và KYC trước khi bật Online!');
    }
  };

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
    }
  };

  return (
    <div className="min-h-full bg-gray-50 flex flex-col">
      {/* Header Profile Area */}
      <div className="bg-white px-6 pt-10 pb-6 shadow-sm z-10 rounded-b-3xl">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-14 h-14 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 font-bold text-xl">
              {user?.fullName.charAt(0)}
            </div>
            <div>
              <h1 className="text-lg font-bold text-gray-900">{user?.fullName}</h1>
              <p className="text-sm text-gray-500 flex items-center gap-1">
                <Briefcase className="w-4 h-4" /> Thợ dịch vụ
              </p>
            </div>
          </div>
        </div>

        {/* Status Toggle Card */}
        <div className={clsx(
          "mt-6 rounded-2xl p-4 flex items-center justify-between border-2 transition-all",
          isOnline ? "bg-orange-50 border-orange-200" : "bg-gray-50 border-gray-100"
        )}>
          <div>
            <h3 className={clsx("font-bold", isOnline ? "text-orange-700" : "text-gray-700")}>
              Trạng thái nhận đơn
            </h3>
            <p className={clsx("text-xs mt-1", isOnline ? "text-orange-600" : "text-gray-500")}>
              {isOnline ? "Hệ thống đang phát đơn cho bạn" : "Bật để nhận đơn ngay"}
            </p>
          </div>
          <div className="flex items-center gap-3">
            {locationUpdateActive && (
              <span className="flex items-center gap-1 text-xs font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded-lg">
                <MapPin className="w-3 h-3 animate-bounce" /> GPS On
              </span>
            )}
            <Switch
              checked={isOnline}
              onChange={handleStatusChange}
              className={isOnline ? 'bg-orange-500' : 'bg-gray-300'}
            />
          </div>
        </div>
      </div>

      {/* Main Content (Mock incoming job) */}
      <div className="flex-1 p-6 flex flex-col justify-center">
        {isOnline ? (
          <div className="flex flex-col items-center justify-center text-center">
            <div className="w-24 h-24 bg-orange-100 rounded-full flex items-center justify-center mb-6 relative">
              <div className="absolute inset-0 border-4 border-orange-500 rounded-full animate-ping opacity-20"></div>
              <Power className="w-10 h-10 text-orange-500 animate-pulse" />
            </div>
            <h2 className="text-xl font-bold text-gray-900 mb-2">Đang tìm đơn phù hợp...</h2>
            <p className="text-gray-500 text-sm max-w-xs">
              Giữ ứng dụng luôn mở. Khi có đơn mới, hệ thống sẽ báo ngay cho bạn.
            </p>
            
            {/* New Job Modal Overlay (MVP simple version) */}
            {newJob && (
              <div className="fixed inset-0 bg-black/60 z-50 flex items-center justify-center p-4">
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
        ) : (
          <div className="flex flex-col items-center justify-center text-center text-gray-400">
            <Power className="w-16 h-16 mb-4 opacity-50" />
            <p>Bạn đang Offline.</p>
          </div>
        )}
      </div>
    </div>
  );
}
