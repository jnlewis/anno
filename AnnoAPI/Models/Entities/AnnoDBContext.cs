using System.Data.Entity;

namespace Anno.Models.Entities
{
	[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
	public partial class AnnoDBContext : DbContext
	{
		public AnnoDBContext() : base("name=Anno")
		{
			Database.SetInitializer<AnnoDBContext>(new AnnoDBInitializer());
		}

		public virtual DbSet<ApiKey> ApiKey { get; set; }

        public virtual DbSet<Customer> Customer { get; set; }

        public virtual DbSet<CustomerBooking> CustomerBooking { get; set; }

        public virtual DbSet<CustomerTicket> CustomerTicket { get; set; }

        public virtual DbSet<Events> Events { get; set; }

        public virtual DbSet<EventsTier> EventsTier { get; set; }

        public virtual DbSet<Host> Host { get; set; }

        public virtual DbSet<Transaction> Transaction { get; set; }

        public virtual DbSet<Wallet> Wallet { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
            //modelBuilder.Entity<Customer>()
            //    .Property(e => e.CustomerName)
            //    .IsUnicode(false);

            //// configures one-to-many relationship
            //modelBuilder.Entity<Order>()
            //    .HasRequired<Customer>(s => s.Customer)
            //    .WithMany(g => g.Orders)
            //    .HasForeignKey<int>(s => s.OrderId);
        }
	}
}
