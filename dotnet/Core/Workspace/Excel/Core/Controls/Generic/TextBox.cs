namespace Application.Excel
{
    using System;
    using System.Globalization;
    using Allors;
    using Allors.Excel;
    using Allors.Workspace;
    using Allors.Workspace.Meta;
    using Newtonsoft.Json.Linq;
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

            this.Cell.Style = this.Object?.Strategy.CanWrite(this.RoleType) == true
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
                    var input = this.Cell.Value;
                    object output = null;

                    // TODO: Koen
                    if (input != null)
                    {
                        switch (this.RoleType.ObjectType.Tag)
                        {
                            case UnitTags.Binary:
                                if (input is string binaryStringValue)
                                {
                                    output = Convert.FromBase64String(binaryStringValue);
                                }

                                break;

                            case UnitTags.Boolean:
                                output = Convert.ToBoolean(input);
                                break;

                            case UnitTags.DateTime:
                                if (double.TryParse(Convert.ToString(input, CultureInfo.CurrentCulture), out double d))
                                {
                                    var dateTime = DateTime.FromOADate(d);
                                    output = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
                                }
                                break;

                            case UnitTags.Decimal:
                                output = Convert.ToDecimal(input);
                                break;

                            case UnitTags.Float:
                                output = Convert.ToDouble(input);
                                break;

                            case UnitTags.Integer:
                                output = Convert.ToInt32(input);
                                break;

                            case UnitTags.String:
                                output = Convert.ToString(input);
                                break;

                            case UnitTags.Unique:
                                if (input is string uniqueStringValue)
                                {
                                    Guid.TryParse(uniqueStringValue, out var guidOutput);
                                    output = guidOutput;
                                }

                                break;

                            default:
                                throw new ArgumentOutOfRangeException("Unknown Unit");
                        }
                    }

                    this.Object.Strategy.SetRole(this.RoleType, output);

                }
                else
                {
                    var relation = this.ToDomain(this.Cell.Value);
                    this.Object.Strategy.SetRole(this.RoleType, relation);
                }

                // TODO: check if role changed
                this.Cell.Style = Constants.ChangedStyle;
            }
            else
            {
                // cell value was changed while not allowed. Reset value.
                this.SetCellValue(this.Object, this.DisplayRoleType ?? this.RoleType);
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
