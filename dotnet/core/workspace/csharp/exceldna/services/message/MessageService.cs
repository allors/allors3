namespace ExcelDNA
{
    using System;
    using System.Reflection;
    using Allors.Excel;

    public class MessageService : IMessageService
    {
        //public void Show(string text, string caption) => MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

        //public bool? ShowDialog(string message, string title) =>
        //    MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information) switch
        //    {
        //        DialogResult.Yes => true,
        //        DialogResult.No => false,
        //        _ => null,
        //    };
        public void Show(string message, string title) => Console.WriteLine(title + ": " + message);

        public bool? ShowDialog(string message, string title)
        {
            Console.WriteLine(title + ": " + message);
            return false;
        }
    }
}
