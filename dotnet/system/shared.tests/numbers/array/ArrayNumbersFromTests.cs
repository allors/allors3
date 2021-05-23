// <copyright file="ArrayNumbersFromTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using System.Runtime.CompilerServices;

    public class ArrayNumbersFromTests : NumbersFromTests
    {
        public override INumbers Numbers { get; }

        public ArrayNumbersFromTests() => this.Numbers = new ArrayNumbers();
    }
}
