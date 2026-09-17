import { useState, useEffect, useMemo } from 'react';

const useCountdown = (targetDate, serverTimeStr) => {
  const [timeLeft, setTimeLeft] = useState(0);

  // El offset debe calcularse una sola vez basado en el serverTimeStr provisto.
  // serverTimeMs representa el tiempo del servidor en el instante (Date.now()) que se recibió serverTimeStr.
  const offsetMs = useMemo(() => {
    if (!serverTimeStr) return 0;
    return new Date(serverTimeStr).getTime() - Date.now();
  }, [serverTimeStr]);

  useEffect(() => {
    if (!targetDate) return;

    const endTimeMs = new Date(targetDate).getTime();

    const calculateTimeLeft = () => {
      const currentSimulatedServerTime = Date.now() + offsetMs;
      const differenceInSeconds = Math.floor((endTimeMs - currentSimulatedServerTime) / 1000);
      return differenceInSeconds > 0 ? differenceInSeconds : 0;
    };

    const initialTimeLeft = calculateTimeLeft();
    if (initialTimeLeft <= 0) {
      setTimeLeft(0);
      return;
    }

    setTimeLeft(initialTimeLeft);

    const timer = setInterval(() => {
      const remaining = calculateTimeLeft();
      setTimeLeft(remaining);
      
      if (remaining <= 0) {
        clearInterval(timer);
      }
    }, 1000);

    return () => clearInterval(timer);
  }, [targetDate, offsetMs]);

  return timeLeft;
};

export default useCountdown;

