import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';
import { IPullResult, Pull } from '@allors/workspace/domain/system';
import { Organisation } from '@allors/workspace/domain/default';
import { M } from '@allors/workspace/meta/default';

@Component({
  templateUrl: './fetch.component.html',
  providers: [SessionService],
})
export class FetchComponent implements OnInit, OnDestroy {
  public organisation: Organisation;
  public organisations: Organisation[];

  private subscription: Subscription;

  constructor(@Self() private sessionService: SessionService, private workspaceService: WorkspaceService, private title: Title, private route: ActivatedRoute) {}

  public ngOnInit() {
    this.title.setTitle('Fetch');
    this.fetch();
  }

  public fetch() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    const { client, workspace } = this.workspaceService;
    const { session } = this.sessionService;
    const m = workspace.configuration.metaPopulation as M;
    const { selections } = m;

    const id = this.route.snapshot.paramMap.get('id');

    const pulls: Pull[] = [
      {
        objectId: parseInt(id),
        results: [
          {
            select: selections.Organisation({
              Owner: {
                OrganisationsWhereOwner: {
                  Owner: {},
                },
              },
            }),
          },
        ],
      },
    ];

    this.subscription = client.pullReactive(session, pulls).subscribe(
      (result: IPullResult) => {
        this.organisation = result.object<Organisation>(m.Organisation);
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
