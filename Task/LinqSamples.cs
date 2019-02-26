// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
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

        //[Category("Task")]
        //[Title("Task 006")]
        //[Description("Displays all customers with not number postal code or without region " +
        //             "or whithout operator's code")]
        //public void Linq006()
        //{
        //    var customers = dataSource.Customers.Where(
        //        c => c.PostalCode != null && c.PostalCode.Any(sym => sym < '0' || sym > '9')
        //             || string.IsNullOrWhiteSpace(c.Region)
        //             || c.Phone.FirstOrDefault() != '(');

        //    foreach (var c in customers)
        //    {
        //        ObjectDumper.Write(c);
        //    }
        //}






        // todo
        [Category("Restriction Operators")]
        [Title("Where - Task 11")]
        [Description("This sample returns ")]
        public void Linq11()
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

        // todo
        [Category("Restriction Operators")]
        [Title("Where - Task 12")]
        [Description("This sample returns ")]
        public void Linq12()
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

        //public void Linq005()
        //{
        //    var customers = dataSource.Customers.Where(c => c.Orders.Any())
        //        .Select(c => new
        //        {
        //            CustomerId = c.CustomerID,
        //            StartDate = c.Orders.OrderBy(o => o.OrderDate).Select(o => o.OrderDate).First(),
        //            TotalSum = c.Orders.Sum(o => o.Total)
        //        }).OrderByDescending(c => c.StartDate.Year)
        //        .ThenByDescending(c => c.StartDate.Month)
        //        .ThenByDescending(c => c.TotalSum)
        //        .ThenByDescending(c => c.CustomerId);

        //    foreach (var c in customers)
        //    {
        //        ObjectDumper.Write($"CustomerId = {c.CustomerId} TotalSum: {c.TotalSum} " +
        //                           $"Month = {c.StartDate.Month} Year = {c.StartDate.Year}");
        //    }
        //}
    }
}
