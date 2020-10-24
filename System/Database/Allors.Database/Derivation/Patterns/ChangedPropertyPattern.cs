// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

using Allors.Meta;

namespace Allors
{
    public abstract class ChangedPropertyPattern : Pattern
    {
        public abstract IPropertyType PropertyType { get; }
    }
}
