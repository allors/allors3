// <copyright file="EmailMessage.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("ab20998b-62b1-4064-a7b9-cc9416edf77a")]
    #endregion
    public partial class EmailMessage : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("5de25d18-3c36-418f-9c85-55a480d58bc5")]
        [Indexed]
        #endregion
        [Derived]
        [Required]
        [Workspace]
        public DateTime DateCreated { get; set; }

        #region Allors
        [Id("c297ff40-e2ad-46af-94fc-c61af6e6a6d6")]
        [Indexed]
        #endregion
        public DateTime DateSending { get; set; }

        #region Allors
        [Id("cc36e90a-dcda-4289-b84f-c947c97847b0")]
        [Indexed]
        #endregion
        [Workspace]
        public DateTime DateSent { get; set; }

        #region Allors
        [Id("e16da480-35ab-4383-940a-5298d0b33b9c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public User Sender { get; set; }

        #region Allors
        [Id("d115bcfb-55e5-4ed8-8a21-f8e4dd5f903d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace]
        public User[] Recipients { get; set; }

        #region Allors
        [Id("CD9C9D1E-3393-46B4-AD61-7AC03019EE08")]
        #endregion
        [Indexed]
        [Size(256)]
        [Workspace]
        public string RecipientEmailAddress { get; set; }

        #region Allors
        [Id("5666ebec-8205-4e5f-b0df-cacfa1af99ce")]
        #endregion
        [Size(1024)]
        [Required]
        [Workspace]
        public string Subject { get; set; }

        #region Allors
        [Id("25be1f1c-ea8b-471e-ad09-b618927dc15a")]
        #endregion
        [Size(-1)]
        [Required]
        [Workspace]
        public string Body { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
