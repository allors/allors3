// <copyright file="Priceable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("26e69a5f-0220-4b60-99bf-26e150bcb64c")]
    #endregion
    public partial interface Priceable : Commentable, Transitional, Object
    {
        #region Allors
        [Id("5ffe1fc4-704e-4a3f-a27f-d4a47c99c37b")]
        #endregion
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        decimal AssignedUnitPrice { get; set; }

        #region Allors
        [Id("6dc95126-3287-46e0-9c21-4d6561262a2e")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        decimal UnitBasePrice { get; set; }

        #region Allors
        [Id("7595b52c-012b-42db-9cf2-46939b39f57f")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        decimal UnitPrice { get; set; }

        #region Allors
        [Id("3722807a-0634-4df2-8964-4778b4edc314")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitSurcharge { get; set; }

        #region Allors
        [Id("37623a64-9f6c-4f35-8e72-c4332037db4d")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitDiscount { get; set; }

        #region Allors
        [Id("131359fb-29f2-4ebb-adc2-1e53a99a4e6b")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal UnitVat { get; set; }

        #region Allors
        [Id("c897fe12-da96-47e6-b00e-920cb9e1c790")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        VatRegime AssignedVatRegime { get; set; }

        #region Allors
        [Id("27f86828-7b4e-4d80-9c3c-095813240c1a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        VatRegime DerivedVatRegime { get; set; }

        #region Allors
        [Id("5367e41e-b1c3-4311-87b4-6ba2732de1e6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        VatRate VatRate { get; set; }

        #region Allors
        [Id("b88638a1-4c91-4b50-80d8-430cf840c38b")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        DiscountAdjustment[] DiscountAdjustments { get; set; }

        #region Allors
        [Id("d4ea50dd-1e6e-44d2-8405-3a98a4b99104")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        SurchargeAdjustment[] SurchargeAdjustments { get; set; }

        #region Allors
        [Id("d0b1e607-07dc-43e2-a003-89559c87a441")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        decimal TotalBasePrice { get; set; }

        #region Allors
        [Id("dc71aecf-1735-4858-b74f-65e805565eed")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal TotalExVat { get; set; }

        #region Allors
        [Id("32792771-06c8-4805-abc4-2e2f9c37c6f3")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal TotalVat { get; set; }

        #region Allors
        [Id("a271f7d4-cda1-4ae9-94e4-dda482bd8cd5")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal TotalIncVat { get; set; }

        #region Allors
        [Id("0284a618-f661-4054-a09d-f94f9f778106")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal GrandTotal { get; set; }

        #region Allors
        [Id("b4398edb-2a36-459d-95a1-5d209462ae02")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal TotalDiscount { get; set; }

        #region Allors
        [Id("05254848-d99a-430e-80cd-e042ded3de71")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalDiscountAsPercentage { get; set; }

        #region Allors
        [Id("b81633d1-5b22-42b9-a484-d401d06022fb")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(5)]
        [Workspace(Default)]
        decimal TotalSurcharge { get; set; }

        #region Allors
        [Id("a573b8bf-42a6-4389-9f46-1def243220bf")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal TotalSurchargeAsPercentage { get; set; }
    }
}
