// <copyright file="DerivedAttribute.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class DerivedAttribute : RepositoryAttribute
    {
        public DerivedAttribute(bool value = true) => this.Value = value;

        public bool Value { get; set; }
    }
}
