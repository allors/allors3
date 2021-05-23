// <copyright file="TransitionalConfiguration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    using Meta;

    public partial class TransitionalConfiguration
    {
        public TransitionalConfiguration(IClass objectType, IRoleType roleType)
        {
            var previousObjectState = objectType.DatabaseRoleTypes.FirstOrDefault(v => v.Name.Equals("Previous" + roleType.Name));
            var lastObjectState = objectType.DatabaseRoleTypes.FirstOrDefault(v => v.Name.Equals("Last" + roleType.Name));

            this.ObjectState = roleType;
            this.PreviousObjectState = previousObjectState ?? throw new Exception("Previous ObjectState is not defined for " + roleType.Name + " in type " + roleType.AssociationType.ObjectType.Name);
            this.LastObjectState = lastObjectState ?? throw new Exception("Last ObjectState is not defined for " + roleType.Name + " in type " + roleType.AssociationType.ObjectType.Name);
        }

        public IRoleType ObjectState { get; set; }

        public IRoleType PreviousObjectState { get; set; }

        public IRoleType LastObjectState { get; set; }
    }
}
