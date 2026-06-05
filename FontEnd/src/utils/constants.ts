export const API_BASE_URL = import.meta.env.VITE_API_URL?.replace('/api/v1', '') || 'http://localhost:8080';
export const API_V1_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080/api/v1';

export const getImageUrl = (path: string) => {
  if (!path) return 'https://via.placeholder.com/300x200?text=No+Image';
  if (path.startsWith('http//')) path = path.replace('http//', 'http://');
  if (path.startsWith('https//')) path = path.replace('https//', 'https://');
  if (path.startsWith('http')) return path; // Already an absolute URL
  return `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
};
