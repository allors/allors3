// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class ProfessionalServicesRelationshipCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdProfessionalServicesRelationsips= changeSet.Created.Select(v=>v.GetObject()).OfType<ProfessionalServicesRelationship>();

                foreach(var professionalServicesRelationship in createdProfessionalServicesRelationsips)
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

        public static void ProfessionalServicesRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("cf9f1b0a-469f-4bd5-836e-d54732a24713")] = new ProfessionalServicesRelationshipCreationDerivation();
        }
    }
}
