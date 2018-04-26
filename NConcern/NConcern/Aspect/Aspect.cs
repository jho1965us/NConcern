using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CNeptune;
using CNeptuneBase;
using CNeptuneBase.ImplementationDetails;

namespace NConcern
{
    /// <summary>
    /// Manage weaving process.
    /// </summary>
    static public partial class Aspect
    {
        static private string m_Token = BitConverter.ToString(typeof(object).Assembly.GetName().GetPublicKeyToken()).Replace("-", string.Empty);
        private const string Neptune = "<Neptune>";
        static private readonly Resource m_Resource = new Resource();
        const int NeptuneMethodIndexUninitialized = -1;
        const int NeptuneMethodIndexMissing = -2;

        /// <summary>
        /// Default value used for aspects with no <see cref="EnableGenericWeavingAttribute"/>
        /// This setting is only intended for backward compatibility it is recommended to handle the decision in the aspect 
        /// </summary>
        static public GenericWeavingFlags DefaultGenericWeavingFlags = GenericWeavingFlags.EnableAllGenericWeaving;

        static Aspect()
        {
            CNeptuneBase.InstantiationListener.Listener = GenericWeavers.TypeInstantiated;
        }

        static private IEnumerable<Type> Explore(Assembly assembly)
        {
            if (string.Equals(BitConverter.ToString(assembly.GetName().GetPublicKeyToken()).Replace("-", string.Empty), Aspect.m_Token, StringComparison.InvariantCultureIgnoreCase)) { return Enumerable.Empty<Type>(); }
            // Nested types already included 
            //return assembly.GetTypes().SelectMany(Aspect.Explore);
            return assembly.GetTypes();
        }

        //static private IEnumerable<Type> Explore(Type type)
        //{
        //    foreach (var _type in type.GetNestedTypes(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly).SelectMany(Aspect.Explore)) { yield return _type; }
        //    yield return type;
        //}

        static private IEnumerable<Type> Explore()
        {
            var _domain = AppDomain.CurrentDomain.GetAssemblies();
            //return _domain.SelectMany(Aspect.Explore);
            foreach (var _assembly in _domain)
            {
                if (!_assembly.IsDefined(typeof(HasNeptuneMethodsAttribute), false)) continue;
                Debug.WriteLine(_assembly.FullName);
                foreach (var _type in Aspect.Explore(_assembly))
                {
                    Debug.Write(".");
                    yield return _type;
                }
                Debug.WriteLine(";");
            }
        }

        /// <summary>
        /// Get all methods managed by at least one aspect.
        /// </summary>
        /// <returns>Enumerable of methods managed by at least one aspect</returns>
        static public IEnumerable<MethodBase> Lookup()
        {
            lock (Aspect.m_Resource)
            {
                return Aspect.Directory.Index();
            }
        }

        /// <summary>
        /// Get all methods managed by the aspect.
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <returns>Enumerable of methods managed by the aspect</returns>
        static public IEnumerable<MethodBase> Lookup<T>()
            where T : class, IAspect, new()
        {
            lock (Aspect.m_Resource)
            {
                return Aspect.Directory.Index<T>();
            }
        }

        /// <summary>
        /// Get all aspects woven on a method.
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>Enumerable of aspects woven in the method</returns>
        static public IEnumerable<Type> Enumerate(MethodBase method)
        {
            lock (Aspect.m_Resource)
            {
                return Aspect.Directory.Index(method);
            }
        }

        /// <summary>
        /// Weave an aspect on a specific method.
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="method">Method</param>
        static public void Weave<T>(MethodBase method)
            where T : class, IAspect, new()
        {
            Weave<T>(method, GetFlags(typeof(T)));
        }

