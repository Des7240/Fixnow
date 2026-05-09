import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { LayoutDashboard, Users, FileBadge, Settings, LogOut, AlertTriangle } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { clsx } from 'clsx';

export default function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuthStore();

  const navItems = [
    { path: '/admin', icon: LayoutDashboard, label: 'Dashboard' },
    { path: '/admin/kyc', icon: FileBadge, label: 'Xét duyệt KYC' },
    { path: '/admin/workers', icon: Users, label: 'Quản lý Thợ' },
    { path: '/admin/disputes', icon: AlertTriangle, label: 'Quản lý Khiếu nại' },
    { path: '/admin/settings', icon: Settings, label: 'Cài đặt' },
  ];

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <div className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-6">
          <div className="text-2xl font-bold text-orange-600 flex items-center gap-2">
            <div className="w-8 h-8 bg-orange-500 rounded-lg flex items-center justify-center">
              <span className="text-white text-sm">FN</span>
            </div>
            FixNow Admin
          </div>
        </div>

        <nav className="flex-1 px-4 space-y-2 mt-4">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.path || 
              (item.path !== '/admin' && location.pathname.startsWith(item.path));

            return (
              <button
                key={item.path}
                onClick={() => navigate(item.path)}
                className={clsx(
                  'w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all',
                  isActive 
                    ? 'bg-orange-50 text-orange-600 font-bold' 
                    : 'text-gray-500 hover:bg-gray-50 hover:text-gray-900 font-medium'
                )}
              >
                <Icon className={clsx('w-5 h-5', isActive ? 'stroke-[2.5px]' : '')} />
                {item.label}
              </button>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-100">
          <div className="flex items-center gap-3 px-4 py-3 mb-2">
            <div className="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center font-bold text-gray-700">
              {user?.fullName.charAt(0)}
            </div>
            <div className="text-left">
              <p className="text-sm font-bold text-gray-900">{user?.fullName}</p>
              <p className="text-xs text-gray-500">Administrator</p>
            </div>
          </div>
          <button 
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-4 py-2.5 text-red-500 hover:bg-red-50 rounded-xl transition-all font-medium text-sm"
          >
            <LogOut className="w-4 h-4" /> Đăng xuất
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 overflow-auto">
        <Outlet />
      </div>
    </div>
  );
}
