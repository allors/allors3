// <copyright file="Sort.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System.Collections.Generic;

    public class Procedure : IVisitable
    {
        public Procedure(string name) => this.Name = name;

        public string Name { get; }

        public IDictionary<string, IObject[]> CollectionByName { get; set; }

        public IDictionary<string, IObject> ObjectByName { get; set; }

        public IDictionary<string, object> ValueByName { get; set; }

        public IDictionary<IObject, long> VersionByObject { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitProcedure(this);
    }
}