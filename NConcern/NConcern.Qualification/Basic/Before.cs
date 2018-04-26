using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using CNeptune;

namespace NConcern.Qualification.Basic
{
    [TestClass]
    [Neptune(false)]
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

        private class Interceptor<TTag> : IAspect
        {
            public IEnumerable<IAdvice> Advise(MethodBase method)
            {
                yield return Advice.Basic.Before((_Instance, _Arguments) =>
                {
                    Interception<TTag>.Sequence();
                    Interception<TTag>.Done = true;
                });
            }

            public class WithSideEffect : IAspect
            {
                public IEnumerable<IAdvice> Advise(MethodBase method)
                {
                    SideEffectInAdvice(method);
                    yield return Advice.Basic.Before((_Instance, _Arguments) =>
                    {
                        Interception<TTag>.Sequence();
                        Interception<TTag>.Done = true;
                    });
                }
                public virtual void SideEffectInAdvice(MethodBase method) { }
            }
        }

        private class TagPair<TTag0,TTag1>
        { }

        private class ImplementTagOnlyIsedOnce<TTag>
        {
            static public bool Used;
        }

        static private void AssertTagOnlyUsedOnce<TTag>()
        {
            Assert.AreEqual(ImplementTagOnlyIsedOnce<TTag>.Used, false);
            ImplementTagOnlyIsedOnce<TTag>.Used = true;
        }


        [TestInitialize]
        public void Initialize()
        {
            lock (Interception.Handle)
            {
                foreach (var _method in Aspect.Lookup())
                {
                    Aspect.Release(_method);
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

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericType<int>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeStateful<int>(_method, true);
                BasicBeforeMethodInGenericTypeStateful<double>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinitionStateful()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericType<>).GetMethod(nameof(GenericType<Dummy>.Method));
                BasicBeforeMethodInGenericTypeStateful<int>(_method, true);
                BasicBeforeMethodInGenericTypeStateful<double>(_method, true);
            }
        }

        private static void BasicBeforeMethodInGenericTypeStateful<T>(MethodInfo method, bool expectInterception)
        {
            var _genericType = new GenericType<T>();
            Interception.Initialize();
            var _3 = (T)Convert.ChangeType(3, typeof(T));
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception.Done, false);
            Aspect.Weave<Before.Interceptor.Parameterized>(method);
            Interception.Initialize();
            var _return = _genericType.Method(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0);
            Assert.AreEqual(Interception.Done, expectInterception);
            if (expectInterception)
            {
                Assert.AreEqual(Interception.Instance, _genericType);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], _3);
            }
            Aspect.Release<Before.Interceptor.Parameterized>(method);
            Interception.Initialize();
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception.Done, false);
        }

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypePattern()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericType<int>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                var _joinpoint = new Func<MethodBase, bool>(_Method => _Method == _method);
                BasicBeforeMethodInGenericTypePattern<int>(_joinpoint, true);
                BasicBeforeMethodInGenericTypePattern<double>(_joinpoint, false);
            }
        }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinitionPattern()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericType<>).GetMethod(nameof(GenericType<Dummy>.Method));
                var _joinpoint = new Func<MethodBase, bool>(_Method => _Method == _method);
                BasicBeforeMethodInGenericTypePattern<int>(_joinpoint, true);
                BasicBeforeMethodInGenericTypePattern<double>(_joinpoint, true);
            }
        }

        private static void BasicBeforeMethodInGenericTypePattern<T>(Func<MethodBase, bool> pattern, bool expectInterception)
        {
            var _genericType = new GenericType<T>();
            Interception.Initialize();
            var _3 = (T)Convert.ChangeType(3, typeof(T));
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception.Done, false);
            Aspect.Weave<Before.Interceptor>(pattern);
            Interception.Initialize();
            var _return = _genericType.Method(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0);
            Assert.AreEqual(Interception.Done, expectInterception);
            Aspect.Release<Before.Interceptor>(pattern);
            Interception.Initialize();
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception.Done, false);
        }

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeEarlyWeaveMatchFirst()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<TaggedGenericType<int, TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveMatchFirst>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeEarlyWeave<int, TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveMatchFirst>(_method, true);
                BasicBeforeMethodInGenericTypeEarlyWeave<double, TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveMatchFirst>(_method, false);
            }
        }

        class TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveMatchFirst { }

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeEarlyWeaveOtherFirst()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<TaggedGenericType<int, TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveOtherFirst>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeEarlyWeave<double, TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveOtherFirst>(_method, false);
                BasicBeforeMethodInGenericTypeEarlyWeave<int, TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveOtherFirst>(_method, true);
            }
        }

        class TAG_BasicBeforeMethodInConstructedGenericTypeEarlyWeaveOtherFirst { }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinitionEarlyWeave()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(TaggedGenericType<,>).GetMethod(nameof(GenericType<Dummy>.Method));
                BasicBeforeMethodInGenericTypeEarlyWeave<int, TAG_BasicBeforeMethodInGenericTypeDefinitionEarlyWeave>(_method, true);
                BasicBeforeMethodInGenericTypeEarlyWeave<double, TAG_BasicBeforeMethodInGenericTypeDefinitionEarlyWeave>(_method, true);
            }
        }

        class TAG_BasicBeforeMethodInGenericTypeDefinitionEarlyWeave { }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TTag">Should be unique for each test case to ensure type not weaved before.</typeparam>
        /// <param name="_method"></param>
        /// <param name="expectInterception"></param>
        private static void BasicBeforeMethodInGenericTypeEarlyWeave<T,TTag>(MethodInfo _method, bool expectInterception) where T : new ()
        {
            AssertTagOnlyUsedOnce<TagPair<T, TTag>>();
            var _3 = (T)Convert.ChangeType(3, typeof(T));
            Aspect.Weave<Before.Interceptor<TTag>>(_method);
            Interception<TTag>.Initialize();
            var _genericType = new TaggedGenericType<T, TTag>();
            var _return = _genericType.Method(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0, typeof(T).Name);
            Assert.AreEqual(Interception<TTag>.Done, expectInterception, typeof(T).Name);
            Aspect.Release<Before.Interceptor<TTag>>(_method);
            Interception<TTag>.Initialize();
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception<TTag>.Done, false, typeof(T).Name);
        }

