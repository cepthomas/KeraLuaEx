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
        public enum TableType { Unknown, Dictionary, IntList, DoubleList, StringList };

        #region Fields
        /// <summary>Dictionary used to store the data.</summary>
        readonly Dictionary<string, object> _elements = new();
        #endregion

        #region Properties
        /// <summary>What this represents.</summary>
        public TableType Type { get; private set; }

        /// <summary>Number of values.</summary>
        public int Count { get { return _elements.Count; } }

        /// <summary>Indexer.</summary>
        public object this[string key] { get { return _elements[key]; } }
        #endregion

        #region Public api.
        /// <summary>
        /// Manufacture contents from a lua table on the top of the stack.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="depth"></param>
        /// <param name="inclFuncs"></param>
        /// <exception cref="SyntaxException"></exception>
        public void Create(Lua l, int depth, bool inclFuncs) // TODO1 refactor this, remove args?
        {
            if (depth > 0)
            {
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
                                    // Get type of value(-1).
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
                                            throw new SyntaxException($"Unsupported list type {l.Type(-1)}");
                                    }
                                }
                                else
                                {
                                    // Must be a dictionary.
                                    Type = TableType.Dictionary;
                                    AddToDict();
                                }
                            }
                            else if (l.IsString(-2))
                            {
                                Type = TableType.Dictionary;
                                AddToDict();
                            }
                            else
                            {
                                throw new SyntaxException($"Invalid key type {l.Type(-2)}");

                            }
                            break;

                        case TableType.Dictionary:
                            AddToDict();
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
                                    throw new SyntaxException($"Inconsistent value type {l.Type(-1)}");
                                }
                            }
                            else
                            {
                                // Assume it must be a dictionary after all.
                                Type = TableType.Dictionary;
                                AddToDict();
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
                                    throw new SyntaxException($"Inconsistent value type {l.Type(-1)}");
                                }
                            }
                            else
                            {
                                // Assume it must be a dictionary after all.
                                Type = TableType.Dictionary;
                                AddToDict();
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
                                    throw new SyntaxException($"Inconsistent value type {l.Type(-1)}");
                                }
                            }
                            else
                            {
                                // Assume it must be a dictionary after all.
                                Type = TableType.Dictionary;
                                AddToDict();
                            }
                            break;
                    }

                    // Remove value(-1), now key on top at(-1).
                    l.Pop(1);
                }
            }

            // Local function.
            void AddToDict()
            {
                object? val = l.Type(-1)! switch
                {
                    //LuaType.Nil => null,
                    LuaType.String => l.ToStringL(-1),
                    LuaType.Number => l.DetermineNumber(-1),
                    LuaType.Boolean => l.ToBoolean(-1),
                    LuaType.Table => l.ToTableEx(depth - 1, inclFuncs), // recursion!
                    LuaType.Function => inclFuncs ? l.ToCFunction(-1) : null,
                    _ => throw new SyntaxException($"Unsupported value type {l.Type(-1)}") // invalid others
                };
                var skey = l.ToStringL(-2);
                _elements.Add(skey!, val!);
            }
        }

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
                throw new InvalidOperationException($"Unsupported value type {tv}");
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
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
                            default: throw new InvalidOperationException($"Unsupported type {f.Value.GetType()} for {f.Key}"); // should never happen
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