import React, { useState, useEffect } from 'react';
import { Toaster } from 'sonner';
import WalletMetrics from './components/WalletMetrics';
import DepositForm from './components/DepositForm';
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

