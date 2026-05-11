import React from 'react';
import { X, Search, Filter } from 'lucide-react';
import { Modal, Select, Slider, InputNumber } from 'antd';

interface Service {
  id: string;
  name: string;
}

interface MarketplaceFiltersProps {
  open: boolean;
  onClose: () => void;
  services: Service[];
  filters: any;
  setFilters: (filters: any) => void;
  onApply: () => void;
}

const URGENCY_LEVELS = [
  { label: 'Bình thường', value: 'NORMAL' },
  { label: 'Gấp', value: 'URGENT' },
  { label: 'Rất gấp', value: 'CRITICAL' },
];

const SORT_OPTIONS = [
  { label: 'Mới nhất', value: 'latest' },
  { label: 'Gần nhất', value: 'nearest' },
  { label: 'Giá cao nhất', value: 'highest_budget' },
  { label: 'Ít báo giá nhất', value: 'least_offers' },
];

export default function MarketplaceFilters({ 
  open, 
  onClose, 
  services, 
  filters, 
  setFilters, 
  onApply 
}: MarketplaceFiltersProps) {
  
  return (
    <Modal
      title={<div className="flex items-center gap-2 font-bold"><Filter className="w-5 h-5" /> Bộ lọc tìm kiếm</div>}
      open={open}
      onCancel={onClose}
      footer={[
        <button 
          key="reset" 
          onClick={() => setFilters({ radius: 10, serviceIds: [], sort: 'latest', minBudget: undefined, maxBudget: undefined, urgencyLevel: undefined })}
          className="px-4 py-2 text-gray-500 font-semibold"
        >
          Thiết lập lại
        </button>,
        <button 
          key="apply" 
          onClick={() => { onApply(); onClose(); }}
          className="px-6 py-2 bg-orange-500 text-white font-bold rounded-xl hover:bg-orange-600"
        >
          Áp dụng
        </button>
      ]}
      width={400}
      centered
    >
      <div className="space-y-6 py-4">
        {/* Radius */}
        <div>
          <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Bán kính tìm kiếm (km)</label>
          <Slider 
            min={1} 
            max={50} 
            value={filters.radius} 
            onChange={(val) => setFilters({ ...filters, radius: val })}
            tooltip={{ formatter: (val) => `${val} km` }}
          />
        </div>

        {/* Services */}
        <div>
          <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Dịch vụ</label>
          <Select
            mode="multiple"
            style={{ width: '100%' }}
            placeholder="Chọn dịch vụ"
            value={filters.serviceIds}
            onChange={(val) => setFilters({ ...filters, serviceIds: val })}
            options={services.map(s => ({ label: s.name, value: s.id }))}
          />
        </div>

        {/* Budget */}
        <div>
          <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Ngân sách dự kiến (VNĐ)</label>
          <div className="flex items-center gap-2">
            <InputNumber 
              placeholder="Min"
              value={filters.minBudget}
              onChange={(val) => setFilters({ ...filters, minBudget: val })}
              formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\n))/g, ',')}
              className="flex-1"
            />
            <span className="text-gray-400">-</span>
            <InputNumber 
              placeholder="Max"
              value={filters.maxBudget}
              onChange={(val) => setFilters({ ...filters, maxBudget: val })}
              formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\n))/g, ',')}
              className="flex-1"
            />
          </div>
        </div>

        {/* Urgency */}
        <div>
          <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Mức độ ưu tiên</label>
          <Select
            style={{ width: '100%' }}
            placeholder="Tất cả"
            allowClear
            value={filters.urgencyLevel}
            onChange={(val) => setFilters({ ...filters, urgencyLevel: val })}
            options={URGENCY_LEVELS}
          />
        </div>

        {/* Sort */}
        <div>
          <label className="block text-xs font-bold text-gray-400 uppercase mb-2">Sắp xếp theo</label>
          <Select
            style={{ width: '100%' }}
            value={filters.sort}
            onChange={(val) => setFilters({ ...filters, sort: val })}
            options={SORT_OPTIONS}
          />
        </div>
      </div>
    </Modal>
  );
}
