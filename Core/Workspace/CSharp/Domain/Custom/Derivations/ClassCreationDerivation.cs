// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace.Meta;
    using Derivations;

    public class ClassCreationDerivation : DomainDerivation
    {
        public ClassCreationDerivation(M m) : base(m, new Guid("C728614E-B129-4F60-8BE8-B10898B29AB4")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.C1.Class)
            };


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var c1 in matches.Cast<C1>())
            {
                c1.C1CreationDerivation = true;
            }
        }
    }
}
