// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Resources;
    using Xunit;
    using Permission = Domain.Permission;

    public class RequestItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestItemRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.RequestItem, this.M.RequestItem.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestItemStateDraftThrowValidationError()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).Build();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtLeastOne>();
            Assert.Contains(this.M.RequestItem.Product, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.RequestItem.ProductFeature, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedProductThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithProductFeature(new ColourBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.Product = new UnifiedGoodBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Contains(this.M.RequestItem.Product, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.RequestItem.ProductFeature, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedProductFeatureThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithProduct(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.ProductFeature = new ColourBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Contains(this.M.RequestItem.Product, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.RequestItem.ProductFeature, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedDescriptionThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithProduct(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.Description = "Description";

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Contains(this.M.RequestItem.Product, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.RequestItem.ProductFeature, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedNeededSkillThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithProduct(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.NeededSkill = new NeededSkillBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Contains(this.M.RequestItem.Product, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.RequestItem.ProductFeature, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedDeliverableThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithProduct(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.Deliverable = new DeliverableBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>().ToList();
            Assert.Contains(this.M.RequestItem.Product, errors.SelectMany(v => v.RoleTypes).Distinct());
            Assert.Contains(this.M.RequestItem.ProductFeature, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedSerialisedItemThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithProductFeature(new ColourBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.RequestItem.SerialisedItem,
                this.M.RequestItem.ProductFeature,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateSubmitted()
        {
            var request = new RequestForInformationBuilder(this.Transaction)
                .WithRequestState(new RequestStates(this.Transaction).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Transaction).Submitted;
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsSubmitted);
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateCancelled()
        {
            var request = new RequestForInformationBuilder(this.Transaction)
                .WithRequestState(new RequestStates(this.Transaction).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Transaction).Cancelled;
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsCancelled);
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateRejected()
        {
            var request = new RequestForInformationBuilder(this.Transaction)
                .WithRequestState(new RequestStates(this.Transaction).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Transaction).Rejected;
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsRejected);
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateQuoted()
        {
            var request = new RequestForInformationBuilder(this.Transaction)
                .WithRequestState(new RequestStates(this.Transaction).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Transaction).Quoted;
            this.Transaction.Derive(false);

            Assert.True(requestItem.RequestItemState.IsQuoted);
        }

        [Fact]
        public void OnChangedUnitOfMeasureDeriveUnitOfMeasure()
        {
            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requestItem.RemoveUnitOfMeasure();
            this.Transaction.Derive(false);

            Assert.Equal(new UnitsOfMeasure(this.Transaction).Piece, requestItem.UnitOfMeasure);
        }

        [Fact]
        public void OnChangedQuantityThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            requestItem.Quantity = 2;

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.SerializedItemQuantity));
        }
    }

    [Trait("Category", "Security")]
    public class RequestItemDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestItemDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.RequestItem, this.M.RequestItem.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var requestItem = new RequestItemBuilder(this.Transaction).WithRequestItemState(new RequestItemStates(this.Transaction).Draft).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var requestItem = new RequestItemBuilder(this.Transaction).WithRequestItemState(new RequestItemStates(this.Transaction).Quoted).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, requestItem.DeniedPermissions);
        }
    }
}
