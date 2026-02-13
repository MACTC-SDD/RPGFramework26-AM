using RPGFramework.Enums;
using RPGFramework.Interfaces;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RPGFramework
{
    [CollectionBuilder(typeof(CatalogBuilder), nameof(CatalogBuilder.Create))]
    internal class Catalog<TKey, TValue> :
        ICatalog<TKey, TValue>
        where TKey : notnull
    {
        #region --- Properties ---
        [JsonInclude]
        public Dictionary<TKey, TValue> Items
        {
            get => _items;
            private set
            {
                _items.Clear();
                foreach (var kvp in value)
                {
                    _items[kvp.Key] = kvp.Value;
                }
            }
        }

        [JsonIgnore] public IEnumerable<TKey> Keys => _items.Keys;
        [JsonIgnore] public IEnumerable<TValue> Values => _items.Values;
        [JsonIgnore] public int Count => _items.Count;
        [JsonIgnore] public string Name => typeof(TValue).Name + "Catalog";
        #endregion

        // Fields
        private readonly Dictionary<TKey, TValue> _items = [];

        public Catalog()
        {
        }

        public Catalog(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var (key, value) in items)
                Add(key, value);
        }

        #region Indexer
        // Creates the indexer to access items by key
        public TValue this[TKey key]
        {
            get
            {
                if (!_items.TryGetValue(key, out var value))
                {
                    throw new KeyNotFoundException($"The key '{key}' was not found in the catalog.");
                }
                return value;
            }
            set
            {
                _items[key] = value;
            }
        }

        public object this[object key]
        {
            get
            {
                if (!_items.TryGetValue((TKey)key, out var value))
                {
                    throw new KeyNotFoundException($"The key '{key}' was not found in the catalog.");
                }
                return value;
            }
            set
            {
                _items[(TKey)key] = (TValue)value;
            }
        }
        #endregion

        #region Add Methods
        public void Add(TKey key, TValue value)
        {
            if (_items.ContainsKey(key))
            {
                throw new ArgumentException($"An item with the key '{key}' already exists in the catalog.");
            }
            _items[key] = value;
        }

        public void Add(object key, object value)
        {
            var typedKey = (TKey)key;
            var typedValue = (TValue)value;
            if (_items.ContainsKey(typedKey))
            {
                throw new ArgumentException($"An item with the key '{key}' already exists in the catalog.");
            }
            _items[typedKey] = typedValue;
        }
        #endregion

        public bool ContainsKey(TKey key) => _items.ContainsKey(key);

        public bool ContainsKey(object key) => _items.ContainsKey((TKey)key);

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => _items.GetEnumerator();

        #region LoadCatalogAsync Method
        public async Task LoadCatalogAsync()
        {
            try
            {
                var loadedCatalog = await GameState.Persistence.LoadCatalogAsync<Catalog<TKey, TValue>>(Name);
                if (loadedCatalog != null)
                {
                    _items.Clear();
                    foreach (var kvp in loadedCatalog._items)
                    {
                        _items[kvp.Key] = kvp.Value;
                    }

                    GameState.Log(DebugLevel.Info, $"Catalog '{Name}' loaded with {_items.Count} items.");
                }
            }
            catch (FileNotFoundException fex)
            {
                GameState.Log(DebugLevel.Error, $"Error loading catalog '{Name}' (will use blank): {fex.Message}");
            } 
            return;
        }
        #endregion

        public bool Remove(TKey key) => _items.Remove(key);
        public bool Remove(object key) => _items.Remove((TKey)key);

        #region SaveCatalogAsync Method
        // Use this method to save the catalog to persistent storage
        public async Task SaveCatalogAsync()
        {
            await GameState.Persistence.SaveCatalogAsync(this, Name);
            return;
        }
        #endregion

        #region TryGetValue Methods
        public bool TryGetValue(TKey key, out TValue? value) => _items.TryGetValue(key, out value);
        //public bool TryGetValue(string key, out object value) => TryGetValue(key, out value);
        public bool TryGetValue(string key, out TValue? value)
        {
            value = default;

            if (_items.Keys.Count > 0 && _items.Keys.First().GetType() != typeof(string))
            {
                return false;
            }

            // Search keys in a case-insensitive manner
            foreach (var kvp in _items)
            {
                if (string.Equals(kvp.Key as string, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    return true;
                }
            }

            return false;
        }
        #endregion

    }

    #region CatalogBuilder Class
    /// <summary>
    /// Provides factory methods for creating instances of the Catalog<TKey, TValue> class from collections of key/value
    /// pairs. This will allow us to add the CollectionBuilder attribute to Catalog so we can support collection expressions.
    /// </summary>
    internal static class CatalogBuilder
    {
        public static Catalog<TKey, TValue> Create<TKey, TValue>(
            ReadOnlySpan<KeyValuePair<TKey, TValue>> items)
            where TKey : notnull
        {
            var catalog = new Catalog<TKey, TValue>();
            foreach (var kvp in items)
            {
                catalog.Add(kvp.Key, kvp.Value);
            }
            return catalog;
        }
    }
    #endregion
}
