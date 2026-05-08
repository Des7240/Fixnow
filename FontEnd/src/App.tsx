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

// Worker Pages
import WorkerLayout from './layouts/WorkerLayout';
import WorkerDashboard from './pages/worker/Dashboard';
import WorkerKYC from './pages/worker/WorkerKYC';
import WorkerProfile from './pages/worker/WorkerProfile';

// Admin Pages
import AdminLayout from './layouts/AdminLayout';
import AdminKYC from './pages/admin/AdminKYC';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        {/* Customer Routes */}
        <Route element={<ProtectedRoute allowedRoles={['CUSTOMER']} />}>
          <Route element={<CustomerLayout />}>
            <Route path="/" element={<CustomerHome />} />
            <Route path="/customer/bookings" element={<BookingsList />} />
            <Route path="/customer/notifications" element={<div>Notifications</div>} />
            <Route path="/customer/profile" element={<div>Profile</div>} />
          </Route>
          {/* Create Booking is full screen, outside bottom nav layout */}
          <Route path="/customer/booking/create" element={<CreateBooking />} />
        </Route>

        {/* Worker Routes */}
        <Route element={<ProtectedRoute allowedRoles={['WORKER']} />}>
          <Route element={<WorkerLayout />}>
            <Route path="/worker" element={<WorkerDashboard />} />
            <Route path="/worker/bookings" element={<div>Worker Bookings</div>} />
            <Route path="/worker/kyc" element={<WorkerKYC />} />
            <Route path="/worker/profile" element={<WorkerProfile />} />
          </Route>
        </Route>

        {/* Admin Routes */}
        <Route element={<ProtectedRoute allowedRoles={['ADMIN']} />}>
          <Route element={<AdminLayout />}>
            <Route path="/admin" element={<div className="p-8 text-2xl font-bold">Admin Dashboard Overview</div>} />
            <Route path="/admin/kyc" element={<AdminKYC />} />
            <Route path="/admin/workers" element={<div className="p-8 text-2xl font-bold">Workers Management</div>} />
            <Route path="/admin/settings" element={<div className="p-8 text-2xl font-bold">Settings</div>} />
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
