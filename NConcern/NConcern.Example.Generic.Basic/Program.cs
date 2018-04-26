using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using NConcern.Example.Logging;

namespace NConcern.Example.Generic.Basic
{
    [EnableGenericWeaving]
    public class Logging : IAspect
    {
        public IEnumerable<IAdvice> Advise(MethodBase method)
        {
            yield return Advice.Basic.Around(new Func<object, object[], Func<object>, object>((_Instance, _Arguments, _Body) =>
            {
                var _trace = new Trace(method, _Arguments.Select(_Argument => _Argument.ToString()).ToArray());
                try
                {
                    var _return = _Body();
                    _trace.Dispose(_return.ToString());
                    return _return;
                }
                catch (Exception exception)
                {
                    _trace.Dispose(exception);
                    throw;
                }
            }));
        }
    }

    static public class Program
    {
        static void Main(string[] args)
        {
            var type = args.Length == 0 ? "int" : args[0];
            Object _return; 
            switch (type)
            {
                case "int":
                    _return = Main<int>();
                    break;
                case "double":
                    _return = Main<double>();
                    break;
            }
        }

        private static object Main<T>()
        { 
            //define a joinpoint
            var _operationContractJoinpoint =
                new Func<MethodBase, bool>(_Method => _Method.IsDefined(typeof(OperationContractAttribute), true));

            //instantiate a calculator
            var _calculator = new GenericCalculator<T>();

            Console.WriteLine("Weaving");
            //weave logging for all operation contract
            Aspect.Weave<Logging>(_operationContractJoinpoint);
            Console.WriteLine("Weaved");


            //invoke an operation contract (logging is enabled)
            dynamic _value15 = 15;
            dynamic _value3 = 3;
            var _return = _calculator.Divide(_value15, _value3);

            Console.WriteLine("Releasing");
            //release logging for all operation contract
            Aspect.Release<Logging>(_operationContractJoinpoint);
            Console.WriteLine("Released");

            //invoke an operation contract (logging is disabled)
            _return = _calculator.Divide(_value15, _value3);

            Console.WriteLine("Weaving");
            //enable back logging aspect.
            Aspect.Weave<Logging>(_operationContractJoinpoint);
            Console.WriteLine("Weaved");

            //invoke an operation with an exception (divide by zero)
            try
            {
                dynamic _value0 = 0;
                _return = _calculator.Divide(_value15, _value0);
            }
            catch
            {
            }

            return _return;
        }
    }
}
