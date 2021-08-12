// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PickListRule : Rule
    {
        public PickListRule(MetaPopulation m) : base(m, new Guid("8D9F3C91-DBA7-44AA-AA60-C1A58CAFDF0D")) =>
            this.Patterns = new Pattern[]
            {
                m.PickList.RolePattern(v => v.Store),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PickList>())
            {
                if (@this.Store.IsImmediatelyPicked)
                {
                    @this.SetPicked();
                }
            }
        }
    }
}
