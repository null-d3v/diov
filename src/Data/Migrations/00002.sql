CREATE TABLE [AdminAuthorization] (
    [AccountId] NVARCHAR(50) NOT NULL,
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [IdentityProvider] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Authorization] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_Authorization] UNIQUE NONCLUSTERED ([AccountId] ASC, [IdentityProvider] ASC)
);