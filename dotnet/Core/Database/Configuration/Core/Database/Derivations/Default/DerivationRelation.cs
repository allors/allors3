// <copyright file="DerivationRelation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Text;
    using Database.Derivations;
    using Meta;

    public class DerivationRelation : IDerivationRelation
    {
        public DerivationRelation(IObject association, IRoleType roleType)
        {
            this.Association = association;
            this.RelationType = roleType.RelationType;
        }

        public DerivationRelation(IObject role, IAssociationType associationType)
        {
            this.Role = role;
            this.RelationType = associationType.RelationType;
        }

        public IRelationType RelationType { get; }

        public IObject Association { get; }

        public IObject Role { get; }

        public static DerivationRelation[] Create(IObject association, params IRoleType[] roleTypes)
        {
            var derivationRoles = new DerivationRelation[roleTypes.Length];
            for (var i = 0; i < derivationRoles.Length; i++)
            {
                derivationRoles[i] = new DerivationRelation(association, roleTypes[i]);
            }

            return derivationRoles;
        }

        public static DerivationRelation[] Create(IObject role, params IAssociationType[] associationTypes)
        {
            var derivationRoles = new DerivationRelation[associationTypes.Length];
            for (var i = 0; i < derivationRoles.Length; i++)
            {
                derivationRoles[i] = new DerivationRelation(role, associationTypes[i]);
            }

            return derivationRoles;
        }

        public static string ToString(IDerivationRelation[] relations)
        {
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var relation in relations)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(relation);
            }

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            if (this.Association != null)
            {
                if (this.RelationType != null)
                {
                    return this.Association.Strategy.Class.Name + "." + this.RelationType.RoleType.Name;
                }

                return this.Association.Strategy.Class.Name;
            }

            if (this.RelationType != null)
            {
                return this.Role.Strategy.Class.Name + "." + this.RelationType.AssociationType.Name;
            }

            return this.Role.Strategy.Class.Name;
        }
    }
}
