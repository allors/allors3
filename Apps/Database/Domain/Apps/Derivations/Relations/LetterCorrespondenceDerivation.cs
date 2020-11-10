// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class LetterCorrespondenceDerivation : DomainDerivation
    {
        public LetterCorrespondenceDerivation(M m) : base(m, new Guid("7C1C3F73-2FE2-4713-8006-682E979E38CE")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.LetterCorrespondence.Subject),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<LetterCorrespondence>())
            {
                @this.WorkItemDescription = $"Letter to {@this.ToParty.PartyName} about {@this.Subject}";
            }
        }
    }
}
