import React from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const MainLayout = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col text-gray-900">
      <nav className="bg-white shadow-md p-4 sticky top-0 z-50">
        <div className="max-w-6xl mx-auto flex justify-between items-center">
          <div className="flex items-center gap-8">
            <Link to="/" className="text-xl font-bold text-blue-600">SubastaYa</Link>
            <div className="flex space-x-6">
              <Link to="/" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Catálogo</Link>
              {isAuthenticated && (
                <>
                  <Link to="/billetera" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Mi Billetera</Link>
                  <Link to="/mis-actividades" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Mis Actividades</Link>
                  <Link to="/publicar" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Publicar Subasta</Link>
                </>
              )}
            </div>
          </div>
          
          {isAuthenticated ? (
            <div className="flex items-center gap-4">
              <span className="text-sm font-semibold text-gray-600">Hola, {user.name}</span>
              <button
                onClick={handleLogout}
                className="text-sm text-red-600 hover:text-red-800 font-bold transition-colors"
              >
                Cerrar sesión
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-4">
              <Link to="/login" className="text-sm font-semibold text-blue-600 hover:text-blue-800 transition-colors">
                Iniciar sesión
              </Link>
              <Link to="/register" className="text-sm font-semibold text-blue-600 hover:text-blue-800 transition-colors">
                Registrarse
              </Link>
            </div>
          )}
        </div>
      </nav>
      
      <main className="flex-grow p-8">
        <div className="max-w-6xl mx-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
};

export default MainLayout;

