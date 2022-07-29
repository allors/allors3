namespace Application.Excel
{
    using Allors.Excel;
    using Allors.Workspace;

    public class TextBox : TextBox<IObject>
    {
        public TextBox(ICell cell) : base(cell)
        {
        }
    }
}