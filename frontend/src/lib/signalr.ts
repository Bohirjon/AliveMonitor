import * as signalR from '@microsoft/signalr';
import type { AuthTokens } from '@/types';

let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection | null {
  return connection;
}

export function createConnection(): signalR.HubConnection {
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/endpoint-status', {
      accessTokenFactory: () => {
        const stored = localStorage.getItem('tokens');
        if (!stored) return '';
        const tokens: AuthTokens = JSON.parse(stored);
        return tokens.accessToken;
      },
    })
    .withAutomaticReconnect()
    .build();

  return connection;
}

export function destroyConnection() {
  if (connection) {
    connection.stop();
    connection = null;
  }
}
