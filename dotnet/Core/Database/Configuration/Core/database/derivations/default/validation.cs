
// <copyright file="ValidationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Collections.Generic;
    using Database.Derivations;
    using Meta;

    public partial class Validation : IValidation
    {
        private readonly List<IDerivationError> errors;

        internal Validation() => this.errors = new List<IDerivationError>();

        public bool HasErrors => this.errors.Count > 0;

        public IDerivationError[] Errors => this.errors.ToArray();

        public void AddError(string error) => this.AddError(new DerivationErrorGeneric(this, relation: null, error));

        public void AddError(IDerivationError derivationError) => this.errors.Add(derivationError);

        public void AddError(IObject association, IRoleType roleType, string errorMessage, params object[] messageParam)
        {
            var error = new DerivationErrorGeneric(this, new DerivationRelation(association, roleType), errorMessage, messageParam);
            this.AddError(error);
        }

        public void AddError(IObject role, IAssociationType associationType, string errorMessage, params object[] messageParam)
        {
            var error = new DerivationErrorGeneric(this, new DerivationRelation(role, associationType), errorMessage, messageParam);
            this.AddError(error);
        }

        public void AssertExists(IObject association, IRoleType roleType)
        {
            if (!association.Strategy.ExistRole(roleType))
            {
                this.AddError(new DerivationErrorRequired(this, association, roleType));
            }
        }

        public void AssertNotExists(IObject association, IRoleType roleType)
        {
            if (association.Strategy.ExistRole(roleType))
            {
                this.AddError(new DerivationErrorNotAllowed(this, association, roleType));
            }
        }

        public void AssertNonEmptyString(IObject association, IRoleType roleType)
        {
            if (association.Strategy.ExistRole(roleType) && association.Strategy.GetUnitRole(roleType).Equals(string.Empty))
            {
                this.AddError(new DerivationErrorRequired(this, association, roleType));
            }
        }

        public void AssertExistsNonEmptyString(IObject association, IRoleType roleType)
        {
            this.AssertExists(association, roleType);
            this.AssertNonEmptyString(association, roleType);
        }

        public void AssertIsUnique(IChangeSet changeSet, IObject association, IRoleType roleType)
        {
            if (changeSet.RoleTypesByAssociation.TryGetValue(association, out var roleTypes) && roleTypes.Contains(roleType))
            {
                var objectType = roleType.AssociationType.ObjectType;
                var role = association.Strategy.GetRole(roleType);

                if (role != null)
                {
                    var transaction = association.Strategy.Transaction;
                    var extent = transaction.Extent(objectType);
                    extent.Filter.AddEquals(roleType, role);
                    if (extent.Count != 1)
                    {
                        this.AddError(new DerivationErrorUnique(this, association, roleType));
                    }
                }
            }
        }

        public void AssertAtLeastOne(IObject association, params IRoleType[] roleTypes)
        {
            foreach (var roleType in roleTypes)
            {
                if (association.Strategy.ExistRole(roleType))
                {
                    return;
                }
            }

            this.AddError(new DerivationErrorAtLeastOne(this, DerivationRelation.Create(association, roleTypes)));
        }

        public void AssertExistsAtMostOne(IObject association, params IRoleType[] roleTypes)
        {
            var count = 0;
            foreach (var roleType in roleTypes)
            {
                if (association.Strategy.ExistRole(roleType))
                {
                    ++count;
                }
            }

            if (count > 1)
            {
                this.AddError(new DerivationErrorAtMostOne(this, DerivationRelation.Create(association, roleTypes)));
            }
        }

        public void AssertAreEqual(IObject association, IRoleType roleType, IRoleType otherRoleType)
        {
            var value = association.Strategy.GetRole(roleType);
            var otherValue = association.Strategy.GetRole(otherRoleType);

            bool equal;
            if (value == null)
            {
                equal = otherValue == null;
            }
            else
            {
                equal = value.Equals(otherValue);
            }

            if (!equal)
            {
                this.AddError(new DerivationErrorEquals(this, DerivationRelation.Create(association, roleType, otherRoleType)));
            }
        }

        public void AssertExists(IObject role, IAssociationType associationType)
        {
            if (!role.Strategy.ExistAssociation(associationType))
            {
                this.AddError(new DerivationErrorRequired(this, role, associationType));
            }
        }
    }
}
