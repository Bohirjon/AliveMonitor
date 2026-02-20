import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '@/context/AuthContext';
import { ThemeProvider } from '@/context/ThemeContext';
import { Toaster } from 'sonner';
import ProtectedRoute from '@/components/auth/ProtectedRoute';
import SignInPage from '@/pages/SignInPage';
import DashboardPage from '@/pages/DashboardPage';
import EndpointDetailPage from '@/pages/EndpointDetailPage';
import SettingsPage from '@/pages/SettingsPage';
import { ErrorBoundary } from '@/components/ui/ErrorBoundary';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
});

function App() {
  return (
    <ErrorBoundary>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/signin" element={<SignInPage />} />
              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute>
                    <DashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/endpoints/:id"
                element={
                  <ProtectedRoute>
                    <EndpointDetailPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/settings"
                element={
                  <ProtectedRoute>
                    <SettingsPage />
                  </ProtectedRoute>
                }
              />
            </Routes>
          </BrowserRouter>
          <Toaster position="top-right" />
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;
