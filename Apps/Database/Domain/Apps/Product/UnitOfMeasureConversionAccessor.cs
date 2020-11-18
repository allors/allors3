// <copyright file="LocalisedText.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class UnitOfMeasureConversionAccessor
    {
        private readonly RoleType roleType;

        public UnitOfMeasureConversionAccessor(RoleType roleType) => this.roleType = roleType;

        public decimal? Get(IObject @object, TimeFrequency toUnitOfMeasure)
        {
            var unitOfMeasureConversions = @object.Strategy.GetCompositeRoles(this.roleType);
            foreach (UnitOfMeasureConversion unitOfMeasureConversion in unitOfMeasureConversions)
            {
                if (unitOfMeasureConversion?.ToUnitOfMeasure?.Equals(toUnitOfMeasure) == true)
                {
                    return unitOfMeasureConversion.ConversionFactor;
                }
            }

            return null;
        }

        public void Set(IObject @object, TimeFrequency toUnitOfMeasure, decimal conversionFactor)
        {
            var unitOfMeasureConversions = @object.Strategy.GetCompositeRoles(this.roleType);
            foreach (UnitOfMeasureConversion existingUnitOfMeasureConversion in unitOfMeasureConversions)
            {
                if (existingUnitOfMeasureConversion?.ToUnitOfMeasure?.Equals(toUnitOfMeasure) == true)
                {
                    existingUnitOfMeasureConversion.ConversionFactor = conversionFactor;
                    return;
                }
            }

            var newUnitOfMeasureConversion = new UnitOfMeasureConversionBuilder(@object.Strategy.Session)
                .WithToUnitOfMeasure(toUnitOfMeasure)
                .WithConversionFactor(conversionFactor)
                .Build();
            @object.Strategy.AddCompositeRole(this.roleType, newUnitOfMeasureConversion);
        }
    }
}
