// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return all presented in market products")]
		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

        [Category("Restriction Operators")]
        [Title("Where - Task 3")]
        [Description("This sample return all customers from London city")]
        public void Linq3()
        {
            var customers =
                from c in dataSource.Customers
                where c.City == "London"
                select c;

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        // done 1
        [Category("Restriction Operators")]
        [Title("Where - Task 4")]
        [Description("This sample returns all customers with sum of orders total greater than X")]
        public void Linq4()
        {
            decimal X = 1_000m;

            var customers =
                from c in dataSource.Customers
                where c.Orders.Sum(ord => ord.Total) > X
                select c;

            ObjectDumper.Write($"Where sum of orders more then {X}.");

            foreach (var c in customers)
            {
                ObjectDumper.Write($"CustomerId = {c.CustomerID} TotalSum = {c.Orders.Sum(o => o.Total)}\n");
            }
        }

        // done 2
        [Category("Restriction Operators")]
        [Title("Where - Task 5")]
        [Description("This sample return for each customer a list of suppliers from the same city and country")]
        public void Linq5()
        {
            var customers =
                from cs in (
                    from sup in dataSource.Suppliers
                    select new
                    {
                        Supplier = sup,
                        Customers =
                            from c in dataSource.Customers
                            where c.City == sup.City && c.Country == sup.Country
                            select c.CustomerID
                    })
                where cs.Customers.Any()
                select cs;

            ObjectDumper.Write("Without grouping.\n");

            foreach (var s in customers)
            {
                ObjectDumper.Write(s.Customers);
                ObjectDumper.Write(s.Supplier);
                ObjectDumper.Write("\n");
            }

            ObjectDumper.Write("With grouping.\n");

            var customersWithGroup =
                from c in dataSource.Customers
                join sup in dataSource.Suppliers on new { c.Country, c.City } equals new { sup.Country, sup.City }
                group c by sup into cSup
                select new
                {
                    Supplier = cSup.Key,
                    Customers = from c in cSup
                                select c.CustomerID
                };

            foreach (var s in customersWithGroup)
            {
                ObjectDumper.Write(s.Customers);
                ObjectDumper.Write(s.Supplier);
                ObjectDumper.Write("\n");
            }
        }

        // done 4
        [Category("Restriction Operators")]
        [Title("Where - Task 7")]
        [Description("This sample returns all customers with their first orders month and year")]
        public void Linq7()
        {
            var customers =
                from c in dataSource.Customers
                where c.Orders.Any()
                select new
                {
                    CustomerId = c.CustomerID,
                    StartDate = (
                        from ord in c.Orders
                        orderby ord.OrderDate
                        select ord).First().OrderDate
                };

            foreach (var c in customers)
            {
                ObjectDumper.Write($"CustomerId: {c.CustomerId} Month: {c.StartDate.Month} Year: {c.StartDate.Year}");
            }
        }

        // done 5
        [Category("Restriction Operators")]
        [Title("Where - Task 8")]
        [Description("This sample returns all customers with their first orders month and year ordered by [year, month, sum of orders total, clientName]")]
        public void Linq8()
        {
            var customers =
                from c1 in
                    (from c in dataSource.Customers
                        where c.Orders.Any()
                        select new
                        {
                            CustomerId = c.CustomerID,
                            StartDate = (
                                from ord in c.Orders
                                orderby ord.OrderDate
                                select ord).First().OrderDate,
                            TotalSum = c.Orders.Sum(o => o.Total)
                        })
                orderby c1.StartDate.Year descending, 
                        c1.StartDate.Month descending, 
                        c1.TotalSum descending,
                        c1.CustomerId descending
                select c1;

            foreach (var c in customers)
            {
                ObjectDumper.Write($"CustomerId: {c.CustomerId} Month: {c.StartDate.Month} Year: {c.StartDate.Year}");
            }
        }

        // done 3
        [Category("Restriction Operators")]
        [Title("Where - Task 9")]
        [Description("This sample returns all customers with at least one order total greater than X")]
        public void Linq9()
        {
            decimal X = 100m;

            var customers =
                from c in dataSource.Customers
                where (from or in c.Orders
                       where or.Total > X
                       select or).Any()
                select c.CustomerID;

            foreach (var c in customers)
            {
                ObjectDumper.Write($"CustomerId: {c}");
            }
        }

        // done 6
        [Category("Restriction Operators")]
        [Title("Where - Task 10")]
        [Description("This sample returns with not number postal code or without region or without operator's code")]
        public void Linq10()
        {
            var customers =
                from c in dataSource.Customers
                where c.PostalCode != null && c.PostalCode.Any(symbol => symbol < '0' || symbol > '9')
                      || string.IsNullOrWhiteSpace(c.Region)
                      || c.Phone.FirstOrDefault() != '('

                select c;

            foreach (var c in customers)
            {
                ObjectDumper.Write($"CustomerId: {c.CustomerID} PostalCode: {c.PostalCode} Phone: {c.Phone} Region: {c.Region}");
            }
        }

        // done 7
        [Category("Restriction Operators")]
        [Title("Where - Task 11")]
        [Description("This sample returns groups of products by categories then by units in stock > 0  then order by unitPrice")]
        public void Linq11()
        {
            var productGroups =
                from g in (
                    from p in dataSource.Products
                    group p by p.Category
                    into gr
                    select gr)
                select new
                {
                    Category = g.Key,
                    ProductsByStock =
                        from g1 in (
                            from p in g
                            group p by p.UnitsInStock > 0
                            into gr
                            select gr)
                        select new 
                        {
                            IsInStock = g1.Key,
                            Products = from g2 in g1
                                        orderby g2.UnitPrice
                                        select g2
                        }
                };

            foreach (var productsByCategory in productGroups)
            {
                ObjectDumper.Write($"Category: {productsByCategory.Category}\n");
                foreach (var productsByStock in productsByCategory.ProductsByStock)
                {
                    ObjectDumper.Write($"\tHas in stock: {productsByStock.IsInStock}");
                    foreach (var product in productsByStock.Products)
                    {
                        ObjectDumper.Write($"\t\tProduct: {product.ProductName} Price: {product.UnitPrice}");
                    }
                }
            }
        }

        // done 8
        [Category("Restriction Operators")]
        [Title("Where - Task 12")]
        [Description("This sample returns grouped products by price: Cheap, Average price, Expensive")]
        public void Linq12()
        {
            decimal cheap = 20;
            decimal average = 50;

            var groupsByPrice =
                from p in dataSource.Products
                group p by p.UnitPrice < cheap ? "Cheap" :
                    p.UnitPrice < average ? "Average" : "Expensive" 
                into groups
                select groups;

            foreach (var group in groupsByPrice)
            {
                ObjectDumper.Write($"{group.Key}:");
                foreach (var product in group)
                {
                    ObjectDumper.Write($"\tProduct: {product.ProductName} Price: {product.UnitPrice}\n");
                }
            }

        }

        // done 9
        [Category("Restriction Operators")]
        [Title("Where - Task 13")]
        [Description("This sample returns average order sum for and average client's intensity for every city")]
        public void Linq13()
        {
            var result =
                from g in (
                    from p in dataSource.Customers
                    group p by p.City
                    into groups
                    select groups)
                select new
                {
                    City = g.Key,
                    Intensity = g.Average(c => c.Orders.Length),
                    AverageIncome = g.Average(c => c.Orders.Sum(ord => ord.Total))
                };

            foreach (var group in result)
            {
                ObjectDumper.Write($"City: {group.City}");
                ObjectDumper.Write($"\tIntensity: {group.Intensity}");
                ObjectDumper.Write($"\tAverage Income: {group.AverageIncome}");
            }
        }

        // done 10
        [Category("Restriction Operators")]
        [Title("Where - Task 14")]
        [Description("This sample returns clients activity statistic by month(without year), by year and by year and month")]
        public void Linq14()
        {
            var stat =
                from cust in dataSource.Customers
                select new
                {
                    CustomerId = cust.CustomerID,
                    MonthStat = 
                        from o in cust.Orders
                        group o by o.OrderDate.Month into gr
                        select new
                        {
                            Month = gr.Key,
                            OrdersCount = gr.Count()
                        },
                    YearStat =
                        from o in cust.Orders
                        group o by o.OrderDate.Year into gr
                        select new
                        {
                            Year = gr.Key,
                            OrdersCount = gr.Count()
                        },
                    YearMonthStat =
                        from o in cust.Orders
                        group o by new { o.OrderDate.Month, o.OrderDate.Year } into gr
                        select new
                        {
                            gr.Key.Year,
                            gr.Key.Month,
                            OrdersCount = gr.Count()
                        }
                };

            foreach (var record in stat)
            {
                ObjectDumper.Write($"CustomerId: {record.CustomerId}");
                ObjectDumper.Write("\tMonths statistic:\n");
                foreach (var ms in record.MonthStat)
                {
                    ObjectDumper.Write($"\t\tMonth: {ms.Month} Orders count: {ms.OrdersCount}");
                }
                ObjectDumper.Write("\tYears statistic:\n");
                foreach (var ys in record.YearStat)
                {
                    ObjectDumper.Write($"\t\tYear: {ys.Year} Orders count: {ys.OrdersCount}");
                }
                ObjectDumper.Write("\tYear and month statistic:\n");
                foreach (var ym in record.YearMonthStat)
                {
                    ObjectDumper.Write($"\t\tYear: {ym.Year} Month: {ym.Month} Orders count: {ym.OrdersCount}");
                }
            }
        }
    }
}
