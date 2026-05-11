import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { SignalRProvider } from './signalr/SignalRContext.tsx';
import moment from 'moment';
import 'moment/locale/vi';

moment.locale('vi');

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SignalRProvider>
      <App />
    </SignalRProvider>
  </StrictMode>,
)
