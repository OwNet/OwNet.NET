namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebObjectIsTracked : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WebObjects", "IsTracked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WebObjects", "IsTracked");
        }
    }
}
