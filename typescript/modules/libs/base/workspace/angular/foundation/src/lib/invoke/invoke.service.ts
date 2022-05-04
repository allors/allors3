import {
  IInvokeResult,
  Method,
  InvokeOptions,
} from '@allors/system/workspace/domain';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { WorkspaceService } from '../workspace/workspace-service';

@Injectable({
  providedIn: 'root',
})
export class InvokeService {
  constructor(private workspaceService: WorkspaceService) {}

  invoke(
    methods: Method | Method[],
    options?: InvokeOptions
  ): Observable<IInvokeResult> {
    return this.workspaceService.contextBuilder().invoke(methods, options);
  }
}
