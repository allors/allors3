// <copyright file="TermType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("1468c86a-4ac4-4c64-a93b-1b0c5f4b41bc")]
    #endregion
    public interface TermType : Enumeration
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("b0e2c7a6-d73e-46bb-be54-ededdb22e9bb")]
        #endregion
        [Indexed]
        [Workspace]
        string Abbreviation { get; set; }

        #region inherited methods
        #endregion
    }
}
