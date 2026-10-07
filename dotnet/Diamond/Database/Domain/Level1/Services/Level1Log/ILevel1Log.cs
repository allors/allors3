// <copyright file="ILevel1Log.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // Where the hooks of Level1, and of the domains that extend it, record that they ran: the
    // domain, the hook and, for a hook on an object or on the objects of a type, its subject. The
    // concrete domain provides the service, as it provides every service a domain declares.
    public interface ILevel1Log
    {
        void Record(string domain, string hook, object subject = null);
    }
}
