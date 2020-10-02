// <copyright file="ObjectBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;
    using Allors.Meta;

    public abstract class ObjectBuilder : IObjectBuilder
    {
        public static object Build(ISession session, IClass @class)
        {
            var metaService = session.Database.State().MetaCache;
            var builderType = metaService.GetBuilderType(@class);
            object[] parameters = { session };
            var builder = (IObjectBuilder)Activator.CreateInstance(builderType, parameters);
            return builder.DefaultBuild();
        }

        public abstract void Dispose();

        public abstract IObject DefaultBuild();
    }
}
