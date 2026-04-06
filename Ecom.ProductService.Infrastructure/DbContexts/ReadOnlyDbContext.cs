using Microsoft.EntityFrameworkCore;

namespace Ecom.ProductService.Infrastructure.DbContexts
{
    public class ReadOnlyDbContext : EcomProductDbContext
    {
        // Truyền thẳng options vào base, không cần convert gì cả
        public ReadOnlyDbContext(DbContextOptions<ReadOnlyDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ép toàn bộ query qua context này không được track để tối ưu slave
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }

        // Chặn tuyệt đối việc ghi ở tầng Slave để bảo vệ dữ liệu Replication
        public override int SaveChanges()
            => throw new InvalidOperationException("Ný ơi, đây là DB Replica (Read-only), đừng ghi vào đây!");

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Ný ơi, đây là DB Replica (Read-only), đừng ghi vào đây!");
    }
}