// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class InterfaceCreationDerivation : DomainDerivation
    {
        public InterfaceCreationDerivation(M m) : base(m, new Guid("7B903979-25E3-4485-A1C7-C02B16B621D9")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.I12.Interface)
            };


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var i12 in matches.Cast<I12>())
            {
                i12.I12CreationDerivation = true;
            }
        }
    }
}
