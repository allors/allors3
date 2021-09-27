import { Injectable } from '@angular/core';
import { SessionState } from './session-state';

@Injectable({
  providedIn: 'root',
})
export class SingletonId extends SessionState {
  constructor() {
    super('State$SingletonId');
  }
}
