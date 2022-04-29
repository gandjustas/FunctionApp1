using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Company.Function.Tests
{
    [TestClass]
    public class Function1Tests
    {
        [TestMethod]
        public void GenerateDaysEmptyFilterTest()
        {
            var today = DateTime.Now;
            var afterTenDays = today.AddDays(10);
            var afterFiveDays = today.AddDays(5);
            var afterSixDays = today.AddDays(6);

            var days = Function1.GenerateDays(today, afterTenDays, new (DateTime, DateTime)[0]);
            Assert.AreEqual((afterTenDays - today).Days + 1, days.Count(), "Empty filter sould return all days");

            var d = today;
            foreach (var day in days)
            {
                Assert.AreEqual(d, day, "Function sould retrun consequent days");
                d = d.AddDays(1);
            }
        }

        [TestMethod]
        public void GenerateDaysFilter()
        {
            var today = DateTime.Now;
            var afterTenDays = today.AddDays(10);
            var afterFiveDays = today.AddDays(5);
            var afterSixDays = today.AddDays(6);

            var days = Function1.GenerateDays(today, afterTenDays, new[] { (afterFiveDays, afterSixDays)} );
            Assert.AreEqual((afterTenDays - today).Days + 1 - 2, days.Count());
        }
    }
}