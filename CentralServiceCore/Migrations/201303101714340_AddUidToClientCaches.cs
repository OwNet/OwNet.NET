namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUidToClientCaches : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientCaches", "Uid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClientCaches", "Uid");
        }
    }
}
