// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class RequestItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestItemDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.RequestItem.ObjectType, this.M.RequestItem.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestItemStateDraftThrowValidationError()
        {
            var request = new RequestForQuoteBuilder(this.Session).Build();

            var requestItem = new RequestItemBuilder(this.Session).Build();
            request.AddRequestItem(requestItem);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertAtLeastOne: RequestItem.Product\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedProductThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithProductFeature(new ColourBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.Product = new UnifiedGoodBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: RequestItem.Product\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedProductFeatureThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithProduct(new UnifiedGoodBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.ProductFeature = new ColourBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: RequestItem.Product\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedDescriptionThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithProduct(new UnifiedGoodBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.Description = "Description";

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: RequestItem.Product\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedNeededSkillThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithProduct(new UnifiedGoodBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.NeededSkill = new NeededSkillBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: RequestItem.Product\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedDeliverableThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithProduct(new UnifiedGoodBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.Deliverable = new DeliverableBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: RequestItem.Product\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedSerialisedItemThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithProductFeature(new ColourBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.SerialisedItem = new SerialisedItemBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: RequestItem.SerialisedItem\nRequestItem.ProductFeature"));
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateSubmitted()
        {
            var request = new RequestForInformationBuilder(this.Session)
                .WithRequestState(new RequestStates(this.Session).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Session).Build();
            request.AddRequestItem(requestItem);
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Session).Submitted;
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsSubmitted);
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateCancelled()
        {
            var request = new RequestForInformationBuilder(this.Session)
                .WithRequestState(new RequestStates(this.Session).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Session).Build();
            request.AddRequestItem(requestItem);
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Session).Cancelled;
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsCancelled);
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateRejected()
        {
            var request = new RequestForInformationBuilder(this.Session)
                .WithRequestState(new RequestStates(this.Session).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Session).Build();
            request.AddRequestItem(requestItem);
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Session).Rejected;
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsRejected);
        }

        [Fact]
        public void OnChangedRequestRequestStateDeriveRequestItemStateQuoted()
        {
            var request = new RequestForInformationBuilder(this.Session)
                .WithRequestState(new RequestStates(this.Session).Anonymous)
                .Build();

            var requestItem = new RequestItemBuilder(this.Session).Build();
            request.AddRequestItem(requestItem);
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsDraft);

            request.RequestState = new RequestStates(this.Session).Quoted;
            this.Session.Derive(false);

            Assert.True(requestItem.RequestItemState.IsQuoted);
        }

        [Fact]
        public void OnChangedUnitOfMeasureDeriveUnitOfMeasure()
        {
            var requestItem = new RequestItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            requestItem.RemoveUnitOfMeasure();
            this.Session.Derive(false);

            Assert.Equal(new UnitsOfMeasure(this.Session).Piece, requestItem.UnitOfMeasure);
        }

        [Fact]
        public void OnChangedQuantityThrowValidationError()
        {
            var requestItem = new RequestItemBuilder(this.Session)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            requestItem.Quantity = 2;

            var expectedMessage = $"{requestItem} { this.M.RequestItem.Quantity} { ErrorMessages.SerializedItemQuantity}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }

    [Trait("Category", "Security")]
    public class RequestItemDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public RequestItemDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.RequestItem.ObjectType, this.M.RequestItem.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestItemStateDraftDeriveDeletePermission()
        {
            var requestItem = new RequestItemBuilder(this.Session).WithRequestItemState(new RequestItemStates(this.Session).Draft).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, requestItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemStateSubmitDeriveDeletePermission()
        {
            var requestItem = new RequestItemBuilder(this.Session).WithRequestItemState(new RequestItemStates(this.Session).Quoted).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, requestItem.DeniedPermissions);
        }
    }
}
