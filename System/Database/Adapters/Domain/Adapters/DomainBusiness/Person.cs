// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Allors;

    public partial class Person
    {
        public static Person Create(ISession session, string name)
        {
            var person = Create(session);
            person.Name = name;
            return person;
        }

        public static Person Create(ISession session, string name, int index)
        {
            var person = Create(session);
            person.Name = name;
            person.Index = index;
            return person;
        }

        public override string ToString() => this.Name;
    }
}
