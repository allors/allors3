// <copyright file="AutomatedAgent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    public partial class AutomatedAgent : User
    {
        #region inherited properties


        public NotificationList NotificationList { get; set; }

        #endregion


        #region inherited methods
        #endregion
    }
}
