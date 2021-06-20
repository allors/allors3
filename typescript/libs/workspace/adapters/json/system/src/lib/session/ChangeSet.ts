import { IChangeSet, IStrategy } from '@allors/workspace/domain/system';
import { AssociationType, RelationType, RoleType } from '@allors/workspace/meta/system';
import { Session } from './Session/Session';
import { SessionStateChangeSet } from './Session/SessionStateChangeSet';
import { Strategy } from './Strategy';

export /* sealed */ class ChangeSet extends IChangeSet {

  public constructor (session: Session, created: ISet<IStrategy>, instantiated: ISet<IStrategy>) {
      this.Session = session;
      this.Created = created;
      this.Instantiated = instantiated;
      this.AssociationsByRoleType = new Dictionary<RoleType, ISet<IStrategy>>();
      this.RolesByAssociationType = new Dictionary<IAssociationType, ISet<IStrategy>>();
  }

  private get Session(): Session {
  }

  IChangeSet.Session: ISession;

  public get Created(): ISet<IStrategy> {
  }

  public get Instantiated(): ISet<IStrategy> {
  }

  public get AssociationsByRoleType(): IDictionary<RoleType, ISet<IStrategy>> {
  }

  public get RolesByAssociationType(): IDictionary<IAssociationType, ISet<IStrategy>> {
  }

  public AddSessionStateChanges(sessionStateChangeSet: IDictionary<IPropertyType, IDictionary<number, Object>>) {
      for (let kvp in sessionStateChangeSet) {
          let ids = kvp.Value.Keys;
          let strategies = new HashSet<IStrategy>(ids.Select(() => {  }, this.Session.GetStrategy(v)));
          switch (kvp.Key) {
              case IAssociationType:
                  /* Warning! Labeled Statements are not Implemented */this.RolesByAssociationType.Add(associationType, strategies);
                  break;
              case RoleType:
                  /* Warning! Labeled Statements are not Implemented */this.AssociationsByRoleType.Add(roleType, strategies);
                  break;
          }

      }

  }

  public Diff(association: Strategy, relationType: RelationType, current: Object, previous: Object) {
      let roleType = relationType.RoleType;
      if (roleType.ObjectType.IsUnit) {
          if (!Equals(current, previous)) {
              this.AddAssociation(relationType, association);
          }

      }
      else if (roleType.IsOne) {
          if (Equals(current, previous)) {
              return;
          }

          if ((previous != null)) {
              this.AddRole(relationType, this.Session.GetStrategy((<number>(previous))));
          }

          if ((current != null)) {
              this.AddRole(relationType, this.Session.GetStrategy((<number>(current))));
          }

          this.AddAssociation(relationType, association);
      }
      else {
          let numbers = this.Session.Workspace.Numbers;
          let hasChange = false;
          let addedRoles = numbers.Except(current, previous);
          for (let v in numbers.Enumerate(addedRoles)) {
              this.AddRole(relationType, this.Session.GetStrategy(v));
              hasChange = true;
          }

          let removedRoles = numbers.Except(previous, current);
          for (let v in numbers.Enumerate(removedRoles)) {
              this.AddRole(relationType, this.Session.GetStrategy(v));
              hasChange = true;
          }

          if (hasChange) {
              this.AddAssociation(relationType, association);
          }

      }

  }

  private AddAssociation(relationType: RelationType, association: Strategy) {
      let roleType = relationType.RoleType;
      if (!this.AssociationsByRoleType.TryGetValue(roleType, /* out */var, associations)) {
          associations = new HashSet<IStrategy>();
          this.AssociationsByRoleType.Add(roleType, associations);
      }

      associations.Add(association);
  }

  private AddRole(relationType: RelationType, role: Strategy) {
      let associationType = relationType.AssociationType;
      if (!this.RolesByAssociationType.TryGetValue(associationType, /* out */var, roles)) {
          roles = new HashSet<IStrategy>();
          this.RolesByAssociationType.Add(associationType, roles);
      }

      roles.Add(role);
  }
}
