// <copyright file="Plugin1.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Repository.Attributes;

// The test plug-in, hosted by Core. It knows Core's model and nothing of the other domains.
[Domain("385a7628-32ef-472b-b982-cd705ab710af")]
[Extends(nameof(Core))]
public struct Plugin1
{
}
