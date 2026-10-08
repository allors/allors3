// <copyright file="Workspace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using System;
    using System.Collections.Generic;
    using Connection;
    using Derivations;
    using Meta;
    using Ranges;

    /// <summary>
    /// A workspace over a connection: it creates sessions, which build objects on the records of
    /// the connection, and gives new objects their ids.
    /// </summary>
    public class Workspace : IWorkspace
    {
        private readonly IDictionary<IRoleType, IRule> ruleByRoleType;
        private readonly IDictionary<IRoleType, IDictionary<IClass, IRule>> rulesByClassByRoleType;

        public Workspace(IDatabaseConnection connection, IObjectFactory objectFactory, IRule[] rules, IWorkspaceServices services, IdGenerator idGenerator = null)
        {
            this.Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.Configuration = new Configuration(connection, objectFactory ?? throw new ArgumentNullException(nameof(objectFactory)), rules ?? Array.Empty<IRule>());
            this.Services = services ?? throw new ArgumentNullException(nameof(services));
            this.IdGenerator = idGenerator ?? new IdGenerator();
            this.StrategyRanges = new DefaultClassRanges<Strategy>();

            this.ruleByRoleType = new Dictionary<IRoleType, IRule>();
            this.rulesByClassByRoleType = new Dictionary<IRoleType, IDictionary<IClass, IRule>>();

            foreach (var rule in this.Configuration.Rules)
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

            this.Services.OnInit(this);
        }

        public IDatabaseConnection Connection { get; }

        public IConfiguration Configuration { get; }

        public IWorkspaceServices Services { get; }

        public IdGenerator IdGenerator { get; }

        public IRanges<long> RecordRanges => this.Connection.Ranges;

        public IRanges<Strategy> StrategyRanges { get; }

        public ISession CreateSession() => new Session(this, this.Services.CreateSessionServices());

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
