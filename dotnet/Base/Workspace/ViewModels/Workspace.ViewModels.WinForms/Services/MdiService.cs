namespace Workspace.ViewModels.WinForms.Services
{
    using System.CodeDom;
    using Controllers;
    using Forms;
    using Microsoft.Extensions.DependencyInjection;
    using Workspace.ViewModels.Services;

    public class MdiService : IMdiService
    {
        public MdiService(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public void Open(Type controllerType)
        {
            var parent = this.ServiceProvider.GetRequiredService<MainForm>();

            if (controllerType == typeof(PersonFormController))
            {
                var form = this.ServiceProvider.GetRequiredService<PersonForm>();
                form.MdiParent = parent;
                form.Show();
            }
        }
    }
}
