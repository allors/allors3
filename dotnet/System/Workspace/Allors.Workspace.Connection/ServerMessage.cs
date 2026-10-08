// <copyright file="ServerMessage.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    /// <summary>
    /// A message the server sends on its own, over a transport that keeps a stream open. Server
    /// push defines its content; until then no transport sends one.
    /// </summary>
    public sealed class ServerMessage
    {
    }
}
