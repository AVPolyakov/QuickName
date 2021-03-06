﻿using System;
using System.Reflection;

namespace QuickName.Tests
{
    class Program
    {
        static void Main()
        {
            var metadata = new Metadata<My1>();
            Console.WriteLine(
                $"{metadata.GetProperyName(_ => _.A001)} " +
                $"{metadata.GetProperyName(_ => _.A002)} " +
                $"{metadata.GetProperyName(_ => _.A003)} " +
                $"{QuickName.GetProperyInfo(() => My3.A011).Name} " +
                $"{QuickName.GetProperyInfo(() => My3.A012).Name}");
        }
    }

    public class My1
    {
        public string A001 { get; set; }
        public int A002 { get; set; }
        public My2 A003 { get; set; }
    }

    public class My2
    {
    }

    public class My3
    {
        public static string A011 { get; set; }
        public static int A012 { get; set; }
    }

    public class Metadata<T>
    {        
    }

    public static class MetadataExtensions
    {
        public static PropertyInfo GetProperyInfo<T, TPropery>(this Metadata<T> metadata, Func<T, TPropery> func) 
            => QuickName.GetProperyInfo(func);

        public static string GetProperyName<T, TPropery>(this Metadata<T> metadata, Func<T, TPropery> func) 
            => metadata.GetProperyInfo(func).Name;
    }
}
