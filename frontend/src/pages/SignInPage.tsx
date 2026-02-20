import { GoogleLogin, GoogleOAuthProvider } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { googleSignIn } from '@/api/auth';
import { toast } from 'sonner';

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? '';

export default function SignInPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  if (isAuthenticated) {
    navigate('/dashboard', { replace: true });
    return null;
  }

  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <div className="flex min-h-screen items-center justify-center bg-background">
        <div className="w-full max-w-sm space-y-8 rounded-lg border border-border bg-card p-8 shadow-sm">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-foreground">AliveMonitor</h1>
            <p className="mt-2 text-sm text-muted-foreground">
              Monitor your APIs and services
            </p>
          </div>
          <div className="flex justify-center">
            <GoogleLogin
              onSuccess={async (response) => {
                try {
                  if (!response.credential) return;
                  const result = await googleSignIn(response.credential);
                  login(result.tokens, result.user);
                  navigate('/dashboard', { replace: true });
                } catch {
                  toast.error('Sign in failed. Please try again.');
                }
              }}
              onError={() => {
                toast.error('Google sign in failed.');
              }}
              theme="outline"
              size="large"
              width={320}
            />
          </div>
        </div>
      </div>
    </GoogleOAuthProvider>
  );
}
