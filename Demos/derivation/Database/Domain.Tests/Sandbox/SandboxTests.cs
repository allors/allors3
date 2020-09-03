// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Domain;
    using Xunit;

    public class SandboxTests : DomainTest
    {
        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void Dummy(object data)
        {
            this.RegisterDerivations((DerivationTypes)data);

            // arrange

            // act

            // assert
        }
    }
}
