// See https://aka.ms/new-console-template for more information

using System;
using Try;

new DataStore<int>().AddAt(50,20);

namespace Try
{
    public class DataStore<T>
    {
        public void AddAt([AutoIndexer] int idx, [AutoIndexer] int y)
        {
            Console.WriteLine(idx);
        }
        public void AddAt([AutoIndexer] int idx, [AutoIndexer] double z)
        {
            Console.WriteLine(idx);
        }
    }

}