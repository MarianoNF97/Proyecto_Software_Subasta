import React, { useState } from 'react';
import apiClient from '../apiClient';
import { toast } from 'sonner';

const DepositForm = ({ onDepositSuccess, userId = 1 }) => {
  const [amount, setAmount] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    const parsedAmount = parseFloat(amount);
    if (isNaN(parsedAmount) || parsedAmount <= 0) {
      toast.error('Por favor, ingresa un monto válido mayor a 0');
      return;
    }

    setIsLoading(true);

    try {
      await apiClient.post('/wallets/deposit', { userId: userId, amount: parsedAmount });
      
      toast.success('Depósito realizado con éxito');
      setAmount(''); // Limpiar el input al resolverse la petición
      if (onDepositSuccess) {
        onDepositSuccess();
      }
    } catch (error) {
      // El interceptor de apiClient ya maneja el toast de error para 400, 404, 409
      // No necesitamos hacer nada adicional aquí a menos que queramos
      console.error('Error al realizar el depósito:', error);
    } finally {
      setIsLoading(false); // Rehabilitar botón / ocultar spinner
    }
  };

  return (
    <div className="w-full max-w-md mx-auto p-6 bg-white rounded-2xl shadow-sm border border-gray-100 mt-6">
      <h2 className="text-xl font-semibold text-gray-800 mb-4">Cargar Saldo</h2>
      
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="flex flex-col gap-2">
          <label htmlFor="amount" className="text-sm font-medium text-gray-600">
            Monto a depositar
          </label>
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">$</span>
            <input
              id="amount"
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00"
              min="0.01"
              step="0.01"
              required
              disabled={isLoading}
              className="w-full pl-8 pr-4 py-3 rounded-xl border border-gray-200 bg-gray-50 text-gray-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all disabled:opacity-50"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={isLoading}
          className={`flex items-center justify-center w-full py-3 rounded-xl font-medium text-white transition-all 
            ${isLoading ? 'bg-blue-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700 active:scale-[0.98]'}`}
        >
          {isLoading ? (
            <>
              {/* Spinner */}
              <svg 
                className="animate-spin -ml-1 mr-2 h-5 w-5 text-white" 
                xmlns="http://www.w3.org/2000/svg" 
                fill="none" 
                viewBox="0 0 24 24"
              >
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Procesando...
            </>
          ) : (
            'Depositar'
          )}
        </button>
      </form>
    </div>
  );
};

export default DepositForm;

