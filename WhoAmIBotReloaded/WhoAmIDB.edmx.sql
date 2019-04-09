
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/09/2019 16:38:37
-- Generated from EDMX file: C:\Users\flmeyer\source\repos\WhoAmIBotReloaded\WhoAmIBotReloaded\WhoAmIDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [whoami];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_GameGroup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Games] DROP CONSTRAINT [FK_GameGroup];
GO
IF OBJECT_ID(N'[dbo].[FK_ChoseRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Players] DROP CONSTRAINT [FK_ChoseRole];
GO
IF OBJECT_ID(N'[dbo].[FK_GamePlayer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Players] DROP CONSTRAINT [FK_GamePlayer];
GO
IF OBJECT_ID(N'[dbo].[FK_PlayerUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Players] DROP CONSTRAINT [FK_PlayerUser];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Groups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Groups];
GO
IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Games];
GO
IF OBJECT_ID(N'[dbo].[Players]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Players];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int  NOT NULL,
    [FirstName] nvarchar(max)  NOT NULL,
    [LastName] nvarchar(max)  NULL,
    [Username] nvarchar(max)  NULL,
    [QuestionsAsked] int  NOT NULL,
    [QuestionsAnswered] int  NOT NULL,
    [DidNotKnow] int  NOT NULL,
    [LanguageCode] nvarchar(max)  NULL,
    [IsGlobalAdmin] bit  NOT NULL,
    [Language] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Groups'
CREATE TABLE [dbo].[Groups] (
    [Id] bigint  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Username] nvarchar(max)  NULL,
    [Type] nvarchar(max)  NOT NULL,
    [JSONSettings] nvarchar(max)  NOT NULL,
    [Language] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Games'
CREATE TABLE [dbo].[Games] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [State] nvarchar(max)  NOT NULL,
    [TimeStarted] datetimeoffset  NOT NULL,
    [TimeEnded] datetimeoffset  NULL,
    [Turns] int  NOT NULL,
    [Group_Id] bigint  NOT NULL
);
GO

-- Creating table 'Players'
CREATE TABLE [dbo].[Players] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Role] nvarchar(max)  NOT NULL,
    [Placement] int  NOT NULL,
    [QuestionsAsked] int  NOT NULL,
    [QuestionsAnswered] int  NOT NULL,
    [GameId] bigint  NOT NULL,
    [Guessed] int  NOT NULL,
    [GuessedRole] bit  NOT NULL,
    [ChoseRoleFor_Id] bigint  NOT NULL,
    [User_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Groups'
ALTER TABLE [dbo].[Groups]
ADD CONSTRAINT [PK_Groups]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [PK_Games]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Players'
ALTER TABLE [dbo].[Players]
ADD CONSTRAINT [PK_Players]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Group_Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [FK_GameGroup]
    FOREIGN KEY ([Group_Id])
    REFERENCES [dbo].[Groups]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameGroup'
CREATE INDEX [IX_FK_GameGroup]
ON [dbo].[Games]
    ([Group_Id]);
GO

-- Creating foreign key on [ChoseRoleFor_Id] in table 'Players'
ALTER TABLE [dbo].[Players]
ADD CONSTRAINT [FK_ChoseRole]
    FOREIGN KEY ([ChoseRoleFor_Id])
    REFERENCES [dbo].[Players]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ChoseRole'
CREATE INDEX [IX_FK_ChoseRole]
ON [dbo].[Players]
    ([ChoseRoleFor_Id]);
GO

-- Creating foreign key on [GameId] in table 'Players'
ALTER TABLE [dbo].[Players]
ADD CONSTRAINT [FK_GamePlayer]
    FOREIGN KEY ([GameId])
    REFERENCES [dbo].[Games]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GamePlayer'
CREATE INDEX [IX_FK_GamePlayer]
ON [dbo].[Players]
    ([GameId]);
GO

-- Creating foreign key on [User_Id] in table 'Players'
ALTER TABLE [dbo].[Players]
ADD CONSTRAINT [FK_PlayerUser]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PlayerUser'
CREATE INDEX [IX_FK_PlayerUser]
ON [dbo].[Players]
    ([User_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------