// <copyright file="ObjectBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Meta;
    using Database;

    public static class DefaultObjectBuilder
    {
        public static object Build(ITransaction transaction, IClass @class)
        {
            var metaService = transaction.Database.Context().MetaCache;
            var builderType = metaService.GetBuilderType(@class);
            object[] parameters = { transaction };
            var builder = (IObjectBuilder)Activator.CreateInstance(builderType, parameters);
            return builder.DefaultBuild();
        }
    }
}
