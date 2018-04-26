using CNeptune;

namespace NConcern.Qualification
{
    [Neptune(false)]
    public class NoNeptuneType
    {
        virtual public int Method(int x, int y)
        {
            return Interception.Sequence();
        }
    }
}