using ChatWebServer.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChatWebServer.DBContext
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            InitializeDatabase();
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.FK_userID)
                .OnDelete(DeleteBehavior.Cascade);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                string connectionString = GetConnectionString();

                optionsBuilder.UseNpgsql(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occurred while trying to configure optionsBuilder.", ex);
                Console.WriteLine("ConnectionString: " + GetConnectionString());
            }
        }

        public string GetConnectionString()
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                Database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE"),
                Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
                Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
            };

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_PORT")) && int.TryParse(Environment.GetEnvironmentVariable("POSTGRES_PORT"), out int port))
                builder.Port = port;
            

            return builder.ToString();
        }

        public void InitializeDatabase()
        {
            string sqlScript = @"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""userID"" SERIAL PRIMARY KEY,
                ""username"" VARCHAR(55) NOT NULL,
                ""password"" VARCHAR(999) NOT NULL,
                ""role"" VARCHAR(55) NOT NULL, 
                ""isActive"" BOOLEAN NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ""Messages"" (
                ""messageID"" SERIAL PRIMARY KEY,
                ""value"" VARCHAR(999) NOT NULL,
                ""FK_userID"" INT NOT NULL,
                ""timeStamp"" TIMESTAMP NOT NULL
            );

            -- Check if the 'fk_user' constraint already exists
            SELECT 1 FROM pg_constraint WHERE conname = 'fk_user' LIMIT 1;
        ";

            using (var connection = new NpgsqlConnection(GetConnectionString()))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(sqlScript, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Check if the 'fk_user' constraint already exists
                bool constraintExists;
                using (var checkConstraintCommand = new NpgsqlCommand("SELECT 1 FROM pg_constraint WHERE conname = 'fk_user' LIMIT 1;", connection))
                {
                    constraintExists = checkConstraintCommand.ExecuteScalar() != null;
                }

                if (!constraintExists)
                {
                    // Add the 'fk_user' constraint if it doesn't exist
                    sqlScript = @"
                    ALTER TABLE ""Messages""
                    ADD CONSTRAINT ""fk_user""
                    FOREIGN KEY (""FK_userID"") REFERENCES ""Users""(""userID"");
                ";

                    using (var addConstraintCommand = new NpgsqlCommand(sqlScript, connection))
                    {
                        addConstraintCommand.ExecuteNonQuery();
                    }
                }

                // Check if "Admin" and "User" users already exist
                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE \"username\" = 'Admin' OR \"username\" = 'User';", connection))
                {
                    int existingUsersCount = Convert.ToInt32(command.ExecuteScalar());

                    if (existingUsersCount == 0)
                    {
                        // Insert "Admin" and "User" users if they don't exist
                        sqlScript = @"
                        INSERT INTO ""Users"" (""userID"", ""username"", ""password"", ""role"", ""isActive"") VALUES 
                        (1, 'Admin', '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918', 'ADMIN', true),
                        (2, 'User', '04F8996DA763B7A969B1028EE3007569EAF3A635486DDAB211D512C85B9DF8FB', 'USER', true);
                    ";

                        using (var insertCommand = new NpgsqlCommand(sqlScript, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }




    }
}
