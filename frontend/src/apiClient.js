import axios from 'axios';
import { toast } from 'sonner';

// Instancia base de Axios
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
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
        // Redirigir a /login en caso de error 401
        window.location.href = '/login';
      } else if ([400, 404, 409].includes(status)) {
        // Mostrar notificación de error genérica
        const errorMessage = data?.error || 'Ha ocurrido un error inesperado';
        toast.error(errorMessage);
      }
    } else {
      toast.error('Error de conexión con el servidor');
    }

    return Promise.reject(error);
  }
);

export default apiClient;
