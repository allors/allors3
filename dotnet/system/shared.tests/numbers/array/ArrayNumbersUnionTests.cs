// <copyright file="ArrayNumbersUnionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    public class ArrayNumbersUnionTests : NumbersUnionTests
    {
        public override INumbers Numbers { get; }

        public ArrayNumbersUnionTests() => this.Numbers = new ArrayNumbers();
    }
}
