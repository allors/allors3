// <copyright file="NewsItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("d50ffc20-9e2d-4362-8e3f-b54d7368d487")]
    #endregion
    public partial class NewsItem : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("1a86dc14-eadc-4aad-83c2-238e31a20658")]
        #endregion

        public bool IsPublished { get; set; }

        #region Allors
        [Id("2f1736ea-0e74-43a9-b047-cc37bc9618fa")]
        #endregion
        [Size(256)]

        public string Title { get; set; }

        #region Allors
        [Id("369f8b36-1bb8-45b6-b02d-6cd7c126cb54")]
        #endregion

        public int DisplayOrder { get; set; }

        #region Allors
        [Id("372331ef-70a4-4a67-8f85-a0907ace9194")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Locale Locale { get; set; }

        #region Allors
        [Id("4a03f057-339f-4dd4-ac89-3b97d27d2170")]
        #endregion
        [Size(-1)]

        public string LongText { get; set; }

        #region Allors
        [Id("7aee11d0-f9b4-450d-83b8-357811e99246")]
        #endregion
        [Size(-1)]

        public string Text { get; set; }

        #region Allors
        [Id("a184408c-a1b0-47b2-821a-a2ab643b523e")]
        #endregion

        public DateTime Date { get; set; }

        #region Allors
        [Id("d29e707f-66dc-4fbf-aba4-63473498dd4b")]
        #endregion

        public bool Announcement { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
