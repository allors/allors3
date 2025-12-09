import { Injectable } from '@angular/core';

@Injectable()
export abstract class ErrorService {
  abstract errorHandler: (error: any) => void;
}
