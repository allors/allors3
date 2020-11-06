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
    using System.Text.RegularExpressions;
    using Allors.Meta;
    using Derivations;
    using Resources;

    public class BankDerivation : DomainDerivation
    {
        public BankDerivation(M m) : base(m, new Guid("ee111e64-2d19-445a-b451-d2b23aadbf10")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Bank.Bic),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Bank>())
            {
                if (!string.IsNullOrEmpty(@this.Bic))
                {
                    if (!Regex.IsMatch(@this.Bic, "^([a-zA-Z]){4}([a-zA-Z]){2}([0-9a-zA-Z]){2}([0-9a-zA-Z]{3})?$"))
                    {
                        validation.AddError($"{@this}, {@this.Bic}, {ErrorMessages.NotAValidBic}");
                    }

                    var country = new Countries(@this.Strategy.Session).FindBy(@this.M.Country.IsoCode, @this.Bic.Substring(4, 2));
                    if (country == null)
                    {
                        validation.AddError($"{@this}, {@this.Bic}, {ErrorMessages.NotAValidBic}");
                    }
                }
            }
        }
    }
}
