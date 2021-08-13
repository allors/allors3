import { IAssociationPattern, IPattern, IPatternKind, IRolePattern, IRule } from '@allors/workspace/domain/system';
import { AssociationType, Class, Composite, RoleType } from '@allors/workspace/meta/system';

export class Engine {
  classesByRule: Map<IRule, Set<Class>>;

  rulesByClass: Map<Class, IRule[]>;

  patternsByRoleTypeByClass: Map<Class, Map<RoleType, Set<IRolePattern>>>;

  patternsByAssociationTypeByClass: Map<Class, Map<AssociationType, Set<IAssociationPattern>>>;

  ruleByPattern: Map<IPattern, IRule>;

  public constructor(rules: IRule[]) {
    this.classesByRule = new Map();
    this.patternsByRoleTypeByClass = new Map();
    this.patternsByAssociationTypeByClass = new Map();
    this.ruleByPattern = new Map();

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

          switch (pattern.kind) {
            case 'RolePattern':
              // for (const cls in patternClasses) {
              //     if (!this.PatternsByRoleTypeByClass.TryGetValue(class, /* out */var, patternsByRoleType)) {
              //         patternsByRoleType = new Dictionary<IRoleType, ISet<IRolePattern>>();
              //         this.PatternsByRoleTypeByClass.Add(class, patternsByRoleType);
              //     }

              //     let roleType = rolePattern.RoleType;
              //     if (!patternsByRoleType.TryGetValue(roleType, /* out */var, patterns)) {
              //         patterns = new HashSet<IRolePattern>();
              //         patternsByRoleType.Add(roleType, patterns);
              //     }

              //     patterns.Add(rolePattern);
              // }

              break;
            case 'AssociationPattern':
              // for (let class in patternClasses) {
              //     if (!this.PatternsByAssociationTypeByClass.TryGetValue(class, /* out */var, patternsByAssociationType)) {
              //         patternsByAssociationType = new Dictionary<IAssociationType, ISet<IAssociationPattern>>();
              //         this.PatternsByAssociationTypeByClass.Add(class, patternsByAssociationType);
              //     }

              //     let associationType = associationPattern.AssociationType;
              //     if (!patternsByAssociationType.TryGetValue(associationType, /* out */var, patterns)) {
              //         patterns = new HashSet<IAssociationPattern>();
              //         patternsByAssociationType.Add(associationType, patterns);
              //     }

              //     patterns.Add(associationPattern);
              // }

              break;
          }
        }
      }

      this.classesByRule.set(rule, ruleClasses);
    }

    // var classes = this.classesByRule.SelectMany(v => v.Value).Distinct();
    // this.rulesByClass = classes.ToDictionary(v => v, v => rules.Where(w => this.ClassesByRule[w].Contains(v)).ToArray());
  }
}
