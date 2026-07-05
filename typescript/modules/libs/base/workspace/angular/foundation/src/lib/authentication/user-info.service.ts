import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { UserId } from '../state/user-id';

interface UserInfoResponse {
  /** User id */
  u: string;

  /** User name */
  userName: string;
}

// Replaces the JWT token response as the source of the logged-in user id: called from the app's
// APP_INITIALIZER (after the workspace is configured), it reads the authenticated /allors/UserInfo
// endpoint and primes UserId state. An unauthenticated bootstrap yields a 401, which the
// UnauthorizedInterceptor turns into a login redirect.
@Injectable({ providedIn: 'root' })
export class UserInfoService {
  constructor(private http: HttpClient, private userId: UserId) {}

  async init(baseUrl: string): Promise<void> {
    const userInfo = await firstValueFrom(
      this.http.get<UserInfoResponse>(`${baseUrl}UserInfo`)
    );
    this.userId.value = Number(userInfo.u);
  }
}
