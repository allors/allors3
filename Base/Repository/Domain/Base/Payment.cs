// <copyright file="Payment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("9f20a35c-d814-4690-a96f-2bcd25f6c6a2")]
    #endregion
    public partial interface Payment : Commentable, UniquelyIdentifiable, Deletable, Object
    {
        #region Allors
        [Id("4c8b7a4f-f151-419e-8365-ce0da0b3a709")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal Amount { get; set; }

        #region Allors
        [Id("5be2e66e-4714-4dc1-a0f2-a9f600815e41")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("7afc9649-43c9-4a60-a298-27361ba59765")]
        #endregion
        [Required]
        [Workspace(Default)]
        DateTime EffectiveDate { get; set; }

        #region Allors
        [Id("a80a1ed7-473b-493b-a9ab-23a682c6ae44")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party Sender { get; set; }

        #region Allors
        [Id("b0c79092-c5d0-426b-b06d-ccec574bb7d9")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        PaymentApplication[] PaymentApplications { get; set; }

        #region Allors
        [Id("f49e4d28-12a9-4575-818b-b475bec0c9d1")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string ReferenceNumber { get; set; }

        #region Allors
        [Id("faafa75e-496c-4220-ae3f-ab7d1e317484")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party Receiver { get; set; }
    }
}
