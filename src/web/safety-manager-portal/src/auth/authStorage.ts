const ACCESS_TOKEN_KEY = 'jsp_access_token';
const REFRESH_TOKEN_KEY = 'jsp_refresh_token';
const USER_KEY = 'jsp_user';
const REMEMBER_KEY = 'jsp_remember_me';

function storage(remember: boolean): Storage {
  return remember ? localStorage : sessionStorage;
}

export function saveAuth(
  accessToken: string,
  refreshToken: string,
  user: object,
  remember: boolean,
) {
  localStorage.setItem(REMEMBER_KEY, remember ? '1' : '0');
  const store = storage(remember);
  store.setItem(ACCESS_TOKEN_KEY, accessToken);
  store.setItem(REFRESH_TOKEN_KEY, refreshToken);
  store.setItem(USER_KEY, JSON.stringify(user));
  if (!remember) {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  } else {
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
  }
}

export function clearAuth() {
  [localStorage, sessionStorage].forEach((s) => {
    s.removeItem(ACCESS_TOKEN_KEY);
    s.removeItem(REFRESH_TOKEN_KEY);
    s.removeItem(USER_KEY);
  });
  localStorage.removeItem(REMEMBER_KEY);
}

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY) ?? sessionStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY) ?? sessionStorage.getItem(REFRESH_TOKEN_KEY);
}

export function getStoredUser<T>(): T | null {
  const raw = localStorage.getItem(USER_KEY) ?? sessionStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as T;
  } catch {
    return null;
  }
}

export function isRememberMe(): boolean {
  return localStorage.getItem(REMEMBER_KEY) === '1';
}
