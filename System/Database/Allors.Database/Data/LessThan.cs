// <copyright file="LessThan.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System.Collections.Generic;

    using Meta;
    using Data;

    public class LessThan : IRolePredicate
    {
        public string[] Dependencies { get; set; }

        public LessThan(IRoleType roleType = null) => this.RoleType = roleType;

        public IRoleType RoleType { get; set; }

        public object Value { get; set; }

        public string Parameter { get; set; }

        bool IPredicate.ShouldTreeShake(IDictionary<string, string> parameters) => this.HasMissingDependencies(parameters) || ((IPredicate)this).HasMissingArguments(parameters);

        bool IPredicate.HasMissingArguments(IDictionary<string, string> parameters) => this.Parameter != null && (parameters == null || !parameters.ContainsKey(this.Parameter));

        void IPredicate.Build(ISession session, IDictionary<string, string> parameters, Database.ICompositePredicate compositePredicate)
        {
            var value = this.Parameter != null ? UnitConvert.Parse(this.RoleType.ObjectType.Id, parameters[this.Parameter]) : this.Value;

            compositePredicate.AddLessThan(this.RoleType, value);
        }

        public void Accept(IVisitor visitor) => visitor.VisitLessThan(this);
    }
}
