// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
    using Resources;

    public class StatementOfWorkRule : Rule
    {
        public StatementOfWorkRule(MetaPopulation m) : base(m, new Guid("8307B027-0A59-409F-B47C-B2B2C98267C8")) =>
            this.Patterns = new Pattern[]
            {
                m.StatementOfWork.RolePattern(v => v.Issuer),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;


            foreach (var @this in matches.Cast<StatementOfWork>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistIssuer
                    && @this.Issuer != @this.CurrentVersion.Issuer)
                {
                    validation.AddError(@this, this.M.StatementOfWork.Issuer, ErrorMessages.InternalOrganisationChanged);
                }
            }
        }
    }
}
