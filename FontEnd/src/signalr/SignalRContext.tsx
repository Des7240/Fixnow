import React, { createContext, useContext, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../stores/authStore';

interface SignalRContextType {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
}

const SignalRContext = createContext<SignalRContextType>({
  connection: null,
  isConnected: false,
});

export const useSignalR = () => useContext(SignalRContext);

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { accessToken, isAuthenticated } = useAuthStore();
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    // Only connect if user is authenticated and we have a token
    if (!isAuthenticated || !accessToken) {
      if (connection) {
        connection.stop();
        setConnection(null);
        setIsConnected(false);
      }
      return;
    }

    const BASE_URL = import.meta.env.VITE_API_URL?.replace('/api/v1', '') || 'http://localhost:8080';
    
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/notification`, {
        accessTokenFactory: () => accessToken,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry strategy
      .configureLogging(signalR.LogLevel.Information)
      .build();

    setConnection(newConnection);
  }, [accessToken, isAuthenticated]);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('SignalR Connected');
          setIsConnected(true);
        })
        .catch((e) => console.log('SignalR Connection Error: ', e));

      connection.onreconnecting((error) => {
        console.log('SignalR Reconnecting...', error);
        setIsConnected(false);
      });

      connection.onreconnected((connectionId) => {
        console.log('SignalR Reconnected. Connection ID:', connectionId);
        setIsConnected(true);
      });

      connection.onclose((error) => {
        console.log('SignalR Connection Closed', error);
        setIsConnected(false);
      });

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  return (
    <SignalRContext.Provider value={{ connection, isConnected }}>
      {children}
    </SignalRContext.Provider>
  );
};
