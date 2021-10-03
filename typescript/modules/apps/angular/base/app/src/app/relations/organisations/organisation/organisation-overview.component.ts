import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { ContextService } from '@allors/workspace/angular/core';
import { TestScope } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { assert } from '@allors/workspace/meta/system';
import { IPullResult } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './organisation-overview.component.html',
  providers: [ContextService],
})
export class OrganisationOverviewComponent extends TestScope implements OnInit, OnDestroy {
  public title: string;

  public m: M;

  public organisation: Organisation;
  public locales: Locale[];

  private subscription: Subscription;

  constructor(@Self() private allors: ContextService, private titleService: Title, private route: ActivatedRoute) {
    super();

    this.title = 'Organisation Overview';
    this.titleService.setTitle(this.title);

    this.m = allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { pullBuilder: p } = this.m;

    this.subscription = this.route.url
      .pipe(
        switchMap((url) => {
          const id = this.route.snapshot.paramMap.get('id');
          assert(id);

          const pulls = [
            p.Organisation({
              objectId: id,
              include: {
                Owner: {},
                Employees: {},
              },
            }),
          ];

          this.allors.context.reset();

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((result: IPullResult) => {
        this.organisation = result.object<Organisation>(this.m.Organisation);
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
