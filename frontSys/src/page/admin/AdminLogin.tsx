import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { useAdminAuth } from "../../context/AdminAuthContext";
import { formatApiError } from "../../utils/formatApiError";
import { useAlert } from "../../context/AlertContext";
import logo from "../../assets/img/Logo_trans.png";
import "../../assets/css/admin.css";

const AdminLogin: React.FC = () => {
  const navigate = useNavigate();
  const { login, isAuthenticated } = useAdminAuth();
  const [identity, setIdentity] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const { error: showError } = useAlert();

  useEffect(() => {
    if (isAuthenticated) {
      void navigate("/admin", { replace: true });
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await login(identity, password);
      void navigate("/admin");
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="admin-app">
      <div className="admin-login-page">
        <div className="admin-login-brand">
          <div className="admin-login-brand-content">
            <img src={logo} alt="Seas Broker" />
            <h1>SEAS BROKER</h1>
            <p>
              Maritime logistics &amp; brokerage management platform. Secure
              admin access for operations, matching, and fleet control.
            </p>
          </div>
        </div>

        <div className="admin-login-form-side">
          <div className="admin-login-card">
            <div className="login-logo">
              <img src={logo} alt="Seas Broker" />
              <span>SEAS BROKER</span>
            </div>

            <h2>Admin Sign In</h2>
            <p className="login-subtitle">
              Enter your credentials to access the dashboard
            </p>

            <form
              onSubmit={(e) => {
                void handleSubmit(e);
              }}
            >
              <div className="admin-field">
                <label htmlFor="identity">Email Address</label>
                <input
                  id="identity"
                  type="email"
                  className="admin-input"
                  value={identity}
                  onChange={(e) => setIdentity(e.target.value)}
                  placeholder="test@test.com"
                  required
                  autoComplete="username"
                />
              </div>

              <div className="admin-field">
                <label htmlFor="password">Password</label>
                <div style={{ position: "relative" }}>
                  <input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    className="admin-input"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="••••••••"
                    required
                    autoComplete="current-password"
                    style={{ paddingRight: "2.75rem" }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    style={{
                      position: "absolute",
                      right: "12px",
                      top: "50%",
                      transform: "translateY(-50%)",
                      background: "none",
                      border: "none",
                      color: "#94a3b8",
                      cursor: "pointer",
                      fontSize: "1.1rem",
                    }}
                    aria-label={
                      showPassword ? "Hide password" : "Show password"
                    }
                  >
                    <i
                      className={
                        showPassword ? "ri-eye-off-line" : "ri-eye-line"
                      }
                    />
                  </button>
                </div>
              </div>

              <button
                type="submit"
                className="admin-btn-primary"
                disabled={loading}
              >
                {loading ? "Signing in…" : "Sign In to Dashboard"}
              </button>
            </form>

            <a href="/" className="admin-login-back">
              <i className="ri-arrow-left-line" /> Back to website
            </a>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminLogin;
