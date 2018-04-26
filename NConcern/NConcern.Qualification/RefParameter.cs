namespace NConcern.Qualification
{
    public class RefParameter
    {
        virtual public int Method(int x, ref int y)
        {
            y = -y;
            return Interception.Sequence();
        }
    }
}