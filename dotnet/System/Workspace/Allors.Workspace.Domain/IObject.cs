// <copyright file="Object.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    /// <summary>
    /// An object of a session. Its id, from <see cref="IIdentifiable"/>, is negative for
    /// <ul>
    /// <li>a database object that is new and has never been pushed</li>
    /// <li>a workspace or session object</li>
    /// </ul>
    /// and positive for a database object that has been pulled.
    /// </summary>
    public interface IObject : IIdentifiable
    {
        IStrategy Strategy { get; }
    }
}
