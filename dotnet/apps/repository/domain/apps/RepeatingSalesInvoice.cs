// <copyright file="RepeatingSalesInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("2439F72A-A435-4070-9A11-EDCDF679FCC9")]
    #endregion
    public partial class RepeatingSalesInvoice : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("9B472F44-5546-4E75-861A-6B7E4E3A068C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Required]
        [Workspace(Default)]
        public SalesInvoice Source { get; set; }

        #region Allors
        [Id("BFB6C78B-42AF-4704-A670-A28126072F47")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public TimeFrequency Frequency { get; set; }

        #region Allors
        [Id("C97F7B2C-7D81-4022-BB4A-C0760BC15F1B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public DayOfWeek DayOfWeek { get; set; }

        #region Allors
        [Id("6BDEBD8A-6993-477B-83A4-F7CC52FBD677")]
        #endregion
        [Required]
        [Workspace(Default)]
        public DateTime NextExecutionDate { get; set; }

        #region Allors
        [Id("7BA2A639-17BA-4296-813E-DEB76F056B87")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public DateTime PreviousExecutionDate { get; set; }

        #region Allors
        [Id("6CC66FEB-8126-437B-8372-4B8EC7827FB6")]
        #endregion
        [Workspace(Default)]
        public DateTime FinalExecutionDate { get; set; }

        #region Allors
        [Id("B32E3E20-5F2D-476A-B11D-19C996436649")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Workspace(Default)]
        public SalesInvoice[] SalesInvoices { get; set; }

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
