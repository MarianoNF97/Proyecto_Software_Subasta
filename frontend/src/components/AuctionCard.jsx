import React from 'react';
import useCountdown from '../hooks/useCountdown';

const AuctionCard = ({ subasta }) => {
  const { imageUrl, title, categoryName, currentPrice, totalBids, endDate } = subasta;
  
  // Uso del Custom Hook para mantener el componente limpio
  const timeLeft = useCountdown(endDate);

  const formatTime = (seconds) => {
    if (seconds <= 0) return 'Subasta finalizada';
    const d = Math.floor(seconds / (3600 * 24));
    const h = Math.floor((seconds % (3600 * 24)) / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    
    if (d > 0) return `${d}d ${h}h ${m}m`;
    if (h > 0) return `${h}h ${m}m ${s}s`;
    return `${m}m ${s}s`;
  };

  const isEndingSoon = timeLeft > 0 && timeLeft < 60;
  const isEnded = timeLeft === 0;

  const formatCurrency = (value = 0) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(value);
  };

  return (
    <div className="flex flex-col bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow duration-300 w-full max-w-sm">
      
      {/* Mitad superior: Imagen y Categoría (Badge flotante) */}
      <div className="relative h-56 w-full bg-gray-100">
        <img 
          src={imageUrl || 'https://via.placeholder.com/400x300?text=Subasta+Sin+Imagen'} 
          alt={title} 
          className="w-full h-full object-cover"
        />
        <span className="absolute top-3 left-3 bg-white/90 backdrop-blur-md px-3 py-1.5 rounded-lg text-xs font-bold text-gray-800 shadow-sm uppercase tracking-wider">
          {categoryName}
        </span>
      </div>

      {/* Mitad inferior: Título y Ofertas */}
      <div className="flex flex-col p-5 flex-grow">
        <h3 className="text-lg font-bold text-gray-900 line-clamp-2 mb-4 leading-tight">
          {title}
        </h3>
        
        <div className="flex justify-between items-end mt-auto pt-2">
          <div className="flex flex-col">
            <span className="text-xs text-gray-500 font-medium mb-1">Oferta actual</span>
            <span className="text-2xl font-black text-blue-600 leading-none">
              {formatCurrency(currentPrice)}
            </span>
          </div>
          <div className="flex flex-col items-end">
            <span className="text-xs text-gray-500 font-medium mb-1">Pujas</span>
            <span className="text-sm font-bold text-gray-700 bg-gray-100 px-2.5 py-1 rounded-md">
              {totalBids}
            </span>
          </div>
        </div>
      </div>

      {/* Pie de la tarjeta: Contador Regresivo */}
      <div className={`border-t p-4 flex items-center justify-center transition-colors
        ${isEnded ? 'bg-gray-50 border-gray-100' : isEndingSoon ? 'bg-red-50 border-red-100' : 'bg-blue-50/50 border-blue-50'}
      `}>
        <div className={`flex items-center gap-2 font-mono font-semibold text-sm
          ${isEnded ? 'text-gray-500' : isEndingSoon ? 'text-red-600 animate-pulse' : 'text-blue-700'}
        `}>
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <polyline points="12 6 12 12 16 14"></polyline>
          </svg>
          <span>{formatTime(timeLeft)}</span>
        </div>
      </div>
      
    </div>
  );
};

export default AuctionCard;

