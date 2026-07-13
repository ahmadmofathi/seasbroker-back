import { BrowserRouter, Routes, Route } from 'react-router';
import ScrollToTop from './component/Home/ScrollToTop';
import { AdminAuthProvider } from './context/AdminAuthContext';
import { AlertProvider } from './context/AlertContext';

import Navbar from './component/Common/Navbar/Navbar';
import Footer from './component/Common/Footer';
import CopyRight from './component/Common/CopyRight';
import Chat from './component/App/Chat';

import Home from './page/Home';
import About from './page/About';
import Service from './page/Service';
import ServiceDetails from './page/ServiceDetails';
import OurTeamArea from './page/OurTeam';
import Testimonials from './page/Testimonial';
import Faqs from './page/Faqs';
import TrackShipmentView from './page/TrackShipmentView';
import PrivacyPolicy from './page/PrivacyPolicy';
import TermsCondition from './page/TermsCondition';
import Error from './page/Error';
import Contact from './page/Contact';
import RequestQuote from './page/RequestQuote';
import TrackShipmentForm from './page/TrackShipmentForm';
import RequestClearance from './page/RequestClearance';
import ClearanceOffice from './page/ClearanceOffices';
import RequestRoute from './page/RequestRoute';
import ShipRoutes from './page/ShipRoutes';

import ProtectedAdminRoute from './component/admin/ProtectedAdminRoute';
import AdminLayout from './page/admin/AdminLayout';
import AdminLogin from './page/admin/AdminLogin';
import AdminDashboard from './page/admin/AdminDashboard';
import AdminChats from './page/admin/AdminChats';
import AdminQuotes from './page/admin/AdminQuotes';
import AdminCargo from './page/admin/AdminCargo';
import AdminVessels from './page/admin/AdminVessels';
import AdminMatching from './page/admin/AdminMatching';
import AdminNotifications from './page/admin/AdminNotifications';
import AdminApiTest from './page/admin/AdminApiTest';

const PublicLayout: React.FC = () => (
  <ScrollToTop>
    <Navbar />
    <Routes>
      <Route path='/' element={<Home />} />
      <Route path='/about' element={<About />} />
      <Route path='/service' element={<Service />} />
      <Route path='/service_details' element={<ServiceDetails />} />
      <Route path='/our_team' element={<OurTeamArea />} />
      <Route path='/testimonials' element={<Testimonials />} />
      <Route path='/faqs' element={<Faqs />} />
      <Route path='/track_ship' element={<TrackShipmentForm />} />
      <Route path='/request_quote' element={<RequestQuote />} />
      <Route path='/privacyPolicy' element={<PrivacyPolicy />} />
      <Route path='/terms' element={<TermsCondition />} />
      <Route path='/contact' element={<Contact />} />
      <Route path='/your_shipment' element={<TrackShipmentView />} />
      <Route path='/clearance_offices' element={<ClearanceOffice />} />
      <Route path='/request_route' element={<RequestRoute />} />
      <Route path='/request_clearance' element={<RequestClearance />} />
      <Route path='/ship_routes' element={<ShipRoutes />} />
      <Route element={<Error />} />
    </Routes>
    <Footer />
    <CopyRight />
    <Chat />
  </ScrollToTop>
);

const AdminRoutes: React.FC = () => (
  <AdminAuthProvider>
    <Routes>
      <Route path="login" element={<AdminLogin />} />
      <Route element={<ProtectedAdminRoute />}>
        <Route element={<AdminLayout />}>
          <Route index element={<AdminDashboard />} />
          <Route path="quotes" element={<AdminQuotes />} />
          <Route path="chats" element={<AdminChats />} />
          <Route path="cargo" element={<AdminCargo />} />
          <Route path="vessels" element={<AdminVessels />} />
          <Route path="matching" element={<AdminMatching />} />
          <Route path="notifications" element={<AdminNotifications />} />
          <Route path="api-test" element={<AdminApiTest />} />
        </Route>
      </Route>
    </Routes>
  </AdminAuthProvider>
);

const App: React.FC = () => {
  return (
    <AlertProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/admin/*" element={<AdminRoutes />} />
          <Route path="/*" element={<PublicLayout />} />
        </Routes>
      </BrowserRouter>
    </AlertProvider>
  );
};

export default App;
