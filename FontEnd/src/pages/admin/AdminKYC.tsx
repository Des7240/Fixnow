import { useState, useEffect } from 'react';
import { Table, Tag, Button, Modal, message, Input } from 'antd';
import { Eye, CheckCircle, XCircle } from 'lucide-react';

export default function AdminKYC() {
  const [kycs, setKycs] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [selectedKyc, setSelectedKyc] = useState<any>(null);
  const [rejectReason, setRejectReason] = useState('');

  // Lấy dữ liệu KYC giả lập cho MVP vì backend có thể chưa có API GET All KYC
  // Nếu có API getAllKycs thì gọi ở đây
  useEffect(() => {
    // Fake data for UI preview since there might not be a 'Get All KYC' endpoint yet
    setKycs([
      {
        id: '1',
        workerName: 'Nguyễn Văn Thợ',
        citizenIdNumber: '001099123456',
        status: 'PENDING',
        submittedAt: new Date().toISOString(),
        frontUrl: 'https://via.placeholder.com/300x200?text=CMND+Front',
        backUrl: 'https://via.placeholder.com/300x200?text=CMND+Back',
        selfieUrl: 'https://via.placeholder.com/200x200?text=Selfie',
      }
    ]);
    setLoading(false);
  }, []);

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

    try {
      // API call to update KYC
      // await axiosInstance.patch(`/admin/kyc/${selectedKyc.id}`, { status, reason: rejectReason });
      
      message.success(`Đã ${status === 'APPROVED' ? 'duyệt' : 'từ chối'} hồ sơ`);
      
      // Update local state
      setKycs(prev => prev.map(k => k.id === selectedKyc.id ? { ...k, status } : k));
      setIsModalVisible(false);
    } catch (err) {
      message.error('Có lỗi xảy ra');
    }
  };

  const columns = [
    {
      title: 'Họ tên thợ',
      dataIndex: 'workerName',
      key: 'workerName',
      fontWeight: 'bold',
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
        <Button type="text" icon={<Eye className="w-4 h-4 text-blue-500" />} onClick={() => showDetail(record)}>
          Xem chi tiết
        </Button>
      ),
    },
  ];

  return (
    <div className="p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Xét duyệt KYC</h1>
        <p className="text-gray-500">Quản lý và duyệt hồ sơ đăng ký của thợ</p>
      </div>

      <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
        <Table 
          columns={columns} 
          dataSource={kycs} 
          rowKey="id" 
          loading={loading}
          pagination={{ pageSize: 10 }}
        />
      </div>

      <Modal
        title="Chi tiết hồ sơ KYC"
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        footer={null}
        width={700}
      >
        {selectedKyc && (
          <div className="space-y-6 pt-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-gray-500 text-sm mb-1">Mặt trước CCCD</p>
                <img src={selectedKyc.frontUrl} alt="Front" className="w-full rounded-xl object-cover h-40 bg-gray-100" />
              </div>
              <div>
                <p className="text-gray-500 text-sm mb-1">Mặt sau CCCD</p>
                <img src={selectedKyc.backUrl} alt="Back" className="w-full rounded-xl object-cover h-40 bg-gray-100" />
              </div>
              <div className="col-span-2">
                <p className="text-gray-500 text-sm mb-1">Ảnh chân dung</p>
                <img src={selectedKyc.selfieUrl} alt="Selfie" className="w-40 h-40 rounded-xl object-cover bg-gray-100 mx-auto" />
              </div>
            </div>

            {selectedKyc.status === 'PENDING' && (
              <div className="bg-gray-50 p-4 rounded-xl mt-6 border border-gray-200">
                <p className="font-semibold text-gray-900 mb-3">Phê duyệt hồ sơ</p>
                
                <div className="mb-4">
                  <Input.TextArea 
                    placeholder="Lý do từ chối (bắt buộc nếu từ chối)..." 
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                    rows={2}
                  />
                </div>

                <div className="flex gap-3 justify-end">
                  <Button 
                    danger 
                    icon={<XCircle className="w-4 h-4" />}
                    onClick={() => handleReview('REJECTED')}
                    className="flex items-center"
                  >
                    Từ chối
                  </Button>
                  <Button 
                    type="primary" 
                    className="bg-green-500 hover:bg-green-600 flex items-center"
                    icon={<CheckCircle className="w-4 h-4" />}
                    onClick={() => handleReview('APPROVED')}
                  >
                    Duyệt hồ sơ
                  </Button>
                </div>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}
