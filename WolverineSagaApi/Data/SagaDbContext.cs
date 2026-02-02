using Microsoft.EntityFrameworkCore;
using WolverineSagaApi.Sagas;

namespace WolverineSagaApi.Data;

public class SagaDbContext : DbContext
{
  public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
  {
  }

  public DbSet<ThreeServiceSaga> ThreeServiceSagas { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configure the saga state entity
    modelBuilder.Entity<ThreeServiceSaga>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.RequestId).IsRequired();
      entity.Property(e => e.InitialData).IsRequired();
      entity.Property(e => e.CurrentStep).IsRequired();
    });
  }
}
