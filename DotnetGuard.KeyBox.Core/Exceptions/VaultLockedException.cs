using System;

namespace DotnetGuard.KeyBox.Core.Exceptions
{
    [Serializable]
    public class VaultLockedException : Exception
    {
        public VaultLockedException()
            : base("The vault is locked. Unlock it with the master password before performing this operation.")
        {
        }

        public VaultLockedException(string message)
            : base(message)
        {
        }

        public VaultLockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
