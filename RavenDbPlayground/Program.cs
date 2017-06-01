using System;
using static System.Console;

namespace RavenDbPlayground
{
    using System.Linq;

    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.Linq;

    public static class Program
    {
        private static void Main(string[] args)
        {
            //CreateEmployeeObjectsInRaven();
            //QueryEmployeeByFirstName();

            //var session = DocumentStore.Value.OpenSession();

            //Konsola.WriteLine(session.Load<Product>("products/1").Name);

            PrintOrder(1);

        }

        private static readonly Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(
            () =>
                {
                    //var store = new DocumentStore { Url = "http://localhost:8080", DefaultDatabase = "Northwind" };
                    var store = new DocumentStore { ConnectionStringName = "ravendb" };
                    return store.Initialize();
                });


        private static void PrintOrder()
        {
            while(true)
            {
                WriteLine("Please, enter an order # (0 to exit): ");

                int orderNumber;
                if(!int.TryParse(ReadLine(), out orderNumber))
                {
                    WriteLine("Order # is invalid.");
                    continue;
                }

                if(orderNumber == 0)
                    break;

                PrintOrder(orderNumber);
            }

            WriteLine("Goodbye!");
        }

        private static void PrintOrder(int orderNumber)
        {
            using(var session = DocumentStore.Value.OpenSession())
            {

                var order = session.Include<Order>(o => o.Company)
                    .Include(o => o.Employee)
                    .Include(o => o.Lines.Select(l => l.Product))
                    .Load(orderNumber);

                if(order == null)
                {
                    WriteLine($"Order #{orderNumber} not found");
                    return;
                }

                WriteLine($"Order #{orderNumber}");

                var c = session.Load<Company>(order.Company);
                WriteLine($"Company: {c.Id} - {c.Name}");

                var e = session.Load<Employee>(order.Employee);
                WriteLine($"Employee: {e.Id} - {e.LastName},{e.FirstName}");

                foreach(var orderLine in order.Lines)
                {

                    var p = session.Load<Product>(orderLine.Product);
                    WriteLine($" - {orderLine.ProductName}, {orderLine.Quantity} x {p.QuantityPerUnit}");
                }


            }
        }

        private static void QueryEmployeeByFirstName()
        {
            using(var session = DocumentStore.Value.OpenSession())
            {
                var address = new Address
                {
                    Street = "Somestreet rd",
                    PostCode = "X13 AAA",
                    City = "Some City"
                };

                using(var enumerator = session.Advanced.Stream(
                            session.Query<Employee>("ByFirstAndLastName", false)
                        .Where(e => e.FirstName.StartsWith("John"))))
                {
                    while(enumerator.MoveNext())
                    {

                        enumerator.Current.Document.Address = address;
                        session.Store(enumerator.Current.Document);
                    }

                };

                session.Advanced.DocumentStore.SetRequestsTimeoutFor(TimeSpan.FromHours(1));

                session.SaveChanges();
            }
        }

        private static void CreateEmployeeObjectsInRaven()
        {
            using(var session = DocumentStore.Value.OpenSession())
            {
                for(var i = 0; i < 90000; i++)
                {

                    var employee = new Employee
                    {
                        FirstName = "John" + i,
                        LastName = "Doe"
                    };

                    session.Store(employee);
                }

                session.SaveChanges();
            }
        }
    }
}
