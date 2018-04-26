namespace NConcern.Qualification
{
    public class GenericType<T>
    {
        public int Method(int x, T y)
        {
            return Interception.Sequence();
        }
    }
}