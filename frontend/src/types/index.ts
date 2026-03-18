export const EndpointStatus = {
  Healthy: 'Healthy',
  Unhealthy: 'Unhealthy',
  Disabled: 'Disabled',
} as const;

export type EndpointStatus = (typeof EndpointStatus)[keyof typeof EndpointStatus];

export interface User {
  id: string;
  email: string;
  name: string;
  avatarUrl?: string;
  alertEmail: string;
  telegramLinked: boolean;
  webhookUrl?: string;
}

export interface MonitoredEndpoint {
  id: string;
  friendlyName: string;
  url: string;
  intervalMinutes: number;
  timeoutSeconds: number;
  maxRetries: number;
  isEnabled: boolean;
  customHeaders?: Record<string, string>;
  expectedStatusCode: number;
  jsonPropertyName?: string;
  jsonPropertyExpectedValue?: string;
  currentStatus: EndpointStatus;
  lastCheckedAt?: string;
  createdAt: string;
  updatedAt: string;
  teamId?: string;
  teamName?: string;
  sslCheckEnabled: boolean;
  sslLastCheckedAt?: string;
  sslCertificateExpiresAt?: string;
  sslDaysUntilExpiry?: number;
}

export interface HealthCheckLog {
  id: string;
  endpointId: string;
  checkedAt: string;
  httpStatusCode?: number;
  responseTimeMs: number;
  isHealthy: boolean;
  errorMessage?: string;
  retryAttempts: number;
}

export interface Incident {
  id: string;
  endpointId: string;
  openedAt: string;
  lastNotifiedAt: string;
  resolvedAt?: string;
  failureCount: number;
}

export interface AnalyticsSummary {
  uptimePercentage: number;
  avgResponseTimeMs: number;
  totalChecks: number;
  totalIncidents: number;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateEndpointRequest {
  friendlyName: string;
  url: string;
  intervalMinutes: number;
  timeoutSeconds: number;
  maxRetries?: number;
  customHeaders?: Record<string, string>;
  expectedStatusCode: number;
  jsonPropertyName?: string;
  jsonPropertyExpectedValue?: string;
  teamId?: string;
  sslCheckEnabled?: boolean;
}

export interface UpdateEndpointRequest extends CreateEndpointRequest {
  id: string;
}

export interface Team {
  id: string;
  name: string;
  memberEmails: string[];
  telegramLinked: boolean;
  webhookUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface LinkCodeResponse {
  code: string;
  deepLink: string;
  expiresAt: string;
}

export interface TelegramStatusResponse {
  isLinked: boolean;
  chatId?: string;
}

export interface CreateTeamRequest {
  name: string;
  memberEmails: string[];
  webhookUrl?: string;
}