        /// <summary>
        /// Weave an aspect on a specific method.
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="method">Method</param>
        static public void Weave<T>(MethodBase method, GenericWeavingFlags genericWeavingFlags)
            where T : class, IAspect, new()
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            lock (Aspect.m_Resource)
            {
                var _enableConstructedGenericWeaving = (genericWeavingFlags & NConcern.GenericWeavingFlags.EnableClosedGenericWeaving) != 0;
                var _isGenericDefinition = method.IsGenericMethodDefinition || method.DeclaringType.IsGenericType;
                if (!_enableConstructedGenericWeaving && _isGenericDefinition)
                {
                    throw new InvalidOperationException("Attempting to weave constructed method without using " + nameof(NConcern.GenericWeavingFlags.EnableClosedGenericWeaving));
                }
                Aspect.Directory.Add<T>(method);
            }
        }

        /// <summary>
        /// Weave an aspect on methods matching with a specific pattern
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="pattern">Pattern</param>
        static public void Weave<T>(Func<MethodBase, bool> pattern)
            where T : class, IAspect, new()
        {
            Weave<T>(pattern, GetFlags(typeof(T)));
        }

        private static GenericWeavingFlags GetFlags(Type aspectType)
        {
            var _customAttributes = (EnableGenericWeavingAttribute)aspectType
                .GetCustomAttributes(typeof(EnableGenericWeavingAttribute), true)
                .FirstOrDefault();
            if (_customAttributes == null)
            {
                _customAttributes = (EnableGenericWeavingAttribute)aspectType.Assembly
                    .GetCustomAttributes(typeof(EnableGenericWeavingAttribute), true)
                    .FirstOrDefault();
            }
            var _flags = _customAttributes != null ? _customAttributes.Flags : Aspect.DefaultGenericWeavingFlags;
            return _flags;
        }

        /// <summary>
        /// Weave an aspect on methods matching with a specific pattern
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="pattern">Pattern</param>
        static public void Weave<T>(Func<MethodBase, bool> pattern, GenericWeavingFlags genericWeavingFlags)
            where T : class, IAspect, new()
        {
            var _weaver = new Weaver<T>(pattern, (genericWeavingFlags & NConcern.GenericWeavingFlags.EnableClosedGenericWeaving) != 0);
            lock (Aspect.m_Resource)
            {
                foreach (var _type in Aspect.Explore())
                {
                    if (!_type.IsDefined(typeof(HasNeptuneMethodsAttribute), false)) { continue; }
                    if (_type.IsGenericTypeDefinition)
                    {
                        if ((genericWeavingFlags & NConcern.GenericWeavingFlags.EnableOpenGenericWeaving) == 0) continue;
                        _weaver.Weave(_type, true);
                    }
                    else
                    {
                        _weaver.Weave(_type, false);
                    }
                }
                if (_weaver.EnableConstructedGenericWeaving)
                {
                    GenericWeavers.Add(_weaver);
                }
            }
        }

        static class GenericWeavers
        {
            static private Sequence<WeaverBase> m_ClosedWeavers = new Sequence<WeaverBase>();
            static private int m_ReadyWeaversCount;
            static private Sequence<Item> m_ConstructedGenerics = new Sequence<Item>(1024);
            static private readonly object m_Lock = new object();
            // todo Jens do we really need SupportsRecursion
            static private readonly ReaderWriterLockSlim m_ReadWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            static private readonly ConcurrentDictionary<Type, TypeItem> m_TypeItems = new ConcurrentDictionary<Type, TypeItem>();
            static private readonly ConcurrentDictionary<Type, TypeDefinitionItem> m_TypeDefinitionItems = new ConcurrentDictionary<Type, TypeDefinitionItem>();
            static private readonly ConcurrentDictionary<MethodBase, GenericMethodDefinition> m_MethodDefinitions = new ConcurrentDictionary<MethodBase, GenericMethodDefinition>();
            static private bool m_WaitingForWeavingCompleted;
            static private bool m_Adding;
            static private bool m_WeaversReleased;

            public abstract class Item
            {
                public abstract void Weave(WeaverBase weaver);
            }

            sealed class MethodItem : Item
            {
                private MethodBase m_method;
                private readonly int m_neptuneMethodIndex;

                public MethodItem(MethodBase method, int neptuneMethodIndex)
                {
                    m_method = method;
                    m_neptuneMethodIndex = neptuneMethodIndex;
                }

                public override void Weave(WeaverBase weaver)
                {
                    weaver.WeaveMethodInConstructedType(m_method, m_neptuneMethodIndex);
                }

