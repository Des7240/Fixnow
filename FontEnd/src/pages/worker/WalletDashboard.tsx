import { useState, useEffect } from 'react';
import { ArrowLeft, Wallet, ArrowDownCircle, ArrowUpCircle, History, Landmark, ShieldCheck } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { message, Modal, Input, Radio } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
import { walletApi } from '../../modules/wallet/walletApi';
import { clsx } from 'clsx';

interface WalletData {
  id: string;
  balance: number;
}

interface Transaction {
  id: string;
  type: 'BOOKING_INCOME' | 'COMMISSION_FEE' | 'WITHDRAWAL' | 'REFUND' | 'DEPOSIT' | 'ADJUSTMENT';
  amount: number;
  createdAt: string;
  description: string;
}

export default function WalletDashboard() {
  const navigate = useNavigate();
  const [wallet, setWallet] = useState<WalletData | null>(null);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);

  // Withdraw Modal
  const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);
  const [withdrawAmount, setWithdrawAmount] = useState('');
  const [bankName, setBankName] = useState('');
  const [accountNumber, setAccountNumber] = useState('');
  const [accountName, setAccountName] = useState('');
  const [withdrawing, setWithdrawing] = useState(false);

  // OTP Modal for Withdrawal
  const [isOtpOpen, setIsOtpOpen] = useState(false);
  const [otpCode, setOtpCode] = useState('');
  const [confirming, setConfirming] = useState(false);

  // Deposit Modal
  const [isDepositOpen, setIsDepositOpen] = useState(false);
  const [depositAmount, setDepositAmount] = useState('');
  const [selectedProvider, setSelectedProvider] = useState<'VNPAY' | 'MOMO'>('VNPAY');
  const [depositing, setDepositing] = useState(false);

  useEffect(() => {
    fetchWallet();
  }, []);

  const fetchWallet = async () => {
    try {
      setLoading(true);
      const res = await axiosInstance.get('/wallet');
      setWallet(res.data);
      
      const txRes = await axiosInstance.get('/wallet/transactions');
      setTransactions(txRes.data);
    } catch (err) {
      console.error(err);
      message.error('Không thể tải thông tin ví');
    } finally {
      setLoading(false);
    }
  };

  const handleDeposit = async () => {
    const amount = parseInt(depositAmount);
    if (isNaN(amount) || amount < 10000) {
      message.error('Số tiền nạp tối thiểu là 10,000đ');
      return;
    }

    setDepositing(true);
    try {
      const res = await axiosInstance.post('/wallet/deposit', {
        amount,
        provider: selectedProvider
      });
      
      if (res.data?.paymentUrl) {
        message.loading('Đang chuyển hướng thanh toán...');
        window.location.href = res.data.paymentUrl;
      } else {
        message.error('Không tạo được yêu cầu nạp tiền');
      }
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi nạp tiền');
    } finally {
      setDepositing(false);
    }
  };

  const handleWithdrawInitiate = async () => {
    const amount = parseInt(withdrawAmount);
    if (isNaN(amount) || amount < 50000) {
      message.error('Số tiền rút tối thiểu là 50,000đ');
      return;
    }
    if (amount > (wallet?.balance || 0)) {
      message.error('Số dư không đủ');
      return;
    }
    if (!bankName || !accountNumber || !accountName) {
      message.error('Vui lòng nhập đầy đủ thông tin tài khoản');
      return;
    }

    setWithdrawing(true);
    try {
      await walletApi.initiateWithdraw({
        amount,
        bankName,
        accountNumber,
        accountName
      });
      message.success('Mã OTP đã được gửi về email của bạn');
      setIsWithdrawOpen(false);
      setIsOtpOpen(true);
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi gửi yêu cầu');
    } finally {
      setWithdrawing(false);
    }
  };

  const handleWithdrawConfirm = async () => {
    if (otpCode.length !== 6) {
      message.error('Vui lòng nhập mã OTP 6 chữ số');
      return;
    }

    setConfirming(true);
    try {
      await walletApi.confirmWithdraw({
        amount: parseInt(withdrawAmount),
        bankName,
        accountNumber,
        accountName,
        otpCode
      });
      message.success('Yêu cầu rút tiền đã được xác nhận!');
      setIsOtpOpen(false);
      setWithdrawAmount('');
      setBankName('');
      setAccountNumber('');
      setAccountName('');
      setOtpCode('');
      fetchWallet();
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Mã OTP không chính xác hoặc đã hết hạn');
    } finally {
      setConfirming(false);
    }
  };

  const getTxIcon = (type: string) => {
    switch (type) {
      case 'BOOKING_INCOME':
      case 'REFUND':
      case 'DEPOSIT':
        return <ArrowDownCircle className="w-5 h-5 text-green-500" />;
      case 'COMMISSION_FEE':
      case 'WITHDRAWAL':
        return <ArrowUpCircle className="w-5 h-5 text-red-500" />;
      default:
        return <History className="w-5 h-5 text-gray-500" />;
    }
  };

  const getTxSign = (amount: number) => {
    return amount > 0 ? '+' : '';
  };

  const getTxColor = (amount: number) => {
    return amount > 0 ? 'text-green-600' : 'text-red-600';
  };

  const translateType = (type: string) => {
    switch (type) {
      case 'BOOKING_INCOME': return 'Thu nhập đơn hàng';
      case 'COMMISSION_FEE': return 'Phí hệ thống';
      case 'WITHDRAWAL': return 'Rút tiền';
      case 'REFUND': return 'Hoàn tiền';
      case 'DEPOSIT': return 'Nạp tiền vào ví';
      case 'ADJUSTMENT': return 'Điều chỉnh';
      default: return type;
    }
  };

  if (loading && !wallet) {
    return (
      <div className="h-screen flex items-center justify-center bg-white">
        <span className="w-10 h-10 border-4 border-orange-500 border-t-transparent rounded-full animate-spin"></span>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      {/* Header */}
      <div className="bg-orange-500 px-6 pt-10 pb-20 text-white rounded-b-[40px] shadow-lg relative z-0">
        <div className="flex items-center gap-4 mb-8">
          <button onClick={() => navigate(-1)} className="p-2 -ml-2 rounded-full hover:bg-orange-600 transition-all">
            <ArrowLeft className="w-6 h-6" />
          </button>
          <h1 className="text-xl font-bold">Ví thu nhập</h1>
        </div>
        
        <div className="text-center">
          <p className="text-orange-100 font-medium mb-1">Số dư khả dụng</p>
          <h2 className="text-4xl font-black mb-2 tracking-tight">
            {(wallet?.balance || 0).toLocaleString('vi-VN')} <span className="text-2xl font-bold">đ</span>
          </h2>
        </div>
      </div>

      {/* Floating Action Buttons */}
      <div className="px-6 -mt-8 relative z-10">
        <div className="bg-white rounded-3xl p-4 shadow-xl shadow-gray-200/50 flex justify-around">
          <button 
            onClick={() => setIsWithdrawOpen(true)}
            className="flex flex-col items-center gap-2 group"
          >
            <div className="w-12 h-12 bg-orange-50 rounded-full flex items-center justify-center group-hover:bg-orange-100 transition-all">
              <Landmark className="w-6 h-6 text-orange-600" />
            </div>
            <span className="text-xs font-bold text-gray-700">Rút tiền</span>
          </button>
          
          <button 
            onClick={() => setIsDepositOpen(true)}
            className="flex flex-col items-center gap-2 group"
          >
            <div className="w-12 h-12 bg-orange-50 rounded-full flex items-center justify-center group-hover:bg-orange-100 transition-all">
              <Wallet className="w-6 h-6 text-orange-600" />
            </div>
            <span className="text-xs font-bold text-gray-700">Nạp tiền</span>
          </button>
        </div>
      </div>

      {/* Transaction History */}
      <div className="px-4 mt-8">
        <h3 className="text-lg font-bold text-gray-900 mb-4 ml-2">Lịch sử giao dịch</h3>
        <div className="space-y-3">
          {transactions.length === 0 ? (
            <div className="text-center py-10 bg-white rounded-3xl border border-gray-100">
              <History className="w-10 h-10 text-gray-300 mx-auto mb-3" />
              <p className="text-gray-400 text-sm">Chưa có giao dịch nào</p>
            </div>
          ) : (
            transactions.map((tx) => (
              <div key={tx.id} className="bg-white p-4 rounded-3xl shadow-sm border border-gray-50 flex items-center gap-4">
                <div className={clsx(
                  "w-12 h-12 rounded-2xl flex items-center justify-center flex-shrink-0",
                  tx.amount > 0 ? 'bg-green-50' : 'bg-red-50'
                )}>
                  {getTxIcon(tx.type)}
                </div>
                <div className="flex-1">
                  <p className="font-bold text-gray-900 text-sm mb-0.5 line-clamp-1">{tx.description || translateType(tx.type)}</p>
                  <p className="text-xs text-gray-400 font-medium">
                    {new Date(tx.createdAt).toLocaleString('vi-VN')}
                  </p>
                </div>
                <div className="text-right">
                  <p className={clsx("font-black text-sm", getTxColor(tx.amount))}>
                    {getTxSign(tx.amount)}{(tx.amount).toLocaleString('vi-VN')}đ
                  </p>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Withdraw Modal */}
      <Modal
        title="Rút tiền về Ngân hàng"
        open={isWithdrawOpen}
        onCancel={() => setIsWithdrawOpen(false)}
        footer={null}
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-2 space-y-4">
          <div className="bg-orange-50 p-4 rounded-2xl text-center">
            <p className="text-xs font-bold text-orange-600 uppercase mb-1">Số dư hiện tại</p>
            <p className="text-2xl font-black text-orange-700">{(wallet?.balance || 0).toLocaleString('vi-VN')} đ</p>
          </div>

          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Số tiền muốn rút (VND)</label>
            <Input
              type="number"
              value={withdrawAmount}
              onChange={e => setWithdrawAmount(e.target.value)}
              placeholder="Tối thiểu 50,000đ"
              className="py-3 px-4 rounded-xl font-bold text-lg"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Tên ngân hàng</label>
            <Input
              value={bankName}
              onChange={e => setBankName(e.target.value)}
              placeholder="VD: Vietcombank, Techcombank..."
              className="py-3 px-4 rounded-xl"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Số tài khoản</label>
              <Input
                value={accountNumber}
                onChange={e => setAccountNumber(e.target.value)}
                placeholder="123456789..."
                className="py-3 px-4 rounded-xl"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Tên chủ tài khoản</label>
              <Input
                value={accountName}
                onChange={e => setAccountName(e.target.value.toUpperCase())}
                placeholder="NGUYEN VAN A"
                className="py-3 px-4 rounded-xl"
              />
            </div>
          </div>

          <button
            onClick={handleWithdrawInitiate}
            disabled={withdrawing}
            className="w-full py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20 disabled:opacity-50 flex justify-center items-center gap-2"
          >
            {withdrawing && <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>}
            Gửi yêu cầu xác thực
          </button>
        </div>
      </Modal>

      {/* OTP Verification Modal */}
      <Modal
        title="Xác thực Rút tiền"
        open={isOtpOpen}
        onCancel={() => setIsOtpOpen(false)}
        footer={null}
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-4 space-y-6 text-center">
          <div className="mx-auto w-16 h-16 bg-orange-50 rounded-full flex items-center justify-center">
            <ShieldCheck className="w-8 h-8 text-orange-500" />
          </div>
          
          <div>
            <p className="text-gray-600 mb-1">Chúng tôi đã gửi mã OTP đến email của bạn.</p>
            <p className="text-sm text-gray-400">Vui lòng kiểm tra hộp thư để lấy mã xác nhận.</p>
          </div>

          <Input
            value={otpCode}
            onChange={e => setOtpCode(e.target.value)}
            maxLength={6}
            placeholder="Mã OTP 6 chữ số"
            className="py-4 rounded-xl text-center text-2xl font-black tracking-[0.5em] focus:ring-orange-500"
          />

          <button
            onClick={handleWithdrawConfirm}
            disabled={confirming || otpCode.length !== 6}
            className="w-full py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20 disabled:opacity-50 flex justify-center items-center gap-2 transition-all"
          >
            {confirming && <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>}
            Xác nhận rút tiền
          </button>
          
          <button 
            onClick={() => {
                setIsOtpOpen(false);
                setIsWithdrawOpen(true);
            }}
            className="text-sm text-gray-500 hover:text-orange-500 font-medium"
          >
            Quay lại chỉnh sửa thông tin
          </button>
        </div>
      </Modal>

      {/* Deposit Modal */}
      <Modal
        title="Nạp tiền vào ví"
        open={isDepositOpen}
        onCancel={() => setIsDepositOpen(false)}
        footer={null}
        centered
        className="rounded-3xl overflow-hidden"
      >
        <div className="py-4 space-y-5">
          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Số tiền muốn nạp (VND)</label>
            <Input
              type="number"
              value={depositAmount}
              onChange={e => setDepositAmount(e.target.value)}
              placeholder="Tối thiểu 10,000đ"
              className="py-3 px-4 rounded-xl font-bold text-lg"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Phương thức thanh toán</label>
            <Radio.Group 
              onChange={e => setSelectedProvider(e.target.value)} 
              value={selectedProvider}
              className="w-full"
            >
              <div className="grid grid-cols-1 gap-2">
                <Radio.Button value="VNPAY" className="h-auto py-3 rounded-xl flex items-center gap-3">
                   VNPay
                </Radio.Button>
                <Radio.Button value="MOMO" className="h-auto py-3 rounded-xl flex items-center gap-3">
                   Ví MoMo
                </Radio.Button>
              </div>
            </Radio.Group>
          </div>

          <button
            onClick={handleDeposit}
            disabled={depositing}
            className="w-full py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20 disabled:opacity-50 flex justify-center items-center"
          >
            {depositing ? <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span> : 'Nạp tiền ngay'}
          </button>
        </div>
      </Modal>
    </div>
  );
}
