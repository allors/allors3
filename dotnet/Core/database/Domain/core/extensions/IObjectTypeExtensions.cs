// <copyright file="IObjectTypeExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System;
    using Meta;

    public static class IObjectTypeExtensions
    {
        public static IObjects GetObjects(this IObjectType objectType, ITransaction transaction)
        {
            var objectFactory = transaction.Database.ObjectFactory;
            var type = objectFactory.Assembly.GetType(objectFactory.Namespace + "." + objectType.PluralName);
            return (IObjects)Activator.CreateInstance(type, transaction);
        }
    }
}
