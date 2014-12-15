namespace CentralServiceCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeColumnsToString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SyncJournalItems", "Columns", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SyncJournalItems", "Columns", c => c.Binary());
        }
    }
}
