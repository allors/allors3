// <copyright file="AffectedObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using Meta;

    public sealed class AffectedObject
    {
        internal AffectedObject(long id, IClass @class, AffectedObjectKind kind)
        {
            this.Id = id;
            this.Class = @class;
            this.Kind = kind;
        }

        public long Id { get; }

        public IClass Class { get; }

        public AffectedObjectKind Kind { get; internal set; }
    }
}
