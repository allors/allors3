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

    public class LetterCorrespondenceDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("7C1C3F73-2FE2-4713-8006-682E979E38CE");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.LetterCorrespondence.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var letterCorrespondence in matches.Cast<LetterCorrespondence>())
            {
                letterCorrespondence.WorkItemDescription = $"Letter to {letterCorrespondence.ToParty.PartyName} about {letterCorrespondence.Subject}";
            }
        }
    }
}
