// <copyright file="Label.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Application.Excel
{
    using System;
    using Allors.Excel;
    using Allors.Workspace;
    using Allors.Workspace.Meta;
    using DateTime = System.DateTime;

    public class Label : IControl
    {
        public Label(ICell cell)
        {
            this.Cell = cell;
        }

        public IObject Object { get; internal set; }

        public IRoleType RoleType { get; internal set; }

        public IRoleType DisplayRoleType { get; internal set; }

        public Func<object, dynamic> GetRelation { get; internal set; }

        public IRoleType RelationType { get; internal set; }

        public ICell Cell { get; set; }

        public void Bind()
        {
            if (this.Object.Strategy.CanRead(this.DisplayRoleType ?? this.RoleType))
            {
                if (this.RelationType != null)
                {
                    var relation = (IObject)this.Object.Strategy.GetRole(this.DisplayRoleType ?? this.RoleType);
                    if (relation != null)
                    {
                        if (relation.Strategy.CanRead(this.RelationType))
                        {
                            this.SetCellValue(relation, this.RelationType);
                        }
                    }
                }
                else
                {
                    this.SetCellValue(this.Object, this.DisplayRoleType ?? this.RoleType);
                }
            }
        }

        public void OnCellChanged()
        {
            // Restore the object value
            this.SetCellValue(this.Object, this.RoleType);
        }

        public void Unbind()
        {
            // TODO
        }

        private void SetCellValue(IObject obj, IRoleType roleType)
        {
            if (roleType.ObjectType.ClrType == typeof(bool))
            {
                if (obj.Strategy.GetRole(roleType) is bool boolvalue && boolvalue)
                {
                    this.Cell.Value = Constants.YES;
                }
                else
                {
                    this.Cell.Value = Constants.NO;
                }
            }
            else if (roleType.ObjectType.ClrType == typeof(DateTime))
            {
                var dt = (DateTime?)obj?.Strategy.GetRole(roleType);
                this.Cell.Value = dt?.ToOADate();
            }
            else
            {
                this.Cell.Value = obj.Strategy.GetRole(roleType);
            }
        }
    }
}
