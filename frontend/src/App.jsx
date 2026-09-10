import React, { useState, useEffect } from 'react';
import { Toaster } from 'sonner';
import WalletMetrics from './components/WalletMetrics';
import DepositForm from './components/DepositForm';
import AuctionCard from './components/AuctionCard';
import CreateAuctionForm from './components/CreateAuctionForm';
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

  // Simular fecha de fin que expira en 10 segundos para probar el color ROJO
  const testSubasta = {
    imagen: "https://images.unsplash.com/photo-1600861194942-f883de0dfe96?auto=format&fit=crop&q=80&w=400",
    titulo: "MacBook Pro M3 Max",
    categoria: "Tecnología",
    precio_actual: 3500000,
    cantidad_pujas: 8,
    fecha_fin: new Date(Date.now() + 10 * 1000).toISOString(), // Expira en 10s
    server_time: new Date().toISOString() // Simulamos la hora del servidor
  };

  return (
    <div className="min-h-screen p-8 text-gray-900">
      <header className="max-w-5xl mx-auto mb-8">
        <h1 className="text-3xl font-bold">Mi Billetera</h1>
        <p className="text-gray-500 mt-2">Gestiona tu saldo y movimientos</p>
      </header>

      <main className="flex flex-col gap-8 max-w-5xl mx-auto">
        <section>
          <WalletMetrics metrics={metrics} />
        </section>

        <section className="flex flex-col md:flex-row gap-8 items-start">
          <DepositForm userId={userId} onDepositSuccess={fetchMetrics} />
          <CreateAuctionForm />
          <AuctionCard subasta={testSubasta} />
        </section>
      </main>

      {/* Proveedor de notificaciones para que funcionen los toast */}
      <Toaster position="bottom-right" richColors />
    </div>
  );
}

export default App;

