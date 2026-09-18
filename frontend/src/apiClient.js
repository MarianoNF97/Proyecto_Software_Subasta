import axios from 'axios';
import { toast } from 'sonner';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response) {
      const { status, data } = error.response;

      if (status === 401) {
        localStorage.removeItem('token');
        
        if (window.location.pathname !== '/login') {
          window.location.href = '/login';
        }
      } else {
        const errorMessage = data?.error || `Error no especificado (HTTP ${status})`;
        toast.error(errorMessage);
      }
    } else {
      toast.error('Error de conexión con el servidor');
    }

    return Promise.reject(error);
  }
);

export default apiClient;
