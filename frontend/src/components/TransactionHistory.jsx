import { useState, useEffect } from 'react';
import apiClient from '../apiClient';

export default function TransactionHistory() {
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchTransactions = async () => {
    try {
      // Fuente API: backend/SubastaYa.Api/Controllers/WalletsController.cs
      // Contrato C#: backend/SubastaYa.Application/DTOs/LedgerTransactionDto.cs
      // Método: GET /wallets/my-transactions
      const res = await apiClient.get('/wallets/my-transactions');
      setTransactions(res.data);
    } catch (err) {
      // Error manejado por el interceptor de apiClient
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransactions();
  }, []);

  if (loading) return <div>Cargando...</div>;

  return (
    <div className="bg-white rounded-xl shadow p-6 mt-6">
      <h3 className="text-lg font-semibold mb-4">Historial de Movimientos</h3>
      {transactions.length === 0 ? (
        <p className="text-gray-500 text-sm">No hay movimientos registrados.</p>
      ) : (
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-gray-500 border-b">
              <th className="pb-2">Fecha</th>
              <th className="pb-2">Tipo</th>
              <th className="pb-2 text-right">Monto</th>
            </tr>
          </thead>
          <tbody>
            {transactions.map(tx => (
              <tr key={tx.id} className="border-b last:border-0">
                <td className="py-2">{new Date(tx.date).toLocaleDateString()}</td>
                <td className="py-2">{tx.type}</td>
                <td className={`py-2 text-right font-mono ${
                  ['DEPOSITO', 'COBRO', 'LIBERACION'].includes(tx.type)
                    ? 'text-green-600' : 'text-red-600'
                }`}>
                  {['DEPOSITO', 'COBRO', 'LIBERACION'].includes(tx.type) ? '+' : '-'}
                  ${tx.amount.toLocaleString('es-AR')}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