                public override string ToString()
                {
                    return string.Format("{0} neptuneMethodIndex={1}", m_method, m_neptuneMethodIndex);
                }
            }

            public sealed class GenericMethodItem : Item
            {
                private readonly MethodBase m_method;
                private readonly int m_neptuneMethodIndex;

                public GenericMethodItem(MethodBase method, int neptuneMethodIndex)
                {
                    m_method = method;
                    m_neptuneMethodIndex = neptuneMethodIndex;
                }

                public override void Weave(WeaverBase weaver)
                {
                    weaver.WeaveConstructedGenericMethod(m_method, m_neptuneMethodIndex);
                }

                public override string ToString()
                {
                    return string.Format("{0} neptuneMethodIndex={1}", m_method, m_neptuneMethodIndex);
                }
            }

            public sealed class GenericMethodDefinition
            {
                public  readonly ConcurrentBag<MethodBase> ConstructedMethods = new ConcurrentBag<MethodBase>();

                public override string ToString()
                {
                    return string.Format("ConstructedMethods.Count={0}", ConstructedMethods.Count);
                }
            }

            static public GenericMethodDefinition GetGenericMethodDefinitionItem(MethodBase genericMethodDefinition)
            {
                return m_MethodDefinitions.GetOrAdd(genericMethodDefinition, _ => new GenericMethodDefinition());
            }

            public sealed class TypeDefinitionItem : TypeItem
            {
                public readonly ConcurrentDictionary<Type, TypeItem> ConstructedTypes = new ConcurrentDictionary<Type, TypeItem>();

                public TypeDefinitionItem(Type type) : base(type)
                {
                }

                public override string ToString()
                {
                    return string.Format("{0} ConstructedTypes.Count={1}", this.Type.Declaration(), this.ConstructedTypes.Count);
                }
            }

            static public TypeDefinitionItem GetTypeDefinitionItem(Type genericTypeDefinition)
            {
                if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ArgumentException("Type is not a generic type definition", nameof(genericTypeDefinition));
                var _typeDefinitionItem = m_TypeDefinitionItems.GetOrAdd(genericTypeDefinition, _Type => new TypeDefinitionItem(_Type));
                if (!_typeDefinitionItem.Initialized)
                {
                    _typeDefinitionItem.Initialize();
                }
                return _typeDefinitionItem;
            }

            static public TypeItem GetTypeItem(Type type)
            {
                if (type.IsGenericTypeDefinition) throw new ArgumentException("Type is not a constructed generictype type", nameof(type));
                var _typeItem = m_TypeItems.GetOrAdd(type, _Type => new TypeItem(_Type));
                if (!_typeItem.Initialized)
                {
                    _typeItem.Initialize();
                }
                return _typeItem;
            }

            public class TypeItem : Item
            {
                public readonly Type Type;
                public MethodBase[] Methods;
                public bool[] InvokedMethods;
                public bool Initialized;
                private readonly object m_Handle = new object();
                public TypeDefinitionItem TypeDefinitionItem { get; private set; }

                public TypeItem(Type type)
                {
                    Type = type;
                }

                public override string ToString()
                {
                    return string.Format("{0} Methods.Length={1}", this.Type.Declaration(), this.Methods.Length);
                }

