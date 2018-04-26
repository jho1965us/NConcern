using System;
using System.ServiceModel;

namespace NConcern.Example.Generic
{
    [ServiceContract]
    public class GenericCalculator<T>
    {
        [OperationContract]
        public T Add(T a, T b)
        {
            Console.WriteLine("Adding");
            dynamic _a = a;
            dynamic _b = b;
            var _result = _a + _b;
            Console.WriteLine("Added");
            return _result;
        }

        [OperationContract]
        public T Divide(T a, T b)
        {
            Console.WriteLine("Dividing");
            dynamic _a = a;
            dynamic _b = b;
            var _result = _a / _b;
            Console.WriteLine("Divided");
            return _result;
        }
    }
}
