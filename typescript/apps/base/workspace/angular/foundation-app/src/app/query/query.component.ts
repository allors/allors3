import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription } from 'rxjs';

import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';

@Component({
  templateUrl: './query.component.html',
  providers: [ContextService],
})
export class QueryComponent implements OnInit, OnDestroy {
  public organisations: Organisation[];

  public organisationCount: number;
  public skip = 1;
  public take = 2;

  private subscription: Subscription;

  constructor(
    @Self() private allors: ContextService,
    private workspaceService: WorkspaceService,
    private title: Title
  ) {
    this.allors.context.name = this.constructor.name;
  }

  public ngOnInit() {
    this.title.setTitle('Query');
    this.query();
  }

  public query() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    const { context } = this.allors;
    const m = context.configuration.metaPopulation as M;
    const { pullBuilder: p } = m;

    const pulls: Pull[] = [
      p.Organisation({
        sorting: [{ roleType: m.Organisation.Name }],
        select: {
          include: {
            Owner: {},
          },
        },
        skip: this.skip || 0,
        take: this.take || 10,
      }),
    ];

    this.subscription = context.pull(pulls).subscribe(
      (result: IPullResult) => {
        this.organisations = result.collection<Organisation>(m.Organisation);
      },
      (error) => {
        alert(error);
      }
    );
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
