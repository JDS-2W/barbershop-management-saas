using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaBarbearia.Migrations
{
    
    public partial class AdicionarColunaFalta : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFalta",
                table: "Agendamentos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFalta",
                table: "Agendamentos");
        }
    }
}
