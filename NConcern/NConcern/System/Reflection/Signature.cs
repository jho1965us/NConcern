using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Reflection
{
    [DebuggerDisplay("({this.Instance == null ? System.String.Empty : System.String.Concat(\"this \", System.String.Join(\", \",  this.m_Signature as System.Collections.Generic.IEnumerable<System.Type>)), nq})")]
    internal sealed class Signature : IEnumerable<Type>
    {
        static public implicit operator Type[](Signature signature)
        {
            return signature == null ? null : signature.m_Signature;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Collection<Type> m_Signature;
        public readonly Type Instance;
        public readonly IList<ParameterInfo> Parameters;
        public readonly int Length;

        public Signature(Type instance, ParameterInfo[] parameters)
        {
            if (instance == null)
            {
                if (parameters == null)
                {
                    this.m_Signature = new Collection<Type>();
                    this.Parameters = new ParameterInfo[0];
                    return;
                }
                this.m_Signature = new Collection<Type>(parameters.Select(parameter => parameter.ParameterType));
                this.Parameters = parameters;
                this.Length = parameters.Length;
                return;
            }
            if (parameters == null)
            {
                this.Instance = instance;
                this.m_Signature = new Collection<Type>(new Type[] { instance });
                this.Parameters = parameters;
                this.Length = 1;
                return;
            }
            this.Instance = instance;
            var _signature = new Type[this.Length = parameters.Length + 1];
            _signature[0] = instance;
            for (var _index = 0; _index < parameters.Length; _index++) { _signature[_index + 1] = parameters[_index].ParameterType; }
            this.m_Signature = new Collection<Type>(_signature);
            this.Parameters = parameters;
        }

        public Signature(ParameterInfo[] parameters)
        {
            if (parameters == null)
            {
                this.m_Signature = new Collection<Type>();
                this.Parameters = parameters;
                return;
            }
            this.m_Signature = new Collection<Type>(parameters.Select(parameter => parameter.ParameterType));
            this.Parameters = parameters;
            this.Length = parameters.Length;
        }

        public Signature(Type instance)
        {
            this.Instance = instance;
            this.m_Signature = new Collection<Type>(new Type[] { instance });
            this.Parameters = new ParameterInfo[0];
            this.Length = 1;
        }

        public Type this[int index]
        {
            get { return this.m_Signature[index]; }
        }
        
        public IEnumerator<Type> GetEnumerator()
        {
            return this.m_Signature.Enumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_Signature.Enumerator();
        }

        override public int GetHashCode()
        {
            var _signature = this.m_Signature;
            var _length = this.Length;
            var _hashcode = _length;
            for (var _index = 0; _index < _length; _index++) { _hashcode = _hashcode ^ _signature[_index].GetHashCode(); }
            return _hashcode;
        }

        override public bool Equals(object value)
        {
            var _signature = value as Signature;
            if (_signature == null || _signature.Length != this.Length) { return false; }
            for (var _index = 0; _index < _signature.Length; _index++) { if (_signature[_index] != this.m_Signature[_index]) { return false; } }
            return true;
        }
    }
}
