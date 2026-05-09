import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { AlertTriangle, ChevronRight } from 'lucide-react';
import { message } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { clsx } from 'clsx';

interface Dispute {
  id: string;
  bookingId: string;
  customerId: string;
  customerName: string;
  workerId: string;
  workerName: string;
  reason: string;
  status: string;
  createdAt: string;
}

export default function DisputeManagement() {
  const navigate = useNavigate();
  const [disputes, setDisputes] = useState<Dispute[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDisputes();
  }, []);

  const fetchDisputes = async () => {
    try {
      setLoading(true);
      const res = await axiosInstance.get('/disputes/admin/all');
      setDisputes(res.data);
    } catch (err) {
      console.error(err);
      message.error('Không thể tải danh sách khiếu nại');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'OPEN': return 'bg-red-100 text-red-600';
      case 'INVESTIGATING': return 'bg-orange-100 text-orange-600';
      case 'RESOLVED': return 'bg-green-100 text-green-600';
      case 'REFUNDED': return 'bg-blue-100 text-blue-600';
      case 'CLOSED': return 'bg-gray-100 text-gray-600';
      default: return 'bg-gray-100 text-gray-600';
    }
  };

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
          <AlertTriangle className="w-8 h-8 text-red-500" /> Quản lý Khiếu nại
        </h1>
      </div>

      <div className="bg-white rounded-3xl shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-left text-sm">
          <thead className="bg-gray-50 border-b border-gray-200 text-gray-600">
            <tr>
              <th className="px-6 py-4 font-bold">Mã Khiếu nại</th>
              <th className="px-6 py-4 font-bold">Người khiếu nại</th>
              <th className="px-6 py-4 font-bold">Người bị khiếu nại</th>
              <th className="px-6 py-4 font-bold">Trạng thái</th>
              <th className="px-6 py-4 font-bold text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {loading ? (
              <tr>
                <td colSpan={5} className="px-6 py-10 text-center text-gray-400">
                  <span className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin inline-block"></span>
                </td>
              </tr>
            ) : disputes.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-10 text-center text-gray-500 font-medium">
                  Không có khiếu nại nào trong hệ thống
                </td>
              </tr>
            ) : (
              disputes.map((dispute) => (
                <tr key={dispute.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4">
                    <p className="font-bold text-gray-900 truncate max-w-[150px]">#{dispute.id.split('-')[0]}</p>
                    <p className="text-xs text-gray-500 italic truncate max-w-[200px] mt-0.5">{dispute.reason}</p>
                    <p className="text-[10px] text-gray-400 mt-1">{new Date(dispute.createdAt).toLocaleString('vi-VN')}</p>
                  </td>
                  <td className="px-6 py-4">
                    <p className="font-bold text-gray-800">{dispute.customerName}</p>
                    <p className="text-xs text-gray-500">#{dispute.customerId.split('-')[0]}</p>
                  </td>
                  <td className="px-6 py-4">
                    <p className="font-bold text-gray-800">{dispute.workerName}</p>
                    <p className="text-xs text-gray-500">#{dispute.workerId.split('-')[0]}</p>
                  </td>
                  <td className="px-6 py-4">
                    <span className={clsx("px-3 py-1 rounded-full text-xs font-bold", getStatusColor(dispute.status))}>
                      {dispute.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <button 
                      onClick={() => navigate(`/admin/disputes/${dispute.id}`)}
                      className="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold rounded-xl transition-colors inline-flex items-center gap-1"
                    >
                      Chi tiết <ChevronRight className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
