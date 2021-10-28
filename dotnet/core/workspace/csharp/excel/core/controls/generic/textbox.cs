namespace Application.Excel
{
    using System;
    using System.Globalization;
    using Allors.Excel;
    using Allors.Workspace;
    using Allors.Workspace.Meta;
    using DateTime = System.DateTime;

    public class TextBox<T> : IControl where T : IObject
    {
        /// <summary>
        /// TextBox is a two-way binding object for excel cell value.
        /// </summary>
        /// <param name="cell"></param>
        public TextBox(ICell cell) => this.Cell = cell;

        public T Object { get; internal set; }

        public IRoleType RoleType { get; internal set; }

        public IRoleType DisplayRoleType { get; internal set; }

        public Func<object, dynamic> ToDomain { get; internal set; }

        /// <summary>
        /// Func called just before writing to the excel cell. Last chance to change the value.
        /// </summary>
        public Func<T, dynamic> ToCell { get; internal set; }

        public ICell Cell { get; set; }

        public IRoleType RelationType { get; internal set; }

        /// <summary>
        /// Factory must provide a new Object when the OnCellChanged event is handled.
        /// </summary>
        public Func<ICell, T> Factory { get; internal set; }

        public void Bind()
        {
            if (this.Object != null && this.Object.Strategy.CanRead(this.DisplayRoleType ?? this.RoleType))
            {
                if (this.RelationType != null)
                {
                    var relation = (T)this.Object.Strategy.GetRole(this.DisplayRoleType ?? this.RoleType);
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

            this.Cell.Style = this.Object?.Strategy.CanWrite(this.RoleType) == true || this.Factory != null
                ? Constants.WriteStyle
                : Constants.ReadOnlyStyle;
        }

        public void OnCellChanged()
        {
            if (this.Object == null && this.Factory != null)
            {
                this.Object = this.Factory(this.Cell);
            }

            if (this.Object?.Strategy.CanWrite(this.RoleType) == true)
            {
                if (this.ToDomain == null)
                {
                    if (this.RoleType.ObjectType.ClrType == typeof(bool))
                    {
                        if (Constants.YES.Equals(Convert.ToString(this.Cell.Value, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase))
                        {
                            this.Object.Strategy.SetRole(this.RoleType, true);
                        }
                        else
                        {
                            if (this.RoleType.IsRequired)
                            {
                                this.Object.Strategy.SetRole(this.RoleType, false);
                            }
                            else
                            {
                                this.Object.Strategy.SetRole(this.RoleType, null);
                            }
                        }
                    }
                    else if (this.RoleType.ObjectType.ClrType == typeof(DateTime))
                    {
                        if (double.TryParse(Convert.ToString(this.Cell.Value, CultureInfo.CurrentCulture), out double d))
                        {
                            var dt = DateTime.FromOADate(d);
                            this.Object.Strategy.SetRole(this.RoleType, dt);
                        }
                        else
                        {
                            if (this.RoleType.IsRequired)
                            {
                                this.Object.Strategy.SetRole(this.RoleType, DateTime.MinValue);
                            }
                            else
                            {
                                this.Object.Strategy.SetRole(this.RoleType, null);
                            }
                        }
                    }
                    else
                    {
                        this.Object.Strategy.SetRole(this.RoleType, this.Cell.Value);
                    }
                }
                else
                {
                    var relation = this.ToDomain(this.Cell.Value);
                    this.Object.Strategy.SetRole(this.RoleType, relation);
                }

                // TODO: check if role changed
                this.Cell.Style = Constants.ChangedStyle;
            }
        }

        public void Unbind()
        {
            // TODO
        }

        private void SetCellValue(T obj, IRoleType roleType)
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
                if (this.ToCell != null)
                {
                    this.Cell.Value = this.ToCell(obj);
                }
                else
                {
                    this.Cell.Value = obj.Strategy.GetRole(roleType);
                }
            }
        }
    }
}
