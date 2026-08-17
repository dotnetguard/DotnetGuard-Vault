using System;

namespace DotnetGuard.KeyBox.Core.Exceptions
{
    [Serializable]
    public class InvalidMasterPasswordException : Exception
    {
        public InvalidMasterPasswordException()
            : base("The master password entered is not correct.")
        {
        }

        public InvalidMasterPasswordException(string message)
            : base(message)
        {
        }

        public InvalidMasterPasswordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
