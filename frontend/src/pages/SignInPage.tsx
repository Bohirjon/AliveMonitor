import { GoogleLogin, GoogleOAuthProvider } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { googleSignIn } from '@/api/auth';
import { toast } from 'sonner';
import { Activity, Shield, Bell, Users } from 'lucide-react';

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? '';

const features = [
  { icon: Activity, text: 'Real-time uptime monitoring' },
  { icon: Shield, text: 'SSL certificate checks' },
  { icon: Bell, text: 'Instant alert notifications' },
  { icon: Users, text: 'Team-based alerting' },
];

export default function SignInPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  if (isAuthenticated) {
    navigate('/dashboard', { replace: true });
    return null;
  }

  const handleSuccess = async (credential: string | undefined) => {
    try {
      if (!credential) return;
      const result = await googleSignIn(credential);
      login(result.tokens, result.user);
      navigate('/dashboard', { replace: true });
    } catch {
      toast.error('Sign in failed. Please try again.');
    }
  };

  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <div className="flex min-h-screen">
        {/* Left Panel — branding & features (hidden on small screens) */}
        <div className="hidden lg:flex lg:w-3/5 flex-col justify-between bg-gradient-to-br from-primary to-primary/80 p-12 text-primary-foreground">
          <div>
            <div className="flex items-center gap-3">
              <Activity className="h-8 w-8" />
              <span className="text-2xl font-bold">AliveMonitor</span>
            </div>
            <p className="mt-4 text-lg text-primary-foreground/80">
              Keep your services alive, always.
            </p>
          </div>

          <div className="space-y-6">
            {features.map(({ icon: Icon, text }) => (
              <div key={text} className="flex items-center gap-4">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary-foreground/10">
                  <Icon className="h-5 w-5" />
                </div>
                <span className="text-sm font-medium">{text}</span>
              </div>
            ))}
          </div>

          <div className="flex items-center gap-3 text-sm text-primary-foreground/70">
            <span className="relative flex h-3 w-3">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-primary-foreground/60" />
              <span className="relative inline-flex h-3 w-3 rounded-full bg-primary-foreground" />
            </span>
            Monitoring systems worldwide
          </div>
        </div>

        {/* Right Panel — sign-in form */}
        <div className="flex w-full flex-col items-center justify-center bg-background px-6 lg:w-2/5">
          {/* Mobile-only branding */}
          <div className="mb-8 flex items-center gap-2 lg:hidden">
            <Activity className="h-6 w-6 text-primary" />
            <span className="text-xl font-bold text-foreground">AliveMonitor</span>
          </div>

          <div className="w-full max-w-sm space-y-8 rounded-lg border border-border bg-card p-8 shadow-sm">
            <div className="text-center">
              <h1 className="text-2xl font-bold text-foreground">Welcome back</h1>
              <p className="mt-2 text-sm text-muted-foreground">
                Sign in to continue to AliveMonitor
              </p>
            </div>
            <div className="flex justify-center">
              <GoogleLogin
                onSuccess={(response) => handleSuccess(response.credential)}
                onError={() => {
                  toast.error('Google sign in failed.');
                }}
                theme="outline"
                size="large"
                width={320}
              />
            </div>
            <p className="text-center text-xs text-muted-foreground">
              By signing in, you agree to our Terms of Service
            </p>
          </div>
        </div>
      </div>
    </GoogleOAuthProvider>
  );
}
