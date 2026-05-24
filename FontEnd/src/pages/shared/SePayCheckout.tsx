import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, CheckCircle2, Copy, ScanLine, AlertCircle } from 'lucide-react';
import { message } from 'antd';

export default function SePayCheckout() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  
  const paymentId = searchParams.get('paymentId');
  const amountStr = searchParams.get('amount');
  const des = searchParams.get('des');
  
  // Lấy cấu hình từ Biến môi trường (Environment Variables)
  const bankAccount = import.meta.env.VITE_SEPAY_ACCOUNT_NUMBER || "0927319622";
  const bankName = import.meta.env.VITE_SEPAY_BANK_NAME || "MBBank";
  
  const [timeLeft, setTimeLeft] = useState(15 * 60); // 15 minutes

  useEffect(() => {
    if (!paymentId || !amountStr || !des) {
      message.error('Thông tin thanh toán không hợp lệ');
      navigate(-1);
    }
  }, [paymentId, amountStr, des, navigate]);

  useEffect(() => {
    const timer = setInterval(() => {
      setTimeLeft(prev => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, []);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  const amount = parseInt(amountStr || '0');
  
  // SePay VietQR Image URL
  const qrUrl = `https://qr.sepay.vn/img?acc=${bankAccount}&bank=${bankName}&amount=${amount}&des=${des}`;

  const copyToClipboard = (text: string, label: string) => {
    navigator.clipboard.writeText(text);
    message.success(`Đã sao chép ${label}`);
  };

  const handlePaid = () => {
    // We redirect to the result page with success=true and provider=sepay.
    // In a real app, the result page should poll the backend to confirm if the webhook arrived.
    navigate(`/payment/result?success=true&provider=sepay&paymentId=${paymentId}`);
  };

  if (!paymentId) return null;

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      <div className="bg-blue-600 px-6 pt-10 pb-24 rounded-b-[40px] shadow-lg relative z-0">
        <div className="flex items-center justify-between mb-6">
          <button onClick={() => navigate(-1)} className="p-2 -ml-2 text-white hover:bg-blue-700 rounded-full transition-all">
            <ArrowLeft className="w-6 h-6" />
          </button>
          <div className="bg-white/20 text-white px-3 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 backdrop-blur-sm">
            <ClockIcon className="w-4 h-4" />
            Đơn hàng hết hạn sau: <span className="font-bold">{formatTime(timeLeft)}</span>
          </div>
        </div>
        
        <div className="text-center text-white">
          <p className="text-blue-100 mb-1">Số tiền thanh toán</p>
          <h2 className="text-4xl font-black mb-2 tracking-tight">
            {amount.toLocaleString('vi-VN')} <span className="text-2xl">đ</span>
          </h2>
        </div>
      </div>

      <div className="px-6 -mt-16 relative z-10 max-w-md mx-auto">
        <div className="bg-white rounded-3xl p-6 shadow-xl shadow-blue-900/5">
          <div className="text-center mb-6">
            <div className="w-16 h-16 bg-blue-50 rounded-2xl flex items-center justify-center mx-auto mb-4 text-blue-600">
              <ScanLine className="w-8 h-8" />
            </div>
            <h3 className="font-bold text-gray-900 text-lg">Quét mã QR để thanh toán</h3>
            <p className="text-sm text-gray-500 mt-1">Sử dụng ứng dụng ngân hàng hoặc ví điện tử</p>
          </div>

          <div className="bg-gray-50 p-4 rounded-2xl flex justify-center mb-6 border border-gray-100">
            {timeLeft > 0 ? (
              <img src={qrUrl} alt="VietQR" className="w-64 h-64 object-contain rounded-xl mix-blend-multiply" />
            ) : (
              <div className="w-64 h-64 flex flex-col items-center justify-center text-gray-400">
                <AlertCircle className="w-12 h-12 mb-2" />
                <p>Mã QR đã hết hạn</p>
              </div>
            )}
          </div>

          <div className="space-y-4">
            <div className="flex justify-between items-center p-3 bg-gray-50 rounded-xl border border-gray-100">
              <div>
                <p className="text-xs text-gray-500 font-medium">Ngân hàng</p>
                <p className="font-bold text-gray-900">{bankName}</p>
              </div>
            </div>
            <div className="flex justify-between items-center p-3 bg-gray-50 rounded-xl border border-gray-100">
              <div>
                <p className="text-xs text-gray-500 font-medium">Số tài khoản</p>
                <p className="font-bold text-gray-900">{bankAccount}</p>
              </div>
              <button onClick={() => copyToClipboard(bankAccount, 'số tài khoản')} className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-all">
                <Copy className="w-4 h-4" />
              </button>
            </div>
            <div className="flex justify-between items-center p-3 bg-blue-50 rounded-xl border border-blue-100">
              <div>
                <p className="text-xs text-blue-600 font-medium">Nội dung chuyển khoản</p>
                <p className="font-black text-blue-700">{des}</p>
              </div>
              <button onClick={() => copyToClipboard(des || '', 'nội dung')} className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all">
                <Copy className="w-4 h-4" />
              </button>
            </div>
          </div>

          <div className="mt-6 pt-6 border-t border-gray-100">
            <p className="text-xs text-gray-500 text-center mb-4">
              Hệ thống sẽ tự động xác nhận sau khi nhận được tiền. Nếu bạn đã chuyển khoản, bạn có thể bấm nút bên dưới.
            </p>
            <button
              onClick={handlePaid}
              className="w-full py-4 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-2xl shadow-lg shadow-blue-600/20 flex items-center justify-center gap-2 transition-all"
            >
              <CheckCircle2 className="w-5 h-5" /> Tôi đã thanh toán
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function ClockIcon(props: any) {
  return (
    <svg {...props} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="12" cy="12" r="10"/>
      <polyline points="12 6 12 12 16 14"/>
    </svg>
  );
}
