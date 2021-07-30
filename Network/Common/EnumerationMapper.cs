namespace Common
{
    using System.Collections;
    using System.Collections.Generic;

    public class EnumerationMapper<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        #region # Fields #

        private readonly Dictionary<T1, T2> directTransformation;
        private readonly Dictionary<T2, T1> reverseTransformation;
        private readonly T1 t1Unknown;
        private readonly T2 t2Unknown;

        #endregion # Fields #

        #region # Constructors #

        public EnumerationMapper()
        {
            this.directTransformation = new Dictionary<T1, T2>();
            this.reverseTransformation = new Dictionary<T2, T1>();
            this.t1Unknown = default(T1);
            this.t2Unknown = default(T2);
        }

        public EnumerationMapper(T1 u1, T2 u2)
        {
            this.directTransformation = new Dictionary<T1, T2>();
            this.reverseTransformation = new Dictionary<T2, T1>();
            this.t1Unknown = u1;
            this.t2Unknown = u2;
        }

        #endregion # Constructors #

        #region # Methods #

        public T2 DirectTransform(T1 t1)
        {
            return Transform(t1);
        }

        public T1 ReverseTransform(T2 t2)
        {
            return Transform(t2);
        }

        public T2 Transform(T1 t1)
        {
            T2 temp;
            return this.directTransformation.TryGetValue(t1, out temp) ? temp : t2Unknown;
        }

        public T1 Transform(T2 t2)
        {
            T1 temp;
            return this.reverseTransformation.TryGetValue(t2, out temp) ? temp : t1Unknown;
        }

        public T2 this[T1 t1] { get { return Transform(t1); } }

        public T1 this[T2 t2] { get { return Transform(t2); } }

        public void Add(T1 t1, T2 t2)
        {
            AddDirectTransform(t1, t2, true);
        }

        public void Add(T1 t1, T2 t2, bool reverse)
        {
            AddDirectTransform(t1, t2, reverse);
        }

        public void AddDirectTransform(T1 t1, T2 t2, bool reverse)
        {
            this.directTransformation.Add(t1, t2);
            if (reverse)
                this.reverseTransformation.Add(t2, t1);
        }

        public void AddReverseTransform(T2 t2, T1 t1, bool direct)
        {
            this.reverseTransformation.Add(t2, t1);
            if (direct)
                this.directTransformation.Add(t1, t2);
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return directTransformation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion # Methods #
    }
}
