namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncJournalItemOperationTypeValue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SyncJournalItems", "OperationTypeValue", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SyncJournalItems", "OperationTypeValue");
        }
    }
}
