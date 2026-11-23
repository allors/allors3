// <copyright file="UniqueAttribute.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : RepositoryAttribute
    {
        public UniqueAttribute(bool value = true) => this.Value = value;

        public bool Value { get; set; }
    }
}
