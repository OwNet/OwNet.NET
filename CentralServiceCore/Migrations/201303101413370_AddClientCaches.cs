namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClientCaches : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientCaches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        WebObject_Id = c.Int(),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WebObjects", t => t.WebObject_Id)
                .ForeignKey("dbo.Workspaces", t => t.Workspace_Id)
                .Index(t => t.WebObject_Id)
                .Index(t => t.Workspace_Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ClientCaches", new[] { "Workspace_Id" });
            DropIndex("dbo.ClientCaches", new[] { "WebObject_Id" });
            DropForeignKey("dbo.ClientCaches", "Workspace_Id", "dbo.Workspaces");
            DropForeignKey("dbo.ClientCaches", "WebObject_Id", "dbo.WebObjects");
            DropTable("dbo.ClientCaches");
        }
    }
}
