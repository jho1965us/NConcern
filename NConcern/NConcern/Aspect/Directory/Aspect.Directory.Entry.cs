using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NConcern
{
    static public partial class Aspect
    {
        static private partial class Directory
        {
            private sealed partial class Entry : IEnumerable<IAspect>
            {
                // todo Jens need aplausible use case before implementing this
                private const string RecursivelyModifyingTheWeavingOfAMethod = "Recursively modifying the weaving of a method not implemented";

                static private string Identity(Type type)
                {
                    var _index = type.Name.IndexOf('`');
                    var _name = _index < 0 ? type.Name : type.Name.Substring(0, _index);
                    if (type.GetGenericArguments().Length == 0) { return string.Concat("<", _name, ">"); }
                    _name = string.Concat(_name, "<", type.GetGenericArguments().Length.ToString(), ">");
                    return string.Concat("<", _name, string.Concat("<", string.Concat(type.GetGenericArguments().Select(_argument => string.Concat("<", _argument.Name, ">"))), ">"), ">");
                }

                static private string Identity(MethodBase method)
                {
                    return string.Concat("<", method.IsConstructor ? method.DeclaringType.Name : method.Name, method.GetGenericArguments().Length > 0 ? string.Concat("<", method.GetGenericArguments().Length, ">") : string.Empty, method.GetParameters().Length > 0 ? string.Concat("<", string.Concat(method.GetParameters().Select(_parameter => Identity(_parameter.ParameterType))), ">") : string.Empty, ">");
                }

                static private FieldInfo Pointer(MethodBase method, bool needSpecialPrepare)
                {
                    foreach (var _instruction in method.Body(needSpecialPrepare))
                    {
                        if (_instruction.Code == OpCodes.Ldsfld)
                        {
                            var _field = _instruction.Value as FieldInfo;
                            if (_field.Name == "<Pointer>") { return _field; }
                        }
                    }
                    throw new NotSupportedException($"type '{ method.DeclaringType.AssemblyQualifiedName }' is not managed by CNeptune and cannot be supervised.");
                }

                public readonly Type Type;
                public readonly MethodBase Method;
                public readonly Activity Activity;
                public readonly ConcurrentDictionary<Entry, object> GenericInstances;
                public Entry GenericDefinition;
                public bool NeedInitialization;
                public readonly bool IsGeneric;
                private readonly LinkedList<IAspect> m_Aspectization;
                private readonly LinkedList<MethodInfo> m_Sequence;
                private readonly Dictionary<IAspect, Activity> m_Dictionary;
                private readonly IntPtr m_Pointer;
                private readonly FieldInfo m_Field;
                public readonly object m_Handle = new object();
                private bool m_IsChanging;
                private int m_neptuneMethodIndex;

                unsafe internal Entry(Type type, MethodBase method, Activity activity, int neptuneMethodIndex)
                {
                    this.Type = type;
                    this.Method = method;
                    this.Activity = activity;
                    this.m_Aspectization = new LinkedList<IAspect>();
                    this.m_Dictionary = new Dictionary<IAspect, Activity>();
                    if (type.IsGenericTypeDefinition || Method.IsGenericMethodDefinition)
                    {
                        this.GenericInstances = new ConcurrentDictionary<Entry,object>();
                    }
                    else
                    {
                        var needSpecialPrepare = false;
                        // todo Jens not good enough
                        //if (type.IsGenericType)
                        //{
                        //    var _preparedMethods = GenericWeavers.GetTypeItem(type).InvokedMethods;
                        //    WeaverBase.GetNeptuneMethodIndex(method, ref neptuneMethodIndex);
                        //    if (!_preparedMethods[neptuneMethodIndex])
                        //    {
                        //        needSpecialPrepare = true;
                        //    }
                        //}
                        this.m_Field = Pointer(method, needSpecialPrepare);
                        this.m_Pointer = (IntPtr) this.m_Field.GetValue(null);
                        this.m_Sequence = new LinkedList<MethodInfo>();
                    }
                    this.m_neptuneMethodIndex = neptuneMethodIndex;
                    this.IsGeneric = method.IsGenericMethod || method.DeclaringType.IsGenericType;
                    this.NeedInitialization = this.IsGeneric;
                }

                private void Update()
                {
                    var _aspectization = this.m_Aspectization.SelectMany(_Aspect => _Aspect.Advise(this.Method)).ToArray();
                    var _pointer = this.m_Pointer;
                    this.m_Sequence.Clear();
                    foreach (var _advice in _aspectization)
                    {
                        if (_advice == null) { continue; }
                        var _method = _advice.Decorate(this.Method, _pointer);
                        this.m_Sequence.AddLast(_method);
                        if (_method != null) { _pointer = _method.Pointer(); }
                    }
                    this.m_Field.SetValue(null, _pointer);
                }

                public void Add(IAspect aspect)
                {
                    // we should always lock parent before child to avoid dead lock
                    lock (this.m_Handle)
                    {
                        if (m_IsChanging) throw new NotImplementedException(RecursivelyModifyingTheWeavingOfAMethod);
                        m_IsChanging = true;
                        try
                        {
                            if (!this.m_Dictionary.ContainsKey(aspect))
                            {
                                this.m_Aspectization.AddFirst(aspect);
                                this.m_Dictionary.Add(aspect, null);
                            }
                            if (!this.Type.IsGenericTypeDefinition && !this.Method.IsGenericMethodDefinition)
                            {
                                this.Update();
                            }
                            else
                            {
                                foreach (var _genericInstance in this.GenericInstances.Keys)
                                {
                                    _genericInstance.Add(aspect);
                                }
                            }
                        }
                        finally
                        {
                            m_IsChanging = false;
                        }
                    }
                }

                private void InstantiateChild(Entry instanceEntry)
                {
                    // we should always lock parent before child to avoid dead lock
                    lock (this.m_Handle)
                    {
                        if (m_IsChanging) throw new NotImplementedException(RecursivelyModifyingTheWeavingOfAMethod);
                        m_IsChanging = true;
                        try
                        {
                            instanceEntry.Instantiate(this);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }
                        finally
                        {
                            m_IsChanging = false;
                        }
                    }
                }

                public void Instantiate(Entry parent)
                {
                    lock (m_Handle)
                    {
                        if (m_IsChanging) throw new NotImplementedException(RecursivelyModifyingTheWeavingOfAMethod);
                        m_IsChanging = true;
                        try
                        {
                            var _changed = false;
                            foreach (var _aspect in parent.m_Aspectization)
                            {
                                if (!this.m_Dictionary.ContainsKey(_aspect))
                                {
                                    this.m_Aspectization.AddLast(_aspect);
                                    this.m_Dictionary.Add(_aspect, null);
                                    _changed = true;
                                }
                            }

                            if (_changed)
                            {
                                if (!Method.IsGenericMethodDefinition)
                                {
                                    this.Update();
                                }
                                else
                                {
                                    foreach (var _instance in parent.GenericInstances.Keys)
                                    {
                                        this.InstantiateChild(_instance);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            m_IsChanging = false;
                        }
                    }
                }

                public void Remove(IAspect aspect)
                {
                    // we should always lock parent before child to avoid dead lock
                    lock (this.m_Handle)
                    {
                        if (m_IsChanging) throw new NotImplementedException(RecursivelyModifyingTheWeavingOfAMethod);
                        m_IsChanging = true;
                        try
                        {
                            if (this.m_Dictionary.Remove(aspect))
                            {
                                this.m_Aspectization.Remove(aspect);
                                if (this.GenericInstances == null)
                                {
                                    this.Update();
                                }
                                else
                                {
                                    foreach (var _genericInstance in this.GenericInstances.Keys)
                                    {
                                        _genericInstance.Remove(aspect);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            m_IsChanging = false;
                        }
                    }
                }

                IEnumerator<IAspect> IEnumerable<IAspect>.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                private IEnumerator<IAspect> GetEnumerator()
                {
                    lock (this.m_Handle)
                    {
                        return this.m_Aspectization.ToArray().Cast<IAspect>().GetEnumerator();
                    }
                }

                public void Initialize()
                {
                    lock (m_Handle)
                    {
                        if (!NeedInitialization) return;
                        WeaverBase.GetNeptuneMethodIndex(this.Method, ref m_neptuneMethodIndex);
                        if (Type.IsGenericTypeDefinition)
                        {
                            // do the remaining outside the lock
                            NeedInitialization = false;
                            goto FinalizeGenericTypeDefinition;
                        }
                        else
                        {
                            if (Method.IsGenericMethodDefinition)
                            {
                                if (Type.IsGenericType)
                                {
                                    AddToDefinition(GenericWeavers.GetTypeDefinitionItem(Type.GetGenericTypeDefinition()));
                                }
                            }
                            else
                            {
                                if (Method.IsGenericMethod)
                                {
                                    AddToDefinition(GenericWeavers.GetTypeItem(Type));
                                }
                                else
                                {
                                    AddToDefinition(GenericWeavers.GetTypeDefinitionItem(Type.GetGenericTypeDefinition()));
                                }
                            }
                            // do the remaining outside the lock
                        }
                        NeedInitialization = false;
                    }

                    // since we are outside the lock a competing thread may have done any of the following but none of it
                    // should have problem with that, but they all may take lock on parent and we cannot allow that while child locked 
                    // due to risk od dead lock (we should always lock parent before child to avoid dead lock)

                    if (this.GenericDefinition != null)
                    {
                        if (this.GenericDefinition.NeedInitialization)
                        {
                            this.GenericDefinition.Initialize();
                        }
                        GenericDefinition.InstantiateChild(this);
                    }

                    if (Method.IsGenericMethodDefinition)
                    {
                        foreach (var _method in GenericWeavers.GetGenericMethodDefinitionItem(Method).ConstructedMethods)
                        {
                            // just need to create the entry it will add it self to this.GenericInstances
                            Obtain(_method, m_neptuneMethodIndex);
                        }
                    }
                    return;
                
                FinalizeGenericTypeDefinition:
                    foreach (var _typeItem in GenericWeavers.GetTypeDefinitionItem(Type).ConstructedTypes.Values)
                    {
                        // todo Jens write a unittest that verify the claim below
                        // we could do it now but then we might trigger type loading earlier than if not used
                        // also the side effect unit tests assume it happens on first invoke
                        if (_typeItem.InvokedMethods[m_neptuneMethodIndex])
                        {
                            // just need to create the entry it will add it self to this.GenericInstances
                            Obtain(_typeItem.Methods[m_neptuneMethodIndex]);
                        }
                    }
                    return;
                }

                private void AddToDefinition(GenericWeavers.TypeItem typeDefinitionItem)
                {
                    var _methodDefinition = typeDefinitionItem.Methods[m_neptuneMethodIndex];
                    GenericDefinition = Aspect.Directory.ObtainRaw(_methodDefinition, m_neptuneMethodIndex);
                    GenericDefinition.GenericInstances.TryAdd(this, null);
                }
            }

            [DebuggerDisplay("{Debugger.Display(this) , nq}")]
            [DebuggerTypeProxy(typeof(Entry.Debugger))]
            private sealed partial class Entry
            {
                private class Debugger
                {
                    static public string Display(Aspect.Directory.Entry map)
                    {
                        return string.Concat(map.Type.Declaration(), ".", map.Method.Declaration(), " = ", map.m_Aspectization.Count.ToString());
                    }

                    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private Aspect.Directory.Entry m_Map;

                    public Debugger(Aspect.Directory.Entry map)
                    {
                        this.m_Map = map;
                    }

                    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                    public IAspect[] View
                    {
                        get { return this.m_Map.m_Aspectization.ToArray(); }
                    }
                }
            }
        }
    }
}