using Microsoft.EntityFrameworkCore;
using GatewayAI.Models;

namespace GatewayAI.Data
{
    public class GatewayContext : DbContext
    {
        public GatewayContext(DbContextOptions<GatewayContext> options) : base(options)
        {
        }

        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<AiRequest> AiRequests { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Seed a default API Key for testing
            modelBuilder.Entity<ApiKey>().HasData(new ApiKey 
            { 
                Id = 1, 
                Key = "gateway-test-key-123", 
                ApplicationName = "TestApp" 
            });
        }
    }
}
