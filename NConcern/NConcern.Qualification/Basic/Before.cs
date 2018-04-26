using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace NConcern.Qualification.Basic
{
    [TestClass]
    public class Before
    {
        private class Interceptor : IAspect
        {
            public IEnumerable<IAdvice> Advise(MethodBase method)
            {
                yield return Advice.Basic.Before(() =>
                {
                    Interception.Sequence();
                    Interception.Done = true;
                });
            }

            public class Parameterized : IAspect
            {
                public IEnumerable<IAdvice> Advise(MethodBase method)
                {
                    yield return Advice.Basic.Before((_Instance, _Arguments) =>
                    {
                        Interception.Sequence();
                        Interception.Done = true;
                        Interception.Instance = _Instance;
                        Interception.Arguments = _Arguments;
                    });
                }
            }
        }

        [TestMethod]
        public void BasicBeforeStaticMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata.Method(() => Static.Method(Argument<int>.Value, Argument<int>.Value));
                Interception.Initialize();
                Static.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Interception.Initialize();
                var _return = Static.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor>(_method);
                Interception.Initialize();
                Static.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeStaticMethodStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata.Method(() => Static.Method(Argument<int>.Value, Argument<int>.Value));
                Interception.Initialize();
                Static.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                var _return = Static.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Assert.AreEqual(Interception.Instance, null);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], 3);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                Static.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeSealedMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<Sealed>.Method(_Instance => _Instance.Method(Argument<int>.Value, Argument<int>.Value));
                var _instance = new Sealed();
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor>(_method);
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeSealedMethodStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<Sealed>.Method(_Instance => _Instance.Method(Argument<int>.Value, Argument<int>.Value));
                var _instance = new Sealed();
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Assert.AreEqual(Interception.Instance, _instance);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], 3);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeVirtualMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<Virtual>.Method(_Instance => _Instance.Method(Argument<int>.Value, Argument<int>.Value));
                var _instance = new Virtual();
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor>(_method);
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeVirtualMethodStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<Virtual>.Method(_Virtual => _Virtual.Method(Argument<int>.Value, Argument<int>.Value));
                var _instance = new Virtual();
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Assert.AreEqual(Interception.Instance, _instance);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], 3);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeOverridenMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<Overriden>.Method(_Overriden => _Overriden.Method(Argument<int>.Value, Argument<int>.Value));
                var _instance = new Overriden();
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor>(_method);
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeOverridenMethodStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<Overriden>.Method(_Overriden => _Overriden.Method(Argument<int>.Value, Argument<int>.Value));
                var _instance = new Overriden();
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, 3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Assert.AreEqual(Interception.Instance, _instance);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], 3);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _instance.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeRefParameterMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<RefParameter>.Method(_Overriden => _Overriden.Method(Argument<int>.Value, ref Argument<int>.Value));
                var _instance = new RefParameter();
                Interception.Initialize();
                _instance.Method(2, ref Interception.Value3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, ref Interception.Value3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor>(_method);
                Interception.Initialize();
                _instance.Method(2, ref Interception.Value3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeRefParameterMethodStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<RefParameter>.Method(_Overriden => _Overriden.Method(Argument<int>.Value, ref Argument<int>.Value));
                var _instance = new RefParameter();
                Interception.Initialize();
                _instance.Method(2, ref Interception.Value3);
                Assert.AreEqual(Interception.Value3, -3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                var _return = _instance.Method(2, ref Interception.Value3);
                Assert.AreEqual(Interception.Value3, -3);
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Assert.AreEqual(Interception.Instance, _instance);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], 3);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _instance.Method(2, ref Interception.Value3);
                Assert.AreEqual(Interception.Value3, -3);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeManyArgumentsInstanceMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<ManyArguments>.Method(_ManyArguments => _ManyArguments.Instance(0, 1, 2, 3, 4, 5, 6, 7, 8, 9));
                var _values = new object[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
                var _manyArguments = new ManyArguments();
                Interception.Initialize();
                var _return = _manyArguments.Instance(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Assert.IsTrue(Interception.MethodArguments.SequenceEqual(_values));
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _return = _manyArguments.Instance(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Assert.IsTrue(Interception.Arguments.SequenceEqual(_values));
                Assert.IsTrue(Interception.MethodArguments.SequenceEqual(_values));
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _manyArguments.Instance(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        public void BasicBeforeManyArgumentsStaticMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<ManyArguments>.Method(_ManyArguments => ManyArguments.Static(0, 1, 2, 3, 4, 5, 6, 7, 8, 9));
                var _values = new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                Interception.Initialize();
                var _return = ManyArguments.Static(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Assert.IsTrue(Interception.MethodArguments.SequenceEqual(_values));
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                _return = ManyArguments.Static(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Assert.IsTrue(Interception.Arguments.SequenceEqual(_values));
                Assert.IsTrue(Interception.MethodArguments.SequenceEqual(_values));
                Assert.AreEqual(_return, 1);
                Assert.AreEqual(Interception.Done, true);
                Aspect.Release<Before.Interceptor.Parameterized>(_method);
                Interception.Initialize();
                ManyArguments.Static(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Assert.AreEqual(Interception.Done, false);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.NotSupportedException))]
        public void BasicBeforeNoNeptuneTypeMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<NoNeptuneType>.Method(_NoNeptuneType => _NoNeptuneType.Method(Argument<int>.Value, Argument<int>.Value));
                var _noNeptuneType = new NoNeptuneType();
                Interception.Initialize();
                _noNeptuneType.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Assert.Fail();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.NotSupportedException))]
        public void BasicBeforeNoNeptuneMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<NoNeptuneMethod>.Method(_NoNeptuneMethod => _NoNeptuneMethod.Method(Argument<int>.Value, Argument<int>.Value));
                var _noNeptuneMethod = new NoNeptuneMethod();
                Interception.Initialize();
                _noNeptuneMethod.Method(2, 3);
                Assert.AreEqual(Interception.Done, false);
                Aspect.Weave<Before.Interceptor>(_method);
                Assert.Fail();
            }
        }

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericType()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericType<int>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericType<int>(_method, true);
                BasicBeforeMethodInGenericType<double>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinition()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericType<>).GetMethod(nameof(GenericType<Dummy>.Method));
                BasicBeforeMethodInGenericType<int>(_method, true);
                BasicBeforeMethodInGenericType<double>(_method, true);
            }
        }

        private static void BasicBeforeMethodInGenericType<T>(MethodInfo _method, bool expectInterception)
        {
            var _genericType = new GenericType<T>();
            Interception.Initialize();
            var _3 = (T)Convert.ChangeType(3, typeof(T));
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T).Name);
            Aspect.Weave<Before.Interceptor>(_method);
            Interception.Initialize();
            var _return = _genericType.Method(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0, typeof(T).Name);
            Assert.AreEqual(Interception.Done, expectInterception, typeof(T).Name);
            Aspect.Release<Before.Interceptor>(_method);
            Interception.Initialize();
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T).Name);
        }
    }
}
