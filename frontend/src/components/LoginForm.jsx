import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../apiClient';
import { toast } from 'sonner';

const LoginForm = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!email || !password) {
      toast.error('Por favor, ingresa tu email y contraseña.');
      return;
    }

    setIsLoading(true);

    try {
      // Usamos el cliente configurado, que ya resuelve el baseUrl
      const response = await apiClient.post('/auth/login', {
        email: email,
        password: password
      });

      // Se asume que el backend retorna { token: "..." }
      const token = response.data?.token || response.data?.Token;
      
      if (token) {
        localStorage.setItem('token', token);
        toast.success('¡Sesión iniciada con éxito!');
        navigate('/'); // Redirige al inicio
      } else {
        toast.error('El servidor no retornó un token válido.');
      }
    } catch (error) {
      // El interceptor en apiClient ya muestra un toast para 400, 401, etc.
      // Pero podemos manejar un error específico de login si lo deseamos.
      console.error('Error durante el login:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 text-gray-800">
      <h1 className="text-4xl font-bold mb-4 text-blue-600">SubastaYa</h1>
      <div className="bg-white p-8 rounded-lg shadow-md w-full max-w-md flex flex-col items-center border border-gray-100">
        <h2 className="text-2xl font-semibold mb-2">Iniciar Sesión</h2>
        <p className="mb-6 text-gray-500 text-sm text-center">
          Ingresa tus credenciales para acceder a la plataforma.
        </p>
        
        <form onSubmit={handleSubmit} className="w-full flex flex-col space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="email">
              Correo Electrónico
            </label>
            <input
              id="email"
              type="email"
              placeholder="tu@email.com"
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isLoading}
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="password">
              Contraseña
            </label>
            <input
              id="password"
              type="password"
              placeholder="••••••••"
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={isLoading}
            />
          </div>

          <button 
            type="submit"
            disabled={isLoading}
            className="w-full py-3 mt-4 bg-blue-600 text-white rounded-lg font-medium shadow hover:bg-blue-700 transition-colors disabled:bg-blue-400 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {isLoading ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Ingresando...
              </>
            ) : (
              'Iniciar Sesión'
            )}
          </button>
        </form>
        
        <div className="mt-6 flex flex-col items-center space-y-2">
          {/* Espacio para futuro enlace de registro */}
          <Link to="/" className="text-sm text-gray-500 hover:text-blue-600 font-medium transition-colors">
            ← Volver al Catálogo
          </Link>
        </div>
      </div>
    </div>
  );
};

export default LoginForm;

