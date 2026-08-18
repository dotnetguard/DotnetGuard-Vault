using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DotnetGuard.KeyBox.Core.Exceptions;
using DotnetGuard.KeyBox.Core.Models;
using DotnetGuard.KeyBox.Core.Security;

namespace DotnetGuard.KeyBox.Data
{
    public class VaultSession
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly VaultRepository _vaultRepository = new VaultRepository();

        private User _currentUser;
        private byte[] _encryptionKey;

        public bool IsUnlocked => _encryptionKey != null;

        public bool SetupRequired => !_userRepository.AnyUserExists();

        public void Register(string username, string masterPassword)
        {
            byte[] salt = CryptoService.GenerateSalt();
            byte[] hash = CryptoService.DeriveHash(masterPassword, salt);

            _userRepository.CreateUser(new User(username, hash, salt));

            Unlock(username, masterPassword);
        }

        public void Unlock(string username, string masterPassword)
        {
            User user = _userRepository.GetByUsername(username);

            if (user == null || !CryptoService.VerifyMasterPassword(masterPassword, user.Salt, user.MasterHash))
            {
                throw new InvalidMasterPasswordException();
            }

            _currentUser = user;
            _encryptionKey = CryptoService.DeriveKey(masterPassword, user.Salt);
        }

        public void Lock()
        {
            if (_encryptionKey != null)
            {
                CryptographicOperations.ZeroMemory(_encryptionKey);
            }

            _encryptionKey = null;
            _currentUser = null;
        }

        public List<VaultEntry> GetEntries()
        {
            EnsureUnlocked();
            return _vaultRepository.GetAllForUser(_currentUser.Id);
        }

        public string RevealPassword(VaultEntry entry)
        {
            EnsureUnlocked();
            return CryptoService.Decrypt(entry.EncryptedPassword, entry.Nonce, entry.Tag, _encryptionKey);
        }

        public void AddEntry(string title, string iconKey, string category, string entryUsername, string plainPassword, string url, string notes)
        {
            EnsureUnlocked();

            CryptoService.Encrypt(plainPassword, _encryptionKey, out byte[] cipherText, out byte[] nonce, out byte[] tag);

            VaultEntry entry = new VaultEntry(title, iconKey, category, entryUsername, cipherText, nonce, tag)
            {
                UserId = _currentUser.Id,
                Url = url,
                Notes = notes
            };

            _vaultRepository.Add(entry);
        }

        public void UpdateEntry(int id, string title, string iconKey, string category, string entryUsername, string plainPassword, string url, string notes)
        {
            EnsureUnlocked();

            CryptoService.Encrypt(plainPassword, _encryptionKey, out byte[] cipherText, out byte[] nonce, out byte[] tag);

            VaultEntry entry = new VaultEntry(title, iconKey, category, entryUsername, cipherText, nonce, tag)
            {
                Id = id,
                UserId = _currentUser.Id,
                Url = url,
                Notes = notes
            };

            _vaultRepository.Update(entry);
        }

        public void DeleteEntry(int id)
        {
            EnsureUnlocked();
            _vaultRepository.Delete(id);
        }

        public List<VaultExportEntry> ExportEntries()
        {
            EnsureUnlocked();

            List<VaultExportEntry> exported = new List<VaultExportEntry>();

            foreach (VaultEntry entry in _vaultRepository.GetAllForUser(_currentUser.Id))
            {
                exported.Add(new VaultExportEntry
                {
                    Title = entry.Title,
                    IconKey = entry.IconKey,
                    Category = entry.Category,
                    EntryUsername = entry.EntryUsername,
                    Url = entry.Url,
                    Notes = entry.Notes,
                    EncryptedPasswordBase64 = Convert.ToBase64String(entry.EncryptedPassword),
                    NonceBase64 = Convert.ToBase64String(entry.Nonce),
                    TagBase64 = Convert.ToBase64String(entry.Tag)
                });
            }

            return exported;
        }

        public int ImportEntries(List<VaultExportEntry> entries)
        {
            EnsureUnlocked();

            int importedCount = 0;

            foreach (VaultExportEntry exported in entries)
            {
                VaultEntry entry = new VaultEntry(
                    exported.Title,
                    exported.IconKey,
                    exported.Category,
                    exported.EntryUsername,
                    Convert.FromBase64String(exported.EncryptedPasswordBase64),
                    Convert.FromBase64String(exported.NonceBase64),
                    Convert.FromBase64String(exported.TagBase64))
                {
                    UserId = _currentUser.Id,
                    Url = exported.Url,
                    Notes = exported.Notes
                };

                _vaultRepository.Add(entry);
                importedCount++;
            }

            return importedCount;
        }

        private void EnsureUnlocked()
        {
            if (!IsUnlocked)
            {
                throw new VaultLockedException();
            }
        }
    }
}
