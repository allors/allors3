import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { Person } from '@allors/workspace/domain/default';
import { IPullResult } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { assert } from '@allors/workspace/meta/system';

@Component({
  templateUrl: './person-overview.component.html',
  providers: [SessionService],
})
export class PersonOverviewComponent extends TestScope implements OnInit, OnDestroy {
  public title: string;
  public m: M;

  public person: Person;
  public locales: Locale[];
  private subscription: Subscription;

  constructor(@Self() private allors: SessionService, private titleService: Title, private route: ActivatedRoute) {
    super();

    this.title = 'Person Overview';
    this.titleService.setTitle(this.title);
    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { pullBuilder: p } = this.m;

    this.subscription = this.route.url
      .pipe(
        switchMap((url: any) => {
          const id = this.route.snapshot.paramMap.get('id');
          assert(id);

          const pulls = [
            p.Person({
              objectId: id,
              include: {
                Photo: {},
              },
            }),
          ];

          this.allors.session.reset();

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded: IPullResult) => {
        this.person = loaded.object<Person>(this.m.Person);
      });
  }
  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public goBack(): void {
    window.history.back();
  }

  public checkType(obj: any): string {
    return obj.objectType.name;
  }
}
