import { useState, useEffect } from 'react';
import { Table, Button, message, Modal, Input, InputNumber, Tabs, Tag } from 'antd';
import { Plus, Settings, ListTree, Activity, Database } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';

export default function AdminSettings() {
  const [services, setServices] = useState<any[]>([]);
  const [auditLogs, setAuditLogs] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsDepositOpen] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  
  // New Service Form State
  const [newService, setNewService] = useState({
    name: '',
    description: '',
    basePrice: 0,
    estimatedDurationMinutes: 60
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [servicesRes, logsRes] = await Promise.all([
        axiosInstance.get('/services'),
        axiosInstance.get('/admin/audit-logs')
      ]);
      setServices(servicesRes.data);
      setAuditLogs(logsRes.data);
    } catch (err) {
      message.error('Không thể tải dữ liệu cấu hình');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateService = async () => {
    if (!newService.name.trim()) {
      message.error('Vui lòng nhập tên dịch vụ');
      return;
    }
    setFormLoading(true);
    try {
      await axiosInstance.post('/services', newService);
      message.success('Đã thêm dịch vụ mới');
      setIsDepositOpen(false);
      setNewService({ name: '', description: '', basePrice: 0, estimatedDurationMinutes: 60 });
      fetchData();
    } catch (err) {
      message.error('Lỗi khi tạo dịch vụ');
    } finally {
      setFormLoading(false);
    }
  };

  const serviceColumns = [
    { title: 'Tên Dịch vụ', dataIndex: 'name', key: 'name', render: (text: string) => <span className="font-bold">{text}</span> },
    { title: 'Mô tả', dataIndex: 'description', key: 'description' },
    { title: 'Giá cơ bản', dataIndex: 'basePrice', key: 'basePrice', render: (val: number) => `${val.toLocaleString()} đ` },
    { title: 'Thời gian ước tính', dataIndex: 'estimatedDurationMinutes', key: 'duration', render: (val: number) => `${val} phút` },
  ];

  const logColumns = [
    { title: 'Thời gian', dataIndex: 'createdAt', key: 'createdAt', render: (date: string) => new Date(date).toLocaleString('vi-VN') },
    { title: 'Hành động', dataIndex: 'actionType', key: 'actionType', render: (type: string) => <Tag color="blue">{type}</Tag> },
    { title: 'Đối tượng', dataIndex: 'entityName', key: 'entityName' },
    { title: 'Người thực hiện', dataIndex: 'actorRole', key: 'actorRole', render: (role: string) => <Tag color={role === 'ADMIN' ? 'red' : 'orange'}>{role}</Tag> },
    { title: 'Chi tiết', dataIndex: 'details', key: 'details', ellipsis: true },
  ];

  return (
    <div className="p-8 bg-gray-50 min-h-full">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
          <Settings className="w-8 h-8 text-orange-500" /> Cấu hình Hệ thống
        </h1>
        <p className="text-gray-500">Quản lý danh mục dịch vụ và theo dõi nhật ký hoạt động.</p>
      </div>

      <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
        <Tabs 
          defaultActiveKey="services"
          items={[
            {
              key: 'services',
              label: <span className="flex items-center gap-2"><ListTree className="w-4 h-4" /> Danh mục Dịch vụ</span>,
              children: (
                <div className="space-y-4">
                  <div className="flex justify-end">
                    <button 
                      onClick={() => setIsDepositOpen(true)}
                      className="px-4 py-2 bg-orange-500 text-white font-bold rounded-xl flex items-center gap-2 hover:bg-orange-600 transition-all"
                    >
                      <Plus className="w-5 h-5" /> Thêm Dịch vụ
                    </button>
                  </div>
                  <Table 
                    columns={serviceColumns} 
                    dataSource={services} 
                    rowKey="id" 
                    loading={loading}
                    pagination={{ pageSize: 5 }}
                  />
                </div>
              )
            },
            {
              key: 'logs',
              label: <span className="flex items-center gap-2"><Activity className="w-4 h-4" /> Nhật ký Hệ thống (Audit)</span>,
              children: (
                <Table 
                  columns={logColumns} 
                  dataSource={auditLogs} 
                  rowKey="id" 
                  loading={loading}
                  pagination={{ pageSize: 10 }}
                />
              )
            },
            {
              key: 'platform',
              label: <span className="flex items-center gap-2"><Database className="w-4 h-4" /> Thông số Nền tảng</span>,
              children: (
                <div className="py-10 grid grid-cols-2 gap-6 max-w-2xl">
                  <div className="p-6 bg-gray-50 rounded-2xl border border-gray-100">
                    <p className="text-xs font-bold text-gray-400 uppercase mb-2">Phí hoa hồng Hệ thống</p>
                    <p className="text-3xl font-black text-gray-900">10 %</p>
                    <p className="text-xs text-gray-500 mt-2 italic">* Đang được áp dụng cho toàn bộ đơn hàng</p>
                  </div>
                  <div className="p-6 bg-gray-50 rounded-2xl border border-gray-100">
                    <p className="text-xs font-bold text-gray-400 uppercase mb-2">Hạn mức rút tiền tối thiểu</p>
                    <p className="text-3xl font-black text-gray-900">50,000 đ</p>
                    <p className="text-xs text-gray-500 mt-2 italic">* Áp dụng cho Ví thu nhập của Thợ</p>
                  </div>
                </div>
              )
            }
          ]}
        />
      </div>

      <Modal
        title="Thêm Dịch vụ mới"
        open={isModalOpen}
        onCancel={() => setIsDepositOpen(false)}
        onOk={handleCreateService}
        confirmLoading={formLoading}
        okText="Lưu lại"
        cancelText="Hủy"
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-4 space-y-4">
          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Tên dịch vụ</label>
            <Input 
              value={newService.name} 
              onChange={e => setNewService({...newService, name: e.target.value})}
              placeholder="VD: Sửa điện nước, Lắp điều hòa..."
              className="rounded-xl py-2"
            />
          </div>
          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Mô tả</label>
            <Input.TextArea 
              value={newService.description} 
              onChange={e => setNewService({...newService, description: e.target.value})}
              placeholder="Mô tả ngắn gọn về dịch vụ..."
              className="rounded-xl"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Giá cơ bản (đ)</label>
              <InputNumber 
                className="w-full rounded-xl py-1"
                min={0}
                value={newService.basePrice}
                onChange={val => setNewService({...newService, basePrice: val || 0})}
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Thời gian ước tính (phút)</label>
              <InputNumber 
                className="w-full rounded-xl py-1"
                min={1}
                value={newService.estimatedDurationMinutes}
                onChange={val => setNewService({...newService, estimatedDurationMinutes: val || 60})}
              />
            </div>
          </div>
        </div>
      </Modal>
    </div>
  );
}
