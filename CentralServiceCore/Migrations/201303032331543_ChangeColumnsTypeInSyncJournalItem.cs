namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeColumnsTypeInSyncJournalItem : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Clients", "Workspace_Id", "dbo.Workspaces");
            DropIndex("dbo.Clients", new[] { "Workspace_Id" });
            CreateTable(
                "dbo.SyncJournalItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        ClientRecordNumber = c.Int(nullable: false),
                        TableName = c.String(),
                        SyncId = c.String(),
                        GroupId = c.Int(),
                        SyncWith = c.Int(),
                        Columns = c.Binary(),
                        DateCreated = c.DateTime(nullable: false),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Workspace_Id)
                .Index(t => t.Workspace_Id);
            
            CreateTable(
                "dbo.SyncJournalStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        GroupId = c.Int(),
                        LastClientRecordNumber = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Workspace_Id)
                .Index(t => t.Workspace_Id);
            
            DropTable("dbo.Clients");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropIndex("dbo.SyncJournalStates", new[] { "Workspace_Id" });
            DropIndex("dbo.SyncJournalItems", new[] { "Workspace_Id" });
            DropForeignKey("dbo.SyncJournalStates", "Workspace_Id", "dbo.Workspaces");
            DropForeignKey("dbo.SyncJournalItems", "Workspace_Id", "dbo.Workspaces");
            DropTable("dbo.SyncJournalStates");
            DropTable("dbo.SyncJournalItems");
            CreateIndex("dbo.Clients", "Workspace_Id");
            AddForeignKey("dbo.Clients", "Workspace_Id", "dbo.Workspaces", "Id");
        }
    }
}
