// <copyright file="IPlugin1Log.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // Where the hooks of Plugin1 record that they ran: the domain, the hook and, for a hook on an
    // object or on the objects of a type, its subject. The plug-in knows only its host, so it
    // declares a service of its own; the concrete domain provides it.
    public interface IPlugin1Log
    {
        void Record(string domain, string hook, object subject = null);
    }
}
