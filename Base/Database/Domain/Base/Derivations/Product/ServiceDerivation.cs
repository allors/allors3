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

    public class ServiceDerivation : DomainDerivation
    {
        public ServiceDerivation(M m) : base(m, new Guid("8A24C272-EE5E-416D-B840-DFF2C82C47F4")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Service.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var serviceExtension in matches.Cast<Service>())
            {
                serviceExtension.BaseOnDeriveVirtualProductPriceComponent();
            }
        }
    }

}
