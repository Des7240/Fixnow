import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { CheckCircle, XCircle } from 'lucide-react';
import { useAuthStore } from '../../stores/authStore';

export default function PaymentReturn() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [status, setStatus] = useState<'LOADING' | 'SUCCESS' | 'FAILED'>('LOADING');

  useEffect(() => {
    const success = searchParams.get('success');
    if (success === 'True' || success === 'true') {
      setStatus('SUCCESS');
    } else {
      setStatus('FAILED');
    }
  }, [searchParams]);

  const handleBack = () => {
    if (user?.role === 'WORKER') {
      // If it's a worker, usually they were in the Wallet or Dashboard
      navigate('/worker/wallet');
    } else {
      // If customer
      navigate('/customer/bookings');
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
      <div className="bg-white rounded-3xl p-8 max-w-sm w-full shadow-lg text-center">
        {status === 'LOADING' && (
          <div className="flex flex-col items-center">
            <span className="w-16 h-16 border-4 border-orange-500 border-t-transparent rounded-full animate-spin mb-4"></span>
            <h2 className="text-xl font-bold text-gray-900">Đang xử lý kết quả...</h2>
          </div>
        )}

        {status === 'SUCCESS' && (
          <div className="flex flex-col items-center">
            <div className="w-20 h-20 bg-green-100 text-green-500 rounded-full flex items-center justify-center mb-6">
              <CheckCircle className="w-10 h-10" />
            </div>
            <h2 className="text-2xl font-black text-gray-900 mb-2">Thanh toán thành công!</h2>
            <p className="text-gray-500 mb-8">
              Giao dịch của bạn đã được xử lý thành công. Cảm ơn bạn đã sử dụng dịch vụ!
            </p>
            <button 
              onClick={handleBack}
              className="w-full py-4 bg-gray-900 hover:bg-black text-white font-bold rounded-2xl shadow-xl shadow-gray-900/20 transition-all"
            >
              {user?.role === 'WORKER' ? 'Về ví thu nhập' : 'Về danh sách đơn hàng'}
            </button>
          </div>
        )}

        {status === 'FAILED' && (
          <div className="flex flex-col items-center">
            <div className="w-20 h-20 bg-red-100 text-red-500 rounded-full flex items-center justify-center mb-6">
              <XCircle className="w-10 h-10" />
            </div>
            <h2 className="text-2xl font-black text-gray-900 mb-2">Thanh toán thất bại</h2>
            <p className="text-gray-500 mb-8">
              Đã có lỗi xảy ra trong quá trình thanh toán hoặc bạn đã hủy giao dịch.
            </p>
            <button 
              onClick={handleBack}
              className="w-full py-4 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-2xl shadow-xl shadow-orange-500/20 transition-all"
            >
              Quay lại
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
