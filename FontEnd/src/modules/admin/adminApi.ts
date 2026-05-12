import axiosInstance from '../../utils/axiosInstance';

export interface SystemConfig {
  configKey: string;
  configValue: string;
  description: string;
}

export interface ServiceCommission {
  id: string;
  serviceId: string;
  service: {
    name: string;
  };
  commissionPercent: number;
}

export interface CreateServiceRequest {
  name: string;
  description?: string;
  iconUrl?: string;
  basePrice: number;
  estimatedDurationMinutes: number;
}

export const adminApi = {
  // Configs
  getConfigs: () =>
    axiosInstance.get<SystemConfig[]>('/admin/system-configs'),

  updateConfig: (data: { key: string; value: string }) =>
    axiosInstance.put('/admin/system-configs', data),

  // Commissions
  getCommissions: () =>
    axiosInstance.get<ServiceCommission[]>('/admin/service-commissions'),

  updateCommission: (data: { serviceId: string; percent: number }) =>
    axiosInstance.put('/admin/service-commissions', data),

  // Services
  getServices: () =>
    axiosInstance.get('/services'),

  createService: (data: CreateServiceRequest) =>
    axiosInstance.post('/admin/services', data),

  updateService: (id: string, data: any) =>
    axiosInstance.put(`/admin/services/${id}`, data),

  deleteService: (id: string) =>
    axiosInstance.delete(`/admin/services/${id}`),

  // Audit Logs
  getAuditLogs: () =>
    axiosInstance.get('/admin/audit-logs'),
};
