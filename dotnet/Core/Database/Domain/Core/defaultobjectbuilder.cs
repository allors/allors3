// <copyright file="ObjectBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Database.Services;
    using Meta;
    
    public static class DefaultObjectBuilder
    {
        public static IObject Build(ITransaction transaction, IClass @class)
        {
            var metaCache = transaction.Database.Services.Get<IMetaCache>();
            var builderType = metaCache.GetBuilderType(@class);
            object[] parameters = { transaction };
            var builder = (IObjectBuilder)Activator.CreateInstance(builderType, parameters);
            return builder.DefaultBuild();
        }
    }
}
