import { useState, useEffect } from 'react';
import { axiosInstance } from '../../api/axios';
import { Tag, Ticket, Plus, Eye, CheckCircle, XCircle } from 'lucide-react';
import { message, Modal, Form, Input, Select, InputNumber, DatePicker, Switch } from 'antd';
import dayjs from 'dayjs';

interface Promotion {
  id: string;
  code: string;
  description: string;
  discountType: string;
  discountValue: number;
  maxDiscountAmount: number | null;
  minOrderValue: number | null;
  startDate: string;
  endDate: string;
  maxUsageLimit: number;
  currentUsageCount: number;
  isActive: boolean;
  applicableServiceId: string | null;
}

export default function AdminPromotions() {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [services, setServices] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [form] = Form.useForm();

  const fetchPromotions = async () => {
    try {
      setLoading(true);
      const res = await axiosInstance.get('/promotions/admin');
      setPromotions(res.data);
    } catch (error) {
      message.error('Không thể tải danh sách khuyến mãi.');
    } finally {
      setLoading(false);
    }
  };

  const fetchServices = async () => {
    try {
      const res = await axiosInstance.get('/services');
      setServices(res.data);
    } catch (error) {}
  };

  useEffect(() => {
    fetchPromotions();
    fetchServices();
  }, []);

  const handleToggleStatus = async (id: string, currentStatus: boolean) => {
    try {
      await axiosInstance.patch(`/promotions/admin/${id}/status`, !currentStatus, {
        headers: { 'Content-Type': 'application/json' }
      });
      message.success('Cập nhật trạng thái thành công!');
      fetchPromotions();
    } catch (error) {
      message.error('Có lỗi xảy ra.');
    }
  };

  const handleCreate = async (values: any) => {
    try {
      const payload = {
        ...values,
        startDate: values.dateRange[0].toISOString(),
        endDate: values.dateRange[1].toISOString(),
        discountType: values.discountType,
        applicableServiceId: values.applicableServiceId === 'all' ? null : values.applicableServiceId
      };
      delete payload.dateRange;

      await axiosInstance.post('/promotions/admin', payload);
      message.success('Tạo mã khuyến mãi thành công!');
      setIsModalVisible(false);
      form.resetFields();
      fetchPromotions();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Có lỗi xảy ra khi tạo mã.');
    }
  };

  return (
    <div className="p-8 max-w-7xl mx-auto">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Quản lý Khuyến mãi</h1>
          <p className="text-gray-500 mt-2">Tạo và quản lý các mã giảm giá cho khách hàng.</p>
        </div>
        <button
          onClick={() => setIsModalVisible(true)}
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-xl font-medium transition-colors"
        >
          <Plus className="w-5 h-5" /> Tạo mã mới
        </button>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-500">Đang tải dữ liệu...</div>
        ) : (
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100">
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Mã (Code)</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Mô tả</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Loại</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Đã dùng</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Trạng thái</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {promotions.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="inline-flex items-center gap-1.5 px-3 py-1 bg-green-50 text-green-700 rounded-lg font-bold font-mono text-sm border border-green-100">
                      <Ticket className="w-4 h-4" /> {item.code}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="font-medium text-gray-900">{item.description}</div>
                    <div className="text-xs text-gray-500 mt-1">
                      Hạn: {dayjs(item.endDate).format('DD/MM/YYYY HH:mm')}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    {item.discountType === 'PERCENTAGE' ? (
                      <span className="text-blue-600 font-medium">Giảm {item.discountValue}%</span>
                    ) : (
                      <span className="text-orange-600 font-medium">Giảm {item.discountValue.toLocaleString()}đ</span>
                    )}
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">
                      {item.currentUsageCount} / {item.maxUsageLimit === 0 ? '∞' : item.maxUsageLimit}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <Switch 
                      checked={item.isActive} 
                      onChange={() => handleToggleStatus(item.id, item.isActive)}
                      className={item.isActive ? 'bg-green-500' : 'bg-gray-300'}
                    />
                  </td>
                  <td className="px-6 py-4 text-right">
                    {/* Thêm chức năng sửa nếu cần, hiện tại chỉ bật tắt */}
                  </td>
                </tr>
              ))}
              {promotions.length === 0 && (
                <tr>
                  <td colSpan={6} className="p-8 text-center text-gray-500">
                    Chưa có mã khuyến mãi nào.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <Modal
        title="Tạo mã khuyến mãi mới"
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        footer={null}
        width={700}
      >
        <Form form={form} layout="vertical" onFinish={handleCreate} className="mt-4">
          <div className="grid grid-cols-2 gap-4">
            <Form.Item name="code" label="Mã khuyến mãi (VD: SUMMER20)" rules={[{ required: true, message: 'Vui lòng nhập mã' }]}>
              <Input placeholder="Nhập mã viết hoa liền nhau..." className="uppercase" />
            </Form.Item>
            <Form.Item name="applicableServiceId" label="Dịch vụ áp dụng" initialValue="all">
              <Select>
                <Select.Option value="all">-- Tất cả dịch vụ --</Select.Option>
                {services.map(s => (
                  <Select.Option key={s.id} value={s.id}>{s.name}</Select.Option>
                ))}
              </Select>
            </Form.Item>
          </div>

          <Form.Item name="description" label="Mô tả" rules={[{ required: true, message: 'Vui lòng nhập mô tả' }]}>
            <Input placeholder="Ví dụ: Giảm 20% cho dịch vụ sửa chữa..." />
          </Form.Item>

          <div className="grid grid-cols-2 gap-4">
            <Form.Item name="discountType" label="Loại giảm giá" initialValue="PERCENTAGE">
              <Select>
                <Select.Option value="PERCENTAGE">Giảm theo phần trăm (%)</Select.Option>
                <Select.Option value="FIXED_AMOUNT">Giảm số tiền cố định (VNĐ)</Select.Option>
              </Select>
            </Form.Item>
            <Form.Item name="discountValue" label="Mức giảm" rules={[{ required: true, message: 'Vui lòng nhập mức giảm' }]}>
              <InputNumber className="w-full" min={1} placeholder="Ví dụ: 20 hoặc 50000" />
            </Form.Item>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Form.Item name="maxDiscountAmount" label="Giảm tối đa (VNĐ) (Tuỳ chọn)">
              <InputNumber className="w-full" min={0} placeholder="Chỉ dùng khi giảm theo %" />
            </Form.Item>
            <Form.Item name="minOrderValue" label="Đơn tối thiểu (VNĐ) (Tuỳ chọn)">
              <InputNumber className="w-full" min={0} placeholder="Ví dụ: 100000" />
            </Form.Item>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Form.Item name="dateRange" label="Thời gian áp dụng" rules={[{ required: true, message: 'Chọn thời gian' }]}>
              <DatePicker.RangePicker showTime className="w-full" />
            </Form.Item>
            <Form.Item name="maxUsageLimit" label="Giới hạn số lượt dùng" initialValue={0}>
              <InputNumber className="w-full" min={0} placeholder="0 = Không giới hạn" />
            </Form.Item>
          </div>

          <div className="flex justify-end gap-3 mt-6">
            <button
              type="button"
              onClick={() => setIsModalVisible(false)}
              className="px-5 py-2.5 rounded-xl font-medium bg-gray-100 text-gray-700 hover:bg-gray-200"
            >
              Hủy
            </button>
            <button
              type="submit"
              className="px-5 py-2.5 rounded-xl font-medium bg-blue-600 text-white hover:bg-blue-700"
            >
              Tạo mã
            </button>
          </div>
        </Form>
      </Modal>
    </div>
  );
}
