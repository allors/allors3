namespace Workspace.ViewModels.WinForms.Forms
{
    using System.Windows.Forms;
    using Workspace.ViewModels.Controllers;

    public partial class PersonForm : Form
    {
        public PersonForm(PersonFormController controller)
        {
            InitializeComponent();
            this.DataContext = controller;
        }

        private void PersonForm_DataContextChanged(object sender, EventArgs e)
            => this.personFormControllerBindingSource.DataSource = this.DataContext;
    }
}
