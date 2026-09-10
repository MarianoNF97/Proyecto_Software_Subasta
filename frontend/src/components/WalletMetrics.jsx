import React from 'react';

const WalletMetrics = ({ metrics }) => {
  const { totalBalance, lockedBalance, availableBalance } = metrics || {};

  const formatCurrency = (value = 0) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(value);
  };

  return (
    <div className="flex flex-col md:flex-row gap-6 w-full max-w-5xl mx-auto p-4">
      
      {/* Tarjeta 1: Saldo Total */}
      <div className="flex-1 bg-white p-6 rounded-2xl shadow-sm flex flex-col justify-center border border-gray-100">
        <span className="text-sm font-medium text-gray-500 mb-1">Saldo Total</span>
        <span className="text-2xl font-semibold text-gray-800">
          {formatCurrency(totalBalance)}
        </span>
      </div>

      {/* Tarjeta 2: Saldo Retenido */}
      <div className="flex-1 bg-white p-6 rounded-2xl shadow-sm flex flex-col justify-center border border-gray-100">
        <div className="flex items-center gap-2 mb-1">
          <svg 
            xmlns="http://www.w3.org/2000/svg" 
            width="16" 
            height="16" 
            viewBox="0 0 24 24" 
            fill="none" 
            stroke="currentColor" 
            strokeWidth="2" 
            strokeLinecap="round" 
            strokeLinejoin="round" 
            className="text-gray-400"
          >
            <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
            <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
          </svg>
          <span className="text-sm font-medium text-gray-500">Saldo Retenido</span>
        </div>
        <span className="text-2xl font-semibold text-gray-800">
          {formatCurrency(lockedBalance)}
        </span>
      </div>

      {/* Tarjeta 3: Saldo Disponible (Destacada visualmente) */}
      <div className="flex-1 bg-green-50 p-6 rounded-2xl shadow-sm flex flex-col justify-center border border-green-100">
        <span className="text-sm font-medium text-green-600 mb-1">Saldo Disponible</span>
        <span className="text-3xl font-bold text-green-700">
          {formatCurrency(availableBalance)}
        </span>
      </div>
      
    </div>
  );
};

export default WalletMetrics;

