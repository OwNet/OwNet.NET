
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 06/24/2012 00:42:09
-- Generated from EDMX file: C:\Users\Matus.MACBOOKPRO\Documents\Visual Studio 2010\Projects\wpfproxy\CentralServerShared\DataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [CentralServerDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CacheAccessLog]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AccessLogs] DROP CONSTRAINT [FK_CacheAccessLog];
GO
IF OBJECT_ID(N'[dbo].[FK_CacheRecommendedUpdate]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RecommendedUpdates] DROP CONSTRAINT [FK_CacheRecommendedUpdate];
GO
IF OBJECT_ID(N'[dbo].[FK_ServerCache]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Caches] DROP CONSTRAINT [FK_ServerCache];
GO
IF OBJECT_ID(N'[dbo].[FK_CachePreviousUpdate]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PreviousUpdates] DROP CONSTRAINT [FK_CachePreviousUpdate];
GO
IF OBJECT_ID(N'[dbo].[FK_ServerActivityLog]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActivityLogs] DROP CONSTRAINT [FK_ServerActivityLog];
GO
IF OBJECT_ID(N'[dbo].[FK_ServerRecommendedUpdate]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RecommendedUpdates] DROP CONSTRAINT [FK_ServerRecommendedUpdate];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupRecommendation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recommendations] DROP CONSTRAINT [FK_GroupRecommendation];
GO
IF OBJECT_ID(N'[dbo].[FK_ServerGroupServer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GroupServers] DROP CONSTRAINT [FK_ServerGroupServer];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupGroupServer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GroupServers] DROP CONSTRAINT [FK_GroupGroupServer];
GO
IF OBJECT_ID(N'[dbo].[FK_ServerRecommendation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Recommendations] DROP CONSTRAINT [FK_ServerRecommendation];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupTag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Tags] DROP CONSTRAINT [FK_GroupTag];
GO
IF OBJECT_ID(N'[dbo].[FK_PredictionServer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Predictions] DROP CONSTRAINT [FK_PredictionServer];
GO
IF OBJECT_ID(N'[dbo].[FK_ServerUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_ServerUser];
GO
IF OBJECT_ID(N'[dbo].[FK_UserUserCookies]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserCookies1] DROP CONSTRAINT [FK_UserUserCookies];
GO
IF OBJECT_ID(N'[dbo].[FK_UserUserGroups]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserGroups1] DROP CONSTRAINT [FK_UserUserGroups];
GO
IF OBJECT_ID(N'[dbo].[FK_GroupUserGroups]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserGroups1] DROP CONSTRAINT [FK_GroupUserGroups];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Servers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Servers];
GO
IF OBJECT_ID(N'[dbo].[AccessLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AccessLogs];
GO
IF OBJECT_ID(N'[dbo].[Caches]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Caches];
GO
IF OBJECT_ID(N'[dbo].[RecommendedUpdates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RecommendedUpdates];
GO
IF OBJECT_ID(N'[dbo].[PreviousUpdates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PreviousUpdates];
GO
IF OBJECT_ID(N'[dbo].[ActivityLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ActivityLogs];
GO
IF OBJECT_ID(N'[dbo].[Groups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Groups];
GO
IF OBJECT_ID(N'[dbo].[Tags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Tags];
GO
IF OBJECT_ID(N'[dbo].[Recommendations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Recommendations];
GO
IF OBJECT_ID(N'[dbo].[GroupServers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GroupServers];
GO
IF OBJECT_ID(N'[dbo].[Predictions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Predictions];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[UserCookies1]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserCookies1];
GO
IF OBJECT_ID(N'[dbo].[UserGroups1]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserGroups1];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Servers'
CREATE TABLE [dbo].[Servers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Password] nvarchar(max)  NOT NULL,
    [Username] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [ServerName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'AccessLogs'
CREATE TABLE [dbo].[AccessLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DownloadedFrom] int  NOT NULL,
    [FetchDuration] float  NOT NULL,
    [AccessedAt] datetime  NOT NULL,
    [CacheCreatedAt] datetime  NOT NULL,
    [Cache_Id] int  NOT NULL
);
GO

-- Creating table 'Caches'
CREATE TABLE [dbo].[Caches] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AbsoluteUri] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [UpdateAt] datetime  NULL,
    [DateUpdated] datetime  NOT NULL,
    [Hash] nvarchar(max)  NOT NULL,
    [UriHash] int  NOT NULL,
    [Type] int  NOT NULL,
    [Size] bigint  NOT NULL,
    [Server_Id] int  NOT NULL
);
GO

-- Creating table 'RecommendedUpdates'
CREATE TABLE [dbo].[RecommendedUpdates] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [Sent] bit  NOT NULL,
    [Priority] int  NOT NULL,
    [Cache_Id] int  NOT NULL,
    [Server_Id] int  NOT NULL
);
GO

-- Creating table 'PreviousUpdates'
CREATE TABLE [dbo].[PreviousUpdates] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Date] datetime  NOT NULL,
    [HoursSincePreviousUpdate] float  NOT NULL,
    [Success] bit  NOT NULL,
    [Cache_Id] int  NOT NULL
);
GO

-- Creating table 'ActivityLogs'
CREATE TABLE [dbo].[ActivityLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AbsoluteUri] nvarchar(max)  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Message] nvarchar(max)  NOT NULL,
    [DateTime] datetime  NOT NULL,
    [Action] int  NOT NULL,
    [Type] int  NOT NULL,
    [UserFirstname] nvarchar(max)  NOT NULL,
    [UserSurname] nvarchar(max)  NOT NULL,
    [ServerId] int  NOT NULL
);
GO

-- Creating table 'Groups'
CREATE TABLE [dbo].[Groups] (
    [Id] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [DateModified] datetime  NOT NULL
);
GO

-- Creating table 'Tags'
CREATE TABLE [dbo].[Tags] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [Group_Id] int  NOT NULL
);
GO

-- Creating table 'Recommendations'
CREATE TABLE [dbo].[Recommendations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AbsoluteUri] nvarchar(max)  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [Group_Id] int  NOT NULL,
    [Server_Id] int  NOT NULL
);
GO

-- Creating table 'GroupServers'
CREATE TABLE [dbo].[GroupServers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [Server_Id] int  NOT NULL,
    [Group_Id] int  NOT NULL
);
GO

-- Creating table 'Predictions'
CREATE TABLE [dbo].[Predictions] (
    [PagesCount] int  NOT NULL,
    [LinksCount] int  NOT NULL,
    [LastPrediction] datetime  NOT NULL,
    [Id] int IDENTITY(1,1) NOT NULL,
    [Server_Id] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(max)  NOT NULL,
    [PasswordHash] nvarchar(max)  NOT NULL,
    [PasswordSalt] nvarchar(max)  NOT NULL,
    [FirstName] nvarchar(max)  NOT NULL,
    [Surname] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [DateModified] datetime  NOT NULL,
    [Server_Id] int  NOT NULL
);
GO

-- Creating table 'UserCookies'
CREATE TABLE [dbo].[UserCookies] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Cookie] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [User_Id] int  NOT NULL
);
GO

-- Creating table 'UserGroups'
CREATE TABLE [dbo].[UserGroups] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [User_Id] int  NOT NULL,
    [Group_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Servers'
ALTER TABLE [dbo].[Servers]
ADD CONSTRAINT [PK_Servers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'AccessLogs'
ALTER TABLE [dbo].[AccessLogs]
ADD CONSTRAINT [PK_AccessLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Caches'
ALTER TABLE [dbo].[Caches]
ADD CONSTRAINT [PK_Caches]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'RecommendedUpdates'
ALTER TABLE [dbo].[RecommendedUpdates]
ADD CONSTRAINT [PK_RecommendedUpdates]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'PreviousUpdates'
ALTER TABLE [dbo].[PreviousUpdates]
ADD CONSTRAINT [PK_PreviousUpdates]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ActivityLogs'
ALTER TABLE [dbo].[ActivityLogs]
ADD CONSTRAINT [PK_ActivityLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Groups'
ALTER TABLE [dbo].[Groups]
ADD CONSTRAINT [PK_Groups]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [PK_Tags]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Recommendations'
ALTER TABLE [dbo].[Recommendations]
ADD CONSTRAINT [PK_Recommendations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GroupServers'
ALTER TABLE [dbo].[GroupServers]
ADD CONSTRAINT [PK_GroupServers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Predictions'
ALTER TABLE [dbo].[Predictions]
ADD CONSTRAINT [PK_Predictions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserCookies'
ALTER TABLE [dbo].[UserCookies]
ADD CONSTRAINT [PK_UserCookies]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserGroups'
ALTER TABLE [dbo].[UserGroups]
ADD CONSTRAINT [PK_UserGroups]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Cache_Id] in table 'AccessLogs'
ALTER TABLE [dbo].[AccessLogs]
ADD CONSTRAINT [FK_CacheAccessLog]
    FOREIGN KEY ([Cache_Id])
    REFERENCES [dbo].[Caches]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CacheAccessLog'
CREATE INDEX [IX_FK_CacheAccessLog]
ON [dbo].[AccessLogs]
    ([Cache_Id]);
GO

-- Creating foreign key on [Cache_Id] in table 'RecommendedUpdates'
ALTER TABLE [dbo].[RecommendedUpdates]
ADD CONSTRAINT [FK_CacheRecommendedUpdate]
    FOREIGN KEY ([Cache_Id])
    REFERENCES [dbo].[Caches]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CacheRecommendedUpdate'
CREATE INDEX [IX_FK_CacheRecommendedUpdate]
ON [dbo].[RecommendedUpdates]
    ([Cache_Id]);
GO

-- Creating foreign key on [Server_Id] in table 'Caches'
ALTER TABLE [dbo].[Caches]
ADD CONSTRAINT [FK_ServerCache]
    FOREIGN KEY ([Server_Id])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServerCache'
CREATE INDEX [IX_FK_ServerCache]
ON [dbo].[Caches]
    ([Server_Id]);
GO

-- Creating foreign key on [Cache_Id] in table 'PreviousUpdates'
ALTER TABLE [dbo].[PreviousUpdates]
ADD CONSTRAINT [FK_CachePreviousUpdate]
    FOREIGN KEY ([Cache_Id])
    REFERENCES [dbo].[Caches]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CachePreviousUpdate'
CREATE INDEX [IX_FK_CachePreviousUpdate]
ON [dbo].[PreviousUpdates]
    ([Cache_Id]);
GO

-- Creating foreign key on [ServerId] in table 'ActivityLogs'
ALTER TABLE [dbo].[ActivityLogs]
ADD CONSTRAINT [FK_ServerActivityLog]
    FOREIGN KEY ([ServerId])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServerActivityLog'
CREATE INDEX [IX_FK_ServerActivityLog]
ON [dbo].[ActivityLogs]
    ([ServerId]);
GO

-- Creating foreign key on [Server_Id] in table 'RecommendedUpdates'
ALTER TABLE [dbo].[RecommendedUpdates]
ADD CONSTRAINT [FK_ServerRecommendedUpdate]
    FOREIGN KEY ([Server_Id])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServerRecommendedUpdate'
CREATE INDEX [IX_FK_ServerRecommendedUpdate]
ON [dbo].[RecommendedUpdates]
    ([Server_Id]);
GO

-- Creating foreign key on [Group_Id] in table 'Recommendations'
ALTER TABLE [dbo].[Recommendations]
ADD CONSTRAINT [FK_GroupRecommendation]
    FOREIGN KEY ([Group_Id])
    REFERENCES [dbo].[Groups]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupRecommendation'
CREATE INDEX [IX_FK_GroupRecommendation]
ON [dbo].[Recommendations]
    ([Group_Id]);
GO

-- Creating foreign key on [Server_Id] in table 'GroupServers'
ALTER TABLE [dbo].[GroupServers]
ADD CONSTRAINT [FK_ServerGroupServer]
    FOREIGN KEY ([Server_Id])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServerGroupServer'
CREATE INDEX [IX_FK_ServerGroupServer]
ON [dbo].[GroupServers]
    ([Server_Id]);
GO

-- Creating foreign key on [Group_Id] in table 'GroupServers'
ALTER TABLE [dbo].[GroupServers]
ADD CONSTRAINT [FK_GroupGroupServer]
    FOREIGN KEY ([Group_Id])
    REFERENCES [dbo].[Groups]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupGroupServer'
CREATE INDEX [IX_FK_GroupGroupServer]
ON [dbo].[GroupServers]
    ([Group_Id]);
GO

-- Creating foreign key on [Server_Id] in table 'Recommendations'
ALTER TABLE [dbo].[Recommendations]
ADD CONSTRAINT [FK_ServerRecommendation]
    FOREIGN KEY ([Server_Id])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServerRecommendation'
CREATE INDEX [IX_FK_ServerRecommendation]
ON [dbo].[Recommendations]
    ([Server_Id]);
GO

-- Creating foreign key on [Group_Id] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [FK_GroupTag]
    FOREIGN KEY ([Group_Id])
    REFERENCES [dbo].[Groups]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupTag'
CREATE INDEX [IX_FK_GroupTag]
ON [dbo].[Tags]
    ([Group_Id]);
GO

-- Creating foreign key on [Server_Id] in table 'Predictions'
ALTER TABLE [dbo].[Predictions]
ADD CONSTRAINT [FK_PredictionServer]
    FOREIGN KEY ([Server_Id])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PredictionServer'
CREATE INDEX [IX_FK_PredictionServer]
ON [dbo].[Predictions]
    ([Server_Id]);
GO

-- Creating foreign key on [Server_Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_ServerUser]
    FOREIGN KEY ([Server_Id])
    REFERENCES [dbo].[Servers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServerUser'
CREATE INDEX [IX_FK_ServerUser]
ON [dbo].[Users]
    ([Server_Id]);
GO

-- Creating foreign key on [User_Id] in table 'UserCookies'
ALTER TABLE [dbo].[UserCookies]
ADD CONSTRAINT [FK_UserUserCookies]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserUserCookies'
CREATE INDEX [IX_FK_UserUserCookies]
ON [dbo].[UserCookies]
    ([User_Id]);
GO

-- Creating foreign key on [User_Id] in table 'UserGroups'
ALTER TABLE [dbo].[UserGroups]
ADD CONSTRAINT [FK_UserUserGroups]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserUserGroups'
CREATE INDEX [IX_FK_UserUserGroups]
ON [dbo].[UserGroups]
    ([User_Id]);
GO

-- Creating foreign key on [Group_Id] in table 'UserGroups'
ALTER TABLE [dbo].[UserGroups]
ADD CONSTRAINT [FK_GroupUserGroups]
    FOREIGN KEY ([Group_Id])
    REFERENCES [dbo].[Groups]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupUserGroups'
CREATE INDEX [IX_FK_GroupUserGroups]
ON [dbo].[UserGroups]
    ([Group_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------