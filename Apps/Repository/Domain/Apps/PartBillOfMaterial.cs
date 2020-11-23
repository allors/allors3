// <copyright file="PartBillOfMaterial.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("d204e616-039c-40c8-81cc-18f3a7345d99")]
    #endregion
    public partial interface PartBillOfMaterial : Commentable, Period, Object
    {
        #region Allors
        [Id("06c3a64a-ef2c-44a0-81ee-1335842cf844")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        Part Part { get; set; }

        #region Allors
        [Id("24de2b73-c51b-47b5-bd80-2022c0e37841")]
        #endregion
        [Size(-1)]

        string Instruction { get; set; }

        #region Allors
        [Id("ac18525c-57ef-4a11-a775-e27c397b334c")]
        #endregion
        [Required]

        int QuantityUsed { get; set; }

        #region Allors
        [Id("eb1b2313-df9b-407d-9cf9-617d58c6f4be")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        Part ComponentPart { get; set; }
    }
}
