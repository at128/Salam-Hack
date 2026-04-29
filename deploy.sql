IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [UpdatedAtUtc] datetimeoffset NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (''10000000-0000-0000-0000-000000000001'', N''10000000-0000-0000-0000-000000000001'', N''Admin'', N''ADMIN''),
    (''10000000-0000-0000-0000-000000000002'', N''10000000-0000-0000-0000-000000000002'', N''User'', N''USER'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425155110_InitialIdentitySchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425155110_InitialIdentitySchema', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425182653_AddRefreshTokens'
)
BEGIN
    CREATE TABLE [refresh_tokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [Family] nvarchar(64) NOT NULL,
        [IsRevoked] bit NOT NULL,
        [IsUsed] bit NOT NULL,
        [ReplacedByTokenHash] nvarchar(128) NULL,
        [DeviceInfo] nvarchar(200) NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        CONSTRAINT [PK_refresh_tokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_refresh_tokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425182653_AddRefreshTokens'
)
BEGIN
    CREATE INDEX [IX_refresh_tokens_ExpiresAt] ON [refresh_tokens] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425182653_AddRefreshTokens'
)
BEGIN
    CREATE INDEX [IX_refresh_tokens_Family_IsRevoked_IsUsed] ON [refresh_tokens] ([Family], [IsRevoked], [IsUsed]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425182653_AddRefreshTokens'
)
BEGIN
    CREATE UNIQUE INDEX [IX_refresh_tokens_TokenHash] ON [refresh_tokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425182653_AddRefreshTokens'
)
BEGIN
    CREATE INDEX [IX_refresh_tokens_UserId_Family] ON [refresh_tokens] ([UserId], [Family]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425182653_AddRefreshTokens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425182653_AddRefreshTokens', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [customers] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Phone] nvarchar(32) NOT NULL,
        [ClientType] nvarchar(50) NOT NULL,
        [CompanyName] nvarchar(200) NULL,
        [Notes] nvarchar(2000) NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_customers] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_customers_Id_UserId] UNIQUE ([Id], [UserId]),
        CONSTRAINT [FK_customers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [services] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ServiceName] nvarchar(200) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [DefaultHourlyRate] decimal(18,2) NOT NULL,
        [DefaultRevisions] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_services] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_services_Id_UserId] UNIQUE ([Id], [UserId]),
        CONSTRAINT [FK_services_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [projects] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [ServiceId] uniqueidentifier NOT NULL,
        [ProjectName] nvarchar(200) NOT NULL,
        [EstimatedHours] decimal(10,2) NOT NULL,
        [ToolCost] decimal(18,2) NOT NULL,
        [Revision] int NOT NULL,
        [IsUrgent] bit NOT NULL,
        [ProfitMargin] decimal(5,2) NOT NULL,
        [SuggestedPrice] decimal(18,2) NOT NULL,
        [MinPrice] decimal(18,2) NOT NULL,
        [AdvanceAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [ActualHours] decimal(10,2) NOT NULL,
        [StartDate] datetimeoffset NOT NULL,
        [EndDate] datetimeoffset NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_projects] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_projects_Id_CustomerId] UNIQUE ([Id], [CustomerId]),
        CONSTRAINT [AK_projects_Id_UserId] UNIQUE ([Id], [UserId]),
        CONSTRAINT [FK_projects_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_projects_customers_CustomerId_UserId] FOREIGN KEY ([CustomerId], [UserId]) REFERENCES [customers] ([Id], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_projects_services_ServiceId_UserId] FOREIGN KEY ([ServiceId], [UserId]) REFERENCES [services] ([Id], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [analyses] (
        [Id] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [WhatHappened] nvarchar(4000) NOT NULL,
        [WhatItMeans] nvarchar(4000) NOT NULL,
        [WhatToDo] nvarchar(4000) NOT NULL,
        [HealthStatus] nvarchar(50) NOT NULL,
        [GeneratedAt] datetimeoffset NOT NULL,
        [Title] nvarchar(200) NULL,
        [Summary] nvarchar(1000) NULL,
        [ConfidenceScore] decimal(5,4) NULL,
        [MetadataJson] nvarchar(max) NULL,
        [ReviewedAtUtc] datetimeoffset NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_analyses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_analyses_projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [projects] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [expenses] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NULL,
        [Category] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [IsRecurring] bit NOT NULL,
        [ExpenseDate] datetimeoffset NOT NULL,
        [RecurrenceInterval] nvarchar(50) NULL,
        [RecurrenceEndDate] datetimeoffset NULL,
        [Currency] nvarchar(10) NOT NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_expenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_expenses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_expenses_projects_ProjectId_UserId] FOREIGN KEY ([ProjectId], [UserId]) REFERENCES [projects] ([Id], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [invoices] (
        [Id] uniqueidentifier NOT NULL,
        [ProjectId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [InvoiceNumber] nvarchar(50) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalWithTax] decimal(18,2) NOT NULL,
        [AdvanceAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [IssueDate] datetimeoffset NOT NULL,
        [DueDate] datetimeoffset NOT NULL,
        [Notes] nvarchar(2000) NULL,
        [Currency] nvarchar(10) NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_invoices_customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_invoices_projects_ProjectId_CustomerId] FOREIGN KEY ([ProjectId], [CustomerId]) REFERENCES [projects] ([Id], [CustomerId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [InvoiceId] uniqueidentifier NULL,
        [ProjectId] uniqueidentifier NULL,
        [Type] nvarchar(50) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [IsRead] bit NOT NULL,
        [ScheduledAt] datetimeoffset NULL,
        [SentAt] datetimeoffset NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_notifications_invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [invoices] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_notifications_projects_ProjectId_UserId] FOREIGN KEY ([ProjectId], [UserId]) REFERENCES [projects] ([Id], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE TABLE [payments] (
        [Id] uniqueidentifier NOT NULL,
        [InvoiceId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] nvarchar(50) NOT NULL,
        [PaymentDate] datetimeoffset NOT NULL,
        [Notes] nvarchar(2000) NULL,
        [Currency] nvarchar(10) NOT NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(128) NULL,
        [LastModifiedUtc] datetimeoffset NOT NULL,
        [LastModifiedBy] nvarchar(128) NULL,
        CONSTRAINT [PK_payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_payments_invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [invoices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_analyses_ProjectId] ON [analyses] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_analyses_ProjectId_Type_GeneratedAt] ON [analyses] ([ProjectId], [Type], [GeneratedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_customers_DeletedAtUtc] ON [customers] ([DeletedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_customers_UserId] ON [customers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_customers_UserId_Email] ON [customers] ([UserId], [Email]) WHERE [DeletedAtUtc] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_expenses_ProjectId_UserId] ON [expenses] ([ProjectId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_expenses_UserId] ON [expenses] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_expenses_UserId_Category] ON [expenses] ([UserId], [Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_expenses_UserId_ExpenseDate] ON [expenses] ([UserId], [ExpenseDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_expenses_UserId_IsRecurring] ON [expenses] ([UserId], [IsRecurring]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_expenses_UserId_ProjectId] ON [expenses] ([UserId], [ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_invoices_CustomerId] ON [invoices] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_invoices_DeletedAtUtc] ON [invoices] ([DeletedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_invoices_DueDate] ON [invoices] ([DueDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_invoices_InvoiceNumber] ON [invoices] ([InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_invoices_ProjectId_CustomerId] ON [invoices] ([ProjectId], [CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_invoices_Status] ON [invoices] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_notifications_InvoiceId] ON [notifications] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_notifications_ProjectId_UserId] ON [notifications] ([ProjectId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_notifications_UserId] ON [notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_notifications_UserId_IsRead_ScheduledAt] ON [notifications] ([UserId], [IsRead], [ScheduledAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_notifications_UserId_ProjectId] ON [notifications] ([UserId], [ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_notifications_UserId_Type] ON [notifications] ([UserId], [Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_payments_InvoiceId] ON [payments] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_payments_Method] ON [payments] ([Method]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_payments_PaymentDate] ON [payments] ([PaymentDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_CustomerId_UserId] ON [projects] ([CustomerId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_DeletedAtUtc] ON [projects] ([DeletedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_ServiceId_UserId] ON [projects] ([ServiceId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_UserId] ON [projects] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_UserId_CustomerId] ON [projects] ([UserId], [CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_UserId_ProjectName] ON [projects] ([UserId], [ProjectName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_UserId_ServiceId] ON [projects] ([UserId], [ServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_projects_UserId_Status] ON [projects] ([UserId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_services_DeletedAtUtc] ON [services] ([DeletedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_services_UserId] ON [services] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    CREATE INDEX [IX_services_UserId_Category] ON [services] ([UserId], [Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_services_UserId_ServiceName] ON [services] ([UserId], [ServiceName]) WHERE [DeletedAtUtc] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427151744_AddDomainEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260427151744_AddDomainEntities', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    ALTER TABLE [invoices] DROP CONSTRAINT [FK_invoices_customers_CustomerId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    DROP INDEX [IX_invoices_InvoiceNumber] ON [invoices];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    ALTER TABLE [invoices] ADD [UserId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    UPDATE i
    SET UserId = p.UserId
    FROM invoices AS i
    INNER JOIN projects AS p
        ON p.Id = i.ProjectId
        AND p.CustomerId = i.CustomerId
    WHERE i.UserId IS NULL
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[invoices]') AND [c].[name] = N'UserId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [invoices] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [invoices] ALTER COLUMN [UserId] uniqueidentifier NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    CREATE INDEX [IX_invoices_CustomerId_UserId] ON [invoices] ([CustomerId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    CREATE INDEX [IX_invoices_UserId] ON [invoices] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_invoices_UserId_InvoiceNumber] ON [invoices] ([UserId], [InvoiceNumber]) WHERE [DeletedAtUtc] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    ALTER TABLE [invoices] ADD CONSTRAINT [FK_invoices_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    ALTER TABLE [invoices] ADD CONSTRAINT [FK_invoices_customers_CustomerId_UserId] FOREIGN KEY ([CustomerId], [UserId]) REFERENCES [customers] ([Id], [UserId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428190000_AddInvoiceUserScopedNumbers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260428190000_AddInvoiceUserScopedNumbers', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429120000_AddExpenseSoftDelete'
)
BEGIN
    ALTER TABLE [expenses] ADD [DeletedAtUtc] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429120000_AddExpenseSoftDelete'
)
BEGIN
    CREATE INDEX [IX_expenses_DeletedAtUtc] ON [expenses] ([DeletedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429120000_AddExpenseSoftDelete'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429120000_AddExpenseSoftDelete', N'9.0.4');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429160226_ExpandProjectProfitMarginPrecision'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[projects]') AND [c].[name] = N'ProfitMargin');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [projects] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [projects] ALTER COLUMN [ProfitMargin] decimal(18,2) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429160226_ExpandProjectProfitMarginPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429160226_ExpandProjectProfitMarginPrecision', N'9.0.4');
END;

COMMIT;
GO

