namespace NConcern.Qualification
{
    public class TaggedGenericType<T, TTag>
    {
        public int Method(int x, T y)
        {
            return Interception<TTag>.Sequence();
        }
    }
}