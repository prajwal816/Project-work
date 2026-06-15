namespace BazaarVoice.Common.Exceptions
{
    /// <summary>
    /// Thrown when a general processing error occurs during file extraction,
    /// XML parsing, or record handling.
    /// </summary>
    [Serializable]
    public class ProcessingException : Exception
    {
        public ProcessingException()
        {
        }

        public ProcessingException(string message)
            : base(message)
        {
        }

        public ProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
