# AutoIndexer
 
A rapid test to generate Index overload extension method. 

### this is proof of concept not actual product.
1. It is incomplete 
2. Not all scenarios are covered as of now
3. No error handling and user warning 

main goal was to prototype it and check feasibility.



### Working
For this Sample
```cs
namespace Try
{
    public class DataStore<T>
    {
        public int Count { get; set; }
        public void AddAt([AutoIndexer] int idx, 
            [AutoIndexer] int y)
        {
            Console.WriteLine(idx);
        }
        public void RemoveAt([AutoIndexer] int idx,
            [AutoIndexer] double z,string k,float f)
        {
            Console.WriteLine(idx);
        }
    }
}
```

Source Generator generates

```cs
namespace Try;
public static class DataStoreExtn0
{
    public static void  AddAt<T>(this DataStore<T> @type,Index idx, Index y)=>
        @type.AddAt(idx.GetOffset(@type.Count), y.GetOffset(@type.Count));

    public static void  RemoveAt<T>(this DataStore<T> @type,Index idx, Index z, string  k, float  f)=>
        @type.RemoveAt(idx.GetOffset(@type.Count), z.GetOffset(@type.Count), k, f);

}
```
