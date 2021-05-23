import { Client } from '@allors/workspace/domain/json/system';

export class AjaxClient implements Client {
  userId: number;
  jwtToken: string;

  constructor(args?: Pick<AjaxClient, 'userId' | 'jwtToken'>) {
    Object.assign(this, args);
  }
}
