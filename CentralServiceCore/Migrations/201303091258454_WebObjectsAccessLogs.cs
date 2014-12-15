namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebObjectsAccessLogs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Caches", "Server_Id", "dbo.Workspaces");
            DropForeignKey("dbo.AccessLogs", "Cache_Id", "dbo.Caches");
            DropForeignKey("dbo.RecommendedUpdates", "Cache_Id", "dbo.Caches");
            DropForeignKey("dbo.PreviousUpdates", "Cache_Id", "dbo.Caches");
            DropIndex("dbo.Caches", new[] { "Server_Id" });
            DropIndex("dbo.AccessLogs", new[] { "Cache_Id" });
            DropIndex("dbo.RecommendedUpdates", new[] { "Cache_Id" });
            DropIndex("dbo.PreviousUpdates", new[] { "Cache_Id" });
            CreateTable(
                "dbo.WebObjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CacheId = c.Long(nullable: false),
                        AbsoluteUri = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateUpdated = c.DateTime(nullable: false),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Workspace_Id)
                .Index(t => t.Workspace_Id)
                .Index(t => t.CacheId);
            
            AddColumn("dbo.AccessLogs", "WebObject_Id", c => c.Int());
            AddColumn("dbo.AccessLogs", "Workspace_Id", c => c.Int());
            AddForeignKey("dbo.AccessLogs", "WebObject_Id", "dbo.WebObjects", "Id");
            AddForeignKey("dbo.AccessLogs", "Workspace_Id", "dbo.Workspaces", "Id");
            AddForeignKey("dbo.RecommendedUpdates", "Cache_Id", "dbo.WebObjects", "Id");
            AddForeignKey("dbo.PreviousUpdates", "Cache_Id", "dbo.WebObjects", "Id");
            CreateIndex("dbo.AccessLogs", "WebObject_Id");
            CreateIndex("dbo.AccessLogs", "Workspace_Id");
            CreateIndex("dbo.RecommendedUpdates", "Cache_Id");
            CreateIndex("dbo.PreviousUpdates", "Cache_Id");
            DropColumn("dbo.AccessLogs", "DownloadedFrom");
            DropColumn("dbo.AccessLogs", "FetchDuration");
            DropColumn("dbo.AccessLogs", "CacheCreatedAt");
            DropColumn("dbo.AccessLogs", "Cache_Id");
            DropTable("dbo.Caches");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AccessLogs", "Cache_Id", c => c.Int());
            AddColumn("dbo.AccessLogs", "CacheCreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.AccessLogs", "FetchDuration", c => c.Double(nullable: false));
            AddColumn("dbo.AccessLogs", "DownloadedFrom", c => c.Int(nullable: false));
            DropIndex("dbo.PreviousUpdates", new[] { "Cache_Id" });
            DropIndex("dbo.RecommendedUpdates", new[] { "Cache_Id" });
            DropIndex("dbo.AccessLogs", new[] { "Workspace_Id" });
            DropIndex("dbo.AccessLogs", new[] { "WebObject_Id" });
            DropIndex("dbo.WebObjects", new[] { "Workspace_Id" });
            DropForeignKey("dbo.PreviousUpdates", "Cache_Id", "dbo.WebObjects");
            DropForeignKey("dbo.RecommendedUpdates", "Cache_Id", "dbo.WebObjects");
            DropForeignKey("dbo.AccessLogs", "Workspace_Id", "dbo.Workspaces");
            DropForeignKey("dbo.AccessLogs", "WebObject_Id", "dbo.WebObjects");
            DropForeignKey("dbo.WebObjects", "Workspace_Id", "dbo.Workspaces");
            DropColumn("dbo.AccessLogs", "Workspace_Id");
            DropColumn("dbo.AccessLogs", "WebObject_Id");
            DropTable("dbo.WebObjects");
            CreateIndex("dbo.PreviousUpdates", "Cache_Id");
            CreateIndex("dbo.RecommendedUpdates", "Cache_Id");
            CreateIndex("dbo.AccessLogs", "Cache_Id");
            CreateIndex("dbo.Caches", "Server_Id");
            AddForeignKey("dbo.PreviousUpdates", "Cache_Id", "dbo.Caches", "Id");
            AddForeignKey("dbo.RecommendedUpdates", "Cache_Id", "dbo.Caches", "Id");
            AddForeignKey("dbo.AccessLogs", "Cache_Id", "dbo.Caches", "Id");
            AddForeignKey("dbo.Caches", "Server_Id", "dbo.Workspaces", "Id");
        }
    }
}
