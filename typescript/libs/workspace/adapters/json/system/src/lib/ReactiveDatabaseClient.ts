import { InvokeRequest, PullRequest, PullResponse, PushRequest } from '@allors/protocol/json/system';
import { IReactiveDatabaseClient, IInvokeResult, InvokeOptions, IPullResult, IPushResult, ISession, Method, Procedure, Pull } from '@allors/workspace/domain/system';
import { Origin } from '@allors/workspace/meta/system';
import { procedureToJson, pullToJson } from './json/toJson';
import { DatabaseOriginState } from './session/originstate/DatabaseOriginState';
import { Session } from './session/Session';
import { Strategy } from './session/Strategy';
import { InvokeResult } from './database/invoke/InvokeResult';
import { PushResult } from './database/push/PushResult';
import { PullResult } from './database/pull/PullResult';

import { IReactiveDatabaseJsonClient } from './IReactiveDatabaseJsonClient';
import { DatabaseConnection } from './database/DatabaseConnection';

import { Observable, of } from 'rxjs';
import { concatMap, map, switchMap, tap } from 'rxjs/operators';

export class ReactiveDatabaseClient implements IReactiveDatabaseClient {
  constructor(public client: IReactiveDatabaseJsonClient) {}

  invokeReactive(session: ISession, methods: Method[], options: InvokeOptions): Observable<IInvokeResult> {
    const invokeRequest: InvokeRequest = {
      l: methods.map((v) => {
        return {
          i: v.object.id,
          v: (v.object.strategy as Strategy).DatabaseOriginState.version,
          m: v.methodType.tag,
        };
      }),
      o:
        options != null
          ? {
              c: options.continueOnError,
              i: options.isolated,
            }
          : null,
    };

    return this.client.invoke(invokeRequest).pipe(switchMap((invokeResponse) => of(new InvokeResult(session, invokeResponse))));
  }

  pullReactive(session: ISession, pulls: Pull[]): Observable<IPullResult> {
    for (const pull of pulls) {
      if (pull.objectId < 0 || pull.object?.id < 0) {
        throw new Error('Id is not in the database');
      }

      if (pull.object != null && pull.object.strategy.cls.origin != Origin.Database) {
        throw new Error('Origin is not Database');
      }
    }

    const pullRequest: PullRequest = {
      l: pulls.map((v) => pullToJson(v)),
    };

    return this.pull(session as Session, pullRequest);
  }

  callReactive(session: ISession, procedure: Procedure, ...pulls: Pull[]): Observable<IPullResult> {
    const pullRequest: PullRequest = {
      p: procedureToJson(procedure),
      l: pulls.map((v) => pullToJson(v)),
    };

    return this.pull(session as Session, pullRequest);
  }

  pushReactive(session: ISession): Observable<IPushResult> {
    const pushToDatabaseTracker = (session as Session).pushToDatabaseTracker;

    const pushRequest: PushRequest = {};

    if (pushToDatabaseTracker.created) {
      pushRequest.n = [...pushToDatabaseTracker.created].map((v) => (v.DatabaseOriginState as DatabaseOriginState).pushNew());
    }

    if (pushToDatabaseTracker.changed) {
      pushRequest.o = [...pushToDatabaseTracker.changed].map((v) => (v.strategy.DatabaseOriginState as DatabaseOriginState).pushExisting());
    }


    return this.client.push(pushRequest).pipe(
      switchMap((pushResponse) => {
        const pushResult = new PushResult(session, pushResponse);

        if (pushResult.hasErrors) {
          return of(pushResult);
        }

        if (pushResponse.n != null) {
          for (const pushResponseNewObject of pushResponse.n) {
            const workspaceId = pushResponseNewObject.w;
            const databaseId = pushResponseNewObject.d;
            (session as Session).onDatabasePushResponseNew(workspaceId, databaseId);
          }
        }

        pushToDatabaseTracker.created = null;
        pushToDatabaseTracker.changed = null;

        if (pushRequest.o != null) {
          for (const id of pushRequest.o.map((v) => v.d)) {
            const strategy = (session as Session).getStrategy(id);
            strategy.onDatabasePushed();
          }
        }

        return of(pushResult);
      })
    );
  }

  private pull(session: Session, pullRequest: PullRequest): Observable<IPullResult> {
    return this.client.pull(pullRequest).pipe(
      concatMap((pullResponse) => {
        const pullResult = new PullResult(session, pullResponse);

        const syncRequest = (session.workspace.database as DatabaseConnection).onPullResonse(pullResponse);
        if (syncRequest.o.length > 0) {
          const database = session.workspace.database as DatabaseConnection;
          return this.client.sync(syncRequest).pipe(
            concatMap((syncResponse) => {
              const securityRequest = database.onSyncResponse(syncResponse);

              for (const v of syncResponse.o) {
                if (!session.strategyByWorkspaceId.has(v.i)) {
                  session.instantiateDatabaseStrategy(v.i);
                } else {
                  const strategy = session.strategyByWorkspaceId.get(v.i);
                  strategy.DatabaseOriginState.onPulled(pullResult);
                }
              }

              if (securityRequest != null) {
                return this.client.security(securityRequest).pipe(
                  concatMap((securityResponse) => {
                    const securityRequest = database.securityResponse(securityResponse);
                    if (securityRequest != null) {
                      return this.client.security(securityRequest).pipe(
                        concatMap((securityResponse) => {
                          database.securityResponse(securityResponse);
                          return of({ pullResult, pullResponse });
                        })
                      );
                    } else {
                      return of({ pullResult, pullResponse });
                    }
                  })
                );
              } else {
                return of({ pullResult, pullResponse });
              }
            })
          );
        } else {
          return of({ pullResult, pullResponse });
        }
      }),
      map(({ pullResult, pullResponse }) => {
        for (const v of pullResponse.p) {
          if (session.strategyByWorkspaceId.has(v.i)) {
            const strategy = session.strategyByWorkspaceId.get(v.i);
            strategy.DatabaseOriginState.onPulled(pullResult);
          } else {
            session.instantiateDatabaseStrategy(v.i);
          }
        }

        return pullResult;
      })
    );
  }
}
