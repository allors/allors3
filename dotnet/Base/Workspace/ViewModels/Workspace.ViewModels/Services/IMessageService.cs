namespace Workspace.ViewModels.Services
{
    public interface IMessageService
    {
        void Show(string message, string title);

        bool? ShowDialog(string message, string title);
    }
}
