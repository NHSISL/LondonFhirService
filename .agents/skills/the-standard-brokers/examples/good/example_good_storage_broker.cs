// ---
// skill: the-standard-brokers
// type: example
// source-section: "1. Brokers"
// demonstrates: "ts-brokers-001, ts-brokers-002, ts-brokers-003, ts-brokers-008, ts-brokers-009"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ✅ Interface named generically — no technology in the name
public interface IStorageBroker
{
    ValueTask<Student> InsertStudentAsync(Student student);
    IQueryable<Student> SelectAllStudents();
    ValueTask<Student> SelectStudentByIdAsync(Guid studentId);
    ValueTask<Student> UpdateStudentAsync(Student student);
    ValueTask<Student> DeleteStudentAsync(Student student);
}

// ✅ Root partial class — configuration injected via IConfiguration
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

// ✅ Per-entity partial class — storage language, no business logic
public partial class StorageBroker
{
    public DbSet<Student> Students { get; set; }

    public async ValueTask<Student> InsertStudentAsync(Student student) =>
        await InsertAsync(student);

    public IQueryable<Student> SelectAllStudents() =>
        SelectAll<Student>();

    public async ValueTask<Student> SelectStudentByIdAsync(Guid studentId) =>
        await SelectAsync<Student>(studentId);

    public async ValueTask<Student> UpdateStudentAsync(Student student) =>
        await UpdateAsync(student);

    public async ValueTask<Student> DeleteStudentAsync(Student student) =>
        await DeleteAsync(student);
}
