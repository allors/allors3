// <copyright file="AssociationType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the AssociationType type.</summary>

namespace Allors.Workspace.Meta
{
    using System.Collections.Generic;

    public interface IInterfaceInternals : ICompositeInternals, IInterface
    {
        new ISet<ICompositeInternals> DirectSubtypes { get; set; }

        new ISet<ICompositeInternals> Subtypes { get; set; }

        new ISet<IClassInternals> Classes { get; set; }
    }
}
