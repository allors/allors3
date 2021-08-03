import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription } from 'rxjs';

import { Organisation } from '@allors/workspace/domain/core';
import { SessionService } from '@allors/workspace/angular/core';

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

  constructor(@Self() private sessionService: SessionService, private title: Title) {}

  public ngOnInit() {
    this.title.setTitle('Query');
    this.query();
  }

  public query() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    // const { m, pull, x } = this.metaService;

    // const pulls = [
    //   pull.Organisation({
    //     predicate: new Like({
    //       roleType: m.Organisation.Name,
    //       value: 'Org%',
    //     }),
    //     include: { Owner: x },
    //     sort: new Sort(m.Organisation.Name),
    //     skip: this.skip || 0,
    //     take: this.take || 10,
    //   }),
    // ];

    // this.allors.context.reset();

    // this.subscription = this.allors.context.load(new PullRequest({ pulls })).subscribe(
    //   (loaded: Loaded) => {
    //     this.organisations = loaded.collections.Organisations as Organisation[];
    //     this.organisationCount = loaded.values.Organisations_count;
    //   },
    //   (error) => {
    //     alert(error);
    //   }
    // );
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
