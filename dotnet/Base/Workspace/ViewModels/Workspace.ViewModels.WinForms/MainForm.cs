namespace Workspace.ViewModels.WinForms
{
    using Controllers;

    public partial class MainForm : Form
    {
        private readonly MainFormController controller;

        public MainForm(MainFormController controller)
        {
            InitializeComponent();
            this.DataContext = controller;
        }

        private void MainForm_DataContextChanged(object sender, EventArgs e)
            => this.mainFormControllerBindingSource.DataSource = this.DataContext;
    }
}
