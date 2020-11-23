// <copyright file="Document.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("1d21adf0-6008-459d-9f6a-3a026e7640bc")]
    #endregion
    public partial interface Document : Printable, Commentable, Object
    {
        #region Allors
        [Id("484d082e-b3e4-4915-a355-43315f466e6d")]
        #endregion
        [Required]
        [Size(256)]

        string Name { get; set; }

        #region Allors
        [Id("6f6ef875-2b0b-4a03-8b2e-bf242b48c843")]
        #endregion
        [Size(-1)]

        string Description { get; set; }

        #region Allors
        [Id("d579e6e7-6791-4b9b-bf20-43ab1a701866")]
        #endregion
        [Size(-1)]

        string Text { get; set; }

        #region Allors
        [Id("e97710fe-8def-44a8-8516-18d4eae8433b")]
        #endregion
        [Size(256)]

        string DocumentLocation { get; set; }
    }
}
