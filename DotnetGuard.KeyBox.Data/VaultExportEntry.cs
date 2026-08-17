namespace DotnetGuard.KeyBox.Data
{
    public class VaultExportEntry
    {
        public string Title { get; set; }
        public string IconKey { get; set; }
        public string Category { get; set; }
        public string EntryUsername { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public string EncryptedPasswordBase64 { get; set; }
        public string NonceBase64 { get; set; }
        public string TagBase64 { get; set; }
    }
}
