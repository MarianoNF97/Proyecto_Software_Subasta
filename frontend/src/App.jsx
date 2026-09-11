import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, useParams } from 'react-router-dom';
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

  const [auctions, setAuctions] = useState([]);
  const [isLoadingAuctions, setIsLoadingAuctions] = useState(true);

  const fetchAuctions = async () => {
    setIsLoadingAuctions(true);
    try {
      // El backend devuelve { id, title, currentPrice, etc. }
      const response = await apiClient.get('/auctions');
      setAuctions(response.data);
    } catch (error) {
      console.error('Error al obtener subastas:', error);
    } finally {
      setIsLoadingAuctions(false);
    }
  };

  useEffect(() => {
    fetchMetrics();
    fetchAuctions();
  }, []);

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
                
                {isLoadingAuctions ? (
                  <div className="flex justify-center items-center py-20">
                    <svg className="animate-spin h-10 w-10 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                  </div>
                ) : auctions.length > 0 ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {auctions.map(auction => (
                      <AuctionCard key={auction.id} subasta={auction} />
                    ))}
                  </div>
                ) : (
                  <div className="text-center text-gray-500 py-10">
                    No hay subastas disponibles en este momento.
                  </div>
                )}
              </div>
            } />
            
            <Route path="/subasta/:id" element={
              <section>
                <LiveBiddingRoom wallet={metrics} userId={userId} />
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

