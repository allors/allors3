import { Client } from "./Client";

export class AjaxClient implements Client {
  userId: number;
  jwtToken: string;

  constructor(args?: Pick<AjaxClient, 'userId' | 'jwtToken'>) {
    Object.assign(this, args);
  }
}
