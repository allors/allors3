// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Repository.Domain
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class Domain
    {
        private readonly List<Domain> directSuperdomains;

        internal Domain(Guid id, string name, DirectoryInfo directoryInfo)
        {
            this.Id = id;
            this.Name = name;
            this.DirectoryInfo = directoryInfo;

            this.directSuperdomains = new List<Domain>();

            this.PartialInterfaceByName = new Dictionary<string, PartialInterface>();
            this.PartialClassBySingularName = new Dictionary<string, PartialClass>();
            this.PartialTypeBySingularName = new Dictionary<string, PartialType>();
        }

        public Guid Id { get; }

        public DirectoryInfo DirectoryInfo { get; }

        public string Name { get; }

        /// <summary>
        /// Gets the domains this domain extends, as declared in [Extends]. Their order plays no part.
        /// </summary>
        public IEnumerable<Domain> DirectSuperdomains => this.directSuperdomains;

        public Dictionary<string, PartialInterface> PartialInterfaceByName { get; }

        public Dictionary<string, PartialClass> PartialClassBySingularName { get; }

        public Dictionary<string, PartialType> PartialTypeBySingularName { get; }

        public override string ToString() => this.Name;

        internal void AddDirectSuperdomain(Domain superdomain) => this.directSuperdomains.Add(superdomain);
    }
}
