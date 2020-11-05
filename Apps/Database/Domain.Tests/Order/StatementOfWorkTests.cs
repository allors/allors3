// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    [Trait("Category", "Security")]
    public class StatementOfWorkSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public StatementOfWorkSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.StatementOfWork.ObjectType, this.M.StatementOfWork.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedRequestForStatementOfWorkStateCreatedDeriveDeletePermission()
        {
            var statementOfWork = new StatementOfWorkBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, statementOfWork.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestForProposalStateDeriveDeletePermission()
        {
            var statementOfWork = new StatementOfWorkBuilder(this.Session).Build();
            this.Session.Derive(false);

            statementOfWork.Send();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, statementOfWork.DeniedPermissions);
        }
    }
}
