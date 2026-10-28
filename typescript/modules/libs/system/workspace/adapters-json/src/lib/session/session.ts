import {
  InvokeRequest,
  PullRequest,
  PullResponse,
  PushRequest,
} from '@allors/system/common/protocol-json';
import { Session as SystemSession } from '@allors/system/workspace/adapters';
import {
  IInvokeResult,
  InvokeOptions,
  IObject,
  IPullResult,
  IPushResult,
  Method,
  Pull,
  ResultError,
} from '@allors/system/workspace/domain';
import { Class } from '@allors/system/workspace/meta';
import { DatabaseConnection } from '../database/database-connection';
import { DatabaseRecord } from '../database/database-record';
import { InvokeResult } from '../database/invoke/invoke-result';
import { PullResult } from '../database/pull/pull-result';
import { PushResult } from '../database/push/push-result';
import { dependenciesToJson, pullToJson } from '../json/to-json';
import { Workspace } from '../workspace/workspace';
import { DatabaseOriginState } from './originstate/database-origin-state';
import { Strategy } from './strategy';

export class Session extends SystemSession {
  database: DatabaseConnection;

  private operationQueue: Promise<unknown> = Promise.resolve();

  constructor(workspace: Workspace) {
    super(workspace);

    this.database = this.workspace.database as DatabaseConnection;
  }

  create<T extends IObject>(cls: Class): T {
    const workspaceId = this.workspace.database.nextId();
    const strategy = new Strategy(this, cls, workspaceId);
    this.addObject(strategy.object);
    this.pushToDatabaseTracker.onCreated(strategy.object);
    this.changeSetTracker.onCreated(strategy.object);
    return strategy.object as T;
  }

  onDelete(strategy: Strategy) {
    this.removeObject(strategy.object);
    this.pushToDatabaseTracker.onDelete(strategy.object);
    this.changeSetTracker.onDelete(strategy.object);
  }

  instantiateDatabaseStrategy(id: number): IObject {
    const databaseRecord = this.workspace.database.getRecord(
      id
    ) as DatabaseRecord;

    if (databaseRecord == null) {
      return null;
    }

    const strategy = Strategy.fromDatabaseRecord(this, databaseRecord);
    this.addObject(strategy.object);
    return strategy.object;
  }

  invoke(
    methodOrMethods: Method | Method[],
    options?: InvokeOptions
  ): Promise<IInvokeResult> {
    return this.enqueue(() => this.invokeCore(methodOrMethods, options));
  }

  private async invokeCore(
    methodOrMethods: Method | Method[],
    options?: InvokeOptions
  ): Promise<IInvokeResult> {
    const methods = Array.isArray(methodOrMethods)
      ? methodOrMethods
      : [methodOrMethods];
    const invokeRequest: InvokeRequest = {
      x: this.context,
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

    const invokeResponse = await this.database.client.invoke(invokeRequest);
    const result = new InvokeResult(this, invokeResponse);

    if (result.hasErrors) {
      throw new ResultError(result);
    }

    return result;
  }

  pull(pullOrPulls: Pull | Pull[]): Promise<IPullResult> {
    return this.enqueue(async () => {
      const pulls = Array.isArray(pullOrPulls) ? pullOrPulls : [pullOrPulls];
      for (const pull of pulls) {
        if (pull.objectId < 0 || pull.object?.id < 0) {
          throw new Error('Id is not in the database');
        }
      }
      const request: PullRequest = {
        x: this.context,
        d: dependenciesToJson(this.dependencies),
        l: pulls.map((v) => pullToJson(v)),
      };
      const response = await this.database.client.pull(request);
      return this.onPull(response, request.x);
    });
  }

  push(): Promise<IPushResult> {
    return this.enqueue(() => this.pushCore());
  }

  private async pushCore(): Promise<IPushResult> {
    const pushRequest: PushRequest = {
      x: this.context,
    };

    if (this.pushToDatabaseTracker.created) {
      pushRequest.n = [...this.pushToDatabaseTracker.created].map((v) =>
        (
          (v.strategy as Strategy).DatabaseOriginState as DatabaseOriginState
        ).pushNew()
      );
    }

    if (this.pushToDatabaseTracker.changed) {
      pushRequest.o = [...this.pushToDatabaseTracker.changed].map((v) =>
        (v as DatabaseOriginState).pushExisting()
      );
    }

    const pushResponse = await this.database.client.push(pushRequest);

    const pushResult = new PushResult(this, pushResponse);

    if (pushResult.hasErrors) {
      throw new ResultError(pushResult);
    }

    if (pushResponse.n != null) {
      for (const pushResponseNewObject of pushResponse.n) {
        const workspaceId = pushResponseNewObject.w;
        const databaseId = pushResponseNewObject.d;
        this.onDatabasePushResponseNew(workspaceId, databaseId);
      }
    }

    this.pushToDatabaseTracker.created = null;
    this.pushToDatabaseTracker.changed = null;

    if (pushRequest.o != null) {
      for (const id of pushRequest.o.map((v) => v.d)) {
        const object = this.getObject(id);
        (object.strategy as Strategy).onDatabasePushed();
      }
    }

    return pushResult;
  }

  private enqueue<T>(operation: () => Promise<T>): Promise<T> {
    const workflow = this.operationQueue.then(operation);
    this.operationQueue = workflow.catch(() => undefined);
    return workflow;
  }

  private async onPull(
    pullResponse: PullResponse,
    context: string
  ): Promise<IPullResult> {
    const result = new PullResult(this, pullResponse);
    if (result.hasErrors) {
      return result;
    }
    const loading = this.database.prepare(
      result.response as PullResponse,
      context
    );
    if (loading.request.o.length > 0) {
      loading.load(await this.database.client.sync(loading.request));
    }
    loading.validateResult();
    const newObjects = new Map<number, IObject>();
    for (const record of loading.records.values()) {
      if (!this.objectByWorkspaceId.has(record.id)) {
        newObjects.set(
          record.id,
          Strategy.fromDatabaseRecord(this, record).object
        );
      }
    }
    this.database.publish(loading);
    for (const record of loading.records.values()) {
      const created = newObjects.get(record.id);
      if (created) {
        this.addObject(created);
      } else {
        (
          this.objectByWorkspaceId.get(record.id).strategy as Strategy
        ).DatabaseOriginState.onPulled(result, record);
      }
    }
    // Bind named results now; later access cannot discover a different connection record.
    void result.objects;
    void result.collections;
    return result;
  }
}
