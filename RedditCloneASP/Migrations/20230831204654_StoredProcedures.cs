using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RedditCloneASP.Migrations {

    public partial class StoredProcedures : Migration {

        private const string sqlProceduresUp = @"Models/Queries/CommentUp.sql";
        private const string sqlProceduresDown = @"Models/Queries/CommentDown.sql";

        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), sqlProceduresUp)));
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), sqlProceduresDown)));
        }


    }
}
