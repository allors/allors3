import { IAssociationPattern, IPattern, IRolePattern, IRule } from '@allors/workspace/domain/system';
import { AssociationType, Class, Composite, RoleType } from '@allors/workspace/meta/system';

export class Engine {
  classesByRule: Map<IRule, Set<Class>>;

  rulesByClass: Map<Class, IRule[]>;

  patternsByRoleTypeByClass: Map<Class, Map<RoleType, Set<IRolePattern>>>;

  patternsByAssociationTypeByClass: Map<Class, Map<AssociationType, Set<IAssociationPattern>>>;

  ruleByPattern: Map<IPattern, IRule>;

  patternsByClassByRule: Map<IRule, Map<Class, IPattern[]>>;

  public constructor(public rules: Readonly<IRule[]>) {
    Object.freeze(this.rules);
    this.classesByRule = new Map();
    this.rulesByClass = new Map();
    this.patternsByRoleTypeByClass = new Map();
    this.patternsByAssociationTypeByClass = new Map();
    this.ruleByPattern = new Map();
    this.patternsByClassByRule = new Map();

    for (const rule of rules) {
      let ruleClasses = new Set<Class>();

      for (const pattern of rule.patterns) {
        this.ruleByPattern.set(pattern, rule);

        let patternClasses: Set<Class>;

        switch (pattern.kind) {
          case 'RolePattern':
            if (pattern.objectType) {
              patternClasses = pattern.objectType.classes;
            } else {
              patternClasses = (pattern.roleType.associationType.objectType as Composite).classes;
            }
            break;
          case 'AssociationPattern':
            if (pattern.AssociationType.roleType.objectType.isComposite) {
              ruleClasses = new Set([...ruleClasses, ...(pattern.AssociationType.roleType.objectType as Composite).classes]);
            }
            break;
        }

        if (patternClasses) {
          ruleClasses = new Set([...ruleClasses, ...patternClasses]);

          let patternsByClass = this.patternsByClassByRule.get(rule);
          if (patternsByClass == null) {
            patternsByClass = new Map();
            this.patternsByClassByRule.set(rule, patternsByClass);
          }

          for (const patternClass of patternClasses) {
            let patterns = patternsByClass.get(patternClass);
            if (patterns == null) {
              patterns = [];
              patternsByClass.set(patternClass, patterns);
            }

            patterns.push(pattern);
          }

          switch (pattern.kind) {
            case 'RolePattern':
              for (const patternClass of patternClasses) {
                let patternsByRoleType = this.patternsByRoleTypeByClass.get(patternClass);
                if (patternsByRoleType == null) {
                  patternsByRoleType = new Map();
                  this.patternsByRoleTypeByClass.set(patternClass, patternsByRoleType);
                }

                const roleType = pattern.roleType;

                let patterns = patternsByRoleType.get(roleType);
                if (patterns == null) {
                  patterns = new Set();
                  patternsByRoleType.set(roleType, patterns);
                }

                patterns.add(pattern);
              }

              break;

            case 'AssociationPattern':
              for (const patternClass of patternClasses) {
                let patternsByAssociationType = this.patternsByAssociationTypeByClass.get(patternClass);
                if (patternsByAssociationType == null) {
                  patternsByAssociationType = new Map();
                  this.patternsByAssociationTypeByClass.set(patternClass, patternsByAssociationType);
                }

                const AssociationType = pattern.AssociationType;

                let patterns = patternsByAssociationType.get(AssociationType);
                if (patterns == null) {
                  patterns = new Set();
                  patternsByAssociationType.set(AssociationType, patterns);
                }

                patterns.add(pattern);
              }

              break;
          }
        }
      }

      this.classesByRule.set(rule, ruleClasses);
    }

    const classes = new Set<Class>();
    for (const [, ruleClasses] of this.classesByRule) {
      for (const ruleClass of ruleClasses) {
        classes.add(ruleClass);
      }
    }

    for (const cls of classes) {
      const classRules = rules.filter((v) => this.classesByRule.get(v)?.has(cls));
      this.rulesByClass.set(cls, classRules);
    }
  }
}
