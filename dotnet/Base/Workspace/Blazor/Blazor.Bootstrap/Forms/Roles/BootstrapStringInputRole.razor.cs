namespace Allors.Workspace.Blazor.Bootstrap.Forms.Roles
{
    public class ABSStringInputBase : RoleField
    {
        public string StringModel
        {
            get => (string)this.Model;
            set => this.Model = value;
        }

        public BlazorStrap.InputType InputType
        {
            get
            {
                if (this.TextType == "number")
                {
                    return BlazorStrap.InputType.Number;
                }

                return BlazorStrap.InputType.Text;
            }
        }
    }
}
