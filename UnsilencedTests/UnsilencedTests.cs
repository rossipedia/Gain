// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnsilencedTests.cs" company="Bryan J. Ross">
//   (c) Bryan J. Ross. This code is provided as-is, with no warranty expressed or implied. Do with it as you will.
// </copyright>
// <summary>
//   The unsilenced tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UnsilencedTests
{
    using System;

    using Unsilenced;
    using NUnit.Framework;

    /// <summary>
    /// The unsilenced tests.
    /// </summary>
    public class UnsilencedTests
    {
        private class Product
        {
            public Product(int id, string name, decimal price)
            {
                this.Id = id;
                this.Name = name;
                this.Price = price;
            }

            public int Id { get; private set; }

            public string Name { get; private set; }

            public decimal Price { get; private set; }

            public override string ToString()
            {
                return string.Format("Product #{0}: {1} ${2}", this.Id, this.Name, this.Price);
            }
        }

        private Product p1;

        private Product p2;

        [SetUp]
        public void SetUp()
        {
            this.p1 = new Product(1, "iPod", 149.99m);
            this.p2 = p1.Change(p => p.Price, 299.99m);
        }

        [Test]
        public void TestNewObject()
        {
            Assert.AreNotSame(this.p1, this.p2);
        }

        [Test]
        public void TestSameId()
        {
            Assert.AreEqual(this.p1.Id, this.p2.Id);
        }

        [Test]
        public void TestSameName()
        {
            Assert.AreEqual(this.p1.Name, this.p2.Name);
        }

        [Test]
        public void TestDifferentPrice()
        {
            Assert.AreNotEqual(this.p1.Price, this.p2.Price);
        }

        [Test]
        public void TestCanChain()
        {
            var p3 = this.p2.Change(p => p.Name, "iPad").Change(p => p.Price, 499.99m);
            var expected = new Product(this.p2.Id, "iPad", 499.99m);

            Assert.AreEqual(expected.Id, p3.Id);
            Assert.AreEqual(expected.Name, p3.Name);
            Assert.AreEqual(expected.Price, p3.Price);

            Assert.AreNotEqual(this.p2, p3);
            Assert.AreNotEqual(this.p1, p3);
        }

        private class Category
        {
            public Category(int id, string name)
            {
                this.Id = id;
                this.Name = name;

                this.ProductCount = 0;
            }

            public int Id { get; private set; }

            public string Name { get; private set; }

            public int ProductCount { get; private set; }
        }

        [Test]
        public void TestMorePropertiesThanParameters()
        {
            var c1 = new Category(1, "Electronics");
            var c2 = c1.Change(c => c.Name, "Apple Electronics");

            Assert.AreNotSame(c1, c2);
        }

        private class Tag
        {
            private readonly int count;

            public Tag(int id, string name, int count)
            {
                this.Id = id;
                this.Name = name;
                this.count = count;
            }

            public int Id { get; private set; }

            public string Name { get; private set; }
        }

        [Test]
        public void TestMoreParametersThanProperties()
        {
            var t1 = new Tag(1, "General", 3);
            Assert.Throws<InvalidOperationException>(() => t1.Change(t => t.Name, "Electronics"));
        }

        class Name
        {
            public Name(string firstName, string lastName)
            {
                this.FirstName = firstName;
                this.LastName = lastName;
            }

            public string FirstName { get; private set; }

            public string LastName { get; private set; }

            public static implicit  operator string(Name n)
            {
                return n.ToString();
            }

            public override string ToString()
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        class Person
        {
            public Person(int id, Name name, int age)
            {
                this.Id = id;
                this.Name = name;
                this.Age = age;
            }

            public int Id { get; private set; }

            public Name Name { get; private set; }

            public int Age { get; private set; }
        }

        [Test]
        public void TestNestedReferenceTypeChangeAreNotSame()
        {
            var p1 = new Person(1, new Name("John", "Doe"), 27);
            var p2 = p1.Change(p => p.Name, new Name("Jane", "Doe"));
            Assert.AreNotSame(p1, p2);
            Assert.AreNotSame(p1.Name, p2.Name);
        }

        [Test]
        public void TestNestedReferenceTypeWithNoChangeAreSame()
        {
            var p1 = new Person(1, new Name("John", "Doe"), 27);
            var p2 = p1.Change(p => p.Age, 32);
            Assert.AreNotSame(p1, p2);
            Assert.AreSame(p1.Name, p2.Name);
        }
    }
}
