using System;

namespace DotnetGuard.KeyBox.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] MasterHash { get; set; }
        public byte[] Salt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User()
        {
        }

        public User(string username, byte[] masterHash, byte[] salt)
        {
            Username = username;
            MasterHash = masterHash;
            Salt = salt;
        }

        public override string ToString()
        {
            return $"User [Id={Id}, Username={Username}]";
        }
    }
}
