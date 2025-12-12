interface LoginCredentials {
  username: string;
  password: string;
}

interface AuthResponse {
  tokenType: string;
  token: string;
  expires: string;
}
