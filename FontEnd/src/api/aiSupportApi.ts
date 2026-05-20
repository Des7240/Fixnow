import axiosInstance from './axios';

export interface AISupportResponse {
  responseText: string;
}

export const aiSupportApi = {
  analyzeProblem: async (problemDescription: string, imageFile?: File | null): Promise<AISupportResponse> => {
    const formData = new FormData();
    formData.append('ProblemDescription', problemDescription);
    if (imageFile) {
      formData.append('Image', imageFile);
    }

    const response = await axiosInstance.post('/AISupport/analyze', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return response.data;
  },
};
