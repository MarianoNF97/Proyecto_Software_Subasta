import React from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'sonner';
import useCountdown from '../hooks/useCountdown';

const AuctionCard = ({ subasta }) => {
  const [imageError, setImageError] = React.useState(false);
  const { id, imageUrl, title, categoryName, currentPrice, totalBids } = subasta;
  
  const rawStatus = subasta?.status ?? subasta?.estado ?? subasta?.Status;
  const normalizedStatus = rawStatus?.toUpperCase() || '';
  
  const startDate = subasta?.startDate ?? subasta?.fechaInicio;
  const endDate = subasta?.endDate ?? subasta?.fechaFin;
  
  const isScheduled = normalizedStatus === 'PROGRAMADA';
  const targetDate = isScheduled ? startDate : endDate;
  
  const timeLeft = useCountdown(targetDate);
  
  let finalStatus = normalizedStatus;
  if (timeLeft <= 0 && normalizedStatus === 'ACTIVA') {
    finalStatus = 'FINALIZADA';
  }

  const isEnded = finalStatus === 'FINALIZADA' || finalStatus === 'DESIERTA';
  
  const isCriticalTime = finalStatus === 'ACTIVA' && timeLeft <= 60 && timeLeft > 0;

  const formatTime = (seconds) => {
    if (seconds <= 0) return '00:00:00';
    const d = Math.floor(seconds / (3600 * 24));
    const h = Math.floor((seconds % (3600 * 24)) / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    
    if (d > 0) return `${d}d ${h}h ${m}m`;
    if (h > 0) return `${h}h ${m}m ${s}s`;
    return `${m}m ${s}s`;
  };

  const formatCurrency = (value = 0) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(value);
  };

  const handleLinkClick = (e) => {
    if (finalStatus === 'PROGRAMADA') {
      e.preventDefault();
      toast.info('Esta subasta aún no ha comenzado. Vuelve más tarde.');
    }
  };

  const getStatusBadge = () => {
    switch (finalStatus) {
      case 'ACTIVA':
        return <span className="absolute top-3 right-3 bg-green-500 text-white px-3 py-1.5 rounded-lg text-xs font-bold shadow-sm uppercase tracking-wider">Activa</span>;
      case 'PROGRAMADA':
        return <span className="absolute top-3 right-3 bg-blue-500 text-white px-3 py-1.5 rounded-lg text-xs font-bold shadow-sm uppercase tracking-wider">Programada</span>;
      case 'FINALIZADA':
      case 'DESIERTA':
        return <span className="absolute top-3 right-3 bg-gray-500 text-white px-3 py-1.5 rounded-lg text-xs font-bold shadow-sm uppercase tracking-wider">{finalStatus}</span>;
      default:
        return null;
    }
  };

  const isDisabled = finalStatus === 'PROGRAMADA' || finalStatus === 'FINALIZADA' || finalStatus === 'DESIERTA';

  return (
    <Link 
      to={`/subasta/${id}`} 
      onClick={handleLinkClick}
      className={`block w-full max-w-sm transition-transform duration-300 ${finalStatus === 'PROGRAMADA' ? 'cursor-not-allowed' : 'hover:-translate-y-1'}`}
    >
      <div className={`flex flex-col bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden transition-shadow duration-300 h-full ${finalStatus === 'PROGRAMADA' ? '' : 'hover:shadow-lg cursor-pointer'}`}>
        
        {}
        <div className="relative h-56 w-full bg-white flex items-center justify-center">
          {imageUrl && !imageError ? (
            <img 
              src={imageUrl} 
              alt={title} 
              onError={() => setImageError(true)}
              className={`w-full h-full object-cover ${isDisabled ? 'opacity-75' : ''}`}
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center bg-white">
              <span className="text-2xl font-bold text-blue-600 tracking-wider">Subasta Ya</span>
            </div>
          )}
          <span className="absolute top-3 left-3 bg-white/90 backdrop-blur-md px-3 py-1.5 rounded-lg text-xs font-bold text-gray-800 shadow-sm uppercase tracking-wider">
            {categoryName}
          </span>
          {getStatusBadge()}
        </div>

        {}
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
          
          {}
          <div className={`p-2 rounded-lg text-center mt-3 border ${
            isCriticalTime 
              ? 'bg-red-50 border-red-200 text-red-600 font-bold animate-pulse' 
              : 'bg-gray-50 border-gray-100 text-gray-700 font-medium'
          }`}>
            {isEnded ? (
              <div className="font-mono font-bold text-sm text-gray-500">
                Subasta Finalizada
              </div>
            ) : (
              <div className={`flex items-center justify-center gap-2 font-mono text-sm ${isCriticalTime ? '' : (isScheduled ? 'text-blue-700 font-semibold' : '')}`}>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10"></circle>
                  <polyline points="12 6 12 12 16 14"></polyline>
                </svg>
                <span>
                  {isScheduled ? `Inicia en: ${formatTime(timeLeft)}` : `Termina en: ${formatTime(timeLeft)}`}
                </span>
              </div>
            )}
          </div>
        </div>
        
      </div>
    </Link>
  );
};

export default AuctionCard;

