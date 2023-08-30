using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace KeraLuaEx
{
    public class TableEx : Dictionary<string, object>
    {
        bool _keysAreInt = true;
        bool _valsAreHomogenous = true;
        //LuaType? valType = null;
        Type? _nativeArrayType = null;

        public Type? ListType { get { return _nativeArrayType; } }

        public void Create(Lua l, int depth, bool inclFuncs)
        {
            if (depth > 0)
            {
                // Put a nil key on stack to mark end of iteration.
                l.PushNil();

                // Key(-1) is replaced by the next key(-1) in table(-2).
                while (l.Next(-2))
                {
                    // Get key(-2) info.
                    LuaType keyType = l.Type(-2)!;
                    _keysAreInt &= l.IsInteger(-2); //TODO0 check/enfrce consecutive values - all flavors.
                    var key = l.ToStringL(-2); // coerce to string keys for plain dictionary.

                    // Get type of value(-1).
                    LuaType valType = l.Type(-1)!;
                    // Save first.
                    //_valType ??= valType;
                    //_valsAreHomogenous &= valType == this.valType;

                    // Save the data.
                    object? val = valType switch
                    {
                        LuaType.Nil => null,
                        LuaType.String => l.ToStringL(-1),
                        LuaType.Number => l.DetermineNumber(-1),
                        LuaType.Boolean => l.ToBoolean(-1),
                        LuaType.Table => l.ToTableEx(depth - 1, inclFuncs), // recursion!
                        LuaType.Function => inclFuncs ? l.ToCFunction(-1) : null,
                        _ => null // ignore others
                    };

                    if (val is not null)
                    {
                        var t = val.GetType();
                        _nativeArrayType ??= t; // init
                        _valsAreHomogenous &= t == _nativeArrayType;

                        Add(key!, val);
                    }

                    // Remove value(-1), now key on top at(-1).
                    l.Pop(1);
                }

                // Patch up type info.
                if (!_keysAreInt || !_valsAreHomogenous)
                {
                    // Not an array.
                    _nativeArrayType = null;
                }
            }
        }

        public override string ToString()
        {
            return $"TableEx:{base.ToString()}";
        }


        public List<int> ToListInt()
        {
            if (_nativeArrayType != typeof(int))
            {
                throw new InvalidOperationException($"Not an int array");
            }

            List<int> list = new();
            foreach (var kv in this)
            {
                list.Add((int)kv.Value);
            }

            return list;
        }

        public List<double> ToListDouble()
        {
            if (_nativeArrayType != typeof(double))
            {
                throw new InvalidOperationException($"Not a double array");
            }

            List<double> list = new();
            foreach (var kv in this)
            {
                list.Add((double)kv.Value);
            }

            return list;
        }

        public List<string> ToListString()
        {
            if (_nativeArrayType != typeof(string))
            {
                throw new InvalidOperationException($"Not a string array");
            }

            List<string> list = new();
            foreach (var kv in this)
            {
                list.Add((string)kv.Value);
            }

            return list;
        }
    }
}