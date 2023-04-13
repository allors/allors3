namespace Workspace.ViewModels.WinForms.Forms
{
    using Features;

    public partial class MainForm : Form
    {
        public MainForm(MainFormViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        private void MainForm_DataContextChanged(object sender, EventArgs e)
            => this.mainFormControllerBindingSource.DataSource = this.DataContext;
    }
}
