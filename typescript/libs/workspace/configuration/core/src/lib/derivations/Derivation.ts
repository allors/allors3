import { ICycle, IDerivation, IObject, IRule, ISession, IValidation, Node } from '@allors/workspace/domain/system';
import { Engine } from './Engine';

export class Derivation implements IDerivation {
  public constructor(session: ISession, engine: Engine, maxDomainDerivationCycles: number) {
    this.session = session;
    this.engine = engine;
    this.maxCycles = maxDomainDerivationCycles;
    this.validation = { errors: [] };
  }

  session: ISession;

  engine: Engine;

  maxCycles: number;

  validation: IValidation;

  public execute(): IValidation {
    let cycles = 0;
    let changeSet = this.session.checkpoint();

    while (changeSet.rolesByAssociationType?.size > 0 || changeSet.associationsByRoleType?.size > 0 || changeSet.created?.size > 0 || changeSet.instantiated?.size > 0) {
      if (++cycles > this.maxCycles) {
        throw new Error('Maximum amount of domain derivation cycles detected');
      }

      const cycle: ICycle = {
        changeSet,
        session: this.session,
        validation: this.validation,
      };

      const matchesByRule: Map<IRule, Set<IObject>> = new Map();

      if (changeSet.instantiated != null) {
        for (const instantiated of changeSet.instantiated.values()) {
          const cls = instantiated.cls;
          const rules = this.engine.rulesByClass.get(cls);
          if (rules != null) {
            for (const rule of rules) {
              let matches = matchesByRule.get(rule);
              if (matches == null) {
                matches = new Set();
                matchesByRule.set(rule, matches);
              }

              matches.add(instantiated.object);
            }
          }
        }
      }

      for (const [roleType, associations] of changeSet.associationsByRoleType) {
        for (const association of associations.values()) {
          const cls = association.cls;
          const patterns = this.engine.patternsByRoleTypeByClass.get(cls)?.get(roleType);
          if (patterns != null) {
            for (const pattern of patterns) {
              const rule = this.engine.ruleByPattern.get(pattern);
              let matches = matchesByRule.get(rule);
              if (matches != null) {
                matches = new Set();
                matchesByRule.set(rule, matches);
              }

              let source: IObject[];

              if (pattern.tree != null) {
                source = pattern.tree.reduce((acc, v) => {
                  acc.push(v.resolve(association.object));
                  return acc;
                }, []);
              } else {
                source = [association.object];
              }

              if (pattern.ofType != null) {
                source = source.filter((v) => pattern.ofType.isAssignableFrom(v.strategy.cls));
              }

              for (const obj of source) {
                matches.add(obj);
              }
            }
          }
        }
      }

      for (const [associationType, roles] of changeSet.rolesByAssociationType) {
        for (const association of roles) {
          const cls = association.cls;
          const patterns = this.engine.patternsByAssociationTypeByClass.get(cls)?.get(associationType);
          if (patterns != null) {
            for (const pattern of patterns) {
              const rule = this.engine.ruleByPattern.get(pattern);
              let matches = matchesByRule.get(rule);
              if (matches != null) {
                matches = new Set();
                matchesByRule.set(rule, matches);
              }

              let source: IObject[];

              if (pattern.tree != null) {
                source = pattern.tree.reduce((acc, v) => {
                  acc.push(v.resolve(association.object));
                  return acc;
                }, []);
              } else {
                source = [association.object];
              }

              if (pattern.ofType != null) {
                source = source.filter((v) => pattern.ofType.isAssignableFrom(v.strategy.cls));
              }

              for (const obj of source) {
                matches.add(obj);
              }
            }
          }
        }
      }

      for (const [rule, matches] of matchesByRule) {
        rule.derive(cycle, [...matches]);
      }

      changeSet = this.session.checkpoint();
    }

    return this.validation;
  }
}