                public void Initialize()
                {
                    lock (m_Handle)
                    {
                        if (Initialized) return;
                        var _neptuneMethodCountFieldInfo = Type.GetField("<NeptuneMethodCount>");
                        if (_neptuneMethodCountFieldInfo == null) { throw new NotSupportedException($"generic type '{Type.AssemblyQualifiedName}' is not managed by CNeptune and cannot be supervised."); }
                        var _neptuneMethodCount = (int) _neptuneMethodCountFieldInfo.GetValue(null);
                        Methods = new MethodBase[_neptuneMethodCount];
                        InvokedMethods = new bool[_neptuneMethodCount];
                        foreach (var _constructor in Type.GetConstructors())
                        {
                            if (_constructor.IsStatic) { continue; }
                            if (_constructor.IsAbstract) { continue; }
                            AddMethod(_constructor);
                        }
                        foreach (var _method in Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                        {
                            if (_method.IsAbstract) { continue; }
                            AddMethod(_method);
                        }
                        if (Type.IsGenericType && !Type.IsGenericTypeDefinition)
                        {
                            TypeDefinitionItem = GetTypeDefinitionItem(Type.GetGenericTypeDefinition());
                            TypeDefinitionItem.ConstructedTypes.TryAdd(Type, this);
                        }
                        Initialized = true;
                    }
                }

                private void AddMethod(MethodBase _method)
                {
                    var _neptuneMethodIndex = NeptuneMethodIndexUninitialized;
                    WeaverBase.GetNeptuneMethodIndex(_method, ref _neptuneMethodIndex);
                    if (_neptuneMethodIndex != NeptuneMethodIndexMissing)
                    {
                        Methods[_neptuneMethodIndex] = _method;
                    }
                }

                public override void Weave(WeaverBase weaver)
                {
                    weaver.WeaveConstructedGenericType(this);
                }
            }

            static public void TypeInstantiated(Type type)
            {
                var _isMethod = type.DeclaringType != null && type.DeclaringType.Name == "<Intermediate>";
                Item _item;
                MethodBase _genericTypeDefinitionMethod = null;
                MethodBase _constructedGenericTypeMethod = null; // possible a generic method definition
                MethodBase _constructedgenericMethod = null;
                int _neptuneMethodIndex = NeptuneMethodIndexUninitialized;
                if (_isMethod)
                {
                    _neptuneMethodIndex = (int) type.GetField("<NeptuneMethodIndex>").GetValue(null);
                    var _intermediate = type.DeclaringType;
                    var _neptune = _intermediate.DeclaringType;
                    var _targetType = _neptune.DeclaringType;
                    var _genericArguments = type.GetGenericArguments();
                    var _typeArgumentCount = 0;
                    if (_targetType.IsGenericTypeDefinition)
                    {
                        _genericTypeDefinitionMethod = GetTypeDefinitionItem(_targetType).Methods[_neptuneMethodIndex];
                        _typeArgumentCount = _targetType.GetGenericArguments().Length;
                        _targetType = _targetType.MakeGenericType(_genericArguments.Subarray(0, _typeArgumentCount));
                    }
                    var _typeItem = GetTypeItem(_targetType);
                    _constructedGenericTypeMethod = _typeItem.Methods[_neptuneMethodIndex];
                    _typeItem.InvokedMethods[_neptuneMethodIndex] = true;
                    if (_constructedGenericTypeMethod.IsGenericMethodDefinition)
                    {
                        var _typeArguments = _targetType.IsGenericType ? _genericArguments.Subarray(_typeArgumentCount) : _genericArguments;
                        _constructedgenericMethod = ((MethodInfo)_constructedGenericTypeMethod).MakeGenericMethod(_typeArguments);
                        GetGenericMethodDefinitionItem(_constructedGenericTypeMethod).ConstructedMethods.Add(_constructedgenericMethod);
                        _item = new GenericMethodItem(_constructedgenericMethod, _neptuneMethodIndex);
                    }
                    else
                    {
                        _item = new MethodItem(_constructedGenericTypeMethod, _neptuneMethodIndex);
                    }
                }
                else
                {
                    _item = GetTypeItem(type);
                    if (!type.IsGenericType)
                    {
                        return;
                    }
                }

                m_ReadWriteLock.EnterReadLock();
                try
                {
                    if (_genericTypeDefinitionMethod != null && Aspect.Directory.ContainsKey(_genericTypeDefinitionMethod))
                    {
                        Aspect.Directory.Register(_constructedGenericTypeMethod, _neptuneMethodIndex);
                    }
                    if (_constructedgenericMethod != null && Aspect.Directory.ContainsKey(_constructedGenericTypeMethod))
                    {
                        Aspect.Directory.Register(_constructedgenericMethod, _neptuneMethodIndex);
                    }
                    foreach (var _weaver in m_ClosedWeavers)
                    {
                        _item.Weave(_weaver);
                    }
                    lock (m_Lock)
                    {
                        Extend(ref m_ConstructedGenerics, _item);
                    }
                }
                finally
                {
                    m_ReadWriteLock.ExitReadLock();
                }
            }

