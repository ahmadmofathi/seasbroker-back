import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import 'remixicon/fonts/remixicon.css';
import "./assets/css/style.css";
import "./assets/css/meanmenu.min.css";
import "./assets/css/responsive.css";
import "./assets/css/animate.min.css";
import "./assets/css/fontawesome.all.min.css";
import "./assets/css/color.css";
import './index.css';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';

const root = document.getElementById('root');
if (!root) {
  throw new Error('Root element not found');
}
createRoot(root).render(
  <StrictMode>
    <App />
  </StrictMode>,
);


