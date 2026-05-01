export type AuthUser = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  bankName?: string | null;
  bankAccountName?: string | null;
  bankIban?: string | null;
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

const AUTH_STORAGE_KEYS = ["accessToken", "accessTokenExpiresOnUtc", "currentUser"] as const;
const PUBLIC_AUTH_PATHS = new Set(["/login", "/register", "/register/verify", "/forgot-password", "/reset-password"]);

function clearAuthStorage(storage: Storage) {
  AUTH_STORAGE_KEYS.forEach((key) => storage.removeItem(key));
}

function getStorageWithAccessToken() {
  if (localStorage.getItem("accessToken")) return localStorage;
  if (sessionStorage.getItem("accessToken")) return sessionStorage;
  return null;
}

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

export function storeAuthSession(result: AuthSessionResponse, rememberMe = true) {
  if (!result.token?.accessToken) return;

  const storage = rememberMe ? localStorage : sessionStorage;
  clearAuthStorage(localStorage);
  clearAuthStorage(sessionStorage);

  storage.setItem("accessToken", result.token.accessToken);
  storage.setItem("accessTokenExpiresOnUtc", result.token.expiresOnUtc ?? "");
  storage.setItem(
    "currentUser",
    JSON.stringify({
      id: result.id,
      email: result.email,
      firstName: result.firstName,
      lastName: result.lastName,
      phoneNumber: result.phoneNumber,
      bankName: result.bankName,
      bankAccountName: result.bankAccountName,
      bankIban: result.bankIban,
      role: result.role,
      createdAt: result.createdAt,
    } satisfies AuthUser),
  );
  window.dispatchEvent(new Event("auth:user-updated"));
}

export function storeCurrentUser(user: AuthUser) {
  const storage = getStorageWithAccessToken() ?? localStorage;
  storage.setItem("currentUser", JSON.stringify(user));
  window.dispatchEvent(new Event("auth:user-updated"));
}

export function clearAuthSession() {
  clearAuthStorage(localStorage);
  clearAuthStorage(sessionStorage);
  window.dispatchEvent(new Event("auth:user-updated"));
}

export function redirectToLogin() {
  if (typeof window === "undefined") return;

  const { pathname, search } = window.location;
  if (PUBLIC_AUTH_PATHS.has(pathname)) return;

  clearAuthSession();
  const returnUrl = `${pathname}${search}`;
  const loginUrl = `/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  window.location.replace(loginUrl);
}

export function getCurrentUser(): AuthUser | null {
  const raw = localStorage.getItem("currentUser") ?? sessionStorage.getItem("currentUser");
  if (!raw) return null;

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    clearAuthSession();
    return null;
  }
}

export function isAuthenticated() {
  return !!getAccessToken();
}

export function isAccessTokenExpired(bufferMs = 30_000) {
  const storage = getStorageWithAccessToken();
  const token = storage?.getItem("accessToken");
  if (!token) return true;

  const expiresOnUtc = storage.getItem("accessTokenExpiresOnUtc");
  if (!expiresOnUtc) return false;

  const expiresAt = Date.parse(expiresOnUtc);
  if (!Number.isFinite(expiresAt)) return false;

  return expiresAt <= Date.now() + bufferMs;
}

export function getAccessToken() {
  return localStorage.getItem("accessToken") ?? sessionStorage.getItem("accessToken");
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

  const storage = getStorageWithAccessToken() ?? localStorage;
  storage.setItem("accessToken", token.accessToken);
  storage.setItem("accessTokenExpiresOnUtc", token.expiresOnUtc ?? "");
  return true;
}

export async function getValidAccessToken() {
  if (!getAccessToken()) {
    redirectToLogin();
    return null;
  }

  if (isAccessTokenExpired()) {
    const refreshed = await refreshAccessToken();
    if (!refreshed) {
      redirectToLogin();
      return null;
    }
  }

  const token = getAccessToken();
  if (!token) redirectToLogin();
  return token;
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
  bankName?: string;
  bankAccountName?: string;
  bankIban?: string;
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
