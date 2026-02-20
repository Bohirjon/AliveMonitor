import { Component, type ReactNode } from 'react';
import { Button } from './Button';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-[400px] flex-col items-center justify-center gap-4 p-8 text-center">
          <h2 className="text-lg font-semibold text-foreground">Something went wrong</h2>
          <p className="text-sm text-muted-foreground">
            {this.state.error?.message ?? 'An unexpected error occurred.'}
          </p>
          <Button onClick={() => this.setState({ hasError: false })}>Try again</Button>
        </div>
      );
    }

    return this.props.children;
  }
}
