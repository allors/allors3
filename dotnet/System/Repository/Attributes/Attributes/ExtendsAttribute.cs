// <copyright file="ExtendsAttribute.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository.Attributes
{
    using System;

    /// <summary>
    /// Names the domains that a domain extends. Their order plays no part: the order of the domains
    /// follows from the inheritance graph and the domain ids.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class ExtendsAttribute : RepositoryAttribute
    {
        public ExtendsAttribute(params string[] values) => this.Values = values;

        public string[] Values { get; set; }
    }
}
