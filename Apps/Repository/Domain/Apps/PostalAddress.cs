// <copyright file="PostalAddress.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("d54b4bba-a84c-4826-85ba-7340714035c7")]
    #endregion
    public partial class PostalAddress : ContactMechanism, GeoLocatable, Object
    {
        #region inherited properties
        public string Description { get; set; }

        public ContactMechanism[] FollowTo { get; set; }

        public ContactMechanismType ContactMechanismType { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Allors
        [Id("c83eb0ff-8503-4f2a-9280-f8e46b382b6a")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Address1 { get; set; }

        #region Allors
        [Id("9475dd68-43f7-4195-bf57-8ce82333980e")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Address2 { get; set; }

        #region Allors
        [Id("5440794c-8569-46fb-a5cb-42dc523e1264")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Address3 { get; set; }

        #region Allors
        [Id("24216a78-41d8-4ffc-958a-2411530eeb94")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        public GeographicBoundary[] PostalAddressBoundaries { get; set; }

        #region Allors
        [Id("2edd7f54-5596-46c1-9f8a-813c947d95fb")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string PostalCode { get; set; }

        #region Allors
        [Id("7166cc1b-1f00-4cef-9875-8092cd4a76a0")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Locality { get; set; }

        #region Allors
        [Id("d92c5fd4-68e9-402b-b540-86053df1b70d")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string Region { get; set; }

        #region Allors
        [Id("c0e1c31b-5506-48c0-b46f-239f89eca08f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Country Country { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
