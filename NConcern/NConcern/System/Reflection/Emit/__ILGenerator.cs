using System;
using System.ComponentModel;
using NConcern;

namespace System.Reflection.Emit
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    static internal class ___ILGenerator
    {
        static public void Emit(this ILGenerator body, Action<ILGenerator> instruction)
        {
            instruction(body);
        }

        static public void Emit(this ILGenerator body, IntPtr function, Type type, Signature signature)
        {
            switch (IntPtr.Size)
            {
                case 4: body.Emit(OpCodes.Ldc_I4, function.ToInt32()); break;
                case 8: body.Emit(OpCodes.Ldc_I8, function.ToInt64()); break;
                default: throw new NotSupportedException();
            }
            body.EmitCalli(OpCodes.Calli, CallingConventions.Standard, type, signature, null);
        }

        static public void Emit(this ILGenerator body, Signature signature, bool reflective)
        {
            if (reflective)
            {
                if (signature.Instance == null)
                {
                    body.Emit(OpCodes.Ldnull);
                    Emit_Ldc_I4(body, signature.Parameters.Count);
                    body.Emit(OpCodes.Newarr, Metadata<object>.Type);
                    for (var _index = 0; _index < signature.Parameters.Count; _index++)
                    {
                        var _parameter = signature.Parameters[_index].ParameterType;
                        body.Emit(OpCodes.Dup);
                        if (_parameter.IsByRef && signature.Parameters[_index].IsOut)
                        {
                            Emit_Ldc_I4(body, _index);
                            EmitLoadDefault(body, _parameter);
                        }
                        else
                        {
                            switch (_index)
                            {
                                case 0:
                                    body.Emit(OpCodes.Ldc_I4_0);
                                    body.Emit(OpCodes.Ldarg_0);
                                    break;
                                case 1:
                                    body.Emit(OpCodes.Ldc_I4_1);
                                    body.Emit(OpCodes.Ldarg_1);
                                    break;
                                case 2:
                                    body.Emit(OpCodes.Ldc_I4_2);
                                    body.Emit(OpCodes.Ldarg_2);
                                    break;
                                case 3:
                                    body.Emit(OpCodes.Ldc_I4_3);
                                    body.Emit(OpCodes.Ldarg_3);
                                    break;
                                case 4:
                                    body.Emit(OpCodes.Ldc_I4_4);
                                    body.Emit(OpCodes.Ldarg, _index);
                                    break;
                                case 5:
                                    body.Emit(OpCodes.Ldc_I4_5);
                                    body.Emit(OpCodes.Ldarg, _index);
                                    break;
                                case 6:
                                    body.Emit(OpCodes.Ldc_I4_6);
                                    body.Emit(OpCodes.Ldarg, _index);
                                    break;
                                case 7:
                                    body.Emit(OpCodes.Ldc_I4_7);
                                    body.Emit(OpCodes.Ldarg, _index);
                                    break;
                                case 8:
                                    body.Emit(OpCodes.Ldc_I4_8);
                                    body.Emit(OpCodes.Ldarg, _index);
                                    break;
                                default:
                                    body.Emit(OpCodes.Ldc_I4, _index);
                                    body.Emit(OpCodes.Ldarg, _index);
                                    break;
                            }
                            if (_parameter.IsValueType) { body.Emit(OpCodes.Box, _parameter); }
                            else if (_parameter.IsByRef) { EmitLoadByRef(body, _parameter); }
                            else { body.Emit(OpCodes.Castclass, Metadata<object>.Type); }
                        }
                        body.Emit(OpCodes.Stelem_Ref);
                    }
                }
                else
                {
                    body.Emit(OpCodes.Ldarg_0);
                    if (signature.Instance != Metadata<object>.Type)
                    {
                        if (signature.Instance.IsValueType) { body.Emit(OpCodes.Box, signature.Instance); }
                        else { body.Emit(OpCodes.Castclass, Metadata<object>.Type); }
                    }
                    Emit_Ldc_I4(body, signature.Parameters.Count);
                    body.Emit(OpCodes.Newarr, Metadata<object>.Type);
                    for (var _index = 0; _index < signature.Parameters.Count; _index++)
                    {
                        var _parameter = signature.Parameters[_index].ParameterType;
                        body.Emit(OpCodes.Dup);
                        if (_parameter.IsByRef && signature.Parameters[_index].IsOut)
                        {
                            Emit_Ldc_I4(body, _index);
                            EmitLoadDefault(body, _parameter);
                        }
                        else
                        {
                            switch (_index)
                            {
                                case 0:
                                    body.Emit(OpCodes.Ldc_I4_0);
                                    body.Emit(OpCodes.Ldarg_1);
                                    break;
                                case 1:
                                    body.Emit(OpCodes.Ldc_I4_1);
                                    body.Emit(OpCodes.Ldarg_2);
                                    break;
                                case 2:
                                    body.Emit(OpCodes.Ldc_I4_2);
                                    body.Emit(OpCodes.Ldarg_3);
                                    break;
                                case 3:
                                    body.Emit(OpCodes.Ldc_I4_3);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                                case 4:
                                    body.Emit(OpCodes.Ldc_I4_4);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                                case 5:
                                    body.Emit(OpCodes.Ldc_I4_5);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                                case 6:
                                    body.Emit(OpCodes.Ldc_I4_6);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                                case 7:
                                    body.Emit(OpCodes.Ldc_I4_7);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                                case 8:
                                    body.Emit(OpCodes.Ldc_I4_8);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                                default:
                                    body.Emit(OpCodes.Ldc_I4, _index);
                                    body.Emit(OpCodes.Ldarg, _index + 1);
                                    break;
                            }
                            if (_parameter.IsValueType) { body.Emit(OpCodes.Box, _parameter); }
                            else if (_parameter.IsByRef) { EmitLoadByRef(body, _parameter); }
                            else { body.Emit(OpCodes.Castclass, Metadata<object>.Type); }
                        }
                        body.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }
            else
            {
                for (var _index = 0; _index < signature.Length; _index++)
                {
                    switch (_index)
                    {
                        case 0: body.Emit(OpCodes.Ldarg_0); break;
                        case 1: body.Emit(OpCodes.Ldarg_1); break;
                        case 2: body.Emit(OpCodes.Ldarg_2); break;
                        case 3: body.Emit(OpCodes.Ldarg_3); break;
                        default: body.Emit(OpCodes.Ldarg, _index); break;
                    }
                }
            }
        }

        static private void Emit_Ldc_I4(ILGenerator body, int index)
        {
            switch (index)
            {
                case 0: body.Emit(OpCodes.Ldc_I4_0); break;
                case 1: body.Emit(OpCodes.Ldc_I4_1); break;
                case 2: body.Emit(OpCodes.Ldc_I4_2); break;
                case 3: body.Emit(OpCodes.Ldc_I4_3); break;
                case 4: body.Emit(OpCodes.Ldc_I4_4); break;
                case 5: body.Emit(OpCodes.Ldc_I4_5); break;
                case 6: body.Emit(OpCodes.Ldc_I4_6); break;
                case 7: body.Emit(OpCodes.Ldc_I4_7); break;
                case 8: body.Emit(OpCodes.Ldc_I4_8); break;
                default: body.Emit(OpCodes.Ldc_I4, index); break;
            }
        }

        static private void EmitLoadByRef(ILGenerator body, Type parameter)
        {
            var _elementType = parameter.GetElementType();
            if (_elementType.IsValueType)
            {
                if (_elementType == typeof(byte) || _elementType == typeof(bool))
                {
                    body.Emit(OpCodes.Ldind_U1);
                }
                else if (_elementType == typeof(sbyte))
                {
                    body.Emit(OpCodes.Ldind_I1);
                }
                else if (_elementType == typeof(short))
                {
                    body.Emit(OpCodes.Ldind_I2);
                }
                else if (_elementType == typeof(ushort) || _elementType == typeof(char))
                {
                    body.Emit(OpCodes.Ldind_U2);
                }
                else if (_elementType == typeof(int))
                {
                    body.Emit(OpCodes.Ldind_I4);
                }
                else if (_elementType == typeof(uint))
                {
                    body.Emit(OpCodes.Ldind_U4);
                }
                else if (_elementType == typeof(long) || _elementType == typeof(ulong))
                {
                    body.Emit(OpCodes.Ldind_I8);
                }
                else if (_elementType == typeof(double))
                {
                    body.Emit(OpCodes.Ldind_R8);
                }
                else if (_elementType == typeof(float))
                {
                    body.Emit(OpCodes.Ldind_R4);
                }
                else
                {
                    body.Emit(OpCodes.Ldobj, _elementType);
                }

                body.Emit(OpCodes.Box, _elementType);
            }
            else
            {
                body.Emit(OpCodes.Ldind_Ref);
            }
        }
        static private void EmitLoadDefault(ILGenerator body, Type parameter)
        {
            var _elementType = parameter.GetElementType();
            if (_elementType.IsValueType)
            {
                if (_elementType == typeof(byte) || _elementType == typeof(bool) || _elementType == typeof(sbyte) || _elementType == typeof(short) ||
                    _elementType == typeof(ushort) || _elementType == typeof(char) || _elementType == typeof(int) || _elementType == typeof(uint))
                {
                    body.Emit(OpCodes.Ldc_I4_0);
                }
                else if (_elementType == typeof(long) || _elementType == typeof(ulong))
                {
                    body.Emit(OpCodes.Ldc_I8, 0);
                }
                else if (_elementType == typeof(double))
                {
                    body.Emit(OpCodes.Ldc_R8, 0.0);
                }
                else if (_elementType == typeof(float))
                {
                    body.Emit(OpCodes.Ldc_R4, 0.0);
                }
                else if (_elementType == typeof(decimal))
                {
                    body.Emit(OpCodes.Ldsfld, typeof(decimal).GetField(nameof(decimal.Zero)));
                }
                else
                {
                    body.Emit(OpCodes.Initobj, _elementType);
                }

                body.Emit(OpCodes.Box, _elementType);
            }
            else
            {
                body.Emit(OpCodes.Ldnull);
            }
        }
    }
}
