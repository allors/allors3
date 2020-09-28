// <copyright file="Position.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("04540476-602f-456a-b300-54166b65c8b1")]
    #endregion
    public partial class Position : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("137841cd-fa69-4704-a6e3-cd710c51af43")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Organisation Organisation { get; set; }

        #region Allors
        [Id("2806ca00-0b79-45e5-835e-b11f45b05f15")]
        #endregion

        public bool Temporary { get; set; }

        #region Allors
        [Id("39298cc2-0869-4dc9-b0ff-bea8269ba958")]
        #endregion

        public DateTime EstimatedThroughDate { get; set; }

        #region Allors
        [Id("6ede43f7-87a5-429c-8fc0-6441ca8753f1")]
        #endregion

        public DateTime EstimatedFromDate { get; set; }

        #region Allors
        [Id("8166d3b6-cc9d-486a-9321-5cd97ff49ddc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public PositionType PositionType { get; set; }

        #region Allors
        [Id("bf81174e-1105-4313-8d42-4a7b03bfc308")]
        #endregion

        public bool Fulltime { get; set; }

        #region Allors
        [Id("cb040fe9-8cdb-4e3a-9a32-e6700f1a8867")]
        #endregion

        public bool Salary { get; set; }

        #region Allors
        [Id("db94dd2c-5f39-4f64-ad6d-ce80bf7a4c22")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public PositionStatus PositionStatus { get; set; }

        #region Allors
        [Id("e1f8d2a3-83a7-4357-9451-858c314dbefc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public BudgetItem ApprovedBudgetItem { get; set; }

        #region Allors
        [Id("ec8beecc-9e28-4103-94d3-249aed76c934")]
        #endregion
        [Required]

        public DateTime ActualFromDate { get; set; }

        #region Allors
        [Id("fc328a1a-4f62-42de-96b2-a61c612a1602")]
        #endregion

        public DateTime ActualThroughDate { get; set; }

        #region inherited methods

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