#region SideEffect tests

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeave()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<TaggedGenericType<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeave>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeave>(_method, true, SideEffectLocation.Weave, SideEffectType.Normal);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeave>(_method, false, SideEffectLocation.Weave, SideEffectType.Normal);
            }
        }

        class TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeave { }

        /// <summary>
        /// This test is not really much different from <see cref="BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeave"/>
        /// </summary>
        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvoke()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<TaggedGenericType<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvoke>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvoke>(_method, true, SideEffectLocation.Invoke, SideEffectType.Normal);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvoke>(_method, false, SideEffectLocation.Invoke, SideEffectType.Normal);
            }
        }

        class TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvoke { }

        [TestMethod]
        // todo Jens not really sure if this can not happen. 
        public void BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeave()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(TaggedGenericType<,>).GetMethod(nameof(TaggedGenericType<Dummy, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeave>.Method));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeave>(_method, true, SideEffectLocation.Weave, SideEffectType.Normal);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeave>(_method, true, SideEffectLocation.Weave, SideEffectType.Normal);
            }
        }

        class TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeave { }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokde()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(TaggedGenericType<,>).GetMethod(nameof(TaggedGenericType<Dummy, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokde>.Method));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokde>(_method, true, SideEffectLocation.Invoke, SideEffectType.Normal);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokde>(_method, true, SideEffectLocation.Invoke, SideEffectType.Normal);
            }
        }

        class TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokde { }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeSameType()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(TaggedGenericType<,>).GetMethod(nameof(TaggedGenericType<Dummy, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeSameType>.Method));
                try
                {
                    BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeSameType>(_method, true, SideEffectLocation.Invoke, SideEffectType.SameType);

                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(TypeInitializationException));
                    Assert.IsInstanceOfType(e.InnerException, typeof(NotImplementedException));
                    Assert.AreEqual(e.InnerException.Message, "Recursively modifying the weaving of a method not implemented");
                }
            }
        }

        class TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeSameType { }

