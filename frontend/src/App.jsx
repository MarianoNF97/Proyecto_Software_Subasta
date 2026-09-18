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
  
  // Estados de filtros locales
  const [statusFilter, setStatusFilter] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [sortConfig, setSortConfig] = useState(''); // 'menor-tiempo', 'mayor-puja', 'menor-puja'
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');

  const fetchAuctions = async () => {
    setIsLoadingAuctions(true);
    try {
      const response = await apiClient.get('/auctions');
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
  }, []);

  const filteredAndSortedAuctions = React.useMemo(() => {
    let result = [...auctions];

    // 1. Filtro por Estado
    if (statusFilter) {
      if (statusFilter === 'FINALIZADA') {
        result = result.filter(a => {
          const s = (a.status || a.estado || '').toUpperCase();
          return s === 'FINALIZADA' || s === 'DESIERTA';
        });
      } else {
        result = result.filter(a => (a.status || a.estado || '').toUpperCase() === statusFilter);
      }
    }

    // 2. Filtro por Categoría
    if (categoryFilter) {
      result = result.filter(a => a.categoryId?.toString() === categoryFilter.toString() || a.categoriaId?.toString() === categoryFilter.toString());
    }

    // 3. Filtro por Rango de Precio
    const minVal = minPrice !== '' ? Number(minPrice) : null;
    const maxVal = maxPrice !== '' ? Number(maxPrice) : null;

    if (minVal !== null) {
      result = result.filter(a => (a.currentPrice ?? a.precioActual ?? a.startingPrice ?? a.precioBase ?? 0) >= minVal);
    }
    
    if (maxVal !== null && (minVal === null || maxVal >= minVal)) {
      result = result.filter(a => (a.currentPrice ?? a.precioActual ?? a.startingPrice ?? a.precioBase ?? 0) <= maxVal);
    }

    // 4. Ordenamiento
    if (sortConfig === 'menor-tiempo') {
      result.sort((a, b) => {
        const dateA = new Date(a.endDate || a.fechaFin).getTime();
        const dateB = new Date(b.endDate || b.fechaFin).getTime();
        return dateA - dateB;
      });
    } else if (sortConfig === 'mayor-puja') {
      result.sort((a, b) => (b.currentPrice ?? b.precioActual ?? b.startingPrice ?? 0) - (a.currentPrice ?? a.precioActual ?? a.startingPrice ?? 0));
    } else if (sortConfig === 'menor-puja') {
      result.sort((a, b) => (a.currentPrice ?? a.precioActual ?? a.startingPrice ?? 0) - (b.currentPrice ?? b.precioActual ?? b.startingPrice ?? 0));
    }

    return result;
  }, [auctions, statusFilter, categoryFilter, minPrice, maxPrice, sortConfig]);

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
                {/* Estructura Principal del Encabezado */}
                <div className="flex flex-col xl:flex-row justify-between items-start mb-8 gap-6">
                  
                  {/* Bloque Izquierdo (Título) */}
                  <h1 className="text-3xl font-bold text-gray-900">Catálogo de Subastas</h1>
                  
                  {/* Bloque Derecho (Agrupación de Controles en 2 Filas) */}
                  <div className="flex flex-col gap-3 w-full xl:w-auto xl:items-end">
                    
                    {/* Fila 1 (Filtros Principales) */}
                    <div className="flex flex-wrap items-center gap-4">
                      <div className="flex rounded-md shadow-sm" role="group">
                        <button
                          type="button"
                          onClick={() => setStatusFilter('')}
                          className={`px-4 py-2 text-sm font-medium border rounded-l-lg transition-colors ${statusFilter === '' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                        >
                          Todas
                        </button>
                        <button
                          type="button"
                          onClick={() => setStatusFilter('ACTIVA')}
                          className={`px-4 py-2 text-sm font-medium border-t border-b transition-colors ${statusFilter === 'ACTIVA' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                        >
                          Activas
                        </button>
                        <button
                          type="button"
                          onClick={() => setStatusFilter('PROGRAMADA')}
                          className={`px-4 py-2 text-sm font-medium border-t border-b border-l transition-colors ${statusFilter === 'PROGRAMADA' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                        >
                          Programadas
                        </button>
                        <button
                          type="button"
                          onClick={() => setStatusFilter('FINALIZADA')}
                          className={`px-4 py-2 text-sm font-medium border rounded-r-md border-l-0 transition-colors ${statusFilter === 'FINALIZADA' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-900 border-gray-200 hover:bg-gray-100'}`}
                        >
                          Finalizadas
                        </button>
                      </div>

                      <select
                        className="text-sm py-1.5 px-3 bg-white border border-gray-200 rounded-md shadow-sm text-gray-600 outline-none focus:ring-2 focus:ring-blue-500"
                        value={categoryFilter}
                        onChange={(e) => setCategoryFilter(e.target.value)}
                      >
                        <option value="">Todas las categorías</option>
                        {categories.map(cat => (
                          <option key={cat.id} value={cat.id}>
                            {cat.nombre}
                          </option>
                        ))}
                      </select>
                    </div>

                    {/* Fila 2 (Filtros Secundarios) */}
                    <div className="flex flex-wrap items-center xl:justify-end gap-3">
                      <select
                        className="text-sm py-1.5 px-3 bg-white border border-gray-200 rounded-md shadow-sm text-gray-600 outline-none focus:ring-2 focus:ring-blue-500"
                        value={sortConfig}
                        onChange={(e) => setSortConfig(e.target.value)}
                      >
                        <option value="">Ordenar por defecto</option>
                        <option value="menor-tiempo">Menor tiempo restante</option>
                        <option value="mayor-puja">Mayor precio</option>
                        <option value="menor-puja">Menor precio</option>
                      </select>

                      <div className="flex items-center gap-2">
                        <input
                          type="number"
                          placeholder="Min $"
                          className="w-24 text-sm py-1.5 px-3 bg-white border border-gray-200 rounded-md shadow-sm text-gray-600 outline-none focus:ring-2 focus:ring-blue-500"
                          value={minPrice}
                          onChange={(e) => setMinPrice(e.target.value)}
                        />
                        <span className="text-gray-300">-</span>
                        <input
                          type="number"
                          placeholder="Max $"
                          className="w-24 text-sm py-1.5 px-3 bg-white border border-gray-200 rounded-md shadow-sm text-gray-600 outline-none focus:ring-2 focus:ring-blue-500"
                          value={maxPrice}
                          onChange={(e) => setMaxPrice(e.target.value)}
                        />
                      </div>
                    </div>
                  </div>
                </div>
                
                {isLoadingAuctions ? (
                  <div className="text-center py-20">
                    <div className="animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full mx-auto" />
                    <p className="text-gray-500 mt-4">Cargando subastas...</p>
                  </div>
                ) : filteredAndSortedAuctions.length === 0 ? (
                  <div className="text-center py-20">
                    <p className="text-gray-400 text-lg">No hay subastas que coincidan con los filtros</p>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {filteredAndSortedAuctions.map(auction => (
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
                  <CreateAuctionForm onAuctionCreated={fetchAuctions} />
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