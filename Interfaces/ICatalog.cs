

namespace RPGFramework.Interfaces
{

    public interface ICatalog
    {        
        object this[object key] { get; set; }
        string Name { get; }
        int Count { get; }
        
        void Add(object key, object value);
        bool ContainsKey(object key);
        Task LoadCatalogAsync();

        Task SaveCatalogAsync();

        bool Remove(object key);

    }

    public interface ICatalog<TKey, TValue> : ICatalog where TKey : notnull
    {
        TValue this[TKey key] { get; set;}
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
     

        void Add(TKey key, TValue value);
        bool ContainsKey(TKey key);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue? value);        
    }
}

