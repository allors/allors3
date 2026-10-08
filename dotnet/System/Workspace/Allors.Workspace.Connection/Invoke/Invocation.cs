// <copyright file="Invocation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;

    /// <summary>
    /// A method to invoke on a database object: the object's id, the version the caller holds,
    /// which the server compares with its own, and the method type.
    /// </summary>
    public sealed class Invocation
    {
        public Invocation(long id, long version, IMethodType methodType)
        {
            this.Id = id;
            this.Version = version;
            this.MethodType = methodType;
        }

        public long Id { get; }

        public long Version { get; }

        public IMethodType MethodType { get; }
    }
}
