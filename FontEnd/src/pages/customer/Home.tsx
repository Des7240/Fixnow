import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import { MapPin, Search, Wrench, Bell, User } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';

// Fix leaflet icon path issue
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

// Mock fallback services if database is empty
const MOCK_SERVICES = [
  { id: '1', name: 'Sửa điện', iconUrl: '⚡' },
  { id: '2', name: 'Sửa nước', iconUrl: '💧' },
];

export default function CustomerHome() {
  const navigate = useNavigate();
  const [position, setPosition] = useState<[number, number]>([21.028511, 105.804817]); // Default Hanoi
  const [selectedService, setSelectedService] = useState<string | null>(null);
  const [services, setServices] = useState<any[]>([]);

  useEffect(() => {
    // Get user's actual location
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => setPosition([pos.coords.latitude, pos.coords.longitude]),
        (err) => console.warn('Geolocation error:', err)
      );
    }

    // Fetch real services from database
    const fetchServices = async () => {
      try {
        const res = await axiosInstance.get('/services');
        setServices(res.data.length > 0 ? res.data : MOCK_SERVICES);
      } catch (err) {
        console.error('Failed to fetch services', err);
        setServices(MOCK_SERVICES);
      }
    };
    fetchServices();
  }, []);

  return (
    <div className="h-screen flex flex-col bg-gray-50 relative">
      {/* Top Navigation */}
      <div className="absolute top-0 left-0 right-0 z-10 px-4 pt-4 pb-2 bg-gradient-to-b from-white/90 to-transparent">
        <div className="flex justify-between items-center mb-4">
          <div className="flex flex-col">
            <span className="text-sm text-gray-500">Vị trí hiện tại</span>
            <div className="flex items-center gap-1 text-gray-900 font-semibold">
              <MapPin className="w-4 h-4 text-orange-500" />
              <span>Đang tải vị trí...</span>
            </div>
          </div>
          <div className="flex gap-3">
            <button className="w-10 h-10 bg-white rounded-full flex items-center justify-center shadow-sm text-gray-600 hover:text-orange-500">
              <Bell className="w-5 h-5" />
            </button>
            <button className="w-10 h-10 bg-white rounded-full flex items-center justify-center shadow-sm text-gray-600 hover:text-orange-500">
              <User className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Search */}
        <div className="relative">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Bạn cần sửa gì hôm nay?"
            className="w-full pl-12 pr-4 py-3.5 bg-white rounded-2xl shadow-lg shadow-gray-200/50 text-gray-900 focus:outline-none focus:ring-2 focus:ring-orange-500/50"
          />
        </div>
      </div>

      {/* Map Area */}
      <div className="flex-1 relative z-0">
        <MapContainer center={position} zoom={15} className="h-full w-full" zoomControl={false}>
          <TileLayer
            url="https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png"
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          />
          <Marker position={position}>
            <Popup>Vị trí của bạn</Popup>
          </Marker>
          <MapUpdater center={position} />
        </MapContainer>
      </div>

      {/* Bottom Sheet for Services */}
      <div className="absolute bottom-0 left-0 right-0 z-10 bg-white rounded-t-3xl shadow-[0_-10px_40px_rgba(0,0,0,0.08)] p-6 pb-8">
        <div className="w-12 h-1.5 bg-gray-200 rounded-full mx-auto mb-6"></div>
        <h3 className="text-xl font-bold text-gray-900 mb-4">Dịch vụ nổi bật</h3>
        <div className="grid grid-cols-4 gap-4 mb-6">
          {services.map((srv) => (
            <button
              key={srv.id}
              onClick={() => setSelectedService(srv.id)}
              className={`flex flex-col items-center gap-2 p-3 rounded-2xl transition-all ${
                selectedService === srv.id 
                  ? 'bg-orange-50 border-2 border-orange-500' 
                  : 'bg-gray-50 border-2 border-transparent hover:bg-gray-100'
              }`}
            >
              <span className="text-2xl">{srv.iconUrl && srv.iconUrl.length < 5 ? srv.iconUrl : '⚡'}</span>
              <span className="text-[10px] font-bold text-gray-700 whitespace-nowrap overflow-hidden text-ellipsis w-full px-1">{srv.name}</span>
            </button>
          ))}
        </div>

        <div className="flex gap-3 mb-6">
          <button 
            onClick={() => navigate(`/customer/booking/create?serviceId=${selectedService}`)}
            className="flex-1 py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-lg shadow-gray-900/30 flex items-center justify-center gap-2 disabled:opacity-50"
            disabled={!selectedService}
          >
            <Wrench className="w-5 h-5" />
            Đặt thợ ngay
          </button>
          <button 
            onClick={() => navigate(`/customer/open-job/create?serviceId=${selectedService}`)}
            className="flex-1 py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/30 flex items-center justify-center gap-2"
          >
            <Search className="w-5 h-5" />
            Đăng tin tìm thợ
          </button>
        </div>
      </div>
    </div>
  );
}

// Helper to update map center dynamically
function MapUpdater({ center }: { center: [number, number] }) {
  const map = useMap();
  useEffect(() => {
    map.setView(center, map.getZoom());
  }, [center, map]);
  return null;
}
