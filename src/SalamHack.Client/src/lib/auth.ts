export type AuthUser = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  role: string;
  createdAt?: string;
  updatedAt?: string | null;
};

export type AuthSessionResponse = AuthUser & {
  createdAt: string;
  token?: {
    accessToken?: string;
    expiresOnUtc?: string;
  };
};

export type TokenResponse = {
  accessToken?: string;
  expiresOnUtc?: string;
};

export type ApiResponse<T> = {
  success?: boolean;
  data?: T;
  message?: string | null;
  errors?: unknown;
  traceId?: string;
};

export function unwrapApiResponse<T>(payload: unknown): T {
  if (payload && typeof payload === "object" && "data" in payload) {
    return (payload as ApiResponse<T>).data as T;
  }

  return payload as T;
}

export function getApiErrorMessage(payload: unknown, fallback: string) {
  if (!payload || typeof payload !== "object") return fallback;

  const response = payload as ApiResponse<unknown> & {
    detail?: string;
    title?: string;
  };

  return response.message ?? response.detail ?? response.title ?? fallback;
}

export function storeAuthSession(result: AuthSessionResponse) {
  if (!result.token?.accessToken) return;

  localStorage.setItem("accessToken", result.token.accessToken);
  localStorage.setItem("accessTokenExpiresOnUtc", result.token.expiresOnUtc ?? "");
  localStorage.setItem(
    "currentUser",
    JSON.stringify({
      id: result.id,
      email: result.email,
      firstName: result.firstName,
      lastName: result.lastName,
      phoneNumber: result.phoneNumber,
      role: result.role,
      createdAt: result.createdAt,
    } satisfies AuthUser),
  );
  window.dispatchEvent(new Event("auth:user-updated"));
}

export function storeCurrentUser(user: AuthUser) {
  localStorage.setItem("currentUser", JSON.stringify(user));
  window.dispatchEvent(new Event("auth:user-updated"));
}

export function clearAuthSession() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("accessTokenExpiresOnUtc");
  localStorage.removeItem("currentUser");
  window.dispatchEvent(new Event("auth:user-updated"));
}

export function getCurrentUser(): AuthUser | null {
  const raw = localStorage.getItem("currentUser");
  if (!raw) return null;

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    clearAuthSession();
    return null;
  }
}

export function isAuthenticated() {
  const token = localStorage.getItem("accessToken");
  return !!token;
}

export function isAccessTokenExpired(bufferMs = 30_000) {
  const token = localStorage.getItem("accessToken");
  if (!token) return true;

  const expiresOnUtc = localStorage.getItem("accessTokenExpiresOnUtc");
  if (!expiresOnUtc) return false;

  const expiresAt = Date.parse(expiresOnUtc);
  if (!Number.isFinite(expiresAt)) return false;

  return expiresAt <= Date.now() + bufferMs;
}

export function getAccessToken() {
  return localStorage.getItem("accessToken");
}

export async function refreshAccessToken() {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/Auth/refresh`, {
    method: "POST",
    credentials: "include",
    headers: {
      Accept: "application/json",
    },
  });

  if (!response.ok) {
    clearAuthSession();
    return false;
  }

  const payload = await response.json().catch(() => null);
  const token = unwrapApiResponse<TokenResponse>(payload);
  if (!token.accessToken) {
    clearAuthSession();
    return false;
  }

  localStorage.setItem("accessToken", token.accessToken);
  localStorage.setItem("accessTokenExpiresOnUtc", token.expiresOnUtc ?? "");
  return true;
}

export async function getValidAccessToken() {
  if (!getAccessToken()) return null;

  if (isAccessTokenExpired()) {
    const refreshed = await refreshAccessToken();
    if (!refreshed) return null;
  }

  return getAccessToken();
}

export async function fetchCurrentProfile(): Promise<AuthUser> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${getApiBaseUrl()}/api/v1/Auth/profile`, {
    method: "GET",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) throw new Error(getApiErrorMessage(payload, "Unable to load profile."));

  return unwrapApiResponse<AuthUser>(payload);
}

export async function updateCurrentProfile(input: {
  firstName: string;
  lastName: string;
  phoneNumber: string;
}): Promise<AuthUser> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${getApiBaseUrl()}/api/v1/Auth/profile`, {
    method: "PUT",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(input),
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) throw new Error(getApiErrorMessage(payload, "Unable to update profile."));

  return unwrapApiResponse<AuthUser>(payload);
}

export async function changeCurrentPassword(input: {
  currentPassword: string;
  newPassword: string;
}) {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${getApiBaseUrl()}/api/v1/Auth/change-password`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(input),
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    throw payload ?? new Error("Unable to change password.");
  }
}

function getApiBaseUrl() {
  return (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
}
