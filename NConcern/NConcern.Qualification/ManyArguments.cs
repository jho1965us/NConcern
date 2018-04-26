namespace NConcern.Qualification
{
    public class ManyArguments
    {
        static public int Static(int a0, int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9)
        {
            Interception.MethodArguments = new object[] {a0, a1, a2, a3, a4, a5, a6, a7, a8, a9};
            return Interception.Sequence();
        }
        public int Instance(int a0, int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9)
        {
            Interception.MethodArguments = new object[] {a0, a1, a2, a3, a4, a5, a6, a7, a8, a9};
            return Interception.Sequence();
        }
    }
}