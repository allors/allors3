// <copyright file="DerivationError.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Collections.Generic;
    using Database.Derivations;
    using Meta;

    public abstract class DerivationError : IDerivationError
    {
        private readonly string message;

        protected DerivationError(IValidation validation, IDerivationRelation[] relations, string errorMessage)
            : this(validation, relations, errorMessage, new object[] { DerivationRelation.ToString(relations) })
        {
        }

        protected DerivationError(IValidation validation, IDerivationRelation[] relations, string errorMessage, object[] errorMessageParameters)
        {
            this.Validation = validation;
            this.Relations = relations;

            try
            {
                if (errorMessageParameters != null && errorMessageParameters.Length > 0)
                {
                    this.message = string.Format(errorMessage, errorMessageParameters);
                }
                else
                {
                    this.message = string.Format(errorMessage, new object[] { DerivationRelation.ToString(relations) });
                }
            }
            catch
            {
                this.message = this.GetType().Name + ": " + DerivationRelation.ToString(this.Relations);
            }
        }

        public IValidation Validation { get; }

        public IDerivationRelation[] Relations { get; }

        public IRoleType[] RoleTypes
        {
            get
            {
                var roleTypes = new List<IRoleType>();
                foreach (var relation in this.Relations)
                {
                    var roleType = relation.RelationType.RoleType;
                    if (!roleTypes.Contains(roleType))
                    {
                        roleTypes.Add(roleType);
                    }
                }

                return roleTypes.ToArray();
            }
        }

        public virtual string Message => this.message;

        public override string ToString() => this.message;
    }
}
