namespace ExcelDNA
{
    using System;
    using Allors.Excel;

    public class LoggerService : ILoggerService
    {
        public void Info(string text) => Console.WriteLine("Info: " + text);
    }
}
