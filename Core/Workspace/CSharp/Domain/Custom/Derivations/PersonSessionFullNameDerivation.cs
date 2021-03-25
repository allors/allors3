// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System;
    using Meta;
    using Workspace.Derivations;

    public class PersonSessionFullNameDerivation : Rule
    {
        public PersonSessionFullNameDerivation(M m) : base(m, new Guid("CCDFC57D-5164-4BE6-9FFA-BA2862ABD006")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.Person.FirstName),
                new AssociationPattern(m.Person.LastName),
            };

        public override void Match(ICycle cycle, IObject match)
        {
            var person = (Person)match;
            person.SessionFullName = $"{person.FirstName} {person.LastName}";
        }
    }
}
