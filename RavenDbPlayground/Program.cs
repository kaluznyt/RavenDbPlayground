using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;

namespace RavenDbPlayground
{
				public static class Program
				{
								private static void Main(string[] args)
								{
												//CreateEmployeeObjectsInRaven();
												QueryEmployeeByFirstName();
								}

								private static IDocumentSession ConnectAndReturnSession()
								{
												return new DocumentStore
												{
																Url = "http://localhost:8081",
																DefaultDatabase = "Northwind"
												}
												.Initialize()
												.OpenSession();
								}

								private static void QueryEmployeeByFirstName()
								{
												using(var session = ConnectAndReturnSession())
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
												using(var session = ConnectAndReturnSession())
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
