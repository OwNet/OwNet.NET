namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncJournalItemSyncWithType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SyncJournalItems", "SyncWith", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SyncJournalItems", "SyncWith", c => c.Int());
        }
    }
}
