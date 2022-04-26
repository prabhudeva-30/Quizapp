using Microsoft.EntityFrameworkCore.Migrations;
using Quizapp.Common;

#nullable disable

namespace Quizapp.Migrations
{
    public partial class seed_data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string PasswordSalt;
            string HashedPassword;

            PasswordManager.CreatePassword("admin", out PasswordSalt, out HashedPassword);

            string PasswordSaltString = PasswordSalt;
            string HashedPasswordString = HashedPassword;

            migrationBuilder.InsertData(
               table: "Users",
               columns: new string[] { "Name", "Password", "PasswordSalt", "Role","Email",  "CreatedAt", "CreatedBy" },
               values: new object[,]
               {
                   {"Prabhu Deva", HashedPasswordString, PasswordSaltString, "Admin", "hr@relevantz.com",  DateTime.Now, 0 }
               });
            string PasswordSalt1;
            string HashedPassword1;

            PasswordManager.CreatePassword("testuser", out PasswordSalt1, out HashedPassword1);

            migrationBuilder.InsertData(
               table: "Users",
               columns: new string[] { "Name", "Password", "PasswordSalt", "Role",  "Email", "CreatedAt", "CreatedBy" },
               values: new object[,]
               {
                   {"test user", HashedPassword1, PasswordSalt1, "Candidate",  "testuser@ofs.com",  DateTime.Now, 1 }
               });


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                  table: "Users",
                  keyColumn: "Email",
                  keyValue: "hr@relevantz.com"
                  );

            migrationBuilder.DeleteData(
                       table: "Users",
                       keyColumn: "Email",
                       keyValue: "testuser@ofs.com"
                       );
        }
    }
}
