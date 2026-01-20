

namespace RPGFramework.Interfaces
{

    public interface ICatalog
    {
        string Name { get; }
        int Count { get; }

        Task LoadCatalogAsync();

        Task SaveCatalogAsync();
    }

    public interface ICatalog<TKey, TValue> : ICatalog where TKey : notnull
    {
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        TValue this[TKey key] { get; set; }

        void Add(TKey key, TValue value);
        bool ContainsKey(TKey key);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue? value);
    }
}

