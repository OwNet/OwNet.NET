namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Workspaces",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false),
                        Name = c.String(),
                        WorkspaceId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Caches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AbsoluteUri = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        UpdateAt = c.DateTime(),
                        DateUpdated = c.DateTime(nullable: false),
                        Hash = c.String(),
                        UriHash = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Size = c.Long(nullable: false),
                        Server_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Server_Id)
                .Index(t => t.Server_Id);
            
            CreateTable(
                "dbo.AccessLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DownloadedFrom = c.Int(nullable: false),
                        FetchDuration = c.Double(nullable: false),
                        AccessedAt = c.DateTime(nullable: false),
                        CacheCreatedAt = c.DateTime(nullable: false),
                        Cache_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Caches", t => t.Cache_Id)
                .Index(t => t.Cache_Id);
            
            CreateTable(
                "dbo.RecommendedUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false),
                        Sent = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Server_Id = c.Int(),
                        Cache_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Server_Id)
                .ForeignKey("dbo.Caches", t => t.Cache_Id)
                .Index(t => t.Server_Id)
                .Index(t => t.Cache_Id);
            
            CreateTable(
                "dbo.PreviousUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        HoursSincePreviousUpdate = c.Double(nullable: false),
                        Success = c.Boolean(nullable: false),
                        Cache_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Caches", t => t.Cache_Id)
                .Index(t => t.Cache_Id);
            
            CreateTable(
                "dbo.ActivityLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AbsoluteUri = c.String(),
                        Title = c.String(),
                        Message = c.String(),
                        DateTime = c.DateTime(nullable: false),
                        Action = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        UserFirstname = c.String(),
                        UserSurname = c.String(),
                        ServerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.ServerId, cascadeDelete: true)
                .Index(t => t.ServerId);
            
            CreateTable(
                "dbo.GroupServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false),
                        Server_Id = c.Int(),
                        Group_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Server_Id)
                .ForeignKey("dbo.Groups", t => t.Group_Id)
                .Index(t => t.Server_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Recommendations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AbsoluteUri = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Server_Id = c.Int(),
                        Group_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Server_Id)
                .ForeignKey("dbo.Groups", t => t.Group_Id)
                .Index(t => t.Server_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Group_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.Group_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.UserGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false),
                        Group_Id = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.Group_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.Group_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        PasswordHash = c.String(),
                        PasswordSalt = c.String(),
                        FirstName = c.String(),
                        Surname = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(nullable: false),
                        Server_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Server_Id)
                .Index(t => t.Server_Id);
            
            CreateTable(
                "dbo.UserCookies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cookie = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Workspace_Id)
                .Index(t => t.Workspace_Id);
            
            CreateTable(
                "dbo.Predictions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PagesCount = c.Int(nullable: false),
                        LinksCount = c.Int(nullable: false),
                        LastPrediction = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Clients", new[] { "Workspace_Id" });
            DropIndex("dbo.UserCookies", new[] { "User_Id" });
            DropIndex("dbo.Users", new[] { "Server_Id" });
            DropIndex("dbo.UserGroups", new[] { "User_Id" });
            DropIndex("dbo.UserGroups", new[] { "Group_Id" });
            DropIndex("dbo.Tags", new[] { "Group_Id" });
            DropIndex("dbo.Recommendations", new[] { "Group_Id" });
            DropIndex("dbo.Recommendations", new[] { "Server_Id" });
            DropIndex("dbo.GroupServers", new[] { "Group_Id" });
            DropIndex("dbo.GroupServers", new[] { "Server_Id" });
            DropIndex("dbo.ActivityLogs", new[] { "ServerId" });
            DropIndex("dbo.PreviousUpdates", new[] { "Cache_Id" });
            DropIndex("dbo.RecommendedUpdates", new[] { "Cache_Id" });
            DropIndex("dbo.RecommendedUpdates", new[] { "Server_Id" });
            DropIndex("dbo.AccessLogs", new[] { "Cache_Id" });
            DropIndex("dbo.Caches", new[] { "Server_Id" });
            DropForeignKey("dbo.Clients", "Workspace_Id", "dbo.Workspaces");
            DropForeignKey("dbo.UserCookies", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "Server_Id", "dbo.Workspaces");
            DropForeignKey("dbo.UserGroups", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.Tags", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.Recommendations", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.Recommendations", "Server_Id", "dbo.Workspaces");
            DropForeignKey("dbo.GroupServers", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.GroupServers", "Server_Id", "dbo.Workspaces");
            DropForeignKey("dbo.ActivityLogs", "ServerId", "dbo.Workspaces");
            DropForeignKey("dbo.PreviousUpdates", "Cache_Id", "dbo.Caches");
            DropForeignKey("dbo.RecommendedUpdates", "Cache_Id", "dbo.Caches");
            DropForeignKey("dbo.RecommendedUpdates", "Server_Id", "dbo.Workspaces");
            DropForeignKey("dbo.AccessLogs", "Cache_Id", "dbo.Caches");
            DropForeignKey("dbo.Caches", "Server_Id", "dbo.Workspaces");
            DropTable("dbo.Predictions");
            DropTable("dbo.Clients");
            DropTable("dbo.UserCookies");
            DropTable("dbo.Users");
            DropTable("dbo.UserGroups");
            DropTable("dbo.Tags");
            DropTable("dbo.Recommendations");
            DropTable("dbo.Groups");
            DropTable("dbo.GroupServers");
            DropTable("dbo.ActivityLogs");
            DropTable("dbo.PreviousUpdates");
            DropTable("dbo.RecommendedUpdates");
            DropTable("dbo.AccessLogs");
            DropTable("dbo.Caches");
            DropTable("dbo.Workspaces");
        }
    }
}
