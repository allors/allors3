// <copyright file="UnitOfMeasureConversionAccessor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class UnitOfMeasureConversionAccessor
    {
        private readonly IRoleType roleType;

        public UnitOfMeasureConversionAccessor(IRoleType roleType) => this.roleType = roleType;

        public decimal? Get(IObject @object, TimeFrequency toUnitOfMeasure)
        {
            foreach (var unitOfMeasureConversion in @object.Strategy.GetCompositesRole<UnitOfMeasureConversion>(this.roleType))
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
            foreach (var existingUnitOfMeasureConversion in @object.Strategy.GetCompositesRole<UnitOfMeasureConversion>(this.roleType))
            {
                if (existingUnitOfMeasureConversion?.ToUnitOfMeasure?.Equals(toUnitOfMeasure) == true)
                {
                    existingUnitOfMeasureConversion.ConversionFactor = conversionFactor;
                    return;
                }
            }

            var newUnitOfMeasureConversion = new UnitOfMeasureConversionBuilder(@object.Strategy.Transaction)
                .WithToUnitOfMeasure(toUnitOfMeasure)
                .WithConversionFactor(conversionFactor)
                .Build();
            @object.Strategy.AddCompositesRole(this.roleType, newUnitOfMeasureConversion);
        }
    }
}
