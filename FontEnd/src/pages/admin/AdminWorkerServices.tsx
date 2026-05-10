import { useState, useEffect } from 'react';
import { axiosInstance } from '../../api/axios';
import { CheckCircle, XCircle, Clock } from 'lucide-react';
import { message } from 'antd';

interface PendingService {
  workerId: string;
  workerName: string;
  workerEmail: string;
  serviceId: string;
  serviceName: string;
  status: string;
}

export default function AdminWorkerServices() {
  const [pendingServices, setPendingServices] = useState<PendingService[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchPendingServices = async () => {
    try {
      setLoading(true);
      const res = await axiosInstance.get('/admin/workers/services/pending');
      setPendingServices(res.data);
    } catch (error) {
      message.error('Không thể tải danh sách kỹ năng chờ duyệt.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPendingServices();
  }, []);

  const handleReview = async (workerId: string, serviceId: string, status: 'APPROVED' | 'REJECTED') => {
    try {
      setProcessing(`${workerId}-${serviceId}`);
      await axiosInstance.patch(`/admin/workers/${workerId}/services/${serviceId}`, {
        status: status
      });
      message.success(status === 'APPROVED' ? 'Đã phê duyệt kỹ năng!' : 'Đã từ chối kỹ năng!');
      setPendingServices(prev => prev.filter(s => !(s.workerId === workerId && s.serviceId === serviceId)));
    } catch (error) {
      message.error('Có lỗi xảy ra khi cập nhật.');
    } finally {
      setProcessing(null);
    }
  };

  return (
    <div className="p-8 max-w-7xl mx-auto">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Duyệt Kỹ Năng Thợ</h1>
        <p className="text-gray-500 mt-2">Xem và phê duyệt các kỹ năng mới được thợ đăng ký.</p>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-500">Đang tải dữ liệu...</div>
        ) : pendingServices.length === 0 ? (
          <div className="p-12 text-center">
            <CheckCircle className="w-16 h-16 text-green-200 mx-auto mb-4" />
            <h3 className="text-xl font-bold text-gray-900">Tất cả đã được xử lý!</h3>
            <p className="text-gray-500 mt-2">Hiện không có kỹ năng nào đang chờ duyệt.</p>
          </div>
        ) : (
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100">
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Thợ</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Kỹ năng đăng ký</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm">Trạng thái</th>
                <th className="px-6 py-4 font-semibold text-gray-600 text-sm text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {pendingServices.map((item) => (
                <tr key={`${item.workerId}-${item.serviceId}`} className="hover:bg-gray-50/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-bold text-gray-900">{item.workerName}</div>
                    <div className="text-sm text-gray-500">{item.workerEmail}</div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="inline-flex px-3 py-1 bg-blue-50 text-blue-700 rounded-full font-medium text-sm">
                      {item.serviceName}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="inline-flex items-center gap-1.5 text-orange-600 bg-orange-50 px-2.5 py-1 rounded-md text-sm font-medium">
                      <Clock className="w-4 h-4" /> Đang chờ
                    </div>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => handleReview(item.workerId, item.serviceId, 'APPROVED')}
                        disabled={processing === `${item.workerId}-${item.serviceId}`}
                        className="p-2 text-green-600 hover:bg-green-50 rounded-lg transition-colors disabled:opacity-50"
                        title="Phê duyệt"
                      >
                        <CheckCircle className="w-6 h-6" />
                      </button>
                      <button
                        onClick={() => handleReview(item.workerId, item.serviceId, 'REJECTED')}
                        disabled={processing === `${item.workerId}-${item.serviceId}`}
                        className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"
                        title="Từ chối"
                      >
                        <XCircle className="w-6 h-6" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
