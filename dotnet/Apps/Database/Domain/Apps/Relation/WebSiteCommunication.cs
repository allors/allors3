// <copyright file="WebSiteCommunication.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WebSiteCommunication
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.WebSiteCommunication, this.M.WebSiteCommunication.CommunicationEventState),
        };
    }
}
