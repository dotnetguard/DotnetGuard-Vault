using System;

namespace DotnetGuard.KeyBox.Core.Models
{
    public class VaultEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string IconKey { get; set; }
        public string Category { get; set; }
        public string EntryUsername { get; set; }
        public byte[] EncryptedPassword { get; set; }
        public byte[] Nonce { get; set; }
        public byte[] Tag { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public VaultEntry()
        {
        }

        public VaultEntry(string title, string iconKey, string category, string entryUsername, byte[] encryptedPassword, byte[] nonce, byte[] tag)
        {
            Title = title;
            IconKey = iconKey;
            Category = category;
            EntryUsername = entryUsername;
            EncryptedPassword = encryptedPassword;
            Nonce = nonce;
            Tag = tag;
        }

        public override string ToString()
        {
            return $"VaultEntry [Id={Id}, Title={Title}, EntryUsername={EntryUsername}]";
        }
    }
}
