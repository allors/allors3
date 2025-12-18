import { delay, pipe, Subscription, tap } from 'rxjs';
import { Component, OnDestroy } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  pathLeaf,
  Pull,
  nodeResolve,
  pathResolve,
} from '@allors/system/workspace/domain';
import {
  Action,
  ActionService,
  DisplayService,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  Scoped,
  ScopedService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { HyperlinkType } from '../hyperlink/hyperlink-type';
import { HyperlinkService } from '../hyperlink/hyperlink.service';
import { IconService } from '../icon/icon.service';
import { Hyperlink } from '../hyperlink/hyperlink';

@Component({
  selector: 'a-mat-dyn-summary-panel',
  templateUrl: './dynamic-summary-panel.component.html',
})
export class AllorsMaterialDynamicSummaryPanelComponent
  extends AllorsViewSummaryPanelComponent
  implements OnDestroy
{
  displayName: RoleType;
  displayDescription: RoleType;
  linkTypes: HyperlinkType[];

  object: IObject;
  icon: string;
  name: string;
  description: string;
  links: Hyperlink[];
  actions: Action[];

  private subscription: Subscription;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    linkService: HyperlinkService,
    private actionService: ActionService,
    private navigation: NavigationService,
    private iconService: IconService,
    private displayService: DisplayService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);

    sharedPullService.register(this);

    this.subscription = this.scopedService.scoped$
      .pipe(
        pipe(delay(1)),
        tap((scoped) => {
          this.scoped = scoped;
          const { objectType } = scoped;
          this.linkTypes = linkService.linkTypes(objectType);
          this.actions = actionService.action(objectType);
          this.icon = iconService.icon(objectType);
          this.displayName = displayService.name(objectType);
          this.displayDescription = displayService.description(objectType);
        }),
        tap(() => {
          this.refreshService.refresh();
        })
      )
      .subscribe();
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    const pull: Pull = {
      objectId: this.scoped.id,
      results: [
        {
          name: prefix,
          include: this.linkTypes.reduce((a, e) => a.concat(e.tree), []),
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string): void {
    this.object = pullResult.object<IObject>(prefix);

    this.name = this.displayName
      ? (this.object.strategy.getUnitRole(this.displayName) as string)
      : null;
    this.description = this.displayDescription
      ? (this.object.strategy.getUnitRole(this.displayDescription) as string)
      : null;

    this.links = this.linkTypes
      .map((linkType) => {
        return linkType.paths
          .map((path) => {
            const leaf = pathLeaf(path);
            const leafObjectType = leaf.propertyType.objectType as Composite;
            const targets = [...pathResolve(this.object, path)];
            return targets.map((target) => {
              const icon = this.iconService.icon(leafObjectType);
              const nameDisplay = this.displayService.name(leafObjectType);
              const name = nameDisplay
                ? (target.strategy.getUnitRole(nameDisplay) as string)
                : null;
              const descriptionDisplay =
                this.displayService.description(leafObjectType);
              const description = descriptionDisplay
                ? (target.strategy.getUnitRole(descriptionDisplay) as string)
                : null;
              const link: Hyperlink = {
                linkType,
                target,
                icon,
                name,
                description,
              };
              return link;
            });
          })
          .reduce((acc, v) => acc.concat(v), []);
      })
      .reduce((acc, v) => acc.concat(v), []);
  }

  navigate(link: Hyperlink): void {
    this.navigation.overview(link.target);
  }

  perform(action: Action): void {
    action.execute(this.object);
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.subscription?.unsubscribe();
  }
}
