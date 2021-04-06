// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class RequestForInformationRule : Rule
    {
        public RequestForInformationRule(MetaPopulation m) : base(m, new Guid("5BCE8864-6EC2-4672-A29D-CA49A6C49718")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.RequestForInformation, m.RequestForInformation.Recipient),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForInformation>())
            {
                if (@this.ExistCurrentVersion
                   && @this.CurrentVersion.ExistRecipient
                   && @this.Recipient != @this.CurrentVersion.Recipient)
                {
                    validation.AddError($"{@this} {this.M.RequestForInformation.Recipient} {ErrorMessages.InternalOrganisationChanged}");
                }
            }
        }
    }
}
