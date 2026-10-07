// <copyright file="Test.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Repository.Attributes;

// The concrete domain of the tree. It extends the functional domain Level2 and selects the
// plug-in Plugin1, so the population is a diamond: it reaches Core through Level2 and Level1,
// and through Plugin1. The order of the names in [Extends] plays no part.
[Domain("bec75779-2d99-4980-866b-0e75755ae9bf")]
[Extends(nameof(Level2), nameof(Plugin1))]
public struct Test
{
}
