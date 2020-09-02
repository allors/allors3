// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class ServiceDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("8A24C272-EE5E-416D-B840-DFF2C82C47F4");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Service.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var serviceExtension in matches.Cast<Service>())
            {
                serviceExtension.BaseOnDeriveVirtualProductPriceComponent();
            }
        }
    }

}
