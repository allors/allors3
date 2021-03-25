// <copyright file="RolePattern.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDerivation type.</summary>

namespace Allors.Workspace
{
    using Meta;

    public class RolePattern : Pattern
    {
        public RolePattern(IRoleType roleType) => this.RoleType = roleType;

        public IRoleType RoleType { get; }
    }
}
