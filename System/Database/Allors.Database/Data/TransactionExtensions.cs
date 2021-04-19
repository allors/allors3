// <copyright file="TransactionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Linq;

    using Meta;

    internal static class TransactionExtensions
    {
        internal static IMetaObject GetMetaObject(this ITransaction @this, object value)
        {
            switch (value)
            {
                case IComposite metaObject:
                    return metaObject;

                case Guid idAsGuid:
                    return @this.Database.MetaPopulation.FindById(idAsGuid);

                case string idAsString:
                    return @this.Database.MetaPopulation.FindById(new Guid(idAsString));

                default:
                    throw new ArgumentException();
            }
        }

        internal static IObject GetObject(this ITransaction @this, object value)
        {
            if (value == null)
            {
                return null;
            }

            switch (value)
            {
                case IObject @object:
                    return @object;

                case long idAsLong:
                    return @this.Instantiate(idAsLong);

                case string idAsString:
                    return @this.Instantiate(idAsString);

                default:
                    throw new ArgumentException();
            }
        }

        internal static IObject[] GetObjects(this ITransaction @this, object value)
        {
            var emptyArray = Array.Empty<IObject>();

            if (value == null)
            {
                return emptyArray;
            }

            if (value is string idAsString)
            {
                return @this.GetObjects(idAsString.Split(','));
            }

            switch (value)
            {
                case IObject[] objects:
                    return objects;

                case long[] idAsLongs:
                    return idAsLongs.Select(@this.Instantiate).Where(v => v != null).ToArray();

                case string[] idAsStrings:
                    return idAsStrings.Select(@this.Instantiate).Where(v => v != null).ToArray();

                case IObject @object:
                    return new[] { @object };

                case long idAsLong:
                    var objectFromLong = @this.Instantiate(idAsLong);
                    return objectFromLong != null ? new[] { objectFromLong } : emptyArray;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
