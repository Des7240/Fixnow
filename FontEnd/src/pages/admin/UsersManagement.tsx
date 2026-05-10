import { useState, useEffect } from 'react';
import { Table, Tag, Button, message, Popconfirm, Input, Select } from 'antd';
import { ShieldAlert, Search, CheckCircle, User as UserIcon } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';

export default function UsersManagement() {
  const [users, setUsers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState('');
  const [filterRole, setFilterRole] = useState<string>('ALL');

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const res = await axiosInstance.get('/admin/users');
      setUsers(res.data);
    } catch (err) {
      message.error('Không thể tải danh sách người dùng');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (id: string, newStatus: string) => {
    try {
      await axiosInstance.patch(`/admin/users/${id}/status`, { status: newStatus });
      message.success(`Đã cập nhật trạng thái người dùng thành ${newStatus}`);
      fetchUsers();
    } catch (err) {
      message.error('Có lỗi xảy ra khi cập nhật trạng thái');
    }
  };

  const filteredUsers = users.filter(u => {
    const matchesSearch = u.fullName.toLowerCase().includes(searchText.toLowerCase()) ||
                         u.email.toLowerCase().includes(searchText.toLowerCase());
    const matchesRole = filterRole === 'ALL' || u.role === filterRole;
    return matchesSearch && matchesRole;
  });

  const columns = [
    {
      title: 'Người dùng',
      key: 'user',
      render: (_: any, record: any) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center text-gray-600 font-bold">
            {record.fullName.charAt(0)}
          </div>
          <div>
            <p className="font-bold text-gray-900">{record.fullName}</p>
            <p className="text-xs text-gray-500">{record.email}</p>
          </div>
        </div>
      ),
    },
    {
      title: 'Vai trò',
      dataIndex: 'role',
      key: 'role',
      render: (role: string) => {
        let color = role === 'ADMIN' ? 'purple' : role === 'WORKER' ? 'orange' : 'blue';
        return <Tag color={color} className="font-bold">{role}</Tag>;
      }
    },
    {
      title: 'Ngày gia nhập',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => new Date(date).toLocaleDateString('vi-VN'),
    },
    {
      title: 'Trạng thái',
      key: 'status',
      dataIndex: 'status',
      render: (status: string) => {
        let color = status === 'ACTIVE' ? 'success' : status === 'BANNED' ? 'error' : 'default';
        return <Tag color={color} className="rounded-full px-3">{status}</Tag>;
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      render: (_: any, record: any) => (
        <div className="flex gap-2">
          {record.role !== 'ADMIN' && (
            <>
              {record.status !== 'BANNED' ? (
                <Popconfirm
                  title="Khóa tài khoản này?"
                  description="Người dùng này sẽ không thể đăng nhập hoặc thực hiện giao dịch."
                  onConfirm={() => handleUpdateStatus(record.id, 'BANNED')}
                  okText="Đồng ý"
                  cancelText="Hủy"
                >
                  <Button 
                    danger 
                    type="text" 
                    icon={<ShieldAlert className="w-4 h-4" />}
                    className="flex items-center gap-1"
                  >
                    Khóa
                  </Button>
                </Popconfirm>
              ) : (
                <Popconfirm
                  title="Kích hoạt lại tài khoản?"
                  description="Cho phép người dùng này tiếp tục hoạt động trên hệ thống."
                  onConfirm={() => handleUpdateStatus(record.id, 'ACTIVE')}
                  okText="Đồng ý"
                  cancelText="Hủy"
                >
                  <Button 
                    type="link" 
                    icon={<CheckCircle className="w-4 h-4" />}
                    className="text-green-600 font-bold flex items-center gap-1 hover:text-green-700"
                  >
                    Kích hoạt
                  </Button>
                </Popconfirm>
              )}
            </>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="p-8 bg-gray-50 min-h-full">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Quản lý Người dùng</h1>
        <p className="text-gray-500">Danh sách toàn bộ thành viên trên hệ thống FixNow</p>
      </div>

      <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
        <div className="mb-6 flex flex-wrap justify-between items-center gap-4">
          <div className="flex items-center gap-4">
            <div className="relative w-80">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 z-10" />
              <Input 
                placeholder="Tìm theo tên, email..." 
                value={searchText}
                onChange={e => setSearchText(e.target.value)}
                className="pl-12 py-3 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-orange-500/20 text-sm"
              />
            </div>
            <Select 
              defaultValue="ALL" 
              style={{ width: 150 }} 
              onChange={setFilterRole}
              className="h-11"
              options={[
                { value: 'ALL', label: 'Tất cả vai trò' },
                { value: 'CUSTOMER', label: 'Khách hàng' },
                { value: 'WORKER', label: 'Thợ' },
                { value: 'ADMIN', label: 'Quản trị viên' },
              ]}
            />
          </div>
          
          <div className="flex gap-2">
            <div className="bg-blue-50 text-blue-600 px-4 py-2 rounded-2xl font-bold text-sm">
              Customers: {users.filter(u => u.role === 'CUSTOMER').length}
            </div>
            <div className="bg-orange-50 text-orange-600 px-4 py-2 rounded-2xl font-bold text-sm">
              Workers: {users.filter(u => u.role === 'WORKER').length}
            </div>
          </div>
        </div>

        <Table 
          columns={columns} 
          dataSource={filteredUsers} 
          rowKey="id" 
          loading={loading}
          pagination={{ pageSize: 10 }}
        />
      </div>
    </div>
  );
}
