import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription } from 'rxjs';

import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';
import { IPullResult, Pull } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';

@Component({
  templateUrl: './query.component.html',
  providers: [SessionService],
})
export class QueryComponent implements OnInit, OnDestroy {
  public organisations: Organisation[];

  public organisationCount: number;
  public skip = 5;
  public take = 5;

  private subscription: Subscription;

  constructor(@Self() private sessionService: SessionService, private workspaceService: WorkspaceService, private title: Title) {}

  public ngOnInit() {
    this.title.setTitle('Query');
    this.query();
  }

  public query() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    const { client, workspace } = this.workspaceService;
    const { session } = this.sessionService;
    const m = workspace.configuration.metaPopulation as M;
    const { trees } = m;

    const pulls: Pull[] = [
      {
        extent: {
          kind: 'Filter',
          objectType: m.Organisation,
          predicate: {
            kind: 'Like',
            roleType: m.Organisation.Name,
            value: 'Org%',
          },
          sorting: [{ roleType: m.Organisation.Name }],
        },
        results: [
          {
            select: {
              include: trees.Organisation({
                Owner: {},
              }),
            },
            skip: this.skip || 0,
            take: this.take || 10,
          },
        ],
      },
    ];

    this.subscription = client.pullReactive(session, pulls).subscribe(
      (result: IPullResult) => {
        this.organisations = result.collection<Organisation>(m.Organisation);
        this.organisationCount = result.value('Organisations_total') as number;
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
