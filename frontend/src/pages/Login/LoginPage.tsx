import { useEffect, useMemo, useState, type FormEvent } from "react";
import axios from "axios";
import {
  Eye,
  EyeOff,
  LoaderCircle,
  LockKeyhole,
  LogIn,
  Mail,
  ShieldCheck,
} from "lucide-react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/useAuth";
import "./LoginPage.css";

interface LoginLocationState {
  from?: {
    pathname?: string;
    search?: string;
    hash?: string;
  };
}

interface ApiErrorResponse {
  message?: string;
}

function getLoginError(error: unknown): string {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    if (error.response?.status === 401) {
      return (
        error.response.data?.message ??
        "Adresse électronique ou mot de passe incorrect."
      );
    }

    if (!error.response) {
      return "Impossible de joindre l’API. " + "Vérifiez qu’elle est démarrée.";
    }
  }

  return "La connexion a échoué. Veuillez réessayer.";
}

function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login, isAuthenticated } = useAuth();

  const location = useLocation();
  const navigate = useNavigate();

  const destination = useMemo(() => {
    const state = location.state as LoginLocationState | null;

    return [
      state?.from?.pathname ?? "/",
      state?.from?.search ?? "",
      state?.from?.hash ?? "",
    ].join("");
  }, [location.state]);

  useEffect(() => {
    if (isAuthenticated) {
      navigate(destination, {
        replace: true,
      });
    }
  }, [destination, isAuthenticated, navigate]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await login({
        email: email.trim(),
        password,
      });
    } catch (error) {
      setErrorMessage(getLoginError(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-container">
        <header className="login-brand">
          <div className="login-brand-shield">
            <ShieldCheck size={42} />
          </div>

          <h1>
            Identity <span>Audit</span>
          </h1>

          <p>CIS Controls 5 &amp; 6</p>
        </header>

        <div className="login-card">
          <header className="login-card-header">
            <h2>Connexion</h2>
            <span className="login-title-line" />
            <p>Accès réservé aux utilisateurs autorisés</p>
          </header>

          <form onSubmit={handleSubmit}>
            {errorMessage && (
              <div className="login-error" role="alert">
                {errorMessage}
              </div>
            )}

            <label htmlFor="login-email">Adresse électronique</label>

            <div className="login-input">
              <Mail size={20} />

              <input
                id="login-email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="nom@identityaudit.local"
                autoComplete="username"
                maxLength={320}
                required
                autoFocus
              />
            </div>

            <label htmlFor="login-password">Mot de passe</label>

            <div className="login-input">
              <LockKeyhole size={20} />

              <input
                id="login-password"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Votre mot de passe"
                autoComplete="current-password"
                maxLength={200}
                required
              />

              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowPassword((current) => !current)}
                aria-label={
                  showPassword
                    ? "Masquer le mot de passe"
                    : "Afficher le mot de passe"
                }
                aria-pressed={showPassword}
              >
                {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            <button
              type="submit"
              className="login-submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <LoaderCircle className="login-spinner" size={20} />
                  Connexion...
                </>
              ) : (
                <>
                  <LogIn size={20} />
                  Se connecter
                </>
              )}
            </button>
          </form>
        </div>

        <footer className="login-footer">© 2026 Identity Audit</footer>
      </section>
    </main>
  );
}

export default LoginPage;
