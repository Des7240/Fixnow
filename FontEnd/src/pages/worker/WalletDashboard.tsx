import { useState, useEffect } from 'react';
import { ArrowLeft, Wallet, ArrowDownCircle, ArrowUpCircle, History, Landmark } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { message, Modal, Input, Radio } from 'antd';
import axiosInstance from '../../utils/axiosInstance';
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
  const [bankInfo, setBankInfo] = useState('');
  const [withdrawing, setWithdrawing] = useState(false);

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

  const handleWithdraw = async () => {
    const amount = parseInt(withdrawAmount);
    if (isNaN(amount) || amount < 50000) {
      message.error('Số tiền rút tối thiểu là 50,000đ');
      return;
    }
    if (amount > (wallet?.balance || 0)) {
      message.error('Số dư không đủ');
      return;
    }
    if (!bankInfo.trim()) {
      message.error('Vui lòng nhập thông tin ngân hàng');
      return;
    }

    setWithdrawing(true);
    try {
      await axiosInstance.post('/wallet/withdraw', {
        amount,
        bankAccountInfo: bankInfo
      });
      message.success('Đã gửi yêu cầu rút tiền thành công!');
      setIsWithdrawOpen(false);
      setWithdrawAmount('');
      setBankInfo('');
      fetchWallet(); // Reload balance
    } catch (err: any) {
      message.error(err.response?.data?.message || 'Lỗi khi rút tiền');
    } finally {
      setWithdrawing(false);
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
        <div className="py-4 space-y-5">
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
            <label className="block text-xs font-bold text-gray-500 uppercase mb-2">Thông tin tài khoản nhận</label>
            <Input.TextArea
              rows={3}
              value={bankInfo}
              onChange={e => setBankInfo(e.target.value)}
              placeholder="VD: Vietcombank - 0123456789 - NGUYEN VAN A"
              className="py-3 px-4 rounded-xl"
            />
          </div>

          <button
            onClick={handleWithdraw}
            disabled={withdrawing}
            className="w-full py-4 bg-orange-500 text-white font-bold rounded-2xl shadow-lg shadow-orange-500/20 disabled:opacity-50 flex justify-center items-center"
          >
            {withdrawing ? <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span> : 'Xác nhận rút tiền'}
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
