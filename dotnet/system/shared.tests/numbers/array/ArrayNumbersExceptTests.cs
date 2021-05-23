// <copyright file="ArrayNumbersExceptTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    public class ArrayNumbersExceptTests : NumbersExceptTests
    {
        public override INumbers Numbers { get; }

        public ArrayNumbersExceptTests() => this.Numbers = new ArrayNumbers();
    }
}
