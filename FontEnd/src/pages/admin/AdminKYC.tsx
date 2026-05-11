import { useState, useEffect } from 'react';
import { Table, Tag, Button, Modal, message, Input } from 'antd';
import { Eye, CheckCircle, XCircle } from 'lucide-react';
import axiosInstance from '../../utils/axiosInstance';

export default function AdminKYC() {
  const [kycs, setKycs] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [selectedKyc, setSelectedKyc] = useState<any>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    fetchKycs();
  }, []);

  const fetchKycs = async () => {
    setLoading(true);
    try {
      const res = await axiosInstance.get('/admin/kyc');
      setKycs(res.data);
    } catch (err) {
      message.error('Không thể tải danh sách KYC');
    } finally {
      setLoading(false);
    }
  };

  const showDetail = (record: any) => {
    setSelectedKyc(record);
    setIsModalVisible(true);
    setRejectReason('');
  };

  const handleReview = async (status: 'APPROVED' | 'REJECTED') => {
    if (status === 'REJECTED' && !rejectReason) {
      message.error('Vui lòng nhập lý do từ chối');
      return;
    }

    setActionLoading(true);
    try {
      await axiosInstance.patch(`/admin/kyc/${selectedKyc.id}`, { 
        status, 
        reason: rejectReason 
      });
      
      message.success(`Đã ${status === 'APPROVED' ? 'duyệt' : 'từ chối'} hồ sơ`);
      setIsModalVisible(false);
      fetchKycs(); // Refresh list
    } catch (err) {
      message.error('Có lỗi xảy ra khi cập nhật');
    } finally {
      setActionLoading(false);
    }
  };

  const columns = [
    {
      title: 'Họ tên thợ',
      dataIndex: 'workerName',
      key: 'workerName',
      render: (text: string) => <span className="font-bold">{text || 'N/A'}</span>,
    },
    {
      title: 'Số CCCD',
      dataIndex: 'citizenIdNumber',
      key: 'citizenIdNumber',
    },
    {
      title: 'Ngày nộp',
      dataIndex: 'submittedAt',
      key: 'submittedAt',
      render: (date: string) => new Date(date).toLocaleDateString('vi-VN'),
    },
    {
      title: 'Trạng thái',
      key: 'status',
      dataIndex: 'status',
      render: (status: string) => {
        let color = status === 'PENDING' ? 'warning' : status === 'APPROVED' ? 'success' : 'error';
        return <Tag color={color}>{status}</Tag>;
      },
    },
    {
      title: 'Thao tác',
      key: 'action',
      render: (_: any, record: any) => (
        <Button 
          type="text" 
          className="flex items-center gap-1 text-blue-600 hover:text-blue-700" 
          icon={<Eye className="w-4 h-4" />} 
          onClick={() => showDetail(record)}
        >
          Xem chi tiết
        </Button>
      ),
    },
  ];

  // Helper to get image full path (handle MinIO or local paths)
  const getImageUrl = (path: string) => {
    if (!path) return 'https://via.placeholder.com/300x200?text=No+Image';
    if (path.startsWith('http')) return path;
    const baseUrl = import.meta.env.VITE_API_URL?.replace('/api/v1', '') || 'http://localhost:8080';
    return `${baseUrl}/${path}`;
  };

  return (
    <div className="p-8 bg-gray-50 min-h-full">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Xét duyệt KYC</h1>
        <p className="text-gray-500">Quản lý và duyệt hồ sơ xác thực danh tính của thợ</p>
      </div>

      <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100">
        <Table 
          columns={columns} 
          dataSource={kycs} 
          rowKey="id" 
          loading={loading}
          pagination={{ pageSize: 10 }}
          className="custom-table"
        />
      </div>

      <Modal
        title={<span className="text-lg font-bold">Chi tiết hồ sơ KYC</span>}
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        footer={null}
        width={800}
        centered
        className="rounded-3xl overflow-hidden"
      >
        {selectedKyc && (
          <div className="space-y-6 pt-4">
            <div className="grid grid-cols-2 gap-6">
              <div>
                <p className="text-gray-400 text-xs font-bold uppercase tracking-wider mb-2">Mặt trước CCCD</p>
                <div className="rounded-2xl overflow-hidden border border-gray-100 bg-gray-50 h-48">
                  <img 
                    src={getImageUrl(selectedKyc.citizenFrontUrl)} 
                    alt="Front" 
                    className="w-full h-full object-contain"
                  />
                </div>
              </div>
              <div>
                <p className="text-gray-400 text-xs font-bold uppercase tracking-wider mb-2">Mặt sau CCCD</p>
                <div className="rounded-2xl overflow-hidden border border-gray-100 bg-gray-50 h-48">
                  <img 
                    src={getImageUrl(selectedKyc.citizenBackUrl)} 
                    alt="Back" 
                    className="w-full h-full object-contain"
                  />
                </div>
              </div>
              <div className="col-span-2">
                <p className="text-gray-400 text-xs font-bold uppercase tracking-wider mb-2 text-center">Ảnh chân dung đối chiếu</p>
                <div className="rounded-2xl overflow-hidden border border-gray-100 bg-gray-50 w-64 h-64 mx-auto">
                  <img 
                    src={getImageUrl(selectedKyc.selfieUrl)} 
                    alt="Selfie" 
                    className="w-full h-full object-cover"
                  />
                </div>
              </div>
            </div>

            <div className="bg-gray-50 p-6 rounded-3xl border border-gray-100 mt-6">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase">Họ tên thợ</p>
                  <p className="text-lg font-bold text-gray-900">{selectedKyc.workerName}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-400 font-bold uppercase">Số CCCD</p>
                  <p className="text-lg font-bold text-gray-900 tracking-widest">{selectedKyc.citizenIdNumber}</p>
                </div>
              </div>

              {selectedKyc.status === 'PENDING' ? (
                <>
                  <div className="mb-4">
                    <label className="block text-xs text-gray-400 font-bold uppercase mb-2">Lý do từ chối (nếu có)</label>
                    <Input.TextArea 
                      placeholder="Nhập lý do nếu bạn quyết định từ chối hồ sơ này..." 
                      value={rejectReason}
                      onChange={(e) => setRejectReason(e.target.value)}
                      rows={3}
                      className="rounded-2xl border-gray-200 focus:border-orange-500 focus:ring-orange-500/10"
                    />
                  </div>

                  <div className="flex gap-4">
                    <Button 
                      danger 
                      size="large"
                      disabled={actionLoading}
                      icon={<XCircle className="w-5 h-5" />}
                      onClick={() => handleReview('REJECTED')}
                      className="flex-1 h-14 rounded-2xl font-bold flex items-center justify-center gap-2"
                    >
                      Từ chối
                    </Button>
                    <Button 
                      type="primary" 
                      size="large"
                      loading={actionLoading}
                      className="flex-[2] h-14 bg-green-500 hover:bg-green-600 border-none rounded-2xl font-bold flex items-center justify-center gap-2"
                      icon={<CheckCircle className="w-5 h-5" />}
                      onClick={() => handleReview('APPROVED')}
                    >
                      Phê duyệt hồ sơ
                    </Button>
                  </div>
                </>
              ) : (
                <div className={clsx(
                  "p-4 rounded-2xl text-center font-bold",
                  selectedKyc.status === 'APPROVED' ? "bg-green-50 text-green-700" : "bg-red-50 text-red-700"
                )}>
                  Hồ sơ đã được {selectedKyc.status === 'APPROVED' ? 'DUYỆT' : 'TỪ CHỐI'} 
                  {selectedKyc.rejectionReason && <p className="text-sm font-medium mt-1">Lý do: {selectedKyc.rejectionReason}</p>}
                </div>
              )}
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
