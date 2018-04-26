using CNeptune;

namespace NConcern.Qualification
{
    public class NoNeptuneMethod
    {
        [Neptune(false)]
        virtual public int Method(int x, int y)
        {
            return Interception.Sequence();
        }
    }
}