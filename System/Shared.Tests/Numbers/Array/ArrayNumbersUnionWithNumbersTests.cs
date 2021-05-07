// <copyright file="ArrayNumbersUnionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using System.Runtime.CompilerServices;

    public class ArrayNumbersUnionWithNumbersTests : NumbersUnionWithNumbersTests
    {
        public override INumbers Numbers { get; }

        public ArrayNumbersUnionWithNumbersTests() => this.Numbers = new ArrayNumbers();
    }
}
