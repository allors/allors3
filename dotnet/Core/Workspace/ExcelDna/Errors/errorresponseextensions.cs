
namespace ExcelDNA
{
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Allors.Workspace;
    using NLog;

    public static class ErrorResponseExtensions
    {
        public static void HandleErrors(this IResult @this, ISession session)
        {
            if (@this.HasErrors)
            {
                @this.Log(session);
                @this.Show();
            }
        }

        public static void Show(this IResult error)
        {
            if (error.AccessErrors?.Count() > 0)
            {
                MessageBox.Show(@"You do not have the required rights.", @"Access Error");
            }
            else if (error.DerivationErrors?.Count() > 0)
            {
                var message = new StringBuilder();
                foreach (var derivationError in error.DerivationErrors)
                {
                    message.Append($" - {derivationError.Message}\n");
                }

                MessageBox.Show(message.ToString(), @"Derivation Errors");
            }
            else if (error.VersionErrors?.Count() > 0 || error.MissingErrors?.Count() > 0)
            {
                MessageBox.Show(@"Modifications were detected since last access.", @"Concurrency Error");
            }
            else
            {
                MessageBox.Show($@"{error.ErrorMessage}", @"General Error");
            }
        }

        public static void Log(this IResult errorResponse, ISession session)
        {
            var logger = LogManager.GetCurrentClassLogger();

            if (errorResponse.AccessErrors?.Count() > 0)
            {
                foreach (var error in errorResponse.AccessErrors)
                {
                    logger.Error("Access error: " + Message(error));
                }
            }
            else if (errorResponse.VersionErrors?.Count() > 0)
            {
                foreach (var error in errorResponse.VersionErrors)
                {
                    logger.Error("Version error: " + Message(error));
                }
            }
            else if (errorResponse.MissingErrors?.Count() > 0)
            {
                foreach (var error in errorResponse.MissingErrors)
                {
                    logger.Error("Missing error: " + Message(error));
                }
            }
            else if (errorResponse.DerivationErrors?.Count() > 0)
            {
                foreach (var error in errorResponse.DerivationErrors)
                {
                    logger.Error("Derivation error: " + error.Message);
                }
            }
            else
            {
                logger.Error($@"{errorResponse.ErrorMessage}");
            }
        }

        private static string Message(IObject @object)
        {
            try
            {
                return @object.ToString();
            }
            catch
            {
                return "error";
            }
        }
    }
}
