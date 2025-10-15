using System;

namespace MdExplorer.AI.Abstractions.Exceptions
{
    public class LicenseRequiredException : Exception
    {
        public LicenseRequiredException()
            : base("This feature requires a valid MdExplorer AI Premium license")
        {
        }

        public LicenseRequiredException(string message) : base(message)
        {
        }

        public LicenseRequiredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
