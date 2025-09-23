using Microsoft.EntityFrameworkCore;

namespace Playground;

internal sealed class TestContext : DbContext
{
    public DbSet<Entity>           Entities            { get; set; } = null!;
    public DbSet<ProjectionEntity> ProjectionEntities  { get; set; } = null!;
}
