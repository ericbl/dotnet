using System.Collections.Generic;

namespace Common.Generic
{
    /// <summary>
    /// Legt ein neues item, wenn ein item mit dem gleichen Schlüssel nicht bereits existiert.
    /// </summary>
    /// <typeparam name="Item">Typ des items</typeparam>
    /// <typeparam name="Key">Typ des Schlüssels</typeparam>
    /// <typeparam name="Data">Payload data, die von der Faktory des Items benutzt werden</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public class LazyCreateItem<Item, Key, Data>
        where Item : class
    {
        /// <summary>
        /// Typ der Factory-Methode zum Erzeugen von Items
        /// </summary>
        /// <param name="key">Schlüssel-Wert des zu erzeugenden Items</param>
        /// <param name="data">Payload-Data des zu erzeugenden Items</param>
        /// <returns>Objekt vom Typ item mit Schlüssel key und Payload-Data data</returns>
        public delegate Item ItemFactory(Key key, Data data);

        /// <summary>
        /// Methode zum Abfragen von bereits existierenden Items
        /// </summary>
        /// <param name="key">Schlüssel-Daten für die Abfrage</param>
        /// <returns>Returns null, wenn ein Item mit dem Schlüssel key nicht existiert, sonst das entsprechende Item
        /// </returns>
        public delegate Item ItemQuery(Key key);

        /// <summary>
        /// Erzeugt Item Objekte
        /// </summary>
        private readonly ItemFactory factory;

        /// <summary>
        /// Fragt existierende Items ab.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public ItemQuery Query { private get; set; }

        /// <summary>
        /// Lookup für die bereits angelegten items.
        /// </summary>
        /// <value>
        /// The lookup.
        /// </value>
        public Dictionary<Key, Item> Lookup { get; private set; }

        /// <summary>
        /// Instanziert das Objekt mit einer Item-Factory
        /// </summary>
        /// <param name="factory">Item-Factory</param>
        /// <param name="query">Query Methode - stellt die Existenz eines Items fest</param>
        public LazyCreateItem(ItemFactory factory, ItemQuery query)
            : this()
        {
            this.factory = factory;
            this.Query = query;
        }

        /// <summary>
        /// Instanziert das Objekt mit einer Item-Factory
        /// </summary>
        /// <param name="factory">Item-Factory</param>
        public LazyCreateItem(ItemFactory factory)
            : this(factory, null)
        {
        }

        /// <summary>
        /// Instanziert das Objekt mit einer Item-Factory
        /// </summary>
        private LazyCreateItem()
        {
            Clear();
        }

        /// <summary>
        /// Löscht alle bis jetzt gecachte Objekte
        /// </summary>
        public void Clear()
        {
            Lookup = new Dictionary<Key, Item>();
        }

        /// <summary>
        /// Legt ein neues item, wenn ein item mit dem gleichen Schlüssel nicht bereits existiert.
        /// Gibt entweder das existierende Item oder das neue mit dem passenden Schlüssel zurück.
        /// </summary>
        /// <param name="key">Schlüssel des Items</param>
        /// <param name="data">Payload-Data des Items</param>
        /// <returns>Ein Item, neu oder bereits vorher existierend</returns>
        public Item LazyCreate(Key key, Data data)
        {
            Item item = null;
            if (Lookup.ContainsKey(key))
            {
                item = Lookup[key];
            }
            else
            {
                if (Query != null)
                {
                    item = Query(key);
                }

                if (item == null)
                {
                    item = factory(key, data);
                }

                Lookup.Add(key, item);
            }

            return item;
        }

        /// <summary>
        /// Holt den vorher hinzugefügtes item mit dem gleichen Schlüssel.
        /// </summary>
        /// <param name="key">Schlüssel des Items</param>
        /// <returns>
        /// Ein Item, nur wenn bereits vorher existierend
        /// </returns>
        public Item GetIfExist(Key key)
        {
            Item item = null;
            if (Lookup.ContainsKey(key))
            {
                item = Lookup[key];
            }

            return item;
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The item removed from the intern dictionary</returns>
        public Item RemoveItem(Key key)
        {
            var item = GetIfExist(key);
            if (item != null)
            {
                Lookup.Remove(key);
            }

            return item;
        }
    }
}
