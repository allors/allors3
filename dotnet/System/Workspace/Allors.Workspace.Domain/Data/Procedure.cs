// <copyright file="Pull.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{
    using System.Collections.Generic;
    using Workspace;

    public class Procedure : IVisitable
    {
        public Procedure(string name) => this.Name = name;

        public string Name { get; }

        public IDictionary<string, IObject[]> Collections { get; set; }

        public IDictionary<string, IObject> Objects { get; set; }

        public IDictionary<string, string> Values { get; set; }

        public IDictionary<IObject, long> Pool { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitProcedure(this);
    }
}
