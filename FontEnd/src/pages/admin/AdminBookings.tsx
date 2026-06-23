import React, { useState, useEffect } from 'react';
import { Table, Input, Select, DatePicker, Button, Space, Tag, message } from 'antd';
import { Search, Filter, Calendar } from 'lucide-react';
import { adminApi, type GetBookingsQuery } from '../../api/admin';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;
const { Option } = Select;

export default function AdminBookings() {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);

  const [queryParams, setQueryParams] = useState<GetBookingsQuery>({
    pageIndex: 1,
    pageSize: 10,
    searchTerm: '',
    status: undefined,
    dateFrom: undefined,
    dateTo: undefined,
  });

  const fetchBookings = async (params: GetBookingsQuery) => {
    setLoading(true);
    try {
      const res = await adminApi.getBookings(params);
      setData(res.items);
      setTotal(res.totalCount);
    } catch (error) {
      message.error('Không thể tải danh sách đơn hàng');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBookings(queryParams);
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

  const handleStatusChange = (value: string) => {
    setQueryParams((prev) => ({
      ...prev,
      status: value,
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
      title: 'Mã Đơn',
      dataIndex: 'id',
      key: 'id',
      render: (text: string) => <span className="font-mono text-xs">{text.substring(0, 8)}...</span>,
    },
    {
      title: 'Dịch vụ',
      dataIndex: ['service', 'name'],
      key: 'service',
      render: (text: string) => <span className="font-medium">{text}</span>,
    },
    {
      title: 'Khách hàng',
      dataIndex: ['customer', 'fullName'],
      key: 'customer',
    },
    {
      title: 'Thợ',
      dataIndex: ['worker', 'fullName'],
      key: 'worker',
      render: (text: string) => text || <span className="text-gray-400 italic">Chưa có</span>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => {
        let color = 'default';
        let text = status;
        switch (status) {
          case 'PENDING': color = 'orange'; text = 'Chờ xử lý'; break;
          case 'MATCHING': color = 'cyan'; text = 'Đang tìm thợ'; break;
          case 'ASSIGNED': color = 'blue'; text = 'Đã nhận'; break;
          case 'ON_THE_WAY': color = 'purple'; text = 'Đang di chuyển'; break;
          case 'WORKING': color = 'geekblue'; text = 'Đang làm việc'; break;
          case 'COMPLETED': color = 'green'; text = 'Hoàn thành'; break;
          case 'CANCELLED': color = 'red'; text = 'Đã hủy'; break;
        }
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: 'Thanh toán',
      dataIndex: 'paymentStatus',
      key: 'paymentStatus',
      render: (status: string) => {
        let color = 'default';
        let text = status;
        if (status === 'PAID') { color = 'green'; text = 'Đã TT'; }
        if (status === 'UNPAID') { color = 'orange'; text = 'Chưa TT'; }
        if (status === 'REFUNDED') { color = 'red'; text = 'Hoàn tiền'; }
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => dayjs(date).format('DD/MM/YYYY HH:mm'),
    },
  ];

  return (
    <div className="p-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-1">Lịch sử Đơn hàng</h1>
        <p className="text-gray-500">Quản lý toàn bộ danh sách đơn hàng trên hệ thống.</p>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
        <div className="flex flex-wrap gap-4 mb-6">
          <Input.Search
            placeholder="Tìm theo Mã đơn, tên KH, tên Thợ..."
            onSearch={handleSearch}
            className="max-w-md"
            allowClear
            enterButton={<div className="flex items-center justify-center h-full px-2"><Search className="w-4 h-4" /></div>}
          />
          
          <Select
            placeholder="Chọn Trạng thái"
            style={{ width: 160 }}
            allowClear
            onChange={handleStatusChange}
          >
            <Option value="PENDING">Chờ xử lý</Option>
            <Option value="MATCHING">Đang tìm thợ</Option>
            <Option value="ASSIGNED">Đã nhận</Option>
            <Option value="ON_THE_WAY">Đang di chuyển</Option>
            <Option value="WORKING">Đang làm việc</Option>
            <Option value="COMPLETED">Hoàn thành</Option>
            <Option value="CANCELLED">Đã hủy</Option>
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
            showTotal: (total) => `Tổng số ${total} đơn hàng`,
          }}
          onChange={handleTableChange}
          scroll={{ x: 800 }}
        />
      </div>
    </div>
  );
}
