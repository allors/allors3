// <copyright file="Workspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using Derivations;
    using Meta;
    using Ranges;

    public abstract class Workspace : IWorkspace
    {
        private readonly IDictionary<IRoleType, IRule> ruleByRoleType;
        private readonly IDictionary<IRoleType, IDictionary<IClass, IRule>> rulesByClassByRoleType;

        protected Workspace(DatabaseConnection database, IWorkspaceServices services, IRanges<long> recordRanges)
        {
            this.DatabaseConnection = database;
            this.Services = services;
            this.RecordRanges = recordRanges;
            this.StrategyRanges = new DefaultClassRanges<Strategy>();

            this.ruleByRoleType = new Dictionary<IRoleType, IRule>();
            this.rulesByClassByRoleType = new Dictionary<IRoleType, IDictionary<IClass, IRule>>();

            foreach (var rule in database.Configuration.Rules)
            {

                var roleType = rule.RoleType;

                if (roleType.AssociationType.ObjectType.IsClass)
                {
                    this.ruleByRoleType.Add(roleType, rule);
                }
                else
                {
                    if (!this.rulesByClassByRoleType.TryGetValue(roleType, out var ruleByClass))
                    {
                        ruleByClass = new Dictionary<IClass, IRule>();
                        this.rulesByClassByRoleType.Add(roleType, ruleByClass);
                    }

                    var objectType = rule.ObjectType;
                    foreach (var cls in objectType.Classes)
                    {
                        ruleByClass.Add(cls, rule);
                    }
                }
            }
        }

        public DatabaseConnection DatabaseConnection { get; }

        public IConfiguration Configuration => this.DatabaseConnection.Configuration;

        public IWorkspaceServices Services { get; }

        public IRanges<long> RecordRanges { get; }

        public IRanges<Strategy> StrategyRanges { get; }

        public abstract ISession CreateSession();

        public IRule GetRule(IRoleType roleType, Strategy strategy)
        {
            if (roleType.AssociationType.ObjectType.IsClass)
            {
                this.ruleByRoleType.TryGetValue(roleType, out var rule);
                return rule;
            }

            if (this.rulesByClassByRoleType.TryGetValue(roleType, out var rulesByClass))
            {
                rulesByClass.TryGetValue(strategy.Class, out var rule);
                return rule;
            }

            return null;
        }
    }
}
