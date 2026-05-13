import React, { useState } from 'react';
import { useAuthStore } from '../../stores/authStore';
import { User, Mail, Shield, Phone, LogOut, ChevronRight, Lock } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../../modules/auth/authApi';
import { Modal, Form, Input, message } from 'antd';

const CustomerProfile: React.FC = () => {
    const { user, logout } = useAuthStore();
    const navigate = useNavigate();
    const [isPasswordModalVisible, setIsPasswordModalVisible] = useState(false);
    const [isProfileModalVisible, setIsProfileModalVisible] = useState(false);
    const [form] = Form.useForm();
    const [profileForm] = Form.useForm();
    const [loading, setLoading] = useState(false);

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

    const handleChangePassword = async (values: any) => {
        setLoading(true);
        try {
            await authApi.changePassword({
                oldPassword: values.oldPassword,
                newPassword: values.newPassword
            });
            message.success('Đổi mật khẩu thành công!');
            setIsPasswordModalVisible(false);
            form.resetFields();
        } catch (error: any) {
            message.error(error.response?.data?.message || 'Mật khẩu cũ không chính xác.');
        } finally {
            setLoading(false);
        }
    };

    const handleUpdateProfile = async (values: any) => {
        setLoading(true);
        try {
            const res = await authApi.updateProfile({
                fullName: values.fullName,
                phoneNumber: values.phoneNumber,
                avatarUrl: user.avatarUrl
            });
            
            // Update local store with new user data and token
            const { accessToken, user: updatedUser } = res.data;
            useAuthStore.getState().setAuth(updatedUser as any, accessToken);
            
            message.success('Cập nhật thông tin thành công!');
            setIsProfileModalVisible(false);
        } catch (error: any) {
            const errorMsg = error.response?.data?.message || error.message || 'Có lỗi xảy ra khi cập nhật.';
            message.error(errorMsg);
            console.error('Update profile error:', error);
        } finally {
            setLoading(false);
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
                            <p className="text-sm font-medium text-gray-800">{user.phoneNumber || 'Đang cập nhật'}</p>
                        </div>
                        <button 
                            onClick={() => {
                                profileForm.setFieldsValue({
                                    fullName: user.fullName,
                                    phoneNumber: user.phoneNumber
                                });
                                setIsProfileModalVisible(true);
                            }}
                            className="text-blue-600 text-sm font-medium"
                        >
                            {user.phoneNumber ? 'Sửa' : 'Thêm'}
                        </button>
                    </div>

                    <div 
                        onClick={() => setIsPasswordModalVisible(true)}
                        className="p-4 flex items-center gap-3 hover:bg-gray-50 cursor-pointer transition-colors"
                    >
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

            {/* Profile Modal */}
            <Modal
                title={
                    <div className="flex items-center gap-2">
                        <User size={20} className="text-blue-500" />
                        <span>Cập nhật thông tin cá nhân</span>
                    </div>
                }
                open={isProfileModalVisible}
                onCancel={() => setIsProfileModalVisible(false)}
                onOk={() => profileForm.submit()}
                confirmLoading={loading}
                okText="Lưu"
                cancelText="Hủy"
                centered
                className="rounded-2xl overflow-hidden"
            >
                <Form
                    form={profileForm}
                    layout="vertical"
                    onFinish={handleUpdateProfile}
                    className="mt-4"
                >
                    <Form.Item
                        name="fullName"
                        label="Họ và tên"
                        rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
                    >
                        <Input placeholder="Nguyễn Văn A" className="rounded-lg py-2" />
                    </Form.Item>

                    <Form.Item
                        name="phoneNumber"
                        label="Số điện thoại"
                        rules={[
                            { required: true, message: 'Vui lòng nhập số điện thoại' },
                            { pattern: /^[0-9]{10,11}$/, message: 'Số điện thoại không hợp lệ' }
                        ]}
                    >
                        <Input placeholder="0987654321" className="rounded-lg py-2" />
                    </Form.Item>
                </Form>
            </Modal>

            {/* Change Password Modal */}
            <Modal
                title={
                    <div className="flex items-center gap-2">
                        <Lock size={20} className="text-blue-500" />
                        <span>Đổi mật khẩu</span>
                    </div>
                }
                open={isPasswordModalVisible}
                onCancel={() => {
                    setIsPasswordModalVisible(false);
                    form.resetFields();
                }}
                onOk={() => form.submit()}
                confirmLoading={loading}
                okText="Cập nhật"
                cancelText="Hủy"
                centered
                className="rounded-2xl overflow-hidden"
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleChangePassword}
                    className="mt-4"
                >
                    <Form.Item
                        name="oldPassword"
                        label="Mật khẩu hiện tại"
                        rules={[{ required: true, message: 'Vui lòng nhập mật khẩu hiện tại' }]}
                    >
                        <Input.Password placeholder="********" className="rounded-lg py-2" />
                    </Form.Item>

                    <Form.Item
                        name="newPassword"
                        label="Mật khẩu mới"
                        rules={[
                            { required: true, message: 'Vui lòng nhập mật khẩu mới' },
                            { min: 6, message: 'Mật khẩu phải từ 6 ký tự trở lên' }
                        ]}
                    >
                        <Input.Password placeholder="********" className="rounded-lg py-2" />
                    </Form.Item>

                    <Form.Item
                        name="confirmPassword"
                        label="Xác nhận mật khẩu mới"
                        dependencies={['newPassword']}
                        rules={[
                            { required: true, message: 'Vui lòng xác nhận mật khẩu mới' },
                            ({ getFieldValue }) => ({
                                validator(_, value) {
                                    if (!value || getFieldValue('newPassword') === value) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(new Error('Mật khẩu xác nhận không khớp!'));
                                },
                            }),
                        ]}
                    >
                        <Input.Password placeholder="********" className="rounded-lg py-2" />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default CustomerProfile;
