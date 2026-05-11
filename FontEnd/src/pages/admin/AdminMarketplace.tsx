import { useEffect, useState } from 'react';
import { Table, Tag, Button, Space, Modal, Input, message, Tabs, Card, Statistic, Row, Col } from 'antd';
import { Search, ShieldAlert, Trash2, Eye, Flag, BarChart3, Users, Briefcase, Clock } from 'lucide-react';
import { useOpenJobStore } from '../../stores/openJobStore';
import type { OpenJob } from '../../stores/openJobStore';
import moment from 'moment';

const { TabPane } = Tabs;

export default function AdminMarketplace() {
  const { adminJobs, loading, fetchAdminJobs, moderateJob, deleteJob } = useOpenJobStore();
  const [searchText, setSearchText] = useState('');
  const [isModerateModalOpen, setIsModerateModalOpen] = useState(false);
  const [selectedJob, setSelectedJob] = useState<OpenJob | null>(null);
  const [moderationReason, setModerationReason] = useState('');

  useEffect(() => {
    fetchAdminJobs();
  }, [fetchAdminJobs]);

  const handleModerate = (job: OpenJob) => {
    setSelectedJob(job);
    setIsModerateModalOpen(true);
  };

  const submitModeration = async (status: string) => {
    if (!selectedJob) return;
    try {
      await moderateJob(selectedJob.id, status, moderationReason);
      message.success(`Đã cập nhật trạng thái kiểm duyệt: ${status}`);
      setIsModerateModalOpen(false);
      setModerationReason('');
    } catch (err) {
      message.error('Lỗi khi cập nhật kiểm duyệt');
    }
  };

  const handleDelete = (job: OpenJob) => {
    Modal.confirm({
      title: 'Xác nhận xóa bài đăng',
      content: `Bạn có chắc chắn muốn xóa bài đăng "${job.title}" không? Hành động này không thể hoàn tác.`,
      okText: 'Xóa',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await deleteJob(job.id);
          message.success('Đã xóa bài đăng');
        } catch (err) {
          message.error('Lỗi khi xóa bài đăng');
        }
      }
    });
  };

  const filteredJobs = adminJobs.filter(job => 
    job.title.toLowerCase().includes(searchText.toLowerCase()) ||
    job.customerName.toLowerCase().includes(searchText.toLowerCase())
  );

  const columns = [
    {
      title: 'Khách hàng',
      dataIndex: 'customerName',
      key: 'customerName',
      render: (text: string, record: OpenJob) => (
        <Space>
          <div className="w-8 h-8 rounded-full bg-gray-200 overflow-hidden">
            {record.customerAvatar && <img src={record.customerAvatar} alt="avatar" className="w-full h-full object-cover" />}
          </div>
          <span className="font-medium">{text}</span>
        </Space>
      )
    },
    {
      title: 'Công việc',
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: OpenJob) => (
        <div>
          <div className="font-bold text-gray-900">{text}</div>
          <div className="text-xs text-gray-400">{record.serviceName}</div>
        </div>
      )
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={status === 'OPEN' ? 'green' : 'default'}>{status}</Tag>
      )
    },
    {
      title: 'Báo giá',
      dataIndex: 'offerCount',
      key: 'offerCount',
      render: (count: number) => <span className="font-bold">{count}</span>
    },
    {
      title: 'Ngày đăng',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => moment(date).format('DD/MM/YYYY HH:mm')
    },
    {
      title: 'Thao tác',
      key: 'action',
      render: (_: any, record: OpenJob) => (
        <Space size="middle">
          <Button icon={<Eye size={16} />} onClick={() => window.open(`/customer/open-jobs/${record.id}/offers`, '_blank')} />
          <Button icon={<Flag size={16} />} danger onClick={() => handleModerate(record)} />
          <Button icon={<Trash2 size={16} />} danger type="primary" onClick={() => handleDelete(record)} />
        </Space>
      ),
    },
  ];

  const stats = {
    total: adminJobs.length,
    active: adminJobs.filter(j => j.status === 'OPEN' || j.status === 'RECEIVING_OFFERS').length,
    conversion: adminJobs.length > 0 
      ? Math.round((adminJobs.filter(j => j.status === 'BOOKING_CREATED').length / adminJobs.length) * 100) 
      : 0
  };

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quản lý Marketplace</h1>
          <p className="text-gray-500">Giám sát và kiểm duyệt các bài đăng tìm thợ trên hệ thống.</p>
        </div>
        <BarChart3 className="w-8 h-8 text-gray-300" />
      </div>

      <Row gutter={16}>
        <Col span={8}>
          <Card variant="borderless" className="shadow-sm rounded-2xl">
            <Statistic 
                title="Tổng bài đăng" 
                value={stats.total} 
                prefix={<Briefcase className="w-4 h-4 mr-2 text-blue-500" />} 
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card variant="borderless" className="shadow-sm rounded-2xl">
            <Statistic 
                title="Đang hoạt động" 
                value={stats.active} 
                styles={{ content: { color: '#3f8600' } }} 
                prefix={<Clock className="w-4 h-4 mr-2" />} 
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card variant="borderless" className="shadow-sm rounded-2xl">
            <Statistic 
                title="Tỷ lệ chuyển đổi" 
                value={stats.conversion} 
                suffix="%" 
                prefix={<Users className="w-4 h-4 mr-2 text-orange-500" />} 
            />
          </Card>
        </Col>
      </Row>

      <Card variant="borderless" className="shadow-sm rounded-3xl overflow-hidden">
        <div className="mb-6 flex justify-between items-center gap-4">
          <Input
            prefix={<Search className="w-4 h-4 text-gray-400" />}
            placeholder="Tìm kiếm bài đăng hoặc khách hàng..."
            className="max-w-md rounded-xl h-10"
            onChange={e => setSearchText(e.target.value)}
          />
          <Space>
             <Button icon={<ShieldAlert size={16} />} className="rounded-xl">Bài đăng vi phạm</Button>
          </Space>
        </div>

        <Table 
            columns={columns} 
            dataSource={filteredJobs} 
            loading={loading}
            rowKey="id"
            pagination={{ pageSize: 10 }}
        />
      </Card>

      <Modal
        title="Kiểm duyệt bài đăng"
        open={isModerateModalOpen}
        onCancel={() => setIsModerateModalOpen(false)}
        footer={[
          <Button key="back" onClick={() => setIsModerateModalOpen(false)}>Hủy</Button>,
          <Button key="flag" type="dashed" danger onClick={() => submitModeration('FLAGGED')}>Đánh dấu vi phạm</Button>,
          <Button key="remove" type="primary" danger onClick={() => submitModeration('REMOVED')}>Xóa bài đăng (Spam)</Button>,
          <Button key="approve" type="primary" className="bg-green-600 border-none" onClick={() => submitModeration('APPROVED')}>Phê duyệt</Button>
        ]}
      >
        <div className="space-y-4 py-4">
          <div>
            <p className="text-xs font-bold text-gray-400 uppercase mb-2">Lý do kiểm duyệt</p>
            <Input.TextArea 
              rows={4} 
              placeholder="Nhập lý do hoặc nội dung vi phạm..." 
              value={moderationReason}
              onChange={e => setModerationReason(e.target.value)}
              className="rounded-xl"
            />
          </div>
          <div className="p-3 bg-orange-50 rounded-xl text-orange-800 text-xs flex gap-2 items-start">
            <ShieldAlert className="w-4 h-4 flex-shrink-0" />
            <p>Việc xóa bài đăng sẽ chuyển trạng thái sang CLOSED và thông báo cho khách hàng.</p>
          </div>
        </div>
      </Modal>
    </div>
  );
}