#region Deadlock tests

        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeaveThreaded()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<TaggedGenericType<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeaveThreaded>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeaveThreaded>(_method, true, SideEffectLocation.Weave, SideEffectType.Threaded);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeaveThreaded>(_method, false, SideEffectLocation.Weave, SideEffectType.Threaded);
            }
        }

        class TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeaveThreaded { }

        /// <summary>
        /// This test is not really much different from <see cref="BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringWeaveThreaded"/>
        /// </summary>
        [TestMethod]
        public void BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvokeThreaded()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<TaggedGenericType<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvokeThreaded>>.Method(_GenericType => _GenericType.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvokeThreaded>(_method, true, SideEffectLocation.Invoke, SideEffectType.Threaded);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvokeThreaded>(_method, false, SideEffectLocation.Invoke, SideEffectType.Threaded);
            }
        }

        class TAG_BasicBeforeMethodInConstructedGenericTypeWithSideEffectInAdviceDuringInvokeThreaded { }

        [TestMethod]
        // todo Jens not really sure if this can not happen. 
        public void BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeaveThreaded()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(TaggedGenericType<,>).GetMethod(nameof(TaggedGenericType<Dummy, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeaveThreaded>.Method));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeaveThreaded>(_method, true, SideEffectLocation.Weave, SideEffectType.Threaded);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeaveThreaded>(_method, true, SideEffectLocation.Weave, SideEffectType.Threaded);
            }
        }

        class TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringWeaveThreaded { }

        [TestMethod]
        public void BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeThreaded()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(TaggedGenericType<,>).GetMethod(nameof(TaggedGenericType<Dummy, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeThreaded>.Method));
                BasicBeforeMethodInGenericTypeWithSideEffect<int, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeThreaded>(_method, true, SideEffectLocation.Invoke, SideEffectType.Threaded);
                BasicBeforeMethodInGenericTypeWithSideEffect<double, TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeThreaded>(_method, true, SideEffectLocation.Invoke, SideEffectType.Threaded);
            }
        }

        class TAG_BasicBeforeMethodInGenericTypeDefinitionWithSideEffectInAdviceDuringInvokdeThreaded { }

