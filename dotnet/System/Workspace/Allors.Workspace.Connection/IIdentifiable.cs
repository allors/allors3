// <copyright file="IIdentifiable.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    /// <summary>
    /// Anything the workspace knows by an id: an object of a session, or a bare id in a layer
    /// without objects. The connection reads the id and nothing else. A database object has a
    /// positive id; a new object that has not been pushed has a negative one.
    /// </summary>
    public interface IIdentifiable
    {
        long Id { get; }
    }
}
