using System.Collections.Generic;
using System.Diagnostics;

namespace WcfBindingPerformance.Services
{
    public static class ArbitraryCalculation
    {
        public static double SortList()
        {
            return SortList(700000);
        }

        public static double SortList(int i)
        {
            var sw = new Stopwatch();
            sw.Start();

            var integers = new List<int>();
            for (int j = 0; j < i; j++)
            {
                integers.Add(1);
            }

            integers.Sort();

            sw.Stop();

            return sw.ElapsedMilliseconds;
        }
    }
}