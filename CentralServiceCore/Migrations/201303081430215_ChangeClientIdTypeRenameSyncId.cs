namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeClientIdTypeRenameSyncId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SyncJournalItems", "Uid", c => c.String());
            AlterColumn("dbo.SyncJournalItems", "ClientId", c => c.String());
            AlterColumn("dbo.SyncJournalStates", "ClientId", c => c.String());
            DropColumn("dbo.SyncJournalItems", "SyncId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SyncJournalItems", "SyncId", c => c.String());
            AlterColumn("dbo.SyncJournalStates", "ClientId", c => c.Int(nullable: false));
            AlterColumn("dbo.SyncJournalItems", "ClientId", c => c.Int(nullable: false));
            DropColumn("dbo.SyncJournalItems", "Uid");
        }
    }
}
