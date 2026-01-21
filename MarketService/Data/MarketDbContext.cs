using Microsoft.EntityFrameworkCore;
using MarketService.Models;

namespace MarketService.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        // New entities
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<SellerScore> SellerScores { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }
        public DbSet<ShippingOrder> ShippingOrders { get; set; }
        public DbSet<ProductSuggestion> ProductSuggestions { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostImage> PostImages { get; set; }

        // Order Management entities
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Conversation configuration - prevent cascade delete cycles
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasOne(c => c.User1)
                    .WithMany(u => u.ConversationsAsUser1)
                    .HasForeignKey(c => c.User1Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.User2)
                    .WithMany(u => u.ConversationsAsUser2)
                    .HasForeignKey(c => c.User2Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.User1Id, c.User2Id }).IsUnique();
            });

            // Message configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Seller)
                    .WithMany(u => u.Products)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.CategoryEntity)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Post configuration
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(p => p.Author)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserVerification configuration
            modelBuilder.Entity<UserVerification>(entity =>
            {
                entity.HasOne(v => v.User)
                    .WithMany(u => u.Verifications)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(v => new { v.UserId, v.Type }).IsUnique();
                entity.HasIndex(v => v.Status);
            });

            // SellerScore configuration
            modelBuilder.Entity<SellerScore>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithOne(u => u.SellerScore)
                    .HasForeignKey<SellerScore>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.OverallScore);
            });

            // Follow configuration
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasOne(f => f.Follower)
                    .WithMany(u => u.Following)
                    .HasForeignKey(f => f.FollowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Following)
                    .WithMany(u => u.Followers)
                    .HasForeignKey(f => f.FollowingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
                entity.HasIndex(f => f.FollowerId);
                entity.HasIndex(f => f.FollowingId);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.HasIndex(c => c.Slug).IsUnique();
            });

            // ShippingAddress configuration
            modelBuilder.Entity<ShippingAddress>(entity =>
            {
                entity.HasOne(sa => sa.User)
                    .WithMany(u => u.ShippingAddresses)
                    .HasForeignKey(sa => sa.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(sa => new { sa.UserId, sa.IsDefault });
            });

            // ShippingOrder configuration
            modelBuilder.Entity<ShippingOrder>(entity =>
            {
                entity.HasOne(so => so.Product)
                    .WithMany()
                    .HasForeignKey(so => so.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(so => so.Seller)
                    .WithMany()
                    .HasForeignKey(so => so.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(so => so.Buyer)
                    .WithMany()
                    .HasForeignKey(so => so.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(so => so.TrackingNumber);
                entity.HasIndex(so => so.Status);
            });

            // ProductSuggestion configuration
            modelBuilder.Entity<ProductSuggestion>(entity =>
            {
                entity.HasOne(ps => ps.User)
                    .WithMany()
                    .HasForeignKey(ps => ps.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.Product)
                    .WithMany()
                    .HasForeignKey(ps => ps.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(ps => new { ps.UserId, ps.CreatedAt });
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasOne(ci => ci.User)
                    .WithMany()
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                    .WithMany()
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Order)
                    .WithMany()
                    .HasForeignKey(r => r.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Reviewer)
                    .WithMany()
                    .HasForeignKey(r => r.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Seller)
                    .WithMany()
                    .HasForeignKey(r => r.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Product)
                    .WithMany()
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PostLike configuration
            modelBuilder.Entity<PostLike>(entity =>
            {
                entity.HasOne(pl => pl.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(pl => pl.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pl => pl.User)
                    .WithMany()
                    .HasForeignKey(pl => pl.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(pl => new { pl.PostId, pl.UserId }).IsUnique();
            });

            // PostComment configuration
            modelBuilder.Entity<PostComment>(entity =>
            {
                entity.HasOne(pc => pc.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(pc => pc.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.User)
                    .WithMany()
                    .HasForeignKey(pc => pc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PostImage configuration
            modelBuilder.Entity<PostImage>(entity =>
            {
                entity.HasOne(pi => pi.Post)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Buyer)
                    .WithMany()
                    .HasForeignKey(o => o.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Seller)
                    .WithMany()
                    .HasForeignKey(o => o.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.ShippingAddress)
                    .WithMany()
                    .HasForeignKey(o => o.ShippingAddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.OrderDate);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.TransactionId);
            });

            // Wishlist configuration
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasOne(w => w.User)
                    .WithMany()
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Product)
                    .WithMany()
                    .HasForeignKey(w => w.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            });

            // Global Query Filters (Soft Delete)
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<ShippingOrder>().HasQueryFilter(o => !o.IsDeleted);

        }
    }
}
