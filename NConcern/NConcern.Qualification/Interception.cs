using System;
using CNeptune;

namespace NConcern.Qualification
{
    static internal class Interception
    {
        static public readonly object Handle = new object();
        static private int m_Sequence;

        static public int Sequence()
        {
            return Interception.m_Sequence++;
        }

        static public bool Done;
        static public object Instance;
        static public object[] Arguments;
        static public object[] MethodArguments;
        static public object Return;
        static public Exception Exception;
        static public int Value3;

        static public void Initialize()
        {
            Interception.m_Sequence = 0;
            Interception.Done = false;
            Interception.Instance = null;
            Interception.Arguments = null;
            Interception.MethodArguments = null;
            Interception.Return = null;
            Interception.Exception = null;
            Interception.Value3 = 3;
        }
    }

    [Neptune(false)] // might interfere with the test
    static internal class Interception<TTag>
    {
        static private int m_Sequence;

        static public int Sequence()
        {
            return Interception<TTag>.m_Sequence++;
        }

        static public bool Done;
        static public object Instance;
        static public object[] Arguments;
        static public object Return;
        static public Exception Exception;

        static public void Initialize()
        {
            Interception<TTag>.m_Sequence = 0;
            Interception<TTag>.Done = false;
            Interception<TTag>.Instance = null;
            Interception<TTag>.Arguments = null;
            Interception<TTag>.Return = null;
            Interception<TTag>.Exception = null;
        }
    }
}
