import axios from 'axios';
import { toast } from 'sonner';

// Instancia base de Axios
const apiClient = axios.create({
  baseURL: '/api',
});

// Interceptor global de respuestas
apiClient.interceptors.response.use(
  (response) => {
    // Retornamos la respuesta tal cual si fue exitosa
    return response;
  },
  (error) => {
    // Verificamos si el error tiene una respuesta del servidor
    if (error.response) {
      const { status, data } = error.response;

      // Interceptamos los códigos HTTP 400, 404 y 409
      if ([400, 404, 409].includes(status)) {
        // Extraemos el mensaje de error o usamos uno genérico
        const errorMessage = data?.error || 'Ha ocurrido un error inesperado';
        
        // Mostramos la notificación usando Sonner
        toast.error(errorMessage);
      }
    } else {
      // Manejo por si el servidor está caído o hay problemas de red
      toast.error('Error de conexión con el servidor');
    }

    // Propagamos el error para quien haya hecho la llamada pueda manejarlo también si lo desea
    return Promise.reject(error);
  }
);

export default apiClient;

