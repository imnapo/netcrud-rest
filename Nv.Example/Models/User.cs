using Bogus;
using Microsoft.EntityFrameworkCore;
using Nv.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Nv.Example.Models
{
    public class User : EntityBase
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public int Age { get; set; }

        [ForeignKey("Address")]
        public int? AddressId { get; set; }

        public Address Address { get; set; }

        public ICollection<UserGame> UserGames { get; set; }
    }

    public class Game : EntityBase
    {
        public string Name { get; set; }

        //public ICollection<User> Users { get; set; }
    }

    public class UserGame : EntityBase
    {
        public User User { get; set; }

        public Game Game { get; set; }
    }


    public class CrubDbContext : DbContext
    {
        public CrubDbContext(DbContextOptions<CrubDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
          .HasOne<Address>(ps => ps.Address)
          .WithOne(jc => jc.User)
          //.HasForeignKey(j => j.)
          .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<UserGame> UserGames { get; set; }

    }

    public static class CrubDbContextExtension
    {
        public static async Task Seed(this CrubDbContext context)
        {
            var testUsers = new Faker<User>()
                        .RuleFor(o => o.Email, f => f.Person.Email)
                        .RuleFor(o => o.Name, f => f.Person.FullName)
                        .RuleFor(o => o.Age, f => f.Random.Number(17,60))
                        .RuleFor(o => o.CreatedAt, f => f.Date.Past())
                        .RuleFor(o => o.Address, (f, u) => new Address { User = u, AddressText = f.Address.FullAddress() })
                        .RuleFor(c => c.ModifiedAt, f => f.Date.Past());


            

            context.UserGames.RemoveRange(context.UserGames);
            context.Users.RemoveRange(context.Users);
            context.Addresses.RemoveRange(context.Addresses);

            var users = testUsers.Generate(100).ToList();
            await context.Users.AddRangeAsync(users);

            var testGames = new Faker<Game>()
                       .RuleFor(o => o.Name, f => f.Lorem.Word());
            var games = testGames.Generate(10).ToList();
            await context.Games.AddRangeAsync(games);



            await context.SaveChangesAsync();

            var testUserGames = new Faker<UserGame>()
                .RuleFor(o => o.User, f => f.PickRandom<User>(users))
          .RuleFor(o => o.Game, f => f.PickRandom<Game>(games));

            var usergames = testUserGames.Generate(100).ToList();
            await context.UserGames.AddRangeAsync(usergames);
            await context.SaveChangesAsync();

        }
    }
}
