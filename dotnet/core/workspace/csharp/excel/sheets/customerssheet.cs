namespace Application.Sheets
{
    using System;
    using System.Drawing;
    using Allors.Excel;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Person = Allors.Workspace.Domain.Person;

    public class CustomersSheet : ISheet, ISaveable
    {
        public CustomersSheet(Program program)
        {
            this.Sheet = program.ActiveWorkbook.AddWorksheet();
            this.Binder = new Binder(this.Sheet, new Style(Color.DeepSkyBlue, Color.Black));
            this.Binder.ToDomained += this.Binder_ToDomained;

            this.M = program.M;
            this.MessageService = program.Workspace.Services.Get<IMessageService>();
            this.ErrorService = program.Workspace.Services.Get<IErrorService>();

            this.Session = program.Workspace.CreateSession();
        }

        private async void Binder_ToDomained(object sender, EventArgs e) => await this.Sheet.Flush();

        public M M { get; set; }

        public ISession Session { get; }

        public IWorksheet Sheet { get; }

        public Binder Binder { get; set; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        public async System.Threading.Tasks.Task Load()
        {
            var pull = new Pull
            {
                Extent = new Filter(this.M.Person),
            };

            var result = await this.Session.PullAsync(pull);

            //this.Session.Reset();

            var customers = result.GetCollection<Person>();

            var index = 0;
            var firstName = new Column { Header = "First Name", Index = index++, NumberFormat = "@" };
            var lastName = new Column { Header = "Last Name", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                firstName,
                lastName,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var customer in customers)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                    this.Sheet[row, column.Index].Style = new Style(Color.Aqua, Color.BurlyWood);
                }

                this.Binder.Set(row, firstName.Index, new RoleTypeBinding(customer, this.M.Person.FirstName));
                this.Binder.Set(row, lastName.Index, new RoleTypeBinding(customer, this.M.Person.LastName));
                row++;
            }

            this.Binder.ResetChangedCells();

            var obsoleteCells = this.Binder.ToCells();
            foreach (var obsoleteCell in obsoleteCells)
            {
                obsoleteCell.Clear();
            }

            await this.Sheet.Flush();
        }

        public async System.Threading.Tasks.Task Save()
        {
            var response = await this.Session.PushAsync();
            if (response.HasErrors)
            {
                this.ErrorService.Handle(response, this.Session);
            }
            else
            {
                this.MessageService.Show("Successfully saved", "Info");
            }

            await this.Load();
        }

        public async System.Threading.Tasks.Task Refresh()
        {
            await this.Load();
        }
    }
}
