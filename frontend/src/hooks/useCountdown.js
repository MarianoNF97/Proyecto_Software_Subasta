import { useState, useEffect } from 'react';

const useCountdown = (targetDate, serverTime) => {
  const [timeLeft, setTimeLeft] = useState(0);

  useEffect(() => {
    if (!targetDate) return;

    // Si no proveen serverTime, intentamos usar local como fallback (no ideal en producción)
    const currentServerTime = serverTime ? new Date(serverTime).getTime() : Date.now();
    const endTime = new Date(targetDate).getTime();
    
    // Calculamos la diferencia inicial confiable
    let differenceInSeconds = Math.floor((endTime - currentServerTime) / 1000);
    
    if (differenceInSeconds <= 0) {
      setTimeLeft(0);
      return;
    }

    setTimeLeft(differenceInSeconds);

    // Descontamos de a 1 segundo del delta verificado, ignorando la hora local del SO
    const timer = setInterval(() => {
      differenceInSeconds -= 1;
      
      if (differenceInSeconds <= 0) {
        clearInterval(timer);
        setTimeLeft(0);
      } else {
        setTimeLeft(differenceInSeconds);
      }
    }, 1000);

    return () => clearInterval(timer);
  }, [targetDate, serverTime]); // Si SignalR empuja un nuevo targetDate, el useEffect se reinicia automáticamente

  return timeLeft;
};

export default useCountdown;

