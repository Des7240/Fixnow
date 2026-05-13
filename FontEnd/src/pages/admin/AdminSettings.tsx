import { useState, useEffect } from 'react';
import { Table, Button, message, Modal, Input, InputNumber, Tabs, Tag, Space, Switch } from 'antd';
import { Plus, Settings, ListTree, Activity, Database, Edit, Trash2, Percent, Coins } from 'lucide-react';
import { adminApi } from '../../modules/admin/adminApi';
import type { SystemConfig, ServiceCommission } from '../../modules/admin/adminApi';

export default function AdminSettings() {
  const [services, setServices] = useState<any[]>([]);
  const [auditLogs, setAuditLogs] = useState<any[]>([]);
  const [configs, setConfigs] = useState<SystemConfig[]>([]);
  const [commissions, setCommissions] = useState<ServiceCommission[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Modals
  const [isServiceModalOpen, setIsServiceModalOpen] = useState(false);
  const [isConfigModalOpen, setIsConfigModalOpen] = useState(false);
  const [isCommissionModalOpen, setIsCommissionModalOpen] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  
  // Forms State
  const [editingService, setEditingService] = useState<any>(null);
  const [serviceForm, setServiceForm] = useState({
    name: '',
    description: '',
    basePrice: 0,
    estimatedDurationMinutes: 60,
    isActive: true
  });

  const [editingConfig, setEditingConfig] = useState<SystemConfig | null>(null);
  const [configValue, setConfigValue] = useState('');

  const [editingCommission, setEditingCommission] = useState<ServiceCommission | null>(null);
  const [commissionPercent, setCommissionPercent] = useState(10);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [servicesRes, logsRes, configsRes, commissionsRes] = await Promise.all([
        adminApi.getServices(),
        adminApi.getAuditLogs(),
        adminApi.getConfigs(),
        adminApi.getCommissions()
      ]);
      setServices(servicesRes.data);
      setAuditLogs(logsRes.data);
      setConfigs(configsRes.data);
      setCommissions(commissionsRes.data);
    } catch (err) {
      message.error('Không thể tải dữ liệu cấu hình');
    } finally {
      setLoading(false);
    }
  };

  const handleSaveService = async () => {
    if (!serviceForm.name.trim()) {
      message.error('Vui lòng nhập tên dịch vụ');
      return;
    }
    setFormLoading(true);
    try {
      if (editingService) {
        await adminApi.updateService(editingService.id, serviceForm);
        message.success('Đã cập nhật dịch vụ');
      } else {
        await adminApi.createService(serviceForm as any);
        message.success('Đã thêm dịch vụ mới');
      }
      setIsServiceModalOpen(false);
      fetchData();
    } catch (err) {
      message.error('Lỗi khi lưu dịch vụ');
    } finally {
      setFormLoading(false);
    }
  };

  const handleDeleteService = (id: string) => {
    Modal.confirm({
      title: 'Xác nhận xóa',
      content: 'Bạn có chắc chắn muốn ngừng kích hoạt dịch vụ này?',
      okText: 'Xác nhận',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await adminApi.deleteService(id);
          message.success('Đã ngừng kích hoạt dịch vụ');
          fetchData();
        } catch (err) {
          message.error('Lỗi khi xóa dịch vụ');
        }
      }
    });
  };

  const handleUpdateConfig = async () => {
    if (!editingConfig) return;
    setFormLoading(true);
    try {
      await adminApi.updateConfig({ key: editingConfig.configKey, value: configValue });
      message.success('Đã cập nhật cấu hình');
      setIsConfigModalOpen(false);
      fetchData();
    } catch (err) {
      message.error('Lỗi khi cập nhật cấu hình');
    } finally {
      setFormLoading(false);
    }
  };

  const handleUpdateCommission = async () => {
    if (!editingCommission) return;
    setFormLoading(true);
    try {
      await adminApi.updateCommission({ 
        serviceId: editingCommission.serviceId, 
        percent: commissionPercent 
      });
      message.success('Đã cập nhật tỷ lệ hoa hồng');
      setIsCommissionModalOpen(false);
      fetchData();
    } catch (err) {
      message.error('Lỗi khi cập nhật hoa hồng');
    } finally {
      setFormLoading(false);
    }
  };

  const serviceColumns = [
    { title: 'Tên Dịch vụ', dataIndex: 'name', key: 'name', render: (text: string) => <span className="font-bold">{text}</span> },
    { title: 'Giá cơ bản', dataIndex: 'basePrice', key: 'basePrice', render: (val: number) => `${val.toLocaleString()} đ` },
    { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: (active: boolean) => <Tag color={active ? 'green' : 'red'}>{active ? 'Đang hoạt động' : 'Tạm dừng'}</Tag> },
    { 
      title: 'Thao tác', 
      key: 'action', 
      render: (_: any, record: any) => (
        <Space size="middle">
          <Button 
            icon={<Edit className="w-4 h-4" />} 
            onClick={() => {
              setEditingService(record);
              setServiceForm({
                name: record.name,
                description: record.description || '',
                basePrice: record.basePrice,
                estimatedDurationMinutes: record.estimatedDurationMinutes,
                isActive: record.isActive
              });
              setIsServiceModalOpen(true);
            }}
          />
          <Button 
            danger 
            icon={<Trash2 className="w-4 h-4" />} 
            onClick={() => handleDeleteService(record.id)}
          />
        </Space>
      )
    },
  ];

  const configColumns = [
    { title: 'Tham số', dataIndex: 'description', key: 'desc', render: (text: string) => <span className="font-medium text-gray-700">{text}</span> },
    { title: 'Giá trị', dataIndex: 'configValue', key: 'val', render: (val: string) => <Tag color="orange" className="font-mono text-sm">{parseInt(val).toLocaleString()} đ</Tag> },
    { 
      title: 'Thao tác', 
      key: 'action', 
      render: (_: any, record: SystemConfig) => (
        <Button 
          type="link" 
          icon={<Edit className="w-4 h-4" />} 
          onClick={() => {
            setEditingConfig(record);
            setConfigValue(record.configValue);
            setIsConfigModalOpen(true);
          }}
        > Sửa</Button>
      )
    },
  ];

  const commissionColumns = [
    { title: 'Dịch vụ', dataIndex: 'name', key: 'service' },
    { 
      title: 'Hoa hồng (%)', 
      key: 'percent', 
      render: (_: any, record: any) => {
        const commission = commissions.find(c => c.serviceId === record.id);
        const percent = commission ? commission.commissionPercent : 10;
        return (
          <Space>
            <Tag color={commission ? "blue" : "default"} className="font-bold">{percent} %</Tag>
            {!commission && <Tag color="orange" className="text-[10px]">Mặc định</Tag>}
          </Space>
        );
      }
    },
    { 
      title: 'Thao tác', 
      key: 'action', 
      render: (_: any, record: any) => {
        const commission = commissions.find(c => c.serviceId === record.id);
        return (
          <Button 
            type="link" 
            icon={<Percent className="w-4 h-4" />} 
            onClick={() => {
              setEditingCommission(commission || { serviceId: record.id, service: record, commissionPercent: 10 } as any);
              setCommissionPercent(commission ? commission.commissionPercent : 10);
              setIsCommissionModalOpen(true);
            }}
          > {commission ? 'Thay đổi' : 'Thiết lập'}</Button>
        );
      }
    },
  ];

  const logColumns = [
    { title: 'Thời gian', dataIndex: 'createdAt', key: 'createdAt', render: (date: string) => new Date(date).toLocaleString('vi-VN') },
    { title: 'Hành động', dataIndex: 'actionType', key: 'actionType', render: (type: string) => <Tag color="blue">{type}</Tag> },
    { title: 'Người thực hiện', dataIndex: 'actorRole', key: 'actorRole', render: (role: string) => <Tag color={role === 'ADMIN' ? 'red' : 'orange'}>{role}</Tag> },
    { title: 'Chi tiết', dataIndex: 'details', key: 'details', ellipsis: true },
  ];

  return (
    <div className="p-8 bg-gray-50 min-h-full">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
          <Settings className="w-8 h-8 text-orange-500" /> Cấu hình Hệ thống
        </h1>
        <p className="text-gray-500">Thiết lập thông số vận hành và quản lý tài chính nền tảng.</p>
      </div>

      <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
        <Tabs 
          defaultActiveKey="platform"
          items={[
            {
              key: 'platform',
              label: <span className="flex items-center gap-2"><Database className="w-4 h-4" /> Thông số & Hoa hồng</span>,
              children: (
                <div className="space-y-10 py-4">
                  <div>
                    <h3 className="text-lg font-bold mb-4 flex items-center gap-2 text-gray-800">
                        <Coins className="w-5 h-5 text-orange-500" /> Hạn mức Tài chính
                    </h3>
                    <Table columns={configColumns} dataSource={configs} rowKey="configKey" pagination={false} size="middle" />
                  </div>
                  
                  <div>
                    <h3 className="text-lg font-bold mb-4 flex items-center gap-2 text-gray-800">
                        <Percent className="w-5 h-5 text-blue-500" /> Phí hoa hồng theo dịch vụ
                    </h3>
                    <Table columns={commissionColumns} dataSource={services} rowKey="id" pagination={{ pageSize: 5 }} size="middle" />
                    <p className="text-xs text-gray-400 mt-2 italic">* Các dịch vụ chưa có trong danh sách sẽ áp dụng mức mặc định 10%.</p>
                  </div>
                </div>
              )
            },
            {
              key: 'services',
              label: <span className="flex items-center gap-2"><ListTree className="w-4 h-4" /> Danh mục Dịch vụ</span>,
              children: (
                <div className="space-y-4 py-4">
                  <div className="flex justify-end">
                    <button 
                      onClick={() => {
                        setEditingService(null);
                        setServiceForm({ name: '', description: '', basePrice: 0, estimatedDurationMinutes: 60, isActive: true });
                        setIsServiceModalOpen(true);
                      }}
                      className="px-4 py-2 bg-orange-500 text-white font-bold rounded-xl flex items-center gap-2 hover:bg-orange-600 transition-all"
                    >
                      <Plus className="w-5 h-5" /> Thêm Dịch vụ
                    </button>
                  </div>
                  <Table columns={serviceColumns} dataSource={services} rowKey="id" loading={loading} pagination={{ pageSize: 5 }} />
                </div>
              )
            },
            {
              key: 'logs',
              label: <span className="flex items-center gap-2"><Activity className="w-4 h-4" /> Nhật ký Hệ thống (Audit)</span>,
              children: (
                <div className="py-4">
                    <Table columns={logColumns} dataSource={auditLogs} rowKey="id" loading={loading} pagination={{ pageSize: 10 }} />
                </div>
              )
            },
          ]}
        />
      </div>

      {/* Service Modal */}
      <Modal
        title={editingService ? "Cập nhật Dịch vụ" : "Thêm Dịch vụ mới"}
        open={isServiceModalOpen}
        onCancel={() => setIsServiceModalOpen(false)}
        onOk={handleSaveService}
        confirmLoading={formLoading}
        centered
        className="rounded-3xl"
      >
        <div className="py-4 space-y-4">
          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Tên dịch vụ</label>
            <Input 
              value={serviceForm.name} 
              onChange={e => setServiceForm({...serviceForm, name: e.target.value})}
              className="rounded-xl py-2"
            />
          </div>
          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Mô tả</label>
            <Input.TextArea 
              value={serviceForm.description} 
              onChange={e => setServiceForm({...serviceForm, description: e.target.value})}
              className="rounded-xl"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Giá cơ bản (đ)</label>
              <InputNumber className="w-full rounded-xl" min={0} value={serviceForm.basePrice} onChange={val => setServiceForm({...serviceForm, basePrice: val || 0})} />
            </div>
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Trạng thái</label>
              <div className="pt-1">
                <Switch checked={serviceForm.isActive} onChange={val => setServiceForm({...serviceForm, isActive: val})} />
                <span className="ml-2 text-sm">{serviceForm.isActive ? 'Đang bật' : 'Tạm dừng'}</span>
              </div>
            </div>
          </div>
        </div>
      </Modal>

      {/* Config Modal */}
      <Modal
        title="Cập nhật thông số hệ thống"
        open={isConfigModalOpen}
        onCancel={() => setIsConfigModalOpen(false)}
        onOk={handleUpdateConfig}
        confirmLoading={formLoading}
        centered
      >
        <div className="py-4">
          <p className="text-sm text-gray-500 mb-4">{editingConfig?.description}</p>
          <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Giá trị mới (VND)</label>
          <InputNumber 
            className="w-full rounded-xl py-1" 
            value={parseInt(configValue)} 
            onChange={val => setConfigValue(val?.toString() || '0')}
            formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={value => value?.replace(/\$\s?|(,*)/g, '') as any}
          />
        </div>
      </Modal>

      {/* Commission Modal */}
      <Modal
        title="Cấu hình hoa hồng dịch vụ"
        open={isCommissionModalOpen}
        onCancel={() => setIsCommissionModalOpen(false)}
        onOk={handleUpdateCommission}
        confirmLoading={formLoading}
        centered
      >
        <div className="py-4">
          <p className="text-sm font-bold mb-4">Dịch vụ: {editingCommission?.service?.name}</p>
          <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Tỷ lệ hoa hồng (%)</label>
          <InputNumber 
            className="w-full rounded-xl" 
            min={0} max={100} 
            value={commissionPercent} 
            onChange={val => setCommissionPercent(val || 0)} 
          />
          <p className="text-xs text-gray-400 mt-2 italic">Ví dụ: Nhập 15 để hệ thống thu 15% phí trên mỗi đơn hàng thành công.</p>
        </div>
      </Modal>
    </div>
  );
}
