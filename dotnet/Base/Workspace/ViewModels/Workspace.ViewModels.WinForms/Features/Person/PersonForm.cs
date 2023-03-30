namespace Workspace.ViewModels.WinForms.Forms
{
    using System.Windows.Forms;
    using Workspace.ViewModels.Controllers;

    public partial class PersonForm : Form
    {
        public PersonForm(PersonFormController controller)
        {
            InitializeComponent();
            this.ViewModel = controller;
            this.DataContext = this.ViewModel;
        }

        public PersonFormController ViewModel { get; set; }

        private void PersonForm_DataContextChanged(object sender, EventArgs e)
            => this.personFormControllerBindingSource.DataSource = this.DataContext;

        private void peopleBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            var current = (PersonModel)this.peopleBindingSource.Current;

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
