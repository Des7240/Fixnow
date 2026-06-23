import axiosClient from './axiosClient';

export const jobApi = {
  getMarketplaceJobs: (lat: number, lng: number, radius: number = 10) => {
    return axiosClient.get(`/open-jobs/marketplace?lat=${lat}&lng=${lng}&radius=${radius}`);
  },
  getJobDetails: (id: string) => {
    return axiosClient.get(`/open-jobs/${id}`);
  },
  submitOffer: (id: string, data: any) => {
    return axiosClient.post(`/open-jobs/${id}/offers`, data);
  },
  saveJob: (id: string) => {
    return axiosClient.post(`/open-jobs/${id}/save`);
  },
  unsaveJob: (id: string) => {
    return axiosClient.delete(`/open-jobs/${id}/save`);
  },
  getSavedJobs: () => {
    return axiosClient.get('/open-jobs/saved');
  }
};
