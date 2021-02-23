using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Task_Management.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public virtual ICollection<ProjectUser> ProjectUser { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //mut contextul bazei de date ca tabelele utilizatorilor, cat si tabelele celorlalte modele din aplicatie sa se regaseasca in aceeasi baza de date.
        public ApplicationDbContext()
            : base("DBConnectionString", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Task_Management.Migrations.Configuration>("DBConnectionString"));
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set;}

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public static void SendEmailNotification(string toEmail, string subject,string content)
        {
            /**
             * SMTP server configuration
             * senderEmail - SMTP username
             * sender Password - SMPT password
             * smtpServer - SMTP Server / HOST
             * smtpPort - the port used to connect on the SMTP server
             *  ------------------------------------
             *  Go to https://www.google.com/settings/security/lesssecureapps and enable less secure apps.
             */

            const string senderEmail = "dawtesting123@gmail.com";
            const string senderPassword = "10meremititele";
            const string smtpServer = "smtp.gmail.com";
            const int smtpPort = 587;

            // Create a new SMTP Cliend that is used to send emails
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            // Create a new email object
            // @param senderEmail - the email adress that sends the email
            // @param toEmail - the recipient of the email
            // @param subject - the subject line of the email
            // @param content - the content of the email
            MailMessage email = new MailMessage(senderEmail, toEmail, subject, content);

            // Set true so the email is received as HTML
            email.IsBodyHtml = true;
            // Allow special characters
            email.BodyEncoding = UTF8Encoding.UTF8;

            try
            { // Send the email
                System.Diagnostics.Debug.WriteLine("Sending email...");
                smtpClient.Send(email);
                System.Diagnostics.Debug.WriteLine("Email sent!");
            }
            catch (Exception e)
            {
                //Failed to send the email
                System.Diagnostics.Debug.WriteLine("Error occured while trying to send the email:");
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}