#endregion

        /// <summary>
        /// Since generic methods and methods in generic type are some times advice upon first invocation there are risk of 
        /// aspects triggering recursion if they use generic class that them self had been weaved.
        /// This class force such a weaving.
        /// In a multi threaded application this also implies the issue of thread safe methods.
        /// </summary>
        /// <typeparam name="TTag1">Use to make a new set of static variables.</typeparam>
        class SideEffect<TTag0,TTag1> : ISideEffect
        {
            private readonly bool m_expectInterception;
            static public bool Enabled { get; protected set; }
            static private bool m_hadSideEffect;
            private readonly SideEffectGenericType<float> m_SideEffectGenericType = new SideEffectGenericType<float>();
            static protected SideEffect<TTag0, TTag1> Instance;
            public MethodInfo WeavedMethod;

            protected virtual void DoSideEffect()
            {
                m_hadSideEffect = true;
                var _return = InvokeSideEffectMethod();
                Assert.AreEqual(_return, 1, typeof(TAG_SideEffect).Name);
                Assert.AreEqual(Interception<TAG_SideEffect>.Done, true, typeof(TAG_SideEffect).Name);
            }

            protected virtual int InvokeSideEffectMethod()
            {
                return m_SideEffectGenericType.SideEffectMethod(2, 3);
            }

            [Neptune(true)]
            private class SideEffectGenericType<T1>
            {
                public int SideEffectMethod(int x, T1 y)
                {
                    return Interception<TAG_SideEffect>.Sequence();
                }
            }

            public SideEffect(bool expectInterception)
            {
                this.m_expectInterception = expectInterception;
                SideEffect<TTag0, TTag1>.Enabled = false;
                SideEffect<TTag0, TTag1>.Instance = this;
            }

            public virtual void Weave()
            {
                WeavedMethod = Metadata.Method(() => m_SideEffectGenericType.SideEffectMethod(Argument<int>.Value, Argument<float>.Value));
                Interception<TAG_SideEffect>.Initialize();
                Weave(WeavedMethod);
                Assert.AreEqual(Interception<TAG_SideEffect>.Done, false);
                Enabled = true;
            }

            protected virtual void Weave(MethodInfo _method)
            {
                Aspect.Weave<Interceptor<TAG_SideEffect>>(_method);
            }

            public virtual void Validate()
            {
                if (Enabled)
                {
                    Assert.AreEqual(m_hadSideEffect, m_expectInterception);
                    Assert.AreEqual(Interception<TAG_SideEffect>.Done, m_expectInterception);
                    Enabled = false;
                }
            }

            public class TAG_SideEffect { }

            public class Interceptor : Interceptor<TTag1>.WithSideEffect
            {
                public override void SideEffectInAdvice(MethodBase method)
                {
                    if (SideEffect<TTag0, TTag1>.Enabled)
                    {
                        // only do the side effect for the current call to BasicBeforeMethodInGenericTypeWithSideEffect
                        var _genericArguments = method.DeclaringType.GetGenericArguments();
                        if (_genericArguments[0] == typeof(TTag0) && _genericArguments[1] == typeof(TTag1))
                        {
                            SideEffect<TTag0, TTag1>.Instance.DoSideEffect();
                        }
                    }
                }
            }
        }

        class ThreadedSideEffect<TTag0, TTag1> : SideEffect<TTag0, TTag1>
        {
            private Thread m_sideEffectThread;
            /// <summary>
            /// The only purpose of this event is to force active thread to change
            /// </summary>
            static private readonly AutoResetEvent m_sideEffectThreadEvent = new AutoResetEvent(false);
            /// <summary>
            /// The only purpose of this event is to force active thread to change
            /// </summary>
            static private readonly AutoResetEvent m_mainThreadEvent = new AutoResetEvent(false);

            static private new ThreadedSideEffect<TTag0, TTag1> Instance => (ThreadedSideEffect<TTag0, TTag1>) SideEffect<TTag0, TTag1>.Instance;

            public ThreadedSideEffect(bool expectInterception) : base(expectInterception)
            {
            }

            protected override void Weave(MethodInfo _method)
            {
                // use pattern weaving to force late weaving i.e. weaving from the side effect thread
                Aspect.Weave<ThreadedSideEffectInterceptor>(_Method => _Method == _method);
            }

            protected override void DoSideEffect()
            {
                // 1.
                Assert.AreEqual(this.m_sideEffectThread, null);
                this.m_sideEffectThread = new Thread(this.SideEffectThread);
                this.m_sideEffectThread.Name = nameof(this.SideEffectThread);
                this.m_sideEffectThread.Start();
                ThreadedSideEffect<TTag0, TTag1>.m_mainThreadEvent.WaitOne();
                // 4.
            }

            private void SideEffectThread()
            {
                // 2.
                base.DoSideEffect();
                // 7.
                m_mainThreadEvent.Set();
            }

            public override void Validate()
            {
                // 5.
                if (Enabled)
                {
                    if (this.m_sideEffectThread != null)
                    {
                        // Continue SideEffectThread 
                        ThreadedSideEffect<TTag0, TTag1>.m_sideEffectThreadEvent.Set();
                        ThreadedSideEffect<TTag0, TTag1>.m_mainThreadEvent.WaitOne();
                        // 8.
                    }
                    base.Validate();
                }
            }

            private static void MainThreadContinue()
            {
            }

            public class ThreadedSideEffectInterceptor : Interceptor<TAG_SideEffect>.WithSideEffect
            {
                public override void SideEffectInAdvice(MethodBase method)
                {
                    // 3.
                    Assert.AreEqual(Thread.CurrentThread, ThreadedSideEffect<TTag0, TTag1>.Instance.m_sideEffectThread);
                    //if (method == SideEffect<TTag0, TTag1>.Instance.WeavedMethod)
                    {
                        // Continue MainThread
                        ThreadedSideEffect<TTag0, TTag1>.m_mainThreadEvent.Set();
                        ThreadedSideEffect<TTag0, TTag1>.m_sideEffectThreadEvent.WaitOne(1000);
                        // 6.
                    }
                }
            }

        }

        class SameTypeSideEffect<TTag0, TTag1> : SideEffect<TTag0, TTag1>
        {
            private readonly TaggedGenericType<TTag0, TTag1> m_SideEffectGenericType = new TaggedGenericType<TTag0, TTag1>();
            public SameTypeSideEffect(bool expectInterception) : base(expectInterception)
            {
            }
            public override void Weave()
            {
                WeavedMethod = Metadata.Method(() => m_SideEffectGenericType.Method(Argument<int>.Value, Argument<TTag0>.Value));
                Enabled = true;
            }
            protected override int InvokeSideEffectMethod()
            {
                var _3 = (TTag0)Convert.ChangeType(3, typeof(TTag0));
                Weave(WeavedMethod);
                return m_SideEffectGenericType.Method(2, _3);
            }
        }

        interface ISideEffect
        {
            void Validate();
        }

        class EmptySideEffect : ISideEffect
        {
            public void Validate()
            { }
        }

        enum SideEffectType { Normal, Threaded, SameType }

        static private ISideEffect CreateSideEffect<TTag0, TTag1>(bool enabled, bool expectInterception, SideEffectType sideEffectType)
        {
            if (!enabled)
            {
                return new EmptySideEffect();
            }
            Before.SideEffect<TTag0, TTag1> _sideEffect;
            switch (sideEffectType)
            {
                case SideEffectType.Threaded:
                    _sideEffect = new ThreadedSideEffect<TTag0, TTag1>(expectInterception);
                    break;
                case SideEffectType.Normal:
                    _sideEffect = new SideEffect<TTag0, TTag1>(expectInterception);
                    break;
                case SideEffectType.SameType:
                    _sideEffect = new SameTypeSideEffect<TTag0, TTag1>(expectInterception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sideEffectType), sideEffectType, null);
            }
            _sideEffect.Weave();
            return _sideEffect;
        }

        enum SideEffectLocation { Weave, Invoke }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TTag">Should be unique for each test case to ensure type not weaved before.</typeparam>
        /// <param name="_method"></param>
        /// <param name="expectInterception"></param>
        static private void BasicBeforeMethodInGenericTypeWithSideEffect<T, TTag>(MethodInfo _method, bool expectInterception, SideEffectLocation location, SideEffectType sideEffectType)
        {
            AssertTagOnlyUsedOnce<TagPair<T, TTag>>();
            var _joinpoint = new Func<MethodBase, bool>(_Method => _Method == _method);
            var _genericType = new TaggedGenericType<T, TTag>();
            var _3 = (T) Convert.ChangeType(3, typeof(T));
            if (location == SideEffectLocation.Weave)
            {
                Interception<TTag>.Initialize();
                _genericType.Method(2, _3);
                Assert.AreEqual(Interception<TTag>.Done, false, typeof(T).Name);
            }
            var _weaveSideEffect = CreateSideEffect<T, TTag>(location == SideEffectLocation.Weave, expectInterception, sideEffectType);
            Aspect.Weave<Before.SideEffect<T, TTag>.Interceptor>(_joinpoint);
            _weaveSideEffect.Validate();
            Interception<TTag>.Initialize();
            int _return;
            var _invokeSideEffect = CreateSideEffect<T, TTag>(location == SideEffectLocation.Invoke, expectInterception, sideEffectType);
            _return = _genericType.Method(2, _3);
            _invokeSideEffect.Validate();
            Assert.AreEqual(_return, expectInterception ? 1 : 0, typeof(T).Name);
            Assert.AreEqual(Interception<TTag>.Done, expectInterception, typeof(T).Name);
            Aspect.Release<Before.SideEffect<T, TTag>.Interceptor>(_method);
            Interception<TTag>.Initialize();
            _genericType.Method(2, _3);
            Assert.AreEqual(Interception<TTag>.Done, false, typeof(T).Name);
        }