            /// <summary>
            /// Not thread safe
            /// </summary>
            static public void Add(WeaverBase weaver)
            {
                if (m_Adding)
                {
                    // might break order of weaving, but probably otherwise safe
                    throw new NotImplementedException("Weaving aspects on closed generics can not be done recursively");
                }

                m_Adding = true;
                m_ReadWriteLock.EnterUpgradeableReadLock();
                try
                {
                    int _index = 0;
                    for (; _index < m_ConstructedGenerics.Count; _index++)
                    {
                        var _constructedGeneric = m_ConstructedGenerics[_index];
                        _constructedGeneric.Weave(weaver);
                    }
                    //m_ReadWriteLock.EnterWriteLock();
                    m_ReadWriteLock.EnterWriteLock();
                    try
                    {
                        for (; _index < m_ConstructedGenerics.Count; _index++)
                        {
                            var _constructedGeneric = m_ConstructedGenerics[_index];
                            _constructedGeneric.Weave(weaver);
                        }

                        Extend(ref m_ClosedWeavers, weaver);
                    }
                    finally
                    {
                        m_ReadWriteLock.ExitWriteLock();
                    }
                }
                finally
                {
                    m_ReadWriteLock.EnterUpgradeableReadLock();
                    m_Adding = false;
                }
            }

            static public void Remove<T>() where T : class, IAspect, new()
            {
                if (m_Adding)
                {
                    throw new NotImplementedException("Releasing and Aspect while Weaving an aspects on a closed generics not supported");
                }
                m_ReadWriteLock.EnterUpgradeableReadLock();
                try
                {
                    var _index = 0;
                    for (; _index < m_ClosedWeavers.Count; _index++)
                    {
                        var _closedWeaver = m_ClosedWeavers[_index];
                        if (_closedWeaver is Weaver<T>)
                        {
                            m_ReadWriteLock.EnterWriteLock();
                            try
                            {
                                m_ClosedWeavers.Remove(_index);
                            }
                            finally
                            {
                                m_ReadWriteLock.ExitWriteLock();
                            }
                            return;
                        }
                    }
                    m_ReadWriteLock.EnterWriteLock();
                    try
                    {
                        for (; _index < m_ClosedWeavers.Count; _index++)
                        {
                            var _closedWeaver = m_ClosedWeavers[_index];
                            if (_closedWeaver is Weaver<T>)
                            {
                                m_ClosedWeavers.Remove(_index);
                                return;
                            }
                        }
                    }
                    finally
                    {
                        m_ReadWriteLock.ExitWriteLock();
                    }
                }
                finally
                {
                    m_ReadWriteLock.EnterUpgradeableReadLock();
                }
            }

            /// <summary>
            /// Not thread safe
            /// </summary>
            static private void Extend<T>(ref Sequence<T> sequence, T value)
            {
                sequence = sequence.Extend(value);
            }

            /// <summary>
            /// <see cref="List{T}"/> like type that is a tiny bit thread safe (but mostly not thread safe).
            /// </summary>
            /// <typeparam name="T"></typeparam>
            [Neptune(false)] // this method is part of non-re-entrant code
            private class Sequence<T> : IEnumerable<T>
            {
                private readonly T[] m_array;
                private int m_capacity;
                public int Count;

                public Sequence(int capacity = 4)
                {
                    m_array = new T[capacity];
                    m_capacity = capacity;
                }

                private Sequence(Sequence<T> source)
                {
                    m_capacity = source.Count * 2;
                    Count = source.Count;
                    m_array = new T[m_capacity];
                    Array.Copy(source.m_array, m_array, Count);
                }

                /// <summary>
                /// Not thread safe
                /// </summary>
                /// <param name="value"></param>
                /// <returns></returns>
                internal Sequence<T> Extend(T value)
                {
                    var _sequence = Count < m_capacity ? this : new Sequence<T>(this);
                    _sequence.m_array[_sequence.Count] = value;
                    _sequence.Count++;
                    return _sequence;
                }

