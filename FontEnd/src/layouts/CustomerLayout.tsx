import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Home, ListOrdered, Bell, User } from 'lucide-react';
import { clsx } from 'clsx';

export default function CustomerLayout() {
  const navigate = useNavigate();
  const location = useLocation();

  const navItems = [
    { path: '/', icon: Home, label: 'Trang chủ' },
    { path: '/customer/bookings', icon: ListOrdered, label: 'Đơn của tôi' },
    { path: '/customer/notifications', icon: Bell, label: 'Thông báo' },
    { path: '/customer/profile', icon: User, label: 'Tài khoản' },
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
              (item.path !== '/' && location.pathname.startsWith(item.path));

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
                  'p-1.5 rounded-xl transition-all duration-300',
                  isActive ? 'bg-orange-50' : 'bg-transparent'
                )}>
                  <Icon className={clsx('w-6 h-6', isActive ? 'stroke-[2.5px]' : 'stroke-2')} />
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
    </div>
  );
}
