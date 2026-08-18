using System.Collections.Generic;
using DotnetGuard.KeyBox.Core.Models;

namespace DotnetGuard.KeyBox.App.Views
{
    public class CategoryGroup
    {
        public string Name { get; set; }
        public List<VaultEntry> Entries { get; set; }
    }
}
