using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NConcern
{
    static public partial class Aspect
    {
        static private partial class Directory
        {
            static private readonly ConcurrentDictionary<MethodBase, Aspect.Directory.Entry> m_Dictionary = new ConcurrentDictionary<MethodBase, Entry>();

            static private Aspect.Directory.Entry Obtain(MethodBase method, int neptuneMethodIndex = NeptuneMethodIndexUninitialized)
            {
                var _method = method;
                if (_method.DeclaringType != _method.ReflectedType)
                {
                    if (_method is MethodInfo) { _method = (_method as MethodInfo).GetBaseDefinition(); }
                    // todo Jens '_Method is ConstructorInfo' seems always false implied by 'FindMembers(MemberTypes.Method, ...)', also no need to find constructors as 'if (_method.DeclaringType != _method.ReflectedType)' seems never true for constructors
                    // todo Jens '_Method is MethodInfo' seems always true implied by 'FindMembers(MemberTypes.Method, ...)'
                    _method = _method.DeclaringType.FindMembers(MemberTypes.Method, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly, (_Method, _Criteria) => _Method is ConstructorInfo || _Method is MethodInfo && (_Method as MethodInfo).GetBaseDefinition() == _method, null).Single() as MethodBase;
                }
                var _entry = ObtainRaw(_method, neptuneMethodIndex);
                if (_entry.NeedInitialization)
                {
                    _entry.Initialize();
                }
                return _entry;
            }

            static private Aspect.Directory.Entry ObtainRaw(MethodBase method, int neptuneMethodIndex)
            {
                return m_Dictionary.GetOrAdd(method, _Method => new Aspect.Directory.Entry(_Method.DeclaringType, _Method, new Aspect.Activity(_Method.DeclaringType, _Method), neptuneMethodIndex));
            }

            static public bool ContainsKey(MethodBase method)
            {
                return Aspect.Directory.m_Dictionary.ContainsKey(method);
            }

            static public IEnumerable<MethodBase> Index()
            {
                return Aspect.Directory.m_Dictionary.Values.Where(_Entry => _Entry.Count() > 0).Select(_Entry => _Entry.Method).ToArray();
            }

            static public IEnumerable<MethodBase> Index<T>()
                where T : class, IAspect, new()
            {
                return Aspect.Directory.m_Dictionary.Values.Where(_Entry => _Entry.Contains(Singleton<T>.Value)).Select(_Entry => _Entry.Method).ToArray();
            }

            static public IEnumerable<Type> Index(MethodBase method)
            {
                var _entry = Aspect.Directory.Obtain(method);
                return _entry.Select(_Aspect => _Aspect.GetType()).ToArray();
            }

            static public void Register(MethodBase method, int neptuneMethodIndex = NeptuneMethodIndexUninitialized)
            {
                Aspect.Directory.Obtain(method);
            }

            static public void Add<T>(MethodBase method, int neptuneMethodIndex = NeptuneMethodIndexUninitialized)
                where T : class, IAspect, new()
            {
                Aspect.Directory.Obtain(method, neptuneMethodIndex).Add(Singleton<T>.Value);
            }

            static public void Remove(MethodBase method)
            {
                var _entry = Aspect.Directory.Obtain(method);
                foreach (var _aspect in _entry.ToArray()) { _entry.Remove(_aspect); }
            }

            static public void Remove<T>(MethodBase method)
                where T : class, IAspect, new()
            {
                Aspect.Directory.Obtain(method).Remove(Singleton<T>.Value);
            }
        }
    }
}