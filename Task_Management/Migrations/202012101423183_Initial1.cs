namespace Task_Management.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Projects", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "Project_ProjectId", "dbo.Projects");
            DropIndex("dbo.Projects", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "Project_ProjectId" });
            CreateTable(
                "dbo.ProjectUsers",
                c => new
                    {
                        ProjectUserId = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Accepted = c.Boolean(nullable: false),
                        JoinDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProjectUserId)
                .ForeignKey("dbo.Projects", t => t.ProjectId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.ProjectId)
                .Index(t => t.UserId);
            
            DropColumn("dbo.Projects", "ApplicationUser_Id");
            DropColumn("dbo.AspNetUsers", "Project_ProjectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Project_ProjectId", c => c.Int());
            AddColumn("dbo.Projects", "ApplicationUser_Id", c => c.String(maxLength: 128));
            DropForeignKey("dbo.ProjectUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ProjectUsers", "ProjectId", "dbo.Projects");
            DropIndex("dbo.ProjectUsers", new[] { "UserId" });
            DropIndex("dbo.ProjectUsers", new[] { "ProjectId" });
            DropTable("dbo.ProjectUsers");
            CreateIndex("dbo.AspNetUsers", "Project_ProjectId");
            CreateIndex("dbo.Projects", "ApplicationUser_Id");
            AddForeignKey("dbo.AspNetUsers", "Project_ProjectId", "dbo.Projects", "ProjectId");
            AddForeignKey("dbo.Projects", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
