import { delay, pipe, Subscription, tap } from 'rxjs';
import { Component, OnDestroy } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  leafPath,
  Pull,
  resolveNode,
  resolvePath,
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
  ObjectInfo,
  ObjectService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { LinkType } from '../link/link-type';
import { LinkService } from '../link/link.service';
import { IconService } from '../icon/icon.service';
import { Link } from '../link/link';

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
  linkTypes: LinkType[];

  object: IObject;
  icon: string;
  name: string;
  description: string;
  links: Link[];
  actions: Action[];

  private subscription: Subscription;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService,
    linkService: LinkService,
    private actionService: ActionService,
    private navigationService: NavigationService,
    private iconService: IconService,
    private displayService: DisplayService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );

    sharedPullService.register(this);

    this.subscription = this.objectService.objectInfo$
      .pipe(
        pipe(delay(1)),
        tap((objectInfo) => {
          this.objectInfo = objectInfo;
          const { objectType } = objectInfo;
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

  onPreScopedPull(pulls: Pull[], scope?: string): void {
    const pull: Pull = {
      objectId: this.objectInfo.id,
      results: [
        {
          name: scope,
          include: this.linkTypes.reduce((a, e) => a.concat(e.tree), []),
        },
      ],
    };

    pulls.push(pull);
  }

  onPostScopedPull(pullResult: IPullResult, scope?: string): void {
    this.object = pullResult.object<IObject>(scope);

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
            const leaf = leafPath(path);
            const leafObjectType = leaf.propertyType.objectType as Composite;
            const targets = [...resolvePath(this.object, path)];
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
              const link: Link = {
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

  navigate(link: Link): void {
    this.navigationService.overview(link.target);
  }

  perform(action: Action): void {
    action.execute(this.object);
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    this.subscription?.unsubscribe();
  }
}
