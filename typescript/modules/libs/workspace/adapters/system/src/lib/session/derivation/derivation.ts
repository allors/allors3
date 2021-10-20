import { ICycle, IDerivation, IObject, IRule, ISession, IValidation, resolve } from '@allors/workspace/domain/system';
import { Engine } from './engine';

export class Derivation implements IDerivation {
  public constructor(public session: ISession, public engine: Engine, public activeRules: Set<IRule>, public maxCycles: number) {
    this.validation = { errors: [] };
  }

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
          const cls = instantiated.strategy.cls;
          const rules = this.engine.rulesByClass.get(cls)?.filter((v) => this.activeRules.has(v));

          if (rules != null) {
            for (const rule of rules) {
              const patternsByClass = this.engine.patternsByClassByRule.get(rule);
              const patterns = patternsByClass?.get(cls);
              if (patterns != null) {
                for (const pattern of patterns) {
                  let matches = matchesByRule.get(rule);
                  if (matches == null) {
                    matches = new Set();
                    matchesByRule.set(rule, matches);
                  }

                  let source: IObject[];

                  if (pattern.tree != null) {
                    source = pattern.tree.reduce((acc, v) => {
                      for (const obj of resolve(instantiated, v, true)) {
                        acc.push(obj);
                      }
                      return acc;
                    }, []);
                  } else {
                    source = [instantiated];
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
        }
      }

      for (const [roleType, associations] of changeSet.associationsByRoleType) {
        for (const association of associations) {
          const cls = association.strategy.cls;
          const patterns = this.engine.patternsByRoleTypeByClass.get(cls)?.get(roleType);

          if (patterns != null) {
            for (const pattern of patterns) {
              const rule = this.engine.ruleByPattern.get(pattern);
              if (!this.activeRules.has(rule)) {
                continue;
              }

              let matches = matchesByRule.get(rule);
              if (matches == null) {
                matches = new Set();
                matchesByRule.set(rule, matches);
              }

              let source: IObject[];

              if (pattern.tree != null) {
                source = pattern.tree.reduce((acc, v) => {
                  acc.push(resolve(association, v, true));
                  return acc;
                }, []);
              } else {
                source = [association];
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
          const cls = association.strategy.cls;
          const patterns = this.engine.patternsByAssociationTypeByClass.get(cls)?.get(associationType);
          if (patterns != null) {
            for (const pattern of patterns) {
              const rule = this.engine.ruleByPattern.get(pattern);
              if (!this.activeRules.has(rule)) {
                continue;
              }

              let matches = matchesByRule.get(rule);
              if (matches == null) {
                matches = new Set();
                matchesByRule.set(rule, matches);
              }

              let source: IObject[];

              if (pattern.tree != null) {
                source = pattern.tree.reduce((acc, v) => {
                  acc.push(resolve(association, v, true));
                  return acc;
                }, []);
              } else {
                source = [association];
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
