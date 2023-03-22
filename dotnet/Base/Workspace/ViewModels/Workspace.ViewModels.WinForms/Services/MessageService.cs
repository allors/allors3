namespace Workspace.ViewModels.WinForms.Services
{
    using Workspace.ViewModels.Services;

    public class MessageService : IMessageService
    {
        public void Show(string text, string caption) => MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public bool? ShowDialog(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information) switch
            {
                DialogResult.Yes => true,
                DialogResult.No => false,
                _ => null,
            };
    }
}
