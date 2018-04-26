namespace NConcern.Qualification
{
    public class GenericMethodInGenericType<T1>
    {
        /// <summary>
        /// Number of generic parameters for method1 should be same as type but differ from that of method2 to catch more error.
        /// </summary>
        public int Method1<T2>(int x, T1 y)
        {
            return Interception.Sequence();
        }
        /// <summary>
        /// Number of generic parameters for type should diff from that of method1 and type to catch more error
        /// also we use the type parameter instead.
        /// </summary>
        public int Method2<T2, T3>(int x, T3 y)
        {
            return Interception.Sequence();
        }
    }
}