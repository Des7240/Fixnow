import { useState, useEffect } from 'react';
import { Clock, CheckCircle, MapPin, Search, ChevronRight } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';
import { clsx } from 'clsx';
import { useNavigate } from 'react-router-dom';

interface Booking {
  id: string;
  serviceId: string;
  status: string;
  address: string;
  createdAt: string;
  totalPrice?: number;
  serviceName?: string;
}

export default function BookingsList() {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    fetchBookings();
  }, []);

  const fetchBookings = async () => {
    try {
      const res = await axiosInstance.get('/bookings');
      setBookings(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'PENDING': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'MATCHING': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'ASSIGNED': return 'bg-indigo-100 text-indigo-800 border-indigo-200';
      case 'ON_THE_WAY': return 'bg-purple-100 text-purple-800 border-purple-200';
      case 'WORKING': return 'bg-orange-100 text-orange-800 border-orange-200';
      case 'COMPLETED': return 'bg-green-100 text-green-800 border-green-200';
      case 'CANCELLED': return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  return (
    <div className="min-h-full bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white px-6 pt-10 pb-4 shadow-sm z-10 sticky top-0">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">Đơn của tôi</h1>
        <div className="relative">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm kiếm đơn hàng..."
            className="w-full pl-12 pr-4 py-3 bg-gray-100 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-orange-500/50"
          />
        </div>
      </div>

      {/* List */}
      <div className="flex-1 p-4 pb-24 overflow-y-auto">
        {loading ? (
          <div className="flex justify-center mt-10">
            <span className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></span>
          </div>
        ) : bookings.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-gray-500">
            <CheckCircle className="w-16 h-16 mb-4 text-gray-300" />
            <p>Bạn chưa có đơn đặt thợ nào</p>
          </div>
        ) : (
          <div className="space-y-4">
            {bookings.map((booking) => (
              <div 
                key={booking.id} 
                onClick={() => navigate(`/customer/bookings/${booking.id}`)}
                className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100 active:scale-[0.98] transition-all cursor-pointer"
              >
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h3 className="font-bold text-gray-900">#{booking.id.split('-')[0]}</h3>
                    <div className="flex items-center gap-1 text-xs text-gray-500 mt-1">
                      <Clock className="w-3.5 h-3.5" />
                      {new Date(booking.createdAt).toLocaleString('vi-VN')}
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    <span className={clsx(
                      'px-2.5 py-1 rounded-lg text-[10px] font-bold border',
                      getStatusColor(booking.status)
                    )}>
                      {booking.status}
                    </span>
                  </div>
                </div>
                
                <div className="flex items-center justify-between mt-4 bg-gray-50 p-3 rounded-xl">
                  <div className="flex items-start gap-2 text-xs text-gray-600">
                    <MapPin className="w-4 h-4 text-orange-500 flex-shrink-0 mt-0.5" />
                    <p className="line-clamp-1">{booking.address}</p>
                  </div>
                  <ChevronRight className="w-4 h-4 text-gray-300" />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
