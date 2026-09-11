import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../apiClient';
import { toast } from 'sonner';

const RegisterForm = () => {
  const [nombre, setNombre] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!nombre || !email || !password) {
      toast.error('Por favor, completa todos los campos.');
      return;
    }

    setIsLoading(true);

    try {
      const response = await apiClient.post('/auth/register', {
        nombreCompleto: nombre,
        email: email,
        password: password
      });

      if (response.status === 201 || response.status === 200) {
        toast.success('Usuario registrado correctamente');
        // Limpiar el formulario
        setNombre('');
        setEmail('');
        setPassword('');
        // Redirigir al login
        navigate('/login');
      }
    } catch (error) {
      // El interceptor global de Axios ya maneja y muestra los errores (400, 409, 500)
      console.error('Error durante el registro:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 text-gray-800">
      <h1 className="text-4xl font-bold mb-4 text-blue-600">SubastaYa</h1>
      <div className="bg-white p-8 rounded-lg shadow-md w-full max-w-md flex flex-col items-center border border-gray-100">
        <h2 className="text-2xl font-semibold mb-2">Crear Cuenta</h2>
        <p className="mb-6 text-gray-500 text-sm text-center">
          Únete a SubastaYa para ofertar y publicar productos.
        </p>
        
        <form onSubmit={handleSubmit} className="w-full flex flex-col space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nombre">
              Nombre Completo
            </label>
            <input
              id="nombre"
              type="text"
              placeholder="Ej. Juan Pérez"
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              disabled={isLoading}
            />
          </div>

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
            className="w-full py-3 mt-4 bg-green-600 text-white rounded-lg font-medium shadow hover:bg-green-700 transition-colors disabled:bg-green-400 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {isLoading ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Registrando...
              </>
            ) : (
              'Registrarse'
            )}
          </button>
        </form>
        
        <div className="mt-6 flex flex-col items-center space-y-2">
          <Link to="/login" className="text-sm text-gray-500 hover:text-blue-600 font-medium transition-colors">
            ¿Ya tienes cuenta? Iniciar Sesión
          </Link>
          <Link to="/" className="text-sm text-gray-500 hover:text-blue-600 font-medium transition-colors mt-2 block">
            ← Volver al Catálogo
          </Link>
        </div>
      </div>
    </div>
  );
};

export default RegisterForm;

