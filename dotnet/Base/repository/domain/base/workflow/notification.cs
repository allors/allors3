// <copyright file="Notification.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("73dcdc68-7571-4ed1-86db-77c914fe2f62")]
    #endregion
    public partial class Notification : Deletable, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9a226bec-31b9-413e-bec1-8dcdf36fa6fb")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        public UniquelyIdentifiable Target { get; set; }

        #region Allors
        [Id("50b1be30-d6a9-49e8-84da-a47647e443f0")]
        #endregion
        [Workspace(Default)]
        [Required]
        public bool Confirmed { get; set; }

        #region Allors
        [Id("70292962-9e0e-4b57-a710-c8ac34f65b11")]
        [Size(1024)]
        #endregion
        [Workspace(Default)]
        [Required]
        public string Title { get; set; }

        #region Allors
        [Id("e83600fc-5411-4c72-9903-80a3741a9165")]
        [Size(-1)]
        #endregion
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("458a8223-9c0f-4475-93c0-82d5cc133f1b")]
        [Derived]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Required]
        public DateTime DateCreated { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion

        #region Allors
        [Id("B445FC66-27AF-4D45-ADA8-4F1409EBBE72")]
        #endregion
        [Workspace(Default)]
        public void Confirm() { }
    }
}
