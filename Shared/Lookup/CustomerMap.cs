using Shared.Models;

namespace Shared.Lookup;

public static class CustomerMap
{
    /// <summary>
    /// Maps the various IDs to their customers. In a real-world scenario, this would come from a database, but
    /// that's outside the scope/budget for this exercise.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<Guid, Customer> Customers()
    {
        return new Dictionary<Guid, Customer>
        {
            {
                new Guid("e4cd5381-e822-4f1b-a991-4e78b4bfb929"),
                new Customer("Counting On You", "1088 Stewart Street", "Indianapolis", "IN", "46204",
                    "CA", "")
            },
            {
                new Guid("f3a7c1e0-8b4d-4f7b-9e6d-1a2b3c4d5e6f"),
                new Customer("Lettuce Turnip the Beat Farms, LLC", "2252 White Avenue", "Corpus Christi", "TX",
                    "78415", "LT", "")
            }
        };
    }
}