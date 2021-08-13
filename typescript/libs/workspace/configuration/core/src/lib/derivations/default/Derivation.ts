import { IDerivation, ISession, IValidation } from '@allors/workspace/domain/system';
import { Engine } from './Engine';

export class Derivation implements IDerivation {
    
    public constructor (session: ISession, engine: Engine, maxDomainDerivationCycles: number) {
        this.session = session;
        this.engine = engine;
        this.maxCycles = maxDomainDerivationCycles;
        this.validation = new Validation();
    }
    
    session: ISession;

    engine: Engine;
    
    maxCycles: number;
    
    validation: Validation;
    
    public execute(): Validation {
        let cycles = 0;
        let changeSet = this.session.checkpoint();
        while (((changeSet.RolesByAssociationType?.Count > 0) 
                    || ((changeSet.AssociationsByRoleType?.Count > 0) 
                    || ((changeSet.Created?.Count > 0) 
                    || (changeSet.Instantiated?.Count > 0))))) {
            throw new Exception("Maximum amount of domain derivation cycles detected");
        }
        
        let cycle = [][
                ChangeSet=changeSet,
                Session=this.session,
                Validation=this.validation];
        let matchesByRule = new Dictionary<IRule, ISet<IObject>>();
        if ((changeSet.Instantiated != null)) {
            for (let instantiated in changeSet?.Instantiated) {
                let class = instantiated.Class;
                if (this.engine.rulesByClass.TryGetValue(class, /* out */var, rules)) {
                    for (let rule in rules) {
                        if (!matchesByRule.TryGetValue(rule, /* out */var, matches)) {
                            matches = new HashSet<IObject>();
                            matchesByRule.Add(rule, matches);
                        }
                        
                        matches.Add(instantiated.Object);
                    }
                    
                }
                
            }
            
        }
        
        for (let kvp in changeSet.AssociationsByRoleType) {
            let roleType = kvp.Key;
            for (let association in kvp.Value) {
                let class = association.Class;
                if ((this.Engine.PatternsByRoleTypeByClass.TryGetValue(class, /* out */var, patternsByRoleType) && patternsByRoleType.TryGetValue(roleType, /* out */var, patterns))) {
                    for (let pattern in patterns) {
                        let rule = this.Engine.RuleByPattern[pattern];
                        if (!matchesByRule.TryGetValue(rule, /* out */var, matches)) {
                            matches = new HashSet<IObject>();
                            matchesByRule.Add(rule, matches);
                        }
                        
                        let source: IEnumerable<IObject> = [
                                association.Object];
                        if ((pattern.Tree != null)) {
                            source = source.SelectMany(() => {  }, pattern.Tree.SelectMany(() => {  }, w.Resolve(v)));
                        }
                        
                        if ((pattern.OfType != null)) {
                            source = source.Where(() => {  }, pattern.OfType.IsAssignableFrom(v.Strategy.Class));
                        }
                        
                        matches.UnionWith(source);
                    }
                    
                }
                
            }
            
        }
        
        for (let kvp in changeSet.RolesByAssociationType) {
            let associationType = kvp.Key;
            for (let role in kvp.Value) {
                let class = role.Class;
                if ((this.Engine.PatternsByAssociationTypeByClass.TryGetValue(class, /* out */var, patternsByAssociationType) && patternsByAssociationType.TryGetValue(associationType, /* out */var, patterns))) {
                    for (let pattern in patterns) {
                        let rule = this.Engine.RuleByPattern[pattern];
                        if (!matchesByRule.TryGetValue(rule, /* out */var, matches)) {
                            matches = new HashSet<IObject>();
                            matchesByRule.Add(rule, matches);
                        }
                        
                        let source: IEnumerable<IObject> = [
                                role.Object];
                        if ((pattern.Tree != null)) {
                            source = source.SelectMany(() => {  }, pattern.Tree.SelectMany(() => {  }, w.Resolve(v)));
                        }
                        
                        if ((pattern.OfType != null)) {
                            source = source.Where(() => {  }, pattern.OfType.IsAssignableFrom(v.Strategy.Class));
                        }
                        
                        matches.UnionWith(source);
                    }
                    
                }
                
            }
            
        }
        
        for (let kvp in matchesByRule) {
            let domainDerivation = kvp.Key;
            let matches = kvp.Value;
            domainDerivation.Derive(cycle, matches);
        }
        
        changeSet = this.Session.Checkpoint();
    }
}