                /// <summary>
                /// Not thread safe
                /// 
                /// </summary>
                /// <param name="index"></param>
                internal void Remove(int index)
                {
                    Array.Copy(m_array, index + 1, m_array, index, Count - index -1);
                    Count--;
                }

                /// <summary>
                /// Thread safe
                /// </summary>
                /// <param name="index"></param>
                /// <returns></returns>
                public T this[int index]
                {
                    get
                    {
                        if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                        return m_array[index];
                    }
                }

                /// <summary>
                /// Thread safe
                /// </summary>
                public IEnumerator<T> GetEnumerator()
                {
                    for (int i = 0; i < Count; i++)
                    {
                        yield return m_array[i];
                    }
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }
        }

        abstract class WeaverBase
        {
            public readonly bool EnableConstructedGenericWeaving;
            protected WeaverBase(bool enableConstructedGenericWeaving)
            {
                EnableConstructedGenericWeaving = enableConstructedGenericWeaving;
            }

            // todo Jens move to GenericWeavers
            static public void GetNeptuneMethodIndex(MethodBase method, ref int neptuneMethodIndex)
            {
                if (neptuneMethodIndex != NeptuneMethodIndexUninitialized) return;
                if (method.IsGenericMethod || method.DeclaringType.IsGenericType)
                {
                    var _neptuneMethodIndexAttribute = (NeptuneMethodIndexAttribute)method.GetCustomAttributes(typeof(NeptuneMethodIndexAttribute), true).SingleOrDefault();
                    if (_neptuneMethodIndexAttribute != null)
                    {
                        neptuneMethodIndex = _neptuneMethodIndexAttribute.NeptuneMethodIndex;
                        return;
                    }
                }
                neptuneMethodIndex = NeptuneMethodIndexMissing;
            }

