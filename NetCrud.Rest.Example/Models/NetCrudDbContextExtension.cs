using Bogus;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace NetCrud.Rest.Example.Models
{
    public static class NetCrudDbContextExtension
    {
        public static async Task Seed(this NetCrudDbContext context)
        {
            if (await context.Customers.AnyAsync()) return;

            var testCustomers = new Faker<Customer>()
                .RuleFor(o => o.Name, f => f.Person.FullName)
                .RuleFor(o => o.Age, f => f.Random.Number(15, 60))
                .RuleFor(o => o.Address, (f, u) => new Address { Customer = u, AddressText = f.Address.FullAddress() });

            var customers = testCustomers.Generate(100).ToList();
            await context.Customers.AddRangeAsync(customers);

            var testProducts = new Faker<Product>()
                .RuleFor(o => o.Price, f => f.Finance.Random.Int(5, 200))
                .RuleFor(o => o.Name, f => f.Commerce.Product());
            var products = testProducts.Generate(10).ToList();
            await context.Products.AddRangeAsync(products);

            var testPurchases = new Faker<Purchase>()
                .RuleFor(o => o.PurchaseDate, f => f.Date.Past())
                .RuleFor(o => o.Customer, f => f.PickRandom(customers))
                .RuleFor(o => o.Product, f => f.PickRandom(products));

            var purchases = testPurchases.Generate(100).ToList();
            await context.Purchases.AddRangeAsync(purchases);

            await context.SaveChangesAsync();

        }
    }
}
