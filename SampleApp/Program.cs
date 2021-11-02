// See https://aka.ms/new-console-template for more information

using System;

new DataStore<int>().AddAt(50,20);

public class DataStore<T>
{
    public void AddAt([AutoIndexer] int idx,int y)
    {
        Console.WriteLine(idx);
    }
}

