using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace KeraLuaEx
{
    public class TableEx
    {
        #region Fields
        /// <summary>Dictionary used to store the data.</summary>
        readonly Dictionary<string, object> _elements = new();
        #endregion

        #region Properties
        /// <summary>What this represents.</summary>
        public TableType Type { get; private set; }
        public enum TableType { Unknown, Dictionary, IntList, DoubleList, StringList };

        /// <summary>Number of values.</summary>
        public int Count { get { return _elements.Count; } }

        /// <summary>Indexer.</summary>
        public object this[string key] { get { return _elements[key]; } }
        #endregion


        #region Public API
        /// <summary>
        /// Manufacture contents from a lua table on the top of the stack.
        /// </summary>
        /// <param name="l"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public TableEx(Lua l)
        {
            // Check for valid value.
            if (l.Type(-1)! != LuaType.Table)
            {
                throw new InvalidOperationException($"Expected table at top of stack but is [{l.Type(-1)}]");
            }

            // Put a nil key on stack to mark end of iteration.
            l.PushNil();

            // Key(-1) is replaced by the next key(-1) in table(-2).
            while (l.Next(-2))
            {
                // Make the types and values easier to digest.

                // Get key info (-2).
                LuaType keyType = l.Type(-2);
                string skey = l.ToStringL(-2)!;
                int? ikey = l.ToInteger(-2);

                // Get val info (-1).
                LuaType valType = l.Type(-1);

                int? ival = valType == LuaType.Number && l.IsInteger(-1) ? l.ToInteger(-1) : null;
                double? nval = valType == LuaType.Number ? l.ToNumber(-1) : null;
                string? sval = valType == LuaType.String ? l.ToStringL(-1) : null;

                bool isDict = true; // assume default

                // switch (Type, ikey, ival, nval, sval) // TODO0 refactor/simplify
                // {
                //     case (TableType.Unknown, null, null, null, null):
                //         //xxx
                //         break;
                // }

                switch (Type)
                {
                    case TableType.Unknown:
                        if (ikey is not null)
                        {
                            if (ikey == 1) // First element gets special processing - what is this?
                            {
                                // Assume start of a list. This will be checked later.
                                isDict = false;
                                if (ival is not null)
                                {
                                    Type = TableType.IntList;
                                    _elements.Add(skey, ival);
                                }
                                else if (nval is not null)
                                {
                                    Type = TableType.DoubleList;
                                    _elements.Add(skey, nval);
                                }
                                else if (sval is not null)
                                {
                                    Type = TableType.StringList;
                                    _elements.Add(skey, sval);
                                }
                                else
                                {
                                    throw new Lua.SyntaxException($"Unsupported list type [{valType}]");
                                }
                            }
                        }
                        else if (skey is not null)
                        {
                             // It's a dict.
                        }
                        else // Bad key type.
                        {
                            throw new Lua.SyntaxException($"Unsupported key type [{keyType}]");
                        }
                        break;

                    case TableType.IntList:
                        // Lists must have consecutive integer keys.
                        if (ikey is not null && ikey == _elements.Count + 1)
                        {
                            isDict = false;
                            if (ival is null)
                            {
                                throw new Lua.SyntaxException($"Inconsistent list value type [{valType}]");
                            }
                            _elements.Add(skey, ival);
                        }
                        break;

                    case TableType.DoubleList:
                        // Lists must have consecutive integer keys.
                        if (ikey is not null && ikey == _elements.Count + 1)
                        {
                            isDict = false;
                            if (nval is null)
                            {
                                throw new Lua.SyntaxException($"Inconsistent list value type [{valType}]");
                            }
                            _elements.Add(skey, nval);
                        }
                        break;

                    case TableType.StringList:
                        // Lists must have consecutive integer keys.
                        if (ikey is not null && ikey == _elements.Count + 1)
                        {
                            isDict = false;
                            if (skey is null)
                            {
                                throw new Lua.SyntaxException($"Inconsistent list value type [{valType}]");
                            }
                            _elements.Add(skey, sval);
                        }
                        break;

                    case TableType.Dictionary:
                        // Take care of this below.
                        break;
                }

                // Common dictionary stuffing.
                if (isDict)
                {
                    Type = TableType.Dictionary;
                    //AddToDict();
                    object? val = valType switch
                    {
                        //LuaType.Nil => null,
                        LuaType.String => l.ToStringL(-1),
                        LuaType.Number => l.DetermineNumber(-1),
                        LuaType.Boolean => l.ToBoolean(-1),
                        LuaType.Table => l.ToTableEx(), // recursion!
                        LuaType.Function => l.ToCFunction(-1),
                        _ => throw new Lua.SyntaxException($"Unsupported value type [{l.Type(-1)}]") // others are invalid
                    };
                    _elements.Add(skey, val??"Ooopsy daisey");
                }

                // Remove value(-1), now key on top at(-1).
                l.Pop(1);
            }
        }

        /*
        public void Create_orig(Lua l, int indent)
        {
            // Check for valid value.
            if (l.Type(-1)! != LuaType.Table)
            {
                throw new InvalidOperationException($"Expected table at top of stack but is [{l.Type(-1)}]");
            }

            // Put a nil key on stack to mark end of iteration.
            l.PushNil();

            // Key(-1) is replaced by the next key(-1) in table(-2).
            while (l.Next(-2))
            {
                // Get key info (-2).
                LuaType keyType = l.Type(-2)!;
                var skey = l.ToStringL(-2);
                var ikey = l.ToInteger(-2);

                // Get val info (-1).
                LuaType valType = l.Type(-1)!;
                //var sval = l.ToStringL(-1);
                //var val = l.ToInteger(-1);

                switch (Type)
                {
                    case TableType.Unknown: // first element has special processing
                        if (l.IsInteger(-2))
                        {
                            if (ikey == 1)
                            {
                                // Assume start of a list.
                                switch (valType!)
                                {
                                    case LuaType.Number:
                                        if (l.IsInteger(-1))
                                        {
                                            Type = TableType.IntList;
                                            _elements.Add(ikey.ToString()!, l.ToInteger(-1)!);
                                        }
                                        else
                                        {
                                            Type = TableType.DoubleList;
                                            _elements.Add(ikey.ToString()!, l.ToNumber(-1)!);
                                        }
                                        break;

                                    case LuaType.String:
                                        Type = TableType.StringList;
                                        _elements.Add(ikey.ToString()!, l.ToStringL(-1)!);
                                        break;

                                    default:
                                        throw new Lua.SyntaxException($"Unsupported list type [{l.Type(-1)}]");
                                }
                            }
                            else
                            {
                                // Must be a dictionary.
                                Type = TableType.Dictionary;
                                AddToDict_x();
                            }
                        }
                        else if (l.IsString(-2))
                        {
                            Type = TableType.Dictionary;
                            AddToDict_x();
                        }
                        else
                        {
                            throw new Lua.SyntaxException($"Invalid key type [{l.Type(-2)}]");

                        }
                        break;

                    case TableType.IntList:
                        // Lists must have consecutive integer keys.
                        if (l.IsInteger(-2) && l.ToInteger(-2) == _elements.Count + 1)
                        {
                            if (l.IsInteger(-1))
                            {
                                _elements.Add(ikey.ToString()!, l.ToInteger(-1)!);
                            }
                            else
                            {
                                throw new Lua.SyntaxException($"Inconsistent value type [{l.Type(-1)}]");
                            }
                        }
                        else
                        {
                            // Assume it must be a dictionary after all.
                            Type = TableType.Dictionary;
                            AddToDict_x();
                        }
                        break;

                    case TableType.DoubleList:
                        // Lists must have consecutive integer keys.
                        if (l.IsInteger(-2) && l.ToInteger(-2) == _elements.Count + 1)
                        {
                            if (l.IsNumber(-1))
                            {
                                _elements.Add(ikey.ToString()!, l.ToNumber(-1)!);
                            }
                            else
                            {
                                throw new Lua.SyntaxException($"Inconsistent value type [{l.Type(-1)}]");
                            }
                        }
                        else
                        {
                            // Assume it must be a dictionary after all.
                            Type = TableType.Dictionary;
                            AddToDict_x();
                        }
                        break;

                    case TableType.StringList:
                        // Lists must have consecutive integer keys.
                        if (l.IsInteger(-2) && l.ToInteger(-2) == _elements.Count + 1)
                        {
                            if (l.IsString(-1))
                            {
                                _elements.Add(ikey.ToString()!, l.ToStringL(-1)!);
                            }
                            else
                            {
                                throw new Lua.SyntaxException($"Inconsistent value type [{l.Type(-1)}]");
                            }
                        }
                        else
                        {
                            // Assume it must be a dictionary after all.
                            Type = TableType.Dictionary;
                            AddToDict_x();
                        }
                        break;

                    case TableType.Dictionary:
                        AddToDict_x();
                        break;
                }

                // Remove value(-1), now key on top at(-1).
                l.Pop(1);
            }

            // Local function.
            void AddToDict_x()
            {
                object? val = l.Type(-1)! switch
                {
                    //LuaType.Nil => null,
                    LuaType.String => l.ToStringL(-1),
                    LuaType.Number => l.DetermineNumber(-1),
                    LuaType.Boolean => l.ToBoolean(-1),
                    LuaType.Table => l.ToTableEx(), // recursion!
                    //LuaType.Table => l.ToTableEx(indent - 1), // recursion!
                    LuaType.Function => l.ToCFunction(-1),
                    _ => throw new Lua.SyntaxException($"Unsupported value type [{l.Type(-1)}]") // others are invalid
                };
                var skey = l.ToStringL(-2);
                _elements.Add(skey!, val!);
            }
        }
        */

        /// <summary>
        /// Get a typed list - if supported.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public List<T> ToList<T>()
        {
            // Check for supported types.
            var tv = typeof(T);
            if ( !(tv.Equals(typeof(string)) || tv.Equals(typeof(double)) || tv.Equals(typeof(int))))
            {
                throw new InvalidOperationException($"Unsupported value type [{tv}]");
            }

            List<T> list = new();
            foreach (var kv in _elements)
            {
                list.Add((T)kv.Value);
            }

            return list;
        }

        /// <summary>
        /// Dump the table into a readable form.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="indent">Indent level for table</param>
        /// <returns>Formatted string.</returns>
        public string Dump(string tableName, int indent = 0)
        {
            List<string> ls = new();
            var sindent = indent > 0 ? new(' ', 4 * indent) : "";

            switch (Type)
            {
                case TableType.Dictionary:
                    ls.Add($"{sindent}{tableName}(Dictionary):");
                    sindent += "    ";

                    foreach (var f in _elements)
                    {
                        switch (f.Value)
                        {
                            case null: ls.Add($"{sindent}{f.Key}(null):"); break;
                            case string s: ls.Add($"{sindent}{f.Key}(string):{s}"); break;
                            case bool b: ls.Add($"{sindent}{f.Key}(bool):{b}"); break;
                            case int l: ls.Add($"{sindent}{f.Key}(int):{l}"); break;
                            case double d: ls.Add($"{sindent}{f.Key}(double):{d}"); break;
                            case TableEx t: ls.Add($"{t.Dump($"{f.Key}", indent + 1)}"); break; // recursion!
                            default: ls.Add($"Unsupported type {f.Value.GetType()} for {f.Key}"); break; // should never happen
                        }
                    }
                    break;

                case TableType.IntList:
                case TableType.DoubleList:
                case TableType.StringList:
                    var sname = Type.ToString().Replace("TableType.", "");
                    List<string> lvals = new();
                    foreach (var f in _elements)
                    {
                        lvals.Add(f.Value.ToString()!);
                    }
                    ls.Add($"{sindent}{tableName}({sname}):[ {string.Join(", ", lvals)} ]");
                    break;

                case TableType.Unknown:
                    ls.Add($"Empty");
                    break;
            }

            return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// Readable.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TableEx";
            //return Dump("TableEx");
        }
        #endregion
    }
}