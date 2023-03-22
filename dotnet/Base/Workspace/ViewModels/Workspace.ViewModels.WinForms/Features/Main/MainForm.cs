namespace Workspace.ViewModels.WinForms.Forms
{
    using Controllers;

    public partial class MainForm : Form
    {
        public MainForm(MainFormController controller)
        {
            this.InitializeComponent();
            this.DataContext = controller;
        }

        private void MainForm_DataContextChanged(object sender, EventArgs e)
            => this.mainFormControllerBindingSource.DataSource = this.DataContext;
    }
}
