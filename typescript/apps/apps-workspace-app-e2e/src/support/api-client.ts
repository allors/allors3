interface AuthenticationTokenRequest {
  /** login */
  l: string;
  /** password */
  p?: string;
}

interface AuthenticationTokenResponse {
  /** Authenticated */
  a: boolean;
  /** User id */
  u: number;
  /** Token */
  t: string;
}

export class ApiClient {
  userId: number | null = null;
  jwtToken: string | null = null;

  constructor(
    public baseUrl: string = 'http://localhost:4020/allors/',
    public authUrl: string = 'TestAuthentication/Token'
  ) {}

  async setup(population = 'full'): Promise<void> {
    const url = `${this.baseUrl}Test/Setup?population=${population}`;
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Failed to setup database: ${response.status}`);
    }
  }

  async ready(): Promise<boolean> {
    try {
      const url = `${this.baseUrl}Test/Ready`;
      const response = await fetch(url);
      return response.ok;
    } catch {
      return false;
    }
  }

  async login(login: string, password = ''): Promise<string | null> {
    const tokenRequest: AuthenticationTokenRequest = {
      l: login,
      p: password,
    };

    const url = `${this.baseUrl}${this.authUrl}`;
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(tokenRequest),
    });

    if (response.ok) {
      const tokenResponse =
        (await response.json()) as AuthenticationTokenResponse;
      if (tokenResponse.a) {
        this.userId = tokenResponse.u;
        this.jwtToken = tokenResponse.t;
        return this.jwtToken;
      }
    }
    return null;
  }

  async waitForServer(timeout = 30000): Promise<void> {
    const start = Date.now();
    while (Date.now() - start < timeout) {
      if (await this.ready()) {
        return;
      }
      await new Promise((resolve) => setTimeout(resolve, 1000));
    }
    throw new Error('Server did not become ready within timeout');
  }
}
