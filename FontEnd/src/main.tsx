import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { SignalRProvider } from './signalr/SignalRContext.tsx';
import { GoogleOAuthProvider } from '@react-oauth/google';
import moment from 'moment';
import 'moment/locale/vi';

moment.locale('vi');

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID || '';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <SignalRProvider>
        <App />
      </SignalRProvider>
    </GoogleOAuthProvider>
  </StrictMode>,
)
