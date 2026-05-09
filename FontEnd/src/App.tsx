import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './components/ProtectedRoute';

// Public Pages
import Login from './pages/public/Login';
import Register from './pages/public/Register';

// Customer Pages
import CustomerLayout from './layouts/CustomerLayout';
import CustomerHome from './pages/customer/Home';
import CreateBooking from './pages/customer/CreateBooking';
import BookingsList from './pages/customer/BookingsList';
import CustomerProfile from './pages/customer/CustomerProfile';

// Worker Pages
import WorkerLayout from './layouts/WorkerLayout';
import WorkerDashboard from './pages/worker/Dashboard';
import WorkerKYC from './pages/worker/WorkerKYC';
import WorkerProfile from './pages/worker/WorkerProfile';
import WorkerBookingsList from './pages/worker/WorkerBookingsList';
import CreateQuotation from './pages/worker/CreateQuotation';
import WalletDashboard from './pages/worker/WalletDashboard';

// Shared Pages
import NotificationsList from './pages/shared/NotificationsList';
import BookingDetail from './pages/shared/BookingDetail';
import PaymentReturn from './pages/shared/PaymentReturn';
import ChatRoom from './pages/shared/ChatRoom';
import CreateDispute from './pages/shared/CreateDispute';

import AdminLayout from './layouts/AdminLayout';
import AdminKYC from './pages/admin/AdminKYC';
import WorkersManagement from './pages/admin/WorkersManagement';
import DisputeManagement from './pages/admin/DisputeManagement';
import AdminDisputeDetail from './pages/admin/AdminDisputeDetail';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminSettings from './pages/admin/AdminSettings';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        {/* Customer & Worker Shared Protected Routes */}
        <Route element={<ProtectedRoute allowedRoles={['CUSTOMER', 'WORKER']} />}>
          <Route path="/payment/result" element={<PaymentReturn />} />
          <Route path="/bookings/:id/chat" element={<ChatRoom />} />
        </Route>

        {/* Customer Routes */}
        <Route element={<ProtectedRoute allowedRoles={['CUSTOMER']} />}>
          <Route element={<CustomerLayout />}>
            <Route path="/" element={<CustomerHome />} />
            <Route path="/customer/bookings" element={<BookingsList />} />
            <Route path="/customer/bookings/:id" element={<BookingDetail />} />
            <Route path="/customer/notifications" element={<NotificationsList />} />
            <Route path="/customer/profile" element={<CustomerProfile />} />
          </Route>
          {/* Create Booking is full screen, outside bottom nav layout */}
          <Route path="/customer/booking/create" element={<CreateBooking />} />
          <Route path="/customer/bookings/:id/dispute" element={<CreateDispute />} />
        </Route>

        {/* Worker Routes */}
        <Route element={<ProtectedRoute allowedRoles={['WORKER']} />}>
          <Route element={<WorkerLayout />}>
            <Route path="/worker" element={<WorkerDashboard />} />
            <Route path="/worker/bookings" element={<WorkerBookingsList />} />
            <Route path="/worker/bookings/:id" element={<BookingDetail />} />
            <Route path="/worker/notifications" element={<NotificationsList />} />
            <Route path="/worker/kyc" element={<WorkerKYC />} />
            <Route path="/worker/profile" element={<WorkerProfile />} />
          </Route>
          {/* Full screen pages */}
          <Route path="/worker/bookings/:id/quotation/create" element={<CreateQuotation />} />
          <Route path="/worker/wallet" element={<WalletDashboard />} />
        </Route>

        {/* Admin Routes */}
        <Route element={<ProtectedRoute allowedRoles={['ADMIN']} />}>
          <Route element={<AdminLayout />}>
            <Route path="/admin" element={<AdminDashboard />} />
            <Route path="/admin/kyc" element={<AdminKYC />} />
            <Route path="/admin/workers" element={<WorkersManagement />} />
            <Route path="/admin/disputes" element={<DisputeManagement />} />
            <Route path="/admin/disputes/:id" element={<AdminDisputeDetail />} />
            <Route path="/admin/settings" element={<AdminSettings />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="/unauthorized" element={<div className="p-10 text-center text-red-500 text-2xl font-bold">Unauthorized Access</div>} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
