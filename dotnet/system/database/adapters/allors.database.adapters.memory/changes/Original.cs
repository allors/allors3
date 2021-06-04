// <copyright file="ChangeLog.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class Original
    {
        internal Original(Strategy strategy) => this.Strategy = strategy;

        public Strategy Strategy { get; }

        public Dictionary<IRoleType, object> OriginalUnitRoleByRoleType { get; private set; }

        public Dictionary<IRoleType, Strategy> OriginalCompositeRoleByRoleType { get; private set; }

        public Dictionary<IRoleType, Strategy[]> OriginalCompositesRoleByRoleType { get; private set; }
        
        public void OnChangingUnitRole(IRoleType roleType, object previousRole)
        {
            this.OriginalUnitRoleByRoleType ??= new Dictionary<IRoleType, object>();

            if (!this.OriginalUnitRoleByRoleType.ContainsKey(roleType))
            {
                this.OriginalUnitRoleByRoleType.Add(roleType, previousRole);
            }
        }

        public void OnChangingCompositeRole(IRoleType roleType, Strategy previousRole)
        {
            this.OriginalCompositeRoleByRoleType ??= new Dictionary<IRoleType, Strategy>();

            if (!this.OriginalCompositeRoleByRoleType.ContainsKey(roleType))
            {
                this.OriginalCompositeRoleByRoleType.Add(roleType, previousRole);
            }
        }

        public void OnChangingCompositesRole(IRoleType roleType, IEnumerable<Strategy> previousRoles)
        {
            this.OriginalCompositesRoleByRoleType ??= new Dictionary<IRoleType, Strategy[]>();

            if (!this.OriginalCompositesRoleByRoleType.ContainsKey(roleType))
            {
                this.OriginalCompositesRoleByRoleType.Add(roleType, previousRoles.ToArray());
            }
        }

        public void Trim(ISet<IRoleType> roleTypes)
        {
        }

        public void Trim(ISet<IAssociationType> associationTypes)
        {
        }
    }
}
