using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineClassManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline: không thay đổi schema, chỉ đánh dấu đã áp dụng
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline: không có rollback schema
        }
    }
}
