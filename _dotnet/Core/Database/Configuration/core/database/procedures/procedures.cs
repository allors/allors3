// <copyright file="v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Procedures : IProcedures
    {
        private readonly IDictionary<string, IProcedure> procedureByLowercaseName;

        public Procedures(Assembly assembly) =>
            this.procedureByLowercaseName = assembly
                .GetTypes()
                .Where(type => type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IProcedure)))
                .ToDictionary(v => v.Name.ToLowerInvariant(), v => (IProcedure)v.GetTypeInfo().GetConstructor(Type.EmptyTypes)?.Invoke(null));

        public IProcedure Get(string name)
        {
            if (name == null)
            {
                return null;
            }

            this.procedureByLowercaseName.TryGetValue(name.ToLowerInvariant(), out var procedure);
            return procedure;
        }
    }
}
