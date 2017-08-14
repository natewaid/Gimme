namespace Gimme
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The collection of type and object mapping
    /// </summary>
    public static class Collection
    {
        private static Dictionary<Key, Func<object>> _known;

        static Collection()
        {
            _known = new Dictionary<Key, Func<object>>();
        }

        /// <summary>
        /// empties the collection of all known instances
        /// </summary>
        public static void Empty()
        {
            _known = new Dictionary<Key, Func<object>>();
        }

        /// <summary>
        /// returns a value indicated if the requested type exists in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool HasA<T>(string name = null)
        {
            var key = new Key(typeof(T), name);
            return _known.ContainsKey(key);
        }

        /// <summary>
        /// Locates a given type in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Locate<T>()
        {
            return Locate<T>(null);
        }

        /// <summary>
        /// Locates a given type in the collection that has been labeled with the given label
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Locate<T>(string name)
        {
            var key = new Key(typeof(T), name);
            return (T)(LocateByKey(key) ?? default(T));
        }

        /// <summary>
        /// Locates an object by Type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object LocateByType(Type t, string name = null)
        {
            var key = new Key(t, name);
            return LocateByKey(key);
        }

        /// <summary>
        /// Registers a type using a given concrete implementation and optionally registers
        /// is with the provided label.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="implementation"></param>
        /// <param name="name"></param>
        public static void Register<T>(T implementation, string name = null)
        {
            var key = new Key(typeof(T), name);
            if (_known.ContainsKey(key))
            {
                throw new ArgumentException("The type is already registered");
            }

            _known.Add(key, () => implementation);
        }

        /// <summary>
        /// Registers a Type to provide the concrete implementation using the provided arguments as constructor arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="constructorArguments"></param>
        public static void Register<T, TImpl>(params object[] constructorArguments) where TImpl : T
        {
            RegisterWithName<T, TImpl>(null, constructorArguments);
        }

        /// <summary>
        /// Returns a new instance of the given type each time one is requested. It uses the same constructor
        /// arguments for each instance. The factory produces multiple objects but the standard register
        /// always produces the same object.
        /// </summary>
        /// <param name="constructorArguments"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <exception cref="ArgumentException">If the type and name are already registered in the collection</exception>
        /// <exception cref="NotSupportedException">If the TImpl cannot be constructed using the given contructorArguments</exception>
        public static void RegisterAsFactory<T, TImpl>(params object[] constructorArguments) where TImpl : T
        {
            RegisterAsFactoryWithName<T, TImpl>(null, constructorArguments);
        }

        /// <summary>
        /// Returns a new instance of the given type each time one is requested by the provided name. It uses the same constructor
        /// arguments for each instance. The factory produces multiple objects but the standard register
        /// always produces the same object.
        /// </summary>
        /// <param name="constructorArguments"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <exception cref="ArgumentException">If the type and name are already registered in the collection</exception>
        /// <exception cref="NotSupportedException">If the TImpl cannot be constructed using the given contructorArguments</exception>
        public static void RegisterAsFactoryWithName<T, TImpl>(string name, params object[] constructorArguments) where TImpl : T
        {
            var key = new Key(typeof(T), name);

            if (_known.ContainsKey(key))
            {
                throw new ArgumentException("The type is already registerd");
            }

            var t = typeof(TImpl);
            var cstr = t.GetConstructors()
                .FirstOrDefault(k => k.GetParameters().Count() == constructorArguments.Count());

            if (cstr == null)
            {
                throw new NotSupportedException("could not find a constructor for " + t + " that accepts " +
                                                constructorArguments.Count() + " arguments.");
            }

            // ReSharper disable once ConvertToLambdaExpression
            _known.Add(key, () => (T)cstr.Invoke(constructorArguments));
        }


        /// <summary>
        /// Registers a type to provide the concrete implementation using the provided arguments as constructor arguments and labels
        /// the instance using the provided name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="name"></param>
        /// <param name="constructorArguments"></param>
        public static void RegisterWithName<T, TImpl>(string name, params object[] constructorArguments) where TImpl : T
        {
            var key = new Key(typeof(T), name);

            if (_known.ContainsKey(key))
            {
                throw new ArgumentException("The type is already registered");
            }

            var t = typeof(TImpl);
            var cstr = t.GetConstructors().FirstOrDefault(k => k.GetParameters().Count() == constructorArguments.Count());

            if (cstr == null)
            {
                throw new NotSupportedException(string.Format("could not find a constructor for {0} that accepts {1} arguments ", t, constructorArguments.Length));
            }

            // construct object before placing into known collection
            // so that the same object will be returned each time
            // instead of constructed anew.
            var obj = (T)cstr.Invoke(constructorArguments);
            _known.Add(key, () => obj);
        }

        /// <summary>
        /// removes the given type (and optionally label) from the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        public static void Remove<T>(string name = null)
        {
            var keys = _known.Keys.Where(k => k.Equals(new Key(typeof(T), name))).ToList();
            foreach (var key in keys)
            {
                _known.Remove(key);
            }
        }

        private static object LocateByKey(Key key)
        {
            object obj = null;

            if (_known.ContainsKey(key))
            {
                obj = _known[key]();
            }

            return obj;
        }

        /// <summary>
        /// Locates all items in the collection that are of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, T>> LocateAll<T>()
        {
            return _known
                .Where(k => k.Key.IsOfType(typeof(T)))
                .Select(k => new KeyValuePair<string, T>(k.Key.Name, (T)k.Value()));
        }

        /// <summary>
        /// Returns a collection of key value pairs where the key is the Key.Description of
        /// the item in the Collection and the value is the string representation of the Type
        /// of that Key.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> AllRegisteredTypes()
        {
            return _known
                .Select(k =>
                    new KeyValuePair<string, string>(
                        k.Key.Description,
                        k.Value().GetType().ToString()
                        ));
        }
    }
}
