import React, { useState, useEffect, useCallback } from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Toaster } from 'sonner';

import WalletMetrics from './components/WalletMetrics';
import DepositForm from './components/DepositForm';
import TransactionHistory from './components/TransactionHistory';
import AuctionCard from './components/AuctionCard';
import CreateAuctionForm from './components/CreateAuctionForm';
import LiveBiddingRoom from './components/LiveBiddingRoom';
import UserDashboard from './components/UserDashboard';

import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './components/MainLayout';
import LoginForm from './components/LoginForm';
import RegisterForm from './components/RegisterForm';
import apiClient from './apiClient';
import { useAuth } from './contexts/AuthContext';

function NotFound() {
  return (
    <div className="text-center py-20">
      <h1 className="text-6xl font-bold text-gray-300">404</h1>
      <p className="text-gray-500 mt-4">Página no encontrada</p>
      <Link to="/" className="text-blue-600 mt-4 inline-block">Volver al catálogo</Link>
    </div>
  );
}

function App() {
  const { user } = useAuth();
  const [metrics, setMetrics] = useState({
    totalBalance: 0,
    lockedBalance: 0,
    availableBalance: 0
  });

  const fetchMetrics = useCallback(async () => {
    try {
      if (!user?.id) return; 

      const response = await apiClient.get(`/wallets/${user.id}`);
      setMetrics(response.data);
    } catch (error) {
      console.error('Error al obtener los saldos:', error);
    }
  }, [user?.id]);

  // ==========================================
  // ESCUCHA GLOBAL DE SIGNALR (Billetera en vivo)
  // ==========================================
  useEffect(() => {
    if (!user?.id) return;

    const hubUrl = (import.meta.env.VITE_API_URL || '/api').replace('/api', '/auctionHub');
    
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => {
        // Escucha el evento emitido cuando se retienen o liberan fondos
        connection.on('WalletUpdated', () => {
          fetchMetrics();
        });
      })
      .catch((err) => console.error('Error conectando SignalR en App:', err));

    return () => {
      connection.stop();
    };
  }, [user?.id, fetchMetrics]);

  const [auctions, setAuctions] = useState([]);
  const [isLoadingAuctions, setIsLoadingAuctions] = useState(true);
  const [categories, setCategories] = useState([]);
  const [filters, setFilters] = useState({
    status: '',
    categoryId: ''
  });

  const fetchAuctions = async () => {
    setIsLoadingAuctions(true);
    try {
      const params = new URLSearchParams();
      if (filters.status) params.append('status', filters.status);
      if (filters.categoryId) params.append('categoryId', filters.categoryId);
      
      const url = `/auctions${params.toString() ? `?${params.toString()}` : ''}`;
      const response = await apiClient.get(url);
      setAuctions(response.data);
    } catch (error) {
      console.error('Error al obtener subastas:', error);
    } finally {
      setIsLoadingAuctions(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const response = await apiClient.get('/categories');
      setCategories(response.data);
    } catch (error) {
      console.error('Error al obtener categorías:', error);
    }
  };

  useEffect(() => {
    fetchMetrics();
    fetchCategories();
  }, [fetchMetrics]);

  useEffect(() => {
    fetchAuctions();
  }, [filters]);

  return (
    <>
      <BrowserRouter>
        <Routes>
          {/* Rutas Públicas */}
          <Route path="/login" element={<LoginForm />} />
          <Route path="/register" element={<RegisterForm />} />

          {/* Rutas dentro de MainLayout */}
          <Route element={<MainLayout />}>
            
            <Route path="/" element={
              <div className="flex flex-col gap-8 mt-4">
                <div className="flex flex-col md:flex-row justify-between items-center gap-4">
                  <h2 className="text-3xl font-bold text-gray-800">Catálogo de Subastas</h2>
                  
                  {/* Barra de Filtros */}
                  <div className="flex flex-col sm:flex-row gap-4 w-full md:w-auto items-center">
                    <div className="flex rounded-md shadow-sm" role="group">
                      <button
                        type="button"
                        onClick={() => setFilters(f => ({ ...f, status: '' }))}
                        className={`px-4 py-2 text-sm font-medium border rounded-l-lg ${filters.status === '' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                      >
                        Todas
                      </button>
                      <button
                        type="button"
                        onClick={() => setFilters(f => ({ ...f, status: 'ACTIVA' }))}
                        className={`px-4 py-2 text-sm font-medium border-t border-b ${filters.status === 'ACTIVA' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                      >
                        Activas
                      </button>
                      <button
                        type="button"
                        onClick={() => setFilters(f => ({ ...f, status: 'PROGRAMADA' }))}
                        className={`px-4 py-2 text-sm font-medium border-t border-b border-l ${filters.status === 'PROGRAMADA' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                      >
                        Programadas
                      </button>
                      <button
                        type="button"
                        onClick={() => setFilters(f => ({ ...f, status: 'FINALIZADA' }))}
                        className={`px-4 py-2 text-sm font-medium border rounded-r-md border-l-0 ${filters.status === 'FINALIZADA' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                      >
                        Finalizadas
                      </button>
                    </div>

                    <select
                      className="border border-gray-300 rounded-lg px-4 py-2 text-sm focus:ring-blue-500 focus:border-blue-500 w-full sm:w-auto bg-white"
                      value={filters.categoryId}
                      onChange={(e) => setFilters(f => ({ ...f, categoryId: e.target.value }))}
                    >
                      <option value="">Todas las categorías</option>
                      {categories.map(cat => (
                        <option key={cat.id} value={cat.id}>
                          {cat.nombre}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                
                {isLoadingAuctions ? (
                  <div className="text-center py-20">
                    <div className="animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full mx-auto" />
                    <p className="text-gray-500 mt-4">Cargando subastas...</p>
                  </div>
                ) : auctions.length === 0 ? (
                  <div className="text-center py-20">
                    <p className="text-gray-400 text-lg">No hay subastas disponibles</p>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {auctions.map(auction => (
                      <AuctionCard key={auction.id} subasta={auction} />
                    ))}
                  </div>
                )}
              </div>
            } />
            
            <Route path="/subasta/:id" element={
              <section>
                <LiveBiddingRoom wallet={metrics} />
              </section>
            } />

            {/* Rutas Protegidas */}
            <Route element={<ProtectedRoute />}>
              
              <Route path="/billetera" element={
                <div className="flex flex-col gap-12">
                  <section>
                    <WalletMetrics metrics={metrics} />
                  </section>
                  <section>
                    <DepositForm onDepositSuccess={fetchMetrics} />
                  </section>
                  <section>
                    {/* El key dinámico fuerza a TransactionHistory a recargar sus movimientos al variar el saldo */}
                    <TransactionHistory key={`${metrics.availableBalance}-${metrics.lockedBalance}`} />
                  </section>
                </div>
              } />

              <Route path="/publicar" element={
                <section className="flex justify-center">
                  <CreateAuctionForm />
                </section>
              } />

              <Route path="/mis-actividades" element={
                <section>
                  <UserDashboard />
                </section>
              } />

            </Route>

          </Route>
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>

      <Toaster position="bottom-right" richColors />
    </>
  );
}

export default App;