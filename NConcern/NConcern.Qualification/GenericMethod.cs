namespace NConcern.Qualification
{
    public class GenericMethod
    {
        public int Method<T>(int x, T y)
        {
            return Interception.Sequence();
        }
    }
}