            public abstract void WeaveMethodInConstructedType(MethodBase method, int neptuneMethodIndex);
            public abstract void WeaveConstructedGenericMethod(MethodBase method, int neptuneMethodIndex);
            public abstract void WeaveConstructedGenericType(GenericWeavers.TypeItem type);
        }
        class Weaver<T> : WeaverBase
            where T : class, IAspect, new()
        {
            private readonly Func<MethodBase, bool> m_pattern;
            public Weaver(Func<MethodBase, bool> pattern, bool enableConstructedGenericWeaving) : base(enableConstructedGenericWeaving)
            {
                m_pattern = pattern;
            }

            public override void WeaveMethodInConstructedType(MethodBase method, int neptuneMethodIndex)
            {
                TryWeave(method, neptuneMethodIndex);
            }

            public override void WeaveConstructedGenericMethod(MethodBase method, int neptuneMethodIndex)
            {
                TryWeave(method, neptuneMethodIndex);
            }


            public override void WeaveConstructedGenericType(GenericWeavers.TypeItem type)
            {
                if (!EnableConstructedGenericWeaving) return;
                for (var _index = 0; _index < type.Methods.Length; _index++)
                {
                    var _method = type.Methods[_index];
                    if (_method.IsAbstract) { continue; }
                    var _neptuneMethodIndex = _index;
                    TryWeave(_method, _neptuneMethodIndex);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="type"></param>
            /// <param name="collectOpenMethods">i.e. if GenericMethodDefinition or Method in GenericTypeDefinition</param>
            public void Weave(Type type, bool collectOpenMethods)
            {
                HashSet<int> _indexes = null;
                foreach (var _constructor in type.GetConstructors())
                {
                    if (_constructor.IsStatic) { continue; }
                    if (_constructor.IsAbstract) { continue; }
                    TryWeave(_constructor);
                }
                foreach (var _method in type.Methods())
                {
                    if (_method.IsAbstract) { continue; }
                    TryWeave(_method);
                }
            }

            private bool TryWeave(MethodBase method, int _neptuneMethodIndex = NeptuneMethodIndexUninitialized)
            {
                if (m_pattern != null && m_pattern(method))
                {
                    GetNeptuneMethodIndex(method, ref _neptuneMethodIndex);
                    Aspect.Directory.Add<T>(method, _neptuneMethodIndex);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Weave an aspect on methods defined as [type] or defined in [type].
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="type">Type</param>
        static public void Weave<T>(Type type)
            where T : class, IAspect, new()
        {
            if (Metadata<Attribute>.Type.IsAssignableFrom(type))
            {
                Aspect.Weave<T>(_Method =>
                {
                    if (_Method.IsAbstract) { return false; }
                    if (_Method.IsDefined(type, true) || _Method.DeclaringType.IsDefined(type, true)) { return true; }
                    var _property = _Method.Property();
                    if (_property != null && _property.IsDefined(type, true)) { return true; }
                    var _method = _Method.GetBaseDefinition();
                    if (_method.IsDefined(type, true) || _method.DeclaringType.IsDefined(type, true)) { return true; }
                    _property = _method.Property();
                    if (_property != null && _property.IsDefined(type, true)) { return true; }
                    // todo Jens do we need to test them all?
                    var _entry = _Method.DeclaringType.GetInterfaces().Select(_Interface => _Method.DeclaringType.GetInterfaceMap(_Interface)).SelectMany(_Map => _Map.TargetMethods.Zip(_Map.InterfaceMethods, (_Interface, _Implementation) => new { Interface = _Interface, Implementation = _Implementation })).FirstOrDefault(_Entry => _Entry.Implementation == _Method);
                    if (_entry != null)
                    {
                        if (_entry.Interface.IsDefined(type, true) || _entry.Interface.DeclaringType.IsDefined(type, true)) { return true; }
                        _property = _entry.Interface.Property();
                        if (_property != null && _property.IsDefined(type, true)) { return true; }
                    }
                    return false;
                });
            }
            else if (type.IsInterface)
            {
                Aspect.Weave<T>(_Method =>
                {
                    if (type.IsAssignableFrom(_Method.DeclaringType)) { return _Method.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(_Method); }
                    return false;
                });
            }
            else if (type.IsClass)
            {
                Aspect.Weave<T>(_Method =>
                {
                    if (_Method.IsPublic && type.IsAssignableFrom(_Method.DeclaringType)) { return true; }
                    return false;
                });
            }
            else { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Release all aspects from a specific method.
        /// </summary>
        /// <param name="method">Method</param>
        static public void Release(MethodBase method)
        {
            lock (Aspect.m_Resource)
            {
                Aspect.Directory.Remove(method);
            }
        }

        /// <summary>
        /// Release all aspects from methods matching with a specific pettern.
        /// </summary>
        /// <param name="pattern">Pattern</param>
        static public void Release(Func<MethodBase, bool> pattern)
        {
            lock (Aspect.m_Resource)
            {
                foreach (var _method in Aspect.Directory.Index().Where(pattern)) { Aspect.Directory.Remove(_method); }
            }
        }

        /// <summary>
        /// Release all aspects from methods defined as [type] or defined in [type].
        /// </summary>
        /// <param name="type">Custom attribute type</param>
        static public void Release(Type type)
        {
            if (Metadata<Attribute>.Type.IsAssignableFrom(type))
            {
                Aspect.Release(_Method =>
                {
                    if (_Method.IsAbstract) { return false; }
                    if (_Method.IsDefined(type, true) || _Method.DeclaringType.IsDefined(type, true)) { return true; }
                    var _property = _Method.Property();
                    if (_property != null && _property.IsDefined(type, true)) { return true; }
                    var _method = _Method.GetBaseDefinition();
                    if (_method.IsDefined(type, true) || _method.DeclaringType.IsDefined(type, true)) { return true; }
                    _property = _method.Property();
                    if (_property != null && _property.IsDefined(type, true)) { return true; }
                    var _entry = _Method.DeclaringType.GetInterfaces().Select(_Interface => _Method.DeclaringType.GetInterfaceMap(_Interface)).SelectMany(_Map => _Map.TargetMethods.Zip(_Map.InterfaceMethods, (_Interface, _Implementation) => new { Interface = _Interface, Implementation = _Implementation })).FirstOrDefault(_Entry => _Entry.Implementation == _Method);
                    if (_entry != null)
                    {
                        if (_entry.Interface.IsDefined(type, true) || _entry.Interface.DeclaringType.IsDefined(type, true)) { return true; }
                        _property = _entry.Interface.Property();
                        if (_property != null && _property.IsDefined(type, true)) { return true; }
                    }
                    return false;
                });
            }
            else if (type.IsInterface)
            {
                Aspect.Release(_Method =>
                {
                    if (type.IsAssignableFrom(_Method.DeclaringType)) { return _Method.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(_Method); }
                    return false;
                });
            }
            else if (type.IsClass)
            {
                Aspect.Release(_Method =>
                {
                    if (_Method.IsPublic && type.IsAssignableFrom(_Method.DeclaringType)) { return true; }
                    return false;
                });
            }
            else { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Release an aspect from all method.
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        static public void Release<T>()
            where T : class, IAspect, new()
        {
            lock (Aspect.m_Resource)
            {
                GenericWeavers.Remove<T>();
                foreach (var _method in Aspect.Directory.Index<T>()) { Aspect.Directory.Remove(_method); }
            }
        }

        /// <summary>
        /// Release an aspect from a specific method.
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="method">Method</param>
        static public void Release<T>(MethodBase method)
            where T : class, IAspect, new()
        {
            lock (Aspect.m_Resource)
            {
                Aspect.Directory.Remove<T>(method);
            }
        }

        /// <summary>
        /// Release an aspect from methods matching with a specific pattern.
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="pattern">Pattern</param>
        static public void Release<T>(Func<MethodBase, bool> pattern)
            where T : class, IAspect, new()
        {
            lock (Aspect.m_Resource)
            {
                foreach (var _method in Aspect.Directory.Index<T>().Where(pattern)) { Aspect.Directory.Remove(_method); }
            }
        }

        /// <summary>
        /// Release an aspect from methods defined as [type] or defined in [type].
        /// </summary>
        /// <typeparam name="T">Aspect</typeparam>
        /// <param name="type">Custom attribute type</param>
        static public void Release<T>(Type type)
            where T : class, IAspect, new()
        {
            if (Metadata<Attribute>.Type.IsAssignableFrom(type))
            {
                Aspect.Release<T>(_Method =>
                {
                    if (_Method.IsAbstract) { return false; }
                    if (_Method.IsDefined(type, true) || _Method.DeclaringType.IsDefined(type, true)) { return true; }
                    var _property = _Method.Property();
                    if (_property != null && _property.IsDefined(type, true)) { return true; }
                    var _method = _Method.GetBaseDefinition();
                    if (_method.IsDefined(type, true) || _method.DeclaringType.IsDefined(type, true)) { return true; }
                    _property = _method.Property();
                    if (_property != null && _property.IsDefined(type, true)) { return true; }
                    var _entry = _Method.DeclaringType.GetInterfaces().Select(_Interface => _Method.DeclaringType.GetInterfaceMap(_Interface)).SelectMany(_Map => _Map.TargetMethods.Zip(_Map.InterfaceMethods, (_Interface, _Implementation) => new { Interface = _Interface, Implementation = _Implementation })).FirstOrDefault(_Entry => _Entry.Implementation == _Method);
                    if (_entry != null)
                    {
                        if (_entry.Interface.IsDefined(type, true) || _entry.Interface.DeclaringType.IsDefined(type, true)) { return true; }
                        _property = _entry.Interface.Property();
                        if (_property != null && _property.IsDefined(type, true)) { return true; }
                    }
                    return false;
                });
            }
            else if (type.IsInterface)
            {
                Aspect.Release<T>(_Method =>
                {
                    if (type.IsAssignableFrom(_Method.DeclaringType)) { return _Method.DeclaringType.GetInterfaceMap(type).TargetMethods.Contains(_Method); }
                    return false;
                });
            }
            else if (type.IsClass)
            {
                Aspect.Release<T>(_Method =>
                {
                    if (_Method.IsPublic && type.IsAssignableFrom(_Method.DeclaringType)) { return true; }
                    return false;
                });
            }
            else { throw new NotSupportedException(); }
        }
    }
}