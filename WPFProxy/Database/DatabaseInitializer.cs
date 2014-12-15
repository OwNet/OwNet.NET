using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFProxy.Database
{
    class DatabaseInitializer
    {
        internal static List<string> InitialSchema()
        {
            return new List<string>() {
@"CREATE TABLE [Users] (
  [ID] int NOT NULL  IDENTITY (1,1)
, [FirstName] nvarchar(100) NOT NULL
, [Surname] nvarchar(100) NOT NULL
, [Mail] nvarchar(100) NULL
, [Current] int NOT NULL DEFAULT 0
);",
@"CREATE TABLE [PrefetchOrders] (
  [FromTitle] nvarchar(4000) NOT NULL
, [FromAbsoluteUri] nvarchar(4000) NOT NULL
, [ToTitle] nvarchar(4000) NOT NULL
, [ToAbsoluteUri] nvarchar(4000) NOT NULL
, [DateCreated] datetime NOT NULL
, [Enabled] bit NOT NULL
, [Completed] bit NOT NULL
, [DateCompleted] datetime NULL
, [Priority] tinyint NOT NULL
, [Status] tinyint NOT NULL
, [IsScheduled] bit NOT NULL
, [Attempts] tinyint NOT NULL
, [Id] int NOT NULL  IDENTITY (1,1)
);",
@"CREATE TABLE [CacheUpdates] (
  [ID] int NOT NULL
, [DateCreated] datetime NULL
, [Priority] int NULL
, [AbsoluteUri] nvarchar(4000) NOT NULL
);",
@"CREATE TABLE [Caches] (
  [ID] int NOT NULL
, [AbsoluteUri] nvarchar(4000) NOT NULL
, [UserAgent] nvarchar(400) NULL
, [Expires] datetime NULL
, [StatusCode] int NULL
, [DateStored] datetime NULL
, [StatusDescription] nvarchar(100) NULL
, [CharacterSet] nvarchar(100) NULL
, [ContentType] nvarchar(100) NULL
, [AccessCount] int NULL DEFAULT 1
, [AccessValue] float NULL
, [DateModified] datetime NULL
, [Size] bigint NULL
, [Parts] int NULL
);",
@"CREATE TABLE [CacheHeaders] (
  [ID] int NOT NULL  IDENTITY (1,1)
, [Key] nvarchar(4000) NULL
, [Value] nvarchar(4000) NULL
, [CacheId] int NOT NULL
);",
@"CREATE TABLE [Blacklist] (
  [RegularExpression] nvarchar(4000) NULL
, [CacheOnServer] int NULL
, [Id] int NOT NULL  IDENTITY (1,1)
, [Title] nvarchar(4000) NULL
);",
@"ALTER TABLE [Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([ID]);",
@"ALTER TABLE [PrefetchOrders] ADD CONSTRAINT [PK__PrefetchOrders__00000000000000FB] PRIMARY KEY ([Id]);",
@"ALTER TABLE [CacheUpdates] ADD CONSTRAINT [PK_CacheUpdates] PRIMARY KEY ([ID]);",
@"ALTER TABLE [Caches] ADD CONSTRAINT [PK_Caches] PRIMARY KEY ([ID]);",
@"ALTER TABLE [CacheHeaders] ADD CONSTRAINT [PK_CacheHeaders] PRIMARY KEY ([ID]);",
@"ALTER TABLE [Blacklist] ADD CONSTRAINT [PK__Blacklist__000000000000019B] PRIMARY KEY ([Id]);",
@"CREATE UNIQUE INDEX [UQ__Users__0000000000000075] ON [Users] ([ID] ASC);",
@"CREATE UNIQUE INDEX [UQ__PrefetchOrders__00000000000000F6] ON [PrefetchOrders] ([Id] ASC);",
@"CREATE UNIQUE INDEX [UQ__CacheUpdates__000000000000008A] ON [CacheUpdates] ([ID] ASC);",
@"CREATE UNIQUE INDEX [UQ__Caches__000000000000012E] ON [Caches] ([ID] ASC);",
@"CREATE UNIQUE INDEX [UQ__CacheHeaders__000000000000003C] ON [CacheHeaders] ([ID] ASC);",
@"CREATE UNIQUE INDEX [UQ__Blacklist__0000000000000196] ON [Blacklist] ([Id] ASC);",
@"ALTER TABLE [CacheHeaders] ADD CONSTRAINT [FK_CacheHeaders_Caches] FOREIGN KEY ([CacheId]) REFERENCES [Caches]([ID]) ON DELETE CASCADE ON UPDATE NO ACTION;"
            };
        }
    }
}
