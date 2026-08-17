CREATE DATABASE IF NOT EXISTS dotnetguard_keybox;
USE dotnetguard_keybox;

CREATE TABLE IF NOT EXISTS Users (
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    Username        VARCHAR(50) NOT NULL UNIQUE,
    MasterHash      VARBINARY(64) NOT NULL,
    Salt            VARBINARY(32) NOT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS VaultEntries (
    Id                  INT AUTO_INCREMENT PRIMARY KEY,
    UserId              INT NOT NULL,
    Title               VARCHAR(100) NOT NULL,
    IconKey             VARCHAR(10) NOT NULL DEFAULT 'OTHER',
    Category            VARCHAR(50) NOT NULL DEFAULT 'GENERAL',
    EntryUsername       VARCHAR(100),
    EncryptedPassword   VARBINARY(512) NOT NULL,
    Nonce               VARBINARY(16) NOT NULL,
    Tag                 VARBINARY(16) NOT NULL,
    Url                 VARCHAR(255),
    Notes               VARCHAR(500),
    CreatedAt           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt           DATETIME NULL,
    CONSTRAINT FK_VaultEntries_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
