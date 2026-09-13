import axios from 'axios';
import { toast } from 'sonner';

// Instancia base de Axios
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
});

// Interceptor de peticiones (Request)
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token'); // Asumiendo que el token se guarda bajo esta clave
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor de respuestas (Response)
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response) {
      const { status, data } = error.response;

      if (status === 401) {
        // Token inválido o expirado, lo eliminamos
        localStorage.removeItem('token');
        
        // Redirigir a /login en caso de error 401, evitando bucle infinito si ya estamos en /login
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
