import React from 'react';
import { Outlet, Link } from 'react-router-dom';

const MainLayout = () => {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col text-gray-900">
      <nav className="bg-white shadow-md p-4 sticky top-0 z-50">
        <div className="max-w-6xl mx-auto flex justify-between items-center">
          <Link to="/" className="text-xl font-bold text-blue-600">SubastaYa</Link>
          <div className="flex space-x-6">
            <Link to="/" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Catálogo</Link>
            <Link to="/billetera" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Mi Billetera</Link>
            <Link to="/publicar" className="text-gray-700 hover:text-blue-600 font-medium transition-colors">Publicar Subasta</Link>
          </div>
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

