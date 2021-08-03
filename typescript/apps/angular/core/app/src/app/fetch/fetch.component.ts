import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import { SessionService } from '@allors/workspace/angular/core';
import { assert } from '@allors/workspace/meta/system';
import { Organisation } from '@allors/workspace/domain/core';

@Component({
  templateUrl: './fetch.component.html',
  providers: [SessionService],
})
export class FetchComponent implements OnInit, OnDestroy {
  public organisation: Organisation;
  public organisations: Organisation[];

  private subscription: Subscription;

  constructor(
    @Self() private sessionService: SessionService,
    private title: Title,
    private route: ActivatedRoute
  ) {}

  public ngOnInit() {
    this.title.setTitle('Fetch');
    this.fetch();
  }

  public fetch() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    // const { pull, x } = this.metaService;

    // const id = this.route.snapshot.paramMap.get('id');
    // assert(id);

    // const pulls = [
    //   pull.Organisation({ object: id, include: { Owner: x } }),
    //   pull.Organisation({
    //     object: id,
    //     fetch: {
    //       Owner: {
    //         OrganisationsWhereOwner: {
    //           include: {
    //             Owner: x,
    //           },
    //         },
    //       },
    //     },
    //   }),
    // ];

    // this.allors.context.reset();

    // this.subscription = this.allors.context
    //   .load(
    //     new PullRequest({
    //       pulls,
    //     })
    //   )
    //   .subscribe(
    //     (loaded: Loaded) => {
    //       this.organisation = loaded.objects.Organisation as Organisation;
    //       this.organisations = loaded.collections.Organisations as Organisation[];
    //     },
    //     (error) => {
    //       alert(error);
    //     }
    //   );
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
