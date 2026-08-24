// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// StorageBroker.cs — root partial class
public partial class StorageBroker : EFxceptionsContext, IStorageBroker
{
    private readonly IConfiguration configuration;

    public StorageBroker(IConfiguration configuration)
    {
        this.configuration = configuration;
        this.Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        string connectionString =
            this.configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);
    }
}

// StorageBroker.{Entity}s.cs — per-entity partial class
public partial class StorageBroker
{
    public DbSet<{Entity}> {Entity}s { get; set; }

    public async ValueTask<{Entity}> Insert{Entity}Async({Entity} {entity}) =>
        await InsertAsync({entity});

    public IQueryable<{Entity}> SelectAll{Entity}s() =>
        SelectAll<{Entity}>();

    public async ValueTask<{Entity}> Select{Entity}ByIdAsync(Guid {entity}Id) =>
        await SelectAsync<{Entity}>({entity}Id);

    public async ValueTask<{Entity}> Update{Entity}Async({Entity} {entity}) =>
        await UpdateAsync({entity});

    public async ValueTask<{Entity}> Delete{Entity}Async({Entity} {entity}) =>
        await DeleteAsync({entity});
}
