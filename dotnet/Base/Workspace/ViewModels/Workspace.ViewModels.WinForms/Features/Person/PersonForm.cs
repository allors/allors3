namespace Workspace.ViewModels.WinForms.Forms
{
    using System.Windows.Forms;
    using Features;

    public partial class PersonForm : Form
    {
        public PersonForm(PersonFormViewModel viewModel)
        {
            InitializeComponent();
            this.ViewModel = viewModel;
            this.DataContext = this.ViewModel;
        }

        public PersonFormViewModel ViewModel { get; set; }

        private void PersonForm_DataContextChanged(object sender, EventArgs e)
            => this.personFormControllerBindingSource.DataSource = this.DataContext;

        private void peopleBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            var current = (PersonViewModel)this.peopleBindingSource.Current;

            this.ViewModel.Selected = current;

            if (this.ViewModel.Selected != current)
            {
                var index = this.peopleBindingSource.List.IndexOf(this.ViewModel.Selected);

                if (index >= 0)
                {
                    this.peopleBindingSource.Position = index;
                    this.peopleBindingSource.CurrencyManager.Refresh();
                }
            }
        }
    }
}
