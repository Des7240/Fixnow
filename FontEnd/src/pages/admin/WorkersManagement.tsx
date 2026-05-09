import { useState, useEffect } from 'react';
import { Table, Tag, Button, message, Popconfirm, Input } from 'antd';
import { ShieldAlert, Search, CheckCircle } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';

export default function WorkersManagement() {
  const [workers, setWorkers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchText, setSearchText] = useState('');

  useEffect(() => {
    fetchWorkers();
  }, []);

  const fetchWorkers = async () => {
    setLoading(true);
    try {
      const res = await axiosInstance.get('/admin/workers');
      setWorkers(res.data);
    } catch (err) {
      message.error('Không thể tải danh sách thợ');
    } finally {
      setLoading(false);
    }
  };

  const handleSuspend = async (id: string) => {
    try {
      await axiosInstance.patch(`/admin/workers/${id}/suspend`);
      message.success('Đã khóa tài khoản thợ');
      fetchWorkers();
    } catch (err) {
      message.error('Có lỗi xảy ra');
    }
  };

  const handleActivate = async (id: string) => {
    try {
      await axiosInstance.patch(`/admin/workers/${id}/activate`);
      message.success('Đã kích hoạt lại tài khoản thợ');
      fetchWorkers();
    } catch (err) {
      message.error('Có lỗi xảy ra');
    }
  };

  const filteredWorkers = workers.filter(w => 
    w.fullName.toLowerCase().includes(searchText.toLowerCase()) ||
    w.email.toLowerCase().includes(searchText.toLowerCase())
  );

  const columns = [
    {
      title: 'Thợ',
      key: 'worker',
      render: (_: any, record: any) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 font-bold">
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
          {record.status !== 'BANNED' ? (
            <Popconfirm
              title="Khóa tài khoản thợ?"
              description="Thợ này sẽ không thể nhận đơn hàng mới."
              onConfirm={() => handleSuspend(record.id)}
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
              description="Cho phép thợ này tiếp tục hoạt động trên hệ thống."
              onConfirm={() => handleActivate(record.id)}
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
        </div>
      ),
    },
  ];

  return (
    <div className="p-8 bg-gray-50 min-h-full">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Quản lý Thợ</h1>
        <p className="text-gray-500">Danh sách toàn bộ thợ trên hệ thống FixNow</p>
      </div>

      <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
        <div className="mb-6 flex justify-between items-center">
          <div className="relative w-80">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 z-10" />
            <Input 
              placeholder="Tìm thợ theo tên, email..." 
              value={searchText}
              onChange={e => setSearchText(e.target.value)}
              className="pl-12 py-3 bg-gray-50 rounded-2xl border-none focus:ring-2 focus:ring-orange-500/20 text-sm"
            />
          </div>
          <div className="flex gap-2">
            <Tag color="success" className="rounded-full px-3 py-1 font-bold">Active: {workers.filter(w => w.status === 'ACTIVE').length}</Tag>
            <Tag color="error" className="rounded-full px-3 py-1 font-bold">Banned: {workers.filter(w => w.status === 'BANNED').length}</Tag>
          </div>
        </div>

        <Table 
          columns={columns} 
          dataSource={filteredWorkers} 
          rowKey="id" 
          loading={loading}
          pagination={{ pageSize: 10 }}
        />
      </div>
    </div>
  );
}
