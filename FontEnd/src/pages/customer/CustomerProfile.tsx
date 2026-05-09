import React from 'react';
import { useAuthStore } from '../../stores/authStore';
import { User, Mail, Shield, Phone, LogOut, ChevronRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../../modules/auth/authApi';

const CustomerProfile: React.FC = () => {
    const { user, logout } = useAuthStore();
    const navigate = useNavigate();

    const handleLogout = async () => {
        try {
            await authApi.logout();
        } catch (error) {
            console.error('Logout error', error);
        } finally {
            logout();
            navigate('/login');
        }
    };

    if (!user) return <div className="p-4 text-center">Đang tải thông tin...</div>;

    return (
        <div className="bg-gray-50 min-h-screen pb-20">
            {/* Header */}
            <div className="bg-blue-600 px-6 pt-12 pb-6 text-white rounded-b-3xl shadow-md">
                <h1 className="text-2xl font-bold mb-4">Tài khoản của tôi</h1>
                <div className="flex items-center gap-4">
                    <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center text-blue-600 shadow-inner">
                        <User size={32} />
                    </div>
                    <div>
                        <h2 className="text-xl font-semibold">{user.fullName}</h2>
                        <p className="text-blue-100 uppercase text-sm font-medium tracking-wide mt-1">
                            Thành viên {user.role === 'CUSTOMER' ? 'Khách hàng' : user.role}
                        </p>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="px-4 mt-6">
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-6">
                    <div className="p-4 border-b border-gray-50 flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-500">
                            <Mail size={20} />
                        </div>
                        <div className="flex-1">
                            <p className="text-xs text-gray-500">Email tĩnh</p>
                            <p className="text-sm font-medium text-gray-800">{user.email}</p>
                        </div>
                    </div>
                    
                    <div className="p-4 border-b border-gray-50 flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-500">
                            <Phone size={20} />
                        </div>
                        <div className="flex-1">
                            <p className="text-xs text-gray-500">Số điện thoại</p>
                            <p className="text-sm font-medium text-gray-800">Đang cập nhật</p>
                        </div>
                        <button className="text-blue-600 text-sm font-medium">Thêm</button>
                    </div>

                    <div className="p-4 flex items-center gap-3 hover:bg-gray-50 cursor-pointer transition-colors">
                        <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-500">
                            <Shield size={20} />
                        </div>
                        <div className="flex-1">
                            <p className="text-sm font-medium text-gray-800">Đổi mật khẩu</p>
                        </div>
                        <ChevronRight size={20} className="text-gray-400" />
                    </div>
                </div>

                <h3 className="text-md font-semibold text-gray-800 mb-3 px-1">Cài đặt & Pháp lý</h3>
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-6">
                     <div className="p-4 border-b border-gray-50 flex items-center justify-between hover:bg-gray-50 cursor-pointer">
                        <span className="text-sm font-medium text-gray-700">Điều khoản sử dụng</span>
                        <ChevronRight size={20} className="text-gray-400" />
                    </div>
                    <div className="p-4 border-b border-gray-50 flex items-center justify-between hover:bg-gray-50 cursor-pointer">
                        <span className="text-sm font-medium text-gray-700">Chính sách bảo mật</span>
                        <ChevronRight size={20} className="text-gray-400" />
                    </div>
                    <div className="p-4 flex items-center justify-between hover:bg-gray-50 cursor-pointer">
                        <span className="text-sm font-medium text-gray-700">Trung tâm trợ giúp</span>
                        <ChevronRight size={20} className="text-gray-400" />
                    </div>
                </div>

                <div className="mt-8 mb-6">
                    <button 
                        onClick={handleLogout}
                        className="w-full bg-white border border-red-200 text-red-600 font-semibold py-3.5 rounded-xl shadow-sm flex justify-center items-center gap-2 hover:bg-red-50 transition-colors"
                    >
                        <LogOut size={20} />
                        Đăng xuất
                    </button>
                </div>
                
                <p className="text-center text-xs text-gray-400">Phiên bản 1.0.0 (MVP)</p>
            </div>
        </div>
    );
};

export default CustomerProfile;