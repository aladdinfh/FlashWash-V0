#pragma warning disable CS8618

using FlashWash.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace FlashWash.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        //DATABASE TABLES HERE
        public DbSet<WashStation> WashStations { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OfferType> OfferTypes { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Request> Requests { get; set; }
    }
}
