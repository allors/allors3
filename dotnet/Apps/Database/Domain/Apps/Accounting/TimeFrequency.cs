// <copyright file="TimeFrequency.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class TimeFrequency
    {
        public decimal? GetConvertToFactor(TimeFrequency timeFrquency)
            => this.UnitOfMeasureConversions?.FirstOrDefault(c => c.ToUnitOfMeasure.Equals(timeFrquency)).ConversionFactor;

        public decimal? ConvertToFrequency(decimal value, TimeFrequency timeFrequency)
        {
            var conversion = this.GetConvertToFactor(timeFrequency);

            if (conversion != null)
            {
                return value * (decimal)conversion;
            }

            return null;
        }

        public string GetName() => this.Name;
    }
}
