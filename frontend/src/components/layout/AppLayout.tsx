import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Activity, Settings, LogOut, Moon, Sun } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { useTheme } from '@/context/ThemeContext';
import { revokeToken } from '@/api/auth';
import { Button } from '@/components/ui/Button';
import type { ReactNode } from 'react';

const navItems = [
  { path: '/dashboard', label: 'Dashboard', icon: Activity },
  { path: '/settings', label: 'Settings', icon: Settings },
];

export default function AppLayout({ children }: { children: ReactNode }) {
  const { user, tokens, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const location = useLocation();
  const navigate = useNavigate();

  const handleSignOut = async () => {
    if (tokens?.refreshToken) {
      try { await revokeToken(tokens.refreshToken); } catch { /* ignore */ }
    }
    logout();
    navigate('/signin');
  };

  return (
    <div className="flex min-h-screen bg-background">
      {/* Sidebar */}
      <aside className="flex w-64 flex-col border-r border-border bg-card">
        <div className="flex h-16 items-center gap-2 border-b border-border px-6">
          <Activity className="h-6 w-6 text-primary" />
          <span className="text-lg font-bold text-foreground">AliveMonitor</span>
        </div>

        <nav className="flex-1 space-y-1 p-4">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-primary/10 text-primary'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                }`}
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="border-t border-border p-4">
          {user && (
            <div className="mb-3 flex items-center gap-3">
              {user.avatarUrl ? (
                <img src={user.avatarUrl} alt="" className="h-8 w-8 rounded-full" />
              ) : (
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-xs font-medium text-primary-foreground">
                  {user.name.charAt(0)}
                </div>
              )}
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-foreground">{user.name}</p>
                <p className="truncate text-xs text-muted-foreground">{user.email}</p>
              </div>
            </div>
          )}
          <div className="flex gap-2">
            <Button variant="ghost" size="icon" onClick={toggleTheme} title="Toggle theme">
              {theme === 'light' ? <Moon className="h-4 w-4" /> : <Sun className="h-4 w-4" />}
            </Button>
            <Button variant="ghost" size="icon" onClick={handleSignOut} title="Sign out">
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto">
        <div className="mx-auto max-w-6xl p-6">{children}</div>
      </main>
    </div>
  );
}
