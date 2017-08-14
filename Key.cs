namespace Gimme
{
    using System;

    internal class Key
    {
        //private readonly string _name;
        private readonly Type _type;

        public Key(Type type, string name = null)
        {
            _type = type;
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() && Equals((Key)obj);
        }

        public bool IsOfType(Type t)
        {
            return t.IsAssignableFrom(_type);
        }

        public string Name { get; private set; }

        /// <summary>
        /// Gets a string description of the type stored. The
        /// description is in the format of [name]:type.
        /// </summary>
        public string Description
        {
            get { return string.Format("[{0}]:{1}", Name, _type); }
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _type.GetHashCode() * 397 ^ Name.GetHashCode();
        }

        private bool Equals(Key other)
        {
            return _type == other._type && string.Equals(Name, other.Name);
        }
    }
}
