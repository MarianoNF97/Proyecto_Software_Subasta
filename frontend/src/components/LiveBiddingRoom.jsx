import React, { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { toast } from 'sonner';
import useCountdown from '../hooks/useCountdown';
import apiClient from '../apiClient';
import { useAuth } from '../contexts/AuthContext';

const LiveBiddingRoom = ({ wallet }) => {
  const { id } = useParams();
  const { user } = useAuth();
  const userId = user?.id;
  
  // 1. Estado de la subasta
  const [auction, setAuction] = useState(null);
  const [isLoadingAuction, setIsLoadingAuction] = useState(true);

  // 2. Estado interactivo en vivo
  const [currentPrice, setCurrentPrice] = useState(0);
  const [endDate, setEndDate] = useState(null);
  const [bidsHistory, setBidsHistory] = useState([]);
  const [latestBidderId, setLatestBidderId] = useState(null);
  const [isBidding, setIsBidding] = useState(false);

  // Consola de puja
  const { imageUrl, title, description, categoryName, minIncrement, status } = auction || {};
  const suggestedBid = currentPrice + (minIncrement || 0);
  const [bidAmount, setBidAmount] = useState(0);

  // ==========================================
  // 1. FETCH DE LA SUBASTA (Inicialización)
  // ==========================================
  useEffect(() => {
    let isMounted = true;
    const fetchAuction = async () => {
      try {
        const response = await apiClient.get(`/auctions/${id}`);
        if (!isMounted) return;

        const serverDateHeader = response.headers['date'];
        if (serverDateHeader) {
          serverTimeRef.current = new Date(serverDateHeader).toISOString();
        } else {
          serverTimeRef.current = new Date().toISOString();
        }

        const data = response.data;
        setAuction(data);
        setCurrentPrice(data.currentPrice ?? data.precioActual ?? 0);
        setEndDate(data.endDate ?? data.fechaFin);

        // Mapear historial inicial de pujas
        const rawBids = data.bids || data.pujas || [];
        const formattedBids = rawBids.map(b => ({
          userId: b.buyerId || b.compradorId || b.comprador_id || b.userId,
          userName: b.buyerName || b.compradorNombre || b.userName,
          amount: b.amount ?? b.monto,
          time: b.time || b.fechaPuja || b.fecha_puja || b.createdAt || new Date().toISOString()
        })).sort((a, b) => new Date(b.time) - new Date(a.time));

        setBidsHistory(formattedBids);

        // Determinar quién va liderando actualmente
        const topBidder = data.highestBidderId || data.compradorGanadorId || formattedBids[0]?.userId;
        if (topBidder) {
          setLatestBidderId(topBidder);
        }
      } catch (error) {
        console.error('Error al cargar la subasta:', error);
      } finally {
        if (isMounted) setIsLoadingAuction(false);
      }
    };

    fetchAuction();
    return () => { isMounted = false; };
  }, [id]);

  useEffect(() => {
    setBidAmount(suggestedBid);
  }, [suggestedBid]);

  // ==========================================
  // 2. INTEGRACIÓN DE SIGNALR (Tiempo Real)
  // ==========================================
  useEffect(() => {
    if (!auction) return;

    const hubUrl = (import.meta.env.VITE_API_URL || '/api').replace('/api', '/auctionHub');
    
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    const startConnection = async () => {
      try {
        await connection.start();
        console.log('✅ Conectado a SignalR - Sala en Vivo');
        await connection.invoke('JoinAuctionGroup', id.toString());
      } catch (err) {
        console.error('❌ Error al conectar a SignalR:', err);
      }
    };

    startConnection();

    // 1. Escuchar nuevas ofertas
    connection.on('ReceiveNewBid', (newBid) => {
      console.log('📬 Nueva puja recibida:', newBid);
      
      const newAmount = newBid.amount ?? newBid.monto ?? newBid.Amount;
      const newUserId = newBid.userId ?? newBid.buyerId ?? newBid.compradorId ?? newBid.BuyerId;
      const newUserName = newBid.userName ?? newBid.buyerName ?? newBid.compradorNombre ?? newBid.BuyerName;

      setCurrentPrice(newAmount);

      // Alerta si el usuario actual estaba liderando y fue superado
      setLatestBidderId(prevLeader => {
        if (prevLeader === userId && newUserId !== userId) {
          toast.error(`¡Has sido superado! Nueva oferta: ${formatCurrency(newAmount)}`);
        }
        return newUserId;
      });

      // Agregar nueva oferta al historial reactivo
      setBidsHistory(prev => [{
        userId: newUserId,
        userName: newUserName,
        amount: newAmount,
        time: newBid.timestamp || newBid.fechaPuja || new Date().toISOString()
      }, ...prev]);
    });

    // 2. Escuchar regla Anti-sniping (Extensión de tiempo en vivo)
    connection.on('AuctionTimeExtended', (data) => {
      console.log('⏱️ Extensión de tiempo recibida:', data);
      const updatedEndDate = data.newEndDate ?? data.NewEndDate;
      if (updatedEndDate) {
        setEndDate(updatedEndDate);
        toast.info("⏱️ ¡Regla anti-sniping activada! Se extendió el tiempo de la subasta por 2 minutos.");
      }
    });

    // 3. Escuchar cierre automático de subasta en vivo
    connection.on('AuctionClosed', (data) => {
      console.log('🏁 Subasta cerrada en tiempo real:', data);
      setAuction(prev => ({
        ...prev,
        status: data.status ?? data.Status ?? 'FINALIZADA'
      }));
      toast.warning("La subasta ha finalizado.");
    });

    // 4. Sincronización post-suspensión (Laptop asleep / Network drop)
    connection.onreconnected(async (connectionId) => {
      console.log(`🔄 Reconectado a SignalR (ID: ${connectionId}). Sincronizando estado oficial...`);
      try {
        const response = await apiClient.get(`/auctions/${id}`);
        const data = response.data;
        
        // Sincronizar el Offset del servidor y el nuevo EndDate
        const serverDateHeader = response.headers['date'];
        if (serverDateHeader) {
          serverTimeRef.current = new Date(serverDateHeader).toISOString();
        } else {
          serverTimeRef.current = new Date().toISOString();
        }
        setEndDate(data.endDate ?? data.fechaFin);

        // Sincronizar pujas, precio y estado
        setCurrentPrice(data.currentPrice ?? data.precioActual ?? 0);
        setAuction(prev => ({ ...prev, status: data.status ?? data.estado ?? prev.status }));

        const rawBids = data.bids || data.pujas || [];
        const formattedBids = rawBids.map(b => ({
          userId: b.buyerId || b.compradorId || b.comprador_id || b.userId,
          userName: b.buyerName || b.compradorNombre || b.userName,
          amount: b.amount ?? b.monto,
          time: b.time || b.fechaPuja || b.fecha_puja || b.createdAt || new Date().toISOString()
        })).sort((a, b) => new Date(b.time) - new Date(a.time));
        
        setBidsHistory(formattedBids);

        const topBidder = data.highestBidderId || data.compradorGanadorId || formattedBids[0]?.userId;
        if (topBidder) setLatestBidderId(topBidder);

        toast.success("Conexión recuperada. Tiempo y estado sincronizados.");
      } catch (err) {
        console.error("Error al resincronizar estado post-reconexión:", err);
      }
    });

    return () => {
      if (connection.state === 'Connected') {
        connection.invoke('LeaveAuctionGroup', id.toString()).catch(console.error);
        connection.stop();
      }
    };
  }, [auction?.id, id, userId]);

  // ==========================================
  // 3. REGLAS DE NEGOCIO Y ESTADOS
  // ==========================================
  const availableBalance = wallet?.availableBalance ?? wallet?.saldoDisponible ?? 0;
  
  const serverTimeRef = useRef(new Date().toISOString());
  const timeLeft = useCountdown(endDate, serverTimeRef.current);

  const isScheduled = status?.toUpperCase() === 'PROGRAMADA';
  const isEnded = timeLeft === 0 || status?.toUpperCase() === 'FINALIZADA' || status?.toUpperCase() === 'CERRADA' || status?.toUpperCase() === 'DESIERTA';
  const isEndingSoon = !isScheduled && timeLeft > 0 && timeLeft < 60;

  const hasInsufficientFunds = bidAmount > availableBalance;
  const userHasParticipated = bidsHistory.some(b => b.userId === userId);
  const isButtonDisabled = isEnded || isScheduled || hasInsufficientFunds || bidAmount < suggestedBid || isBidding;

  // Enviar oferta al backend
  const handleBidSubmit = async (e) => {
    e.preventDefault();
    if (isButtonDisabled) return;

    setIsBidding(true);
    try {
      // Rutas RESTful Nivel 2: compatibilidad con /bids o /auctions/bids
      let response;
      const payload = {
        auctionId: parseInt(id),
        buyerId: userId,
        amount: parseFloat(bidAmount)
      };

      try {
        response = await apiClient.post('/bids', payload);
      } catch (err) {
        if (err.response?.status === 404) {
          response = await apiClient.post('/auctions/bids', payload);
        } else {
          throw err;
        }
      }
      
      if (response.status === 200 || response.status === 201) {
        toast.success("Oferta enviada exitosamente");
      }
    } catch (error) {
      console.error('Error al pujar:', error);
      const serverMessage = error.response?.data?.message || error.response?.data?.detail;
      toast.error(serverMessage || "No se pudo procesar la oferta.");
    } finally {
      setIsBidding(false);
    }
  };

  // Formato de temporizador: Xd Xh Xm si > 24 horas
  const formatTime = (seconds) => {
    if (seconds <= 0) return '00:00:00';
    const d = Math.floor(seconds / 86400);
    const h = Math.floor((seconds % 86400) / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    const pad = (num) => num.toString().padStart(2, '0');

    if (d > 0) {
      return `${d}d ${pad(h)}h ${pad(m)}m`;
    }
    return `${pad(h)}:${pad(m)}:${pad(s)}`;
  };

  const formatCurrency = (value = 0) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency', currency: 'ARS', minimumFractionDigits: 0, maximumFractionDigits: 0
    }).format(value);
  };

  if (isLoadingAuction) {
    return (
      <div className="flex justify-center items-center py-40">
        <svg className="animate-spin h-12 w-12 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" strokeWidth="4"></circle>
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
      </div>
    );
  }

  if (!auction) {
    return <div className="p-8 text-center text-xl text-gray-500 font-semibold">Subasta no encontrada</div>;
  }

  return (
    <div className="w-full max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
      
      {/* COLUMNA IZQUIERDA: Detalle e Historial */}
      <div className="lg:col-span-8 flex flex-col gap-6">
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="relative h-96 w-full bg-gray-100">
            <img 
              src={imageUrl || 'https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=800&q=80'} 
              alt={title} 
              onError={(e) => {
                e.currentTarget.src = 'https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=800&q=80';
              }}
              className="w-full h-full object-cover"
            />
            <span className="absolute top-4 left-4 bg-white/90 backdrop-blur-md px-4 py-2 rounded-lg text-sm font-bold text-gray-800 shadow-sm uppercase tracking-wider">
              {categoryName}
            </span>
          </div>
          
          <div className="p-8">
            <h1 className="text-3xl font-extrabold text-gray-900 mb-4 leading-tight">{title}</h1>
            <h2 className="text-lg font-semibold text-gray-800 mb-2 border-b pb-2">Descripción del Lote</h2>
            <p className="text-gray-600 leading-relaxed whitespace-pre-wrap">
              {description || 'No hay descripción disponible para este lote.'}
            </p>
          </div>
        </div>

        {/* Historial de Pujas Reactivo */}
        <div className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
          <h2 className="text-lg font-semibold text-gray-800 mb-4 border-b pb-2">Historial de Movimientos</h2>
          {bidsHistory.length === 0 ? (
            <p className="text-gray-500 italic">No hay ofertas recientes. ¡Sé el primero en pujar!</p>
          ) : (
            <div className="flex flex-col gap-3">
              {bidsHistory.map((bid, index) => (
                <div key={index} className="flex justify-between items-center bg-gray-50 p-3 rounded-lg border border-gray-100 animate-fade-in">
                  <div className="flex items-center gap-3">
                    <span className="font-mono text-gray-400 text-xs">{new Date(bid.time).toLocaleTimeString()}</span>
                    <span className="font-semibold text-gray-700">
                      {bid.userName ? bid.userName : (bid.userId === userId ? 'Tú (Oferta Actual)' : `Usuario #${bid.userId}`)}
                    </span>
                  </div>
                  <span className="font-bold text-gray-900">{formatCurrency(bid.amount)}</span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* COLUMNA DERECHA: Panel de Acción */}
      <div className="lg:col-span-4 flex flex-col gap-6 sticky top-8">
        
        {/* Temporizador y Badges de Estado */}
        <div className={`p-8 rounded-3xl shadow-sm border text-center transition-colors flex flex-col items-center justify-center
          ${isEnded 
            ? 'bg-gray-100 border-gray-200' 
            : isScheduled
              ? 'bg-purple-50 border-purple-200'
              : isEndingSoon 
                ? 'bg-yellow-50 border-yellow-300 shadow-yellow-100/50' 
                : 'bg-blue-50 border-blue-100'
          }
        `}>
          <span className={`text-sm font-bold tracking-widest uppercase mb-2 
            ${isEnded ? 'text-gray-500' : isScheduled ? 'text-purple-600' : isEndingSoon ? 'text-yellow-600' : 'text-blue-500'}`}>
            {isEnded ? 'Subasta Finalizada' : isScheduled ? 'Subasta Programada' : isEndingSoon ? '¡Últimos Segundos!' : 'Tiempo Restante'}
          </span>
          <div className={`text-4xl md:text-5xl font-black font-mono tracking-tighter mb-4
            ${isEnded ? 'text-gray-400' : isScheduled ? 'text-purple-700' : isEndingSoon ? 'text-red-600 animate-pulse' : 'text-blue-700'}`}>
            {formatTime(timeLeft)}
          </div>
          
          {/* Indicador de Liderando / Superado */}
          {!isEnded && !isScheduled && latestBidderId && (
            latestBidderId === userId ? (
              <span className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-full bg-green-100 text-green-700 font-bold text-sm shadow-sm border border-green-200">
                ● Vas Liderando
              </span>
            ) : userHasParticipated ? (
              <span className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-full bg-red-100 text-red-700 font-bold text-sm shadow-sm border border-red-200 animate-pulse">
                ▲ Fuiste Superado (Outbid)
              </span>
            ) : null
          )}
        </div>

        {/* Consola de Puja */}
        <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100 flex flex-col gap-6">
          <div className="flex flex-col gap-1">
            <span className="text-sm font-medium text-gray-500">Oferta Actual</span>
            <span className="text-4xl font-extrabold text-gray-900 transition-all">
              {formatCurrency(currentPrice)}
            </span>
          </div>

          {isScheduled ? (
            <div className="bg-purple-50 border border-purple-200 text-purple-800 p-4 rounded-xl text-center font-medium text-sm">
              Esta subasta está programada. Las pujas se habilitarán automáticamente al iniciar el evento.
            </div>
          ) : (
            <form onSubmit={handleBidSubmit} className="flex flex-col gap-4">
              <div className="flex flex-col gap-2">
                <label className="text-sm font-semibold text-gray-700">Tu Oferta (Mínimo: {formatCurrency(suggestedBid)})</label>
                <div className="relative">
                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-bold">$</span>
                  <input 
                    type="number"
                    value={bidAmount}
                    onChange={(e) => setBidAmount(Number(e.target.value))}
                    min={suggestedBid}
                    step={minIncrement || 1}
                    disabled={isEnded || isScheduled}
                    className="w-full pl-8 pr-4 py-4 rounded-xl border border-gray-200 text-xl font-bold text-gray-800 bg-gray-50 focus:outline-none focus:ring-4 focus:ring-blue-500/20 focus:border-blue-500 transition-all disabled:opacity-50"
                  />
                </div>
                
                {hasInsufficientFunds && !isEnded && (
                  <p className="text-red-500 text-sm font-medium mt-1">
                    Saldo disponible insuficiente ({formatCurrency(availableBalance)})
                  </p>
                )}
              </div>

              <button 
                type="submit"
                disabled={isButtonDisabled}
                className={`w-full py-4 rounded-xl text-xl font-black uppercase tracking-wider transition-all shadow-md flex items-center justify-center
                  ${isEnded 
                    ? 'bg-gray-300 text-gray-500 cursor-not-allowed shadow-none' 
                    : isButtonDisabled 
                      ? 'bg-gray-400 text-white cursor-not-allowed shadow-none'
                      : 'bg-blue-600 hover:bg-blue-700 active:bg-blue-800 text-white hover:-translate-y-0.5 hover:shadow-lg'
                  }
                `}
              >
                {isBidding ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Procesando...
                  </>
                ) : isEnded ? 'Subasta Cerrada' : 'Ofertar Ahora'}
              </button>
            </form>
          )}
        </div>

      </div>
    </div>
  );
};

export default LiveBiddingRoom;