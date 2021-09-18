// <copyright file="NotificationList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("b6579993-4ff1-4853-b048-1f8e67419c00")]
    #endregion
    public partial class NotificationList : Deletable, Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("4516c5c1-73a0-4fdc-ac3c-aefaf417c8ba")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        #endregion
        [Workspace(Default)]
        public Notification[] Notifications { get; set; }

        #region Allors
        [Id("89487904-053e-470f-bcf9-0e01165b0143")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        #endregion
        [Derived]
        [Workspace(Default)]
        public Notification[] UnconfirmedNotifications { get; set; }

        #region Allors
        [Id("438fdc30-25ac-4d33-9a55-0ef817c05479")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        #endregion
        [Derived]
        [Workspace(Default)]
        public Notification[] ConfirmedNotifications { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        public void Delete()
        {
        }

        #endregion

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
    }
}
