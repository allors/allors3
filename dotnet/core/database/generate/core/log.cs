namespace Allors.Meta.Generation
{
    public abstract class Log
    {
        public bool ErrorOccured { get; protected set; }

        /// <summary>
        /// Log error messages.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        public abstract void Error(object sender, string message);
    }
}
