using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class AppDataContext : DbContext{
    public required DbSet<Aluno> Alunos { get; set; }
    public required DbSet<IMC> IMCs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=gabriel.db");
    }

}
