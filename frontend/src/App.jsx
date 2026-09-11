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
import LoginForm from './components/LoginForm';
import RegisterForm from './components/RegisterForm';
import apiClient from './apiClient';

function App() {
  const [metrics, setMetrics] = useState({
    totalBalance: 0,
    lockedBalance: 0,
    availableBalance: 0
  });

  const getUserIdFromToken = () => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return parseInt(payload.sub, 10);
    } catch (e) {
      return null;
    }
  };

  const userId = getUserIdFromToken();

  const fetchMetrics = async () => {
    try {
      const token = localStorage.getItem('token');
      if (!token || !userId) return; 

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
          <Route path="/login" element={<LoginForm />} />
          <Route path="/register" element={<RegisterForm />} />

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

