# DotnetGuard KeyBox

A self-hosted, offline-first password manager built with C# / .NET 6, WPF and MySQL — no third-party password-manager library, no cloud sync. Everything runs on your own machine against your own local MySQL server.

Built by [dotnetguard.blog](https://dotnetguard.blog).

## Screenshots

| Unlock | Vault overview | Add entry |
|---|---|---|
<img width="366" height="277" alt="image" src="https://github.com/user-attachments/assets/a586224e-c985-4701-a236-f37776b79f49" />
<img width="1026" height="598" alt="image" src="https://github.com/user-attachments/assets/b8f4f74c-0c3a-4436-acd2-fc4a2b8fc75f" />
<img width="367" height="513" alt="image" src="https://github.com/user-attachments/assets/bea1426b-ea07-4027-b752-4e82bbc1f45b" />

## Features

- Master-password-protected vault (PBKDF2, 210k iterations)
- Every stored password encrypted with AES-256-GCM, unique nonce per entry
- Category tree with collapsible groups
- Quick actions: `Ctrl+C` copies the selected entry's password, `Ctrl+B` copies its username — both auto-clear the clipboard after 15 seconds
- Clickable URLs, per-entry notes, per-entry icon
- Auto-lock after 5 minutes of inactivity
- Export / import vault backups (`.guard` files)
- Dark, terminal-styled UI

## Requirements

- Windows 10/11
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) running locally (or reachable at whatever host you configure)
- To build from source: [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

## Running the pre-built app

1. Download `DotnetGuard.KeyBox.App.exe` from the [Releases](../../releases) page.
2. Make sure MySQL Server is installed and running.
3. Run the exe. On first launch it shows a **Database setup** screen — enter your MySQL server/username/password. The app creates the database and tables automatically; nothing to run by hand.
4. Set your master password on the next screen. This is the only thing that unlocks your vault — there is no recovery option if you forget it, by design.

## Building from source

```bash
git clone https://github.com/dotnetguard/DotnetGuard-Vault.git
cd DotnetGuard-Vault
dotnet build
dotnet run --project DotnetGuard.KeyBox.App
```

## Publishing a standalone exe

A publish profile is already set up (`DotnetGuard.KeyBox.App/Properties/PublishProfiles/FolderProfile.pubxml`) that produces a self-contained, single-file `win-x64` executable — the person running it does **not** need .NET installed separately.

From Visual Studio: right-click `DotnetGuard.KeyBox.App` → **Publish** → pick the `FolderProfile`.

From the CLI:

```bash
dotnet publish DotnetGuard.KeyBox.App -p:PublishProfile=FolderProfile
```

The output lands in `DotnetGuard.KeyBox.App/bin/Release/net6.0-windows/publish/win-x64/`.

## Project structure

```
DotnetGuard.KeyBox.Core   Models, custom exceptions, CryptoService (PBKDF2 + AES-GCM)
DotnetGuard.KeyBox.Data   ADO.NET repositories, VaultSession, DatabaseInitializer, AppSettings
DotnetGuard.KeyBox.App    WPF UI (Views), dark theme, app icon
sql/schema.sql            Reference schema (the app creates this automatically — you don't need to run it by hand)
```

## Security design

- The master password is never stored. Only a PBKDF2 hash + salt are kept, used to verify future unlock attempts.
- The AES-256 encryption key is derived from the master password on unlock and kept in memory only; it's zeroed out on lock.
- Each entry has its own random nonce; AES-GCM's authentication tag detects tampering with stored ciphertext.
- Connection settings (MySQL host/user/password) live in `%AppData%\DotnetGuard.KeyBox\settings.json` on the machine running the app — never bundled into the repo or the published exe.

## Export / import

Export writes a `.guard` file containing entries with passwords **still encrypted** (never plaintext on disk). This means a `.guard` file can only be re-imported into the *same* vault (same master password + salt) — it's a backup/restore feature, not a way to move passwords to a different, unrelated vault installation.

## License

No license file yet — all rights reserved by default until one is added.
