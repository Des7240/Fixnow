import React, { useState, useEffect } from 'react';
import { Table, Input, Select, DatePicker, Tag, message } from 'antd';
import { Search } from 'lucide-react';
import { adminApi, type GetTransactionsQuery } from '../../api/admin';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;
const { Option } = Select;

export default function AdminTransactions() {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);

  const [queryParams, setQueryParams] = useState<GetTransactionsQuery>({
    pageIndex: 1,
    pageSize: 10,
    searchTerm: '',
    type: undefined,
    dateFrom: undefined,
    dateTo: undefined,
  });

  const fetchTransactions = async (params: GetTransactionsQuery) => {
    setLoading(true);
    try {
      const res = await adminApi.getTransactions(params);
      setData(res.items);
      setTotal(res.totalCount);
    } catch (error) {
      message.error('Không thể tải lịch sử giao dịch');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransactions(queryParams);
  }, [queryParams]);

  const handleTableChange = (pagination: any) => {
    setQueryParams((prev) => ({
      ...prev,
      pageIndex: pagination.current,
      pageSize: pagination.pageSize,
    }));
  };

  const handleSearch = (value: string) => {
    setQueryParams((prev) => ({
      ...prev,
      searchTerm: value,
      pageIndex: 1,
    }));
  };

  const handleTypeChange = (value: string) => {
    setQueryParams((prev) => ({
      ...prev,
      type: value,
      pageIndex: 1,
    }));
  };

  const handleDateChange = (dates: any) => {
    if (dates && dates.length === 2) {
      setQueryParams((prev) => ({
        ...prev,
        dateFrom: dates[0].toISOString(),
        dateTo: dates[1].toISOString(),
        pageIndex: 1,
      }));
    } else {
      setQueryParams((prev) => ({
        ...prev,
        dateFrom: undefined,
        dateTo: undefined,
        pageIndex: 1,
      }));
    }
  };

  const columns = [
    {
      title: 'Mã Giao Dịch',
      dataIndex: 'transactionCode',
      key: 'transactionCode',
      render: (text: string, record: any) => (
        <span className="font-mono text-xs">
          {text || record.id.substring(0, 8) + '...'}
        </span>
      ),
    },
    {
      title: 'Khách hàng / Thợ',
      key: 'customer',
      render: (_: any, record: any) => (
        <div>
          <div className="font-medium">{record.customerName}</div>
          <div className="text-xs text-gray-500">{record.customerEmail}</div>
        </div>
      ),
    },
    {
      title: 'Loại',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => {
        let color = 'default';
        let text = type;
        if (type === 'BOOKING') { color = 'blue'; text = 'Thanh toán Đơn'; }
        if (type === 'WALLET_DEPOSIT') { color = 'green'; text = 'Nạp ví'; }
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: 'Số tiền',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount: number) => (
        <span className="font-semibold text-green-600">
          {amount.toLocaleString('vi-VN')} ₫
        </span>
      ),
    },
    {
      title: 'Cổng thanh toán',
      dataIndex: 'provider',
      key: 'provider',
      render: (provider: string) => <Tag color="purple">{provider}</Tag>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => {
        let color = 'default';
        let text = status;
        if (status === 'SUCCESS') { color = 'green'; text = 'Thành công'; }
        if (status === 'PENDING') { color = 'orange'; text = 'Chờ xử lý'; }
        if (status === 'FAILED') { color = 'red'; text = 'Thất bại'; }
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: 'Thời gian',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => dayjs(date).format('DD/MM/YYYY HH:mm:ss'),
    },
  ];

  return (
    <div className="p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-1">Lịch sử Giao dịch</h1>
        <p className="text-gray-500">Quản lý toàn bộ các giao dịch thanh toán trên hệ thống.</p>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
        <div className="flex flex-wrap gap-4 mb-6">
          <Input.Search
            placeholder="Tìm mã GD, mã KH..."
            onSearch={handleSearch}
            className="max-w-md"
            allowClear
            enterButton={<div className="flex items-center justify-center h-full px-2"><Search className="w-4 h-4" /></div>}
          />
          
          <Select
            placeholder="Loại giao dịch"
            style={{ width: 160 }}
            allowClear
            onChange={handleTypeChange}
          >
            <Option value="BOOKING">Thanh toán Đơn</Option>
            <Option value="WALLET_DEPOSIT">Nạp ví</Option>
          </Select>

          <RangePicker 
            onChange={handleDateChange} 
            format="DD/MM/YYYY"
            placeholder={['Từ ngày', 'Đến ngày']}
          />
        </div>

        <Table
          columns={columns}
          dataSource={data}
          rowKey="id"
          loading={loading}
          pagination={{
            current: queryParams.pageIndex,
            pageSize: queryParams.pageSize,
            total: total,
            showSizeChanger: true,
            showTotal: (total) => `Tổng số ${total} giao dịch`,
          }}
          onChange={handleTableChange}
          scroll={{ x: 800 }}
        />
      </div>
    </div>
  );
}
