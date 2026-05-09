import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { SignalRProvider } from './signalr/SignalRContext'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SignalRProvider>
      <App />
    </SignalRProvider>
  </StrictMode>,
)
