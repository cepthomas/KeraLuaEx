using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Collections;


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
        public enum TableType { Unknown, Dictionary, IntList, DoubleList, StringList }; // FUTURE ListTableEx?

        /// <summary>All the names.</summary>
        public List<string> Names { get { var n = _elements.Keys.ToList(); return n; } } 

        /// <summary>Number of values.</summary>
        public int Count { get { return _elements.Count; } }

        /// <summary>Indexer.</summary>
        public object this[string key] { get { return _elements[key]; } }
        #endregion

        #region Public API
        /// <summary>
        /// Populate from a lua table on the top of the stack. FUTURE arbitrary indexes.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="index">Table is in the stack at index.</param> TODO1 index not used!
        /// <exception cref="SyntaxException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public TableEx(Lua l, int index)
        {
            // Check for valid value.
            if (l.Type(-1)! != LuaType.Table)
            {
                throw new InvalidOperationException($"Expected table at top of stack but is {l.Type(-1)}");
            }

            // First key.
            l.PushNil();

            // Key(-1) is replaced by the next key(-1) in table(-2).
            while (l.Next(-2))
            {
                // Make the processing easier to digest.

                // Get key info (-2).
                LuaType keyType = l.Type(-2);
                string? skey = keyType == LuaType.String ? l.ToStringL(-2) : null;
                int? ikey = keyType == LuaType.Number && l.IsInteger(-2) ? l.ToInteger(-2) : null;

                // Get val info (-1).
                LuaType valType = l.Type(-1);

                int? ival = valType == LuaType.Number && l.IsInteger(-1) ? l.ToInteger(-1) : null;
                double? dval = valType == LuaType.Number ? l.ToNumber(-1) : null;
                string? sval = valType == LuaType.String ? l.ToStringL(-1) : null;

                bool isDict = true; // default assumption

                switch (Type)
                {
                    case TableType.Unknown:
                        if (ikey is not null) // Probably a list.
                        {
                            if (ikey == 1) // Assume start of a list - determine value type.
                            {
                                isDict = false;
                                if (ival is not null)
                                {
                                    Type = TableType.IntList;
                                    _elements.Add(ikey.ToString()!, ival);
                                }
                                else if (dval is not null)
                                {
                                    Type = TableType.DoubleList;
                                    _elements.Add(ikey.ToString()!, dval);
                                }
                                else if (sval is not null)
                                {
                                    Type = TableType.StringList;
                                    _elements.Add(ikey.ToString()!, sval);
                                }
                                else
                                {
                                    throw new SyntaxException($"Unsupported list type {valType}");
                                }
                            }
                        }
                        else if (skey is not null) // It's a dict.
                        {
                             // Nothing to do right now - see below.
                        }
                        else // Invalid key type.
                        {
                            throw new SyntaxException($"Invalid key type {keyType}");
                        }
                        break;

                    case TableType.IntList:
                    case TableType.DoubleList:
                    case TableType.StringList:
                        // Lists must have consecutive integer keys.
                        if (ikey is not null && ikey == _elements.Count + 1)
                        {
                            isDict = false;

                            if (Type == TableType.IntList && ival is not null)
                            {
                                _elements.Add(ikey.ToString()!, ival);
                            }
                            else if (Type == TableType.DoubleList && dval is not null)
                            {
                                _elements.Add(ikey.ToString()!, dval);
                            }
                            else if (Type == TableType.StringList && sval is not null)
                            {
                                _elements.Add(ikey.ToString()!, sval);
                            }
                            else
                            {
                                throw new SyntaxException($"Inconsistent list value type {valType}");
                            }
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
                    object? val = valType switch
                    {
                        //LuaType.Nil => null,
                        LuaType.String => l.ToStringL(-1),
                        LuaType.Number => l.DetermineNumber(-1),
                        LuaType.Boolean => l.ToBoolean(-1),
                        LuaType.Table => l.ToTableEx(-1), // recursion!
                        //LuaType.Function => l.ToCFunction(-1),
                        _ => null //throw new SyntaxException($"Unsupported value type {l.Type(-1)}") // others are invalid
                    };


                    if (val is not null)
                    {
                        //Debug.WriteLine($"=== depth:{_depth} key:{skey} valType:{valType} val:{val}");
                        _elements.Add(skey, val);
                    }
                    //_elements.Add(skey, val ?? "Ooopsy daisey");
                }

                // Remove value(-1), now key on top at(-1).
                l.Pop(1);
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
            if (!(tv.Equals(typeof(string)) || tv.Equals(typeof(double)) || tv.Equals(typeof(int))))
            {
                throw new InvalidOperationException($"Unsupported list value type [{tv}]");
            }

            List<T> list = new();
            foreach (var kv in _elements)
            {
                list.Add((T)kv.Value);
            }

            return list;
        }

        /// <summary>
        /// Dump the table into a readable string.
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
                    ls.Add($"{sindent}{tableName}(Empty)");
                    break;
            }

            return string.Join(Environment.NewLine, ls);
        }

        ///// <summary>
        ///// Readable.
        ///// </summary>
        ///// <returns></returns>
        //public override string ToString()
        //{
        //    //return "TableEx";
        //    return Dump("TableEx");
        //    //List<string> ls = new() { "TableEx" };
        //    //foreach (var f in _elements)
        //    //{
        //    //    ls.Add($"{f.Key}:{f.Value}");
        //    //}
        //    //return string.Join (" ", ls);
        //}
        #endregion
    }
}