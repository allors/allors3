// <copyright file="StepExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Data
{
    using Meta;

    public static class StepExtensions
    {
        public static Allors.Data.Step Load(this Step @this, ISession session)
        {
            var metaPopulation = session.Database.ObjectFactory.MetaPopulation;
            var propertyType = (IPropertyType)metaPopulation.FindAssociationType(@this.AssociationType) ?? metaPopulation.FindRoleType(@this.RoleType);

            return new Allors.Data.Step
            {
                PropertyType = propertyType,
                Next = @this.Next?.Load(session),
                Include = @this.Include?.Load(session),
            };
        }
    }
}
