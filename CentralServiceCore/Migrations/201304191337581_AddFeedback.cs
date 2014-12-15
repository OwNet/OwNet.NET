namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFeedback : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Output = c.String(),
                        ClientId = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Workspace_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workspaces", t => t.Workspace_Id)
                .Index(t => t.Workspace_Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Feedbacks", new[] { "Workspace_Id" });
            DropForeignKey("dbo.Feedbacks", "Workspace_Id", "dbo.Workspaces");
            DropTable("dbo.Feedbacks");
        }
    }
}
