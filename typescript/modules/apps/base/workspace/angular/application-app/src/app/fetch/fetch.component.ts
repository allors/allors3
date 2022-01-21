import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import {
  ContextService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { Organisation } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  templateUrl: './fetch.component.html',
  providers: [ContextService],
})
export class FetchComponent implements OnInit, OnDestroy {
  public organisation: Organisation;
  public organisations: Organisation[];

  private subscription: Subscription;

  constructor(
    @Self() private allors: ContextService,
    private workspaceService: WorkspaceService,
    private title: Title,
    private route: ActivatedRoute
  ) {
    this.allors.context.name = this.constructor.name;
  }

  public ngOnInit() {
    this.title.setTitle('Fetch');
    this.fetch();
  }

  public fetch() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    const { context } = this.allors;
    const m = context.configuration.metaPopulation as M;
    const { pullBuilder: p } = m;

    const id = this.route.snapshot.paramMap.get('id');

    const pulls: Pull[] = [
      p.Organisation({
        objectId: id,
        results: [
          {},
          {
            select: {
              Owner: {
                OrganisationsWhereOwner: {
                  include: {
                    Owner: {},
                  },
                },
              },
            },
          },
        ],
      }),
    ];

    this.subscription = context.pull(pulls).subscribe(
      (result: IPullResult) => {
        this.organisation = result.object<Organisation>(m.Organisation);
        this.organisations = result.collection<Organisation>(
          m.Person.OrganisationsWhereOwner
        );
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
