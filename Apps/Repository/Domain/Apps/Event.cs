// <copyright file="Event.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("aad26d12-9e80-410c-ab99-57064bd3dd2e")]
    #endregion
    public partial class Event : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("189505d9-434f-4d12-a6ab-44edcf44801c")]
        #endregion

        public bool RegistrationRequired { get; set; }

        #region Allors
        [Id("1a4f5119-23c5-4cbe-afdb-565c0e8f9e80")]
        #endregion
        [Size(256)]

        public string Link { get; set; }

        #region Allors
        [Id("6eb8fbc4-7fbd-4eb6-8944-01737b1182cc")]
        #endregion
        [Size(256)]

        public string Location { get; set; }

        #region Allors
        [Id("78cfaf88-c3c4-41d1-b9f0-f69a82646930")]
        #endregion
        [Size(-1)]

        public string Text { get; set; }

        #region Allors
        [Id("79b05cf2-2175-4724-acdd-88bc05f15881")]
        #endregion
        [Size(-1)]

        public string AnnouncementText { get; set; }

        #region Allors
        [Id("7a66f2bc-bfb1-420a-a383-acf3092ca48b")]
        #endregion

        public DateTime From { get; set; }

        #region Allors
        [Id("7d73d60c-bcb2-4be6-bc60-e4420a8d0417")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Locale Locale { get; set; }

        #region Allors
        [Id("b044d498-2995-41d2-8487-0ec323b011bc")]
        #endregion
        [Size(256)]

        public string Title { get; set; }

        #region Allors
        [Id("cbc5a9f6-cd08-41aa-a4aa-dac9a8a802ac")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Media Photo { get; set; }

        #region Allors
        [Id("d9d15920-705f-4ca3-bfa1-47bd5d5b7238")]
        #endregion

        public bool Announce { get; set; }

        #region Allors
        [Id("de61dd0d-1f8e-4a55-9fe4-f44cf35b6a31")]
        #endregion

        public DateTime To { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}