#endregion

        [TestMethod]
        public void BasicBeforeConstructedGenericMethod()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericMethod>.Method(_GenericMethod => _GenericMethod.Method<int>(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeGenericMethod<int>(_method, true);
                BasicBeforeGenericMethod<double>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethodDefinition()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethod).GetMethod(nameof(GenericMethod.Method));
                BasicBeforeGenericMethod<int>(_method, true);
                BasicBeforeGenericMethod<double>(_method, true);
            }
        }

        private static void BasicBeforeGenericMethod<T>(MethodInfo _method, bool expectInterception)
        {
            var _genericMethod = new GenericMethod();
            Interception.Initialize();
            var _3 = (T)Convert.ChangeType(3, typeof(T));
            _genericMethod.Method(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T).Name);
            Aspect.Weave<Before.Interceptor>(_method);
            Interception.Initialize();
            var _return = _genericMethod.Method(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0, typeof(T).Name);
            Assert.AreEqual(Interception.Done, expectInterception, typeof(T).Name);
            Aspect.Release<Before.Interceptor>(_method);
            Interception.Initialize();
            _genericMethod.Method(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T).Name);
        }

        [TestMethod]
        public void BasicBeforeConstructedGenericMethodStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericMethod>.Method(_GenericMethod => _GenericMethod.Method(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeGenericMethodStateful<int>(_method, true);
                BasicBeforeGenericMethodStateful<double>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethodDefinitionStateful()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethod).GetMethod(nameof(GenericMethod.Method));
                BasicBeforeGenericMethodStateful<int>(_method, true);
                BasicBeforeGenericMethodStateful<double>(_method, true);
            }
        }

        private static void BasicBeforeGenericMethodStateful<T>(MethodInfo _method, bool expectInterception)
        {
            var _genericMethod = new GenericMethod();
            Interception.Initialize();
            var _3 = (T)Convert.ChangeType(3, typeof(T));
            _genericMethod.Method(2, _3);
            Assert.AreEqual(Interception.Done, false);
            Aspect.Weave<Before.Interceptor.Parameterized>(_method);
            Interception.Initialize();
            var _return = _genericMethod.Method(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0);
            Assert.AreEqual(Interception.Done, expectInterception);
            if (expectInterception)
            {
                Assert.AreEqual(Interception.Instance, _genericMethod);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], _3);
            }
            Aspect.Release<Before.Interceptor.Parameterized>(_method);
            Interception.Initialize();
            _genericMethod.Method(2, _3);
            Assert.AreEqual(Interception.Done, false);
        }

        [TestMethod]
        public void BasicBeforeConstructedGenericMethodInConstructedGenericType()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericMethodInGenericType<int>>.Method(_GenericMethod => _GenericMethod.Method1<int>(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeGenericMethodInGenericType<int,int>(_method, true);
                BasicBeforeGenericMethodInGenericType<int,double>(_method, false);
                BasicBeforeGenericMethodInGenericType<double,int>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethodDefinitionInConstructedGenericType()
        {
            lock (Interception.Handle)
            {
                var _nestedTypes = typeof(GenericMethodInGenericType<int>).GetNestedTypes(BindingFlags.NonPublic|BindingFlags.Public);
                var _types0 = _nestedTypes[0].GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
                var _types01 = _types0[1].GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
                var _constructorInfos = _types01[0].GetConstructors(BindingFlags.NonPublic | BindingFlags.Public |BindingFlags.Static);
                //MethodBase m = _constructorInfos[0];
                //method.Body(_constructorInfos[0].GetBodyAsByteArray())
                var _method = typeof(GenericMethodInGenericType<int>).GetMethod(nameof(GenericMethodInGenericType<int>.Method1));
                BasicBeforeGenericMethodInGenericType<int,int>(_method, true);
                BasicBeforeGenericMethodInGenericType<int,double>(_method, true);
                BasicBeforeGenericMethodInGenericType<double,int>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethodDefinitionInGenericTypeDefinition()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethodInGenericType<>).GetMethod(nameof(GenericMethodInGenericType<Dummy>.Method1));
                BasicBeforeGenericMethodInGenericType<int,int>(_method, true);
                BasicBeforeGenericMethodInGenericType<int,double>(_method, true);
                BasicBeforeGenericMethodInGenericType<double,int>(_method, true);
            }
        }

        private static void BasicBeforeGenericMethodInGenericType<T1,T2>(MethodInfo _method, bool expectInterception)
        {
            var _genericMethod = new GenericMethodInGenericType<T1>();
            Interception.Initialize();
            var _3 = (T1)Convert.ChangeType(3, typeof(T1));
            _genericMethod.Method1<T2>(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T1).Name);
            Aspect.Weave<Before.Interceptor>(_method);
            Interception.Initialize();
            var _return = _genericMethod.Method1<T2>(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0, typeof(T1).Name);
            Assert.AreEqual(Interception.Done, expectInterception, typeof(T1).Name);
            Aspect.Release<Before.Interceptor>(_method);
            Interception.Initialize();
            _genericMethod.Method1<T2>(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T1).Name);
        }

        [TestMethod]
        public void BasicBeforeConstructedGenericMethod2InConstructedGenericType()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericMethodInGenericType<int>>.Method(_GenericMethod => _GenericMethod.Method2<int,short>(Argument<int>.Value, Argument<short>.Value));
                BasicBeforeGenericMethod2InGenericType<int,int,short>(_method, true);
                BasicBeforeGenericMethod2InGenericType<int,double,short>(_method, false);
                BasicBeforeGenericMethod2InGenericType<double,int,short>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethod2DefinitionInConstructedGenericType()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethodInGenericType<int>).GetMethod(nameof(GenericMethodInGenericType<int>.Method2));
                BasicBeforeGenericMethod2InGenericType<int,int,short>(_method, true);
                BasicBeforeGenericMethod2InGenericType<int,double,short>(_method, true);
                BasicBeforeGenericMethod2InGenericType<double,int,short>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethod2DefinitionInGenericTypeDefinition()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethodInGenericType<>).GetMethod(nameof(GenericMethodInGenericType<Dummy>.Method2));
                BasicBeforeGenericMethod2InGenericType<int,int,short>(_method, true);
                BasicBeforeGenericMethod2InGenericType<int,double,short>(_method, true);
                BasicBeforeGenericMethod2InGenericType<double,int,short>(_method, true);
            }
        }

        private static void BasicBeforeGenericMethod2InGenericType<T1,T2,T3>(MethodInfo _method, bool expectInterception)
        {
            var _genericMethod = new GenericMethodInGenericType<T1>();
            Interception.Initialize();
            var _3 = (T3)Convert.ChangeType(3, typeof(T3));
            _genericMethod.Method2<T2, T3>(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T1).Name);
            Aspect.Weave<Before.Interceptor>(_method);
            Interception.Initialize();
            var _return = _genericMethod.Method2<T2, T3>(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0, typeof(T1).Name);
            Assert.AreEqual(Interception.Done, expectInterception, typeof(T1).Name);
            Aspect.Release<Before.Interceptor>(_method);
            Interception.Initialize();
            _genericMethod.Method2<T2, T3>(2, _3);
            Assert.AreEqual(Interception.Done, false, typeof(T1).Name);
        }

        [TestMethod]
        public void BasicBeforeConstructedGenericMethodInConstructedGenericTypeStateful()
        {
            lock (Interception.Handle)
            {
                var _method = Metadata<GenericMethodInGenericType<int>>.Method(_GenericMethod => _GenericMethod.Method1<int>(Argument<int>.Value, Argument<int>.Value));
                BasicBeforeGenericMethodInGenericTypeStateful<int,int>(_method, true);
                BasicBeforeGenericMethodInGenericTypeStateful<int,double>(_method, false);
                BasicBeforeGenericMethodInGenericTypeStateful<double,int>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethodDefinitionInConstructedGenericTypeStateful()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethodInGenericType<int>).GetMethod(nameof(GenericMethodInGenericType<int>.Method1));
                BasicBeforeGenericMethodInGenericTypeStateful<int,int>(_method, true);
                BasicBeforeGenericMethodInGenericTypeStateful<int,double>(_method, true);
                BasicBeforeGenericMethodInGenericTypeStateful<double,int>(_method, false);
            }
        }

        [TestMethod]
        public void BasicBeforeGenericMethodDefinitionInGenericTypeDefinitionStateful()
        {
            lock (Interception.Handle)
            {
                var _method = typeof(GenericMethodInGenericType<>).GetMethod(nameof(GenericMethodInGenericType<Dummy>.Method1));
                BasicBeforeGenericMethodInGenericTypeStateful<int,int>(_method, true);
                BasicBeforeGenericMethodInGenericTypeStateful<int,double>(_method, true);
                BasicBeforeGenericMethodInGenericTypeStateful<double,int>(_method, true);
            }
        }

        private static void BasicBeforeGenericMethodInGenericTypeStateful<T1,T2>(MethodInfo _method, bool expectInterception)
        {
            var _genericMethod = new GenericMethodInGenericType<T1>();
            Interception.Initialize();
            var _3 = (T1)Convert.ChangeType(3, typeof(T1));
            _genericMethod.Method1<T2>(2, _3);
            Assert.AreEqual(Interception.Done, false);
            Aspect.Weave<Before.Interceptor.Parameterized>(_method);
            Interception.Initialize();
            var _return = _genericMethod.Method1<T2>(2, _3);
            Assert.AreEqual(_return, expectInterception ? 1 : 0);
            Assert.AreEqual(Interception.Done, expectInterception);
            if (expectInterception)
            {
                Assert.AreEqual(Interception.Instance, _genericMethod);
                Assert.AreEqual(Interception.Arguments.Length, 2);
                Assert.AreEqual(Interception.Arguments[0], 2);
                Assert.AreEqual(Interception.Arguments[1], _3);
            }
            Aspect.Release<Before.Interceptor.Parameterized>(_method);
            Interception.Initialize();
            _genericMethod.Method1<T2>(2, _3);
            Assert.AreEqual(Interception.Done, false);
        }

        // todo Jens constructor test: constructor, static construct with and without Neptune attribute
        // todo Jens GenericMethod with GenericParameter like 'Foo<T>(List<T> values, T value)'
    }
}
