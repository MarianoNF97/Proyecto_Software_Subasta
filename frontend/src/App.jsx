import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from 'sonner';

import WalletMetrics from './components/WalletMetrics';
import DepositForm from './components/DepositForm';
import AuctionCard from './components/AuctionCard';
import CreateAuctionForm from './components/CreateAuctionForm';
import LiveBiddingRoom from './components/LiveBiddingRoom';

import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './components/MainLayout';
import apiClient from './apiClient';

function App() {
  const [metrics, setMetrics] = useState({
    totalBalance: 0,
    lockedBalance: 0,
    availableBalance: 0
  });

  const userId = 1; // Hardcodeado por ahora hasta tener el contexto de Auth

  const fetchMetrics = async () => {
    try {
      const token = localStorage.getItem('token');
      if (!token) return; // No cargar saldos si no estamos logueados
      
      // MOCK para poder probar las vistas protegidas sin que el backend nos eche con un 401
      if (token === 'fake-jwt-token') {
        setMetrics({ totalBalance: 10000, lockedBalance: 2000, availableBalance: 8000 });
        return;
      }

      const response = await apiClient.get(`/wallets/${userId}`);
      setMetrics(response.data);
    } catch (error) {
      console.error('Error al obtener los saldos:', error);
    }
  };

  useEffect(() => {
    fetchMetrics();
  }, []);

  const testSubasta = {
    imagen: "https://images.unsplash.com/photo-1600861194942-f883de0dfe96?auto=format&fit=crop&q=80&w=800",
    titulo: "MacBook Pro M3 Max 64GB RAM 2TB SSD",
    descripcion: "Notebook Apple MacBook Pro 16 pulgadas, chip M3 Max. Teclado en español, color Space Black. Equipo sellado en caja original con garantía internacional de 1 año.\n\nIdeal para desarrolladores, editores de video y profesionales exigentes.",
    categoria: "Tecnología",
    precio_actual: 3500000,
    incremento_minimo: 50000,
    cantidad_pujas: 8,
    estado: 'ABIERTA',
    fecha_fin: new Date(Date.now() + 60 * 5 * 1000).toISOString(), // 5 minutos
    server_time: new Date().toISOString()
  };

  return (
    <>
      <BrowserRouter>
        <Routes>
          {/* Rutas Públicas sin Layout */}
          <Route path="/login" element={
            <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 text-gray-800">
              <h1 className="text-4xl font-bold mb-4 text-blue-600">SubastaYa</h1>
              <div className="bg-white p-8 rounded-lg shadow-md w-full max-w-md flex flex-col items-center">
                <h2 className="text-2xl font-semibold mb-6">Iniciar Sesión</h2>
                <p className="mb-8 text-gray-500 text-center">Simulación de Login para probar el enrutamiento protegido.</p>
                
                <button 
                  onClick={() => {
                    localStorage.setItem('token', 'fake-jwt-token');
                    window.location.href = '/';
                  }}
                  className="w-full py-3 bg-blue-600 text-white rounded-lg font-medium shadow hover:bg-blue-700 transition-colors mb-4"
                >
                  Ingresar (Crear Token de Prueba)
                </button>
                
                <a href="/" className="text-blue-500 hover:text-blue-700 font-medium">
                  Volver al Catálogo
                </a>
              </div>
            </div>
          } />

          {/* Rutas dentro de MainLayout */}
          <Route element={<MainLayout />}>
            
            <Route path="/" element={
              <div className="flex flex-col gap-8 mt-4">
                <h2 className="text-3xl font-bold text-gray-800">Catálogo de Subastas</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  <AuctionCard subasta={testSubasta} />
                </div>
              </div>
            } />
            
            <Route path="/subasta/:id" element={
              <section>
                <LiveBiddingRoom 
                  auction={testSubasta} 
                  wallet={metrics} 
                  serverTime={testSubasta.server_time} 
                  userId={userId} 
                />
              </section>
            } />

            {/* Rutas Protegidas dentro de MainLayout */}
            <Route element={<ProtectedRoute />}>
              
              <Route path="/billetera" element={
                <div className="flex flex-col gap-12">
                  <section>
                    <WalletMetrics metrics={metrics} />
                  </section>
                  <section>
                    <DepositForm userId={userId} onDepositSuccess={fetchMetrics} />
                  </section>
                </div>
              } />

              <Route path="/publicar" element={
                <section className="flex justify-center">
                  <CreateAuctionForm />
                </section>
              } />

            </Route>

          </Route>
        </Routes>
      </BrowserRouter>

      <Toaster position="bottom-right" richColors />
    </>
  );
}

export default App;

