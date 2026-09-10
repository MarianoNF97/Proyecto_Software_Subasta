import React, { useState, useEffect } from 'react';
import { Toaster } from 'sonner';
import WalletMetrics from './components/WalletMetrics';
import DepositForm from './components/DepositForm';
import AuctionCard from './components/AuctionCard';
import CreateAuctionForm from './components/CreateAuctionForm';
import LiveBiddingRoom from './components/LiveBiddingRoom';
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
    <div className="min-h-screen p-8 text-gray-900">
      <header className="max-w-6xl mx-auto mb-8">
        <h1 className="text-3xl font-bold">SubastaYa</h1>
        <p className="text-gray-500 mt-2">Plataforma de Subastas en Tiempo Real</p>
      </header>

      <main className="flex flex-col gap-12 max-w-6xl mx-auto">
        <section>
          <WalletMetrics metrics={metrics} />
        </section>

        {/* Sala de Subasta de prueba */}
        <section>
          <LiveBiddingRoom auction={testSubasta} wallet={metrics} serverTime={testSubasta.server_time} userId={userId} />
        </section>

        {/* Formularios y Tarjetas restablecidos */}
        <section className="flex flex-col xl:flex-row gap-8 items-start">
          <CreateAuctionForm />
          <AuctionCard subasta={testSubasta} />
        </section>
        <section>
          <DepositForm userId={userId} onDepositSuccess={fetchMetrics} />
        </section>
      </main>

      {/* Proveedor de notificaciones para que funcionen los toast */}
      <Toaster position="bottom-right" richColors />
    </div>
  );
}

export default App;

