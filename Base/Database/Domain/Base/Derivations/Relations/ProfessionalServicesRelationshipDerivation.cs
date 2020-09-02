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

    public class ProfessionalServicesRelationshipCreationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("DB0D802D-94D2-4850-91FC-703778ECFFC7");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.ProfessionalServicesRelationship.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var professionalServicesRelationship in matches.Cast<ProfessionalServicesRelationship>())
            {
                professionalServicesRelationship.Parties = new Party[] { professionalServicesRelationship.Professional, professionalServicesRelationship.ProfessionalServicesProvider };

                if (!professionalServicesRelationship.ExistProfessional | !professionalServicesRelationship.ExistProfessionalServicesProvider)
                {
                    // TODO: Move Delete
                    professionalServicesRelationship.Delete();
                }
            }
        }
    }
}
