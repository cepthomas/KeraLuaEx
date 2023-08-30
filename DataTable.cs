using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;

namespace KeraLuaEx
{
    /// <summary>C# representation of a lua table.
    /// Intended for carrying data only. Supported value types: 
    /// LuaType             C# Type
    /// -----------------   ---------------
    /// LuaType.Nil         null
    /// LuaType.String      string
    /// LuaType.Boolean     bool
    /// LuaType.Number      int or double
    /// LuaType.Table       List or Dictionary
    ///
    /// Lua tables support both array and map types. To be considered an array:
    ///  - all keys must be integers and not sparse.
    ///  - all values must be the same type.
    /// To be considered a map:
    ///  - all keys must be strings and unique.
    ///  - values can be any supported type.
    /// </summary>
    public class DataTableX
    {
        #region Types
        /// <summary>What am I?</summary>
        public enum TableType { Unknown, List, Dictionary, Invalid }

        /// <summary>Representation of a lua table field.</summary>
        public record TableField(object Key, object Value);
        #endregion

        #region Fields
        /// <summary>The collection of fields.</summary>
        readonly List<TableField> _tableFields = new();

        /// <summary>Ensure list homogenity.</summary>
        Type? _listValueType = null;
        #endregion

        #region Properties
        /// <summary>Representation of a lua table.</summary>
        public TableType Type { get; private set; } = TableType.Unknown;

        /// <summary>How many elements.</summary>
        public int Count { get { return _tableFields.Count; } }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataTableX()
        {
        }

        /// <summary>
        /// Construct from a list.
        /// </summary>
        /// <param name="vals"></param>
        public DataTableX(List<double> vals)
        {
            vals.ForEach(v => _tableFields.Add(new(vals.Count + 1, v)));
            Type = TableType.List;
        }

        /// <summary>
        /// Construct from a list.
        /// </summary>
        /// <param name="vals"></param>
        public DataTableX(List<int> vals)
        {
            vals.ForEach(v => _tableFields.Add(new(vals.Count + 1, v)));
            Type = TableType.List;
        }

        /// <summary>
        /// Construct from a list.
        /// </summary>
        /// <param name="vals"></param>
        public DataTableX(List<string> vals)
        {
            vals.ForEach(v => _tableFields.Add(new(vals.Count + 1, v)));
            Type = TableType.List;
        }

        /// <summary>
        /// Construct from a dictionary.
        /// </summary>
        /// <param name="vals"></param>
        public DataTableX(Dictionary<string, object> vals)
        {
            vals.ToList().ForEach(kv => _tableFields.Add(new(kv.Key, kv.Value)));
            Type = TableType.Dictionary;
        }
        #endregion

        #region Iteration
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerator<DataTableX> GetEnumerator()
        //{
        //    return new DataTableEnumerator(_tableFields);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return (IEnumerator)GetEnumerator();
        //}

        /// <summary>
        /// Indexer for string fields of the table. Used for dictionary only.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object? this[string index]
        {
            get
            {
                if (Type == TableType.Dictionary)
                {
                    // If key exists return it else null.
                    var match = _tableFields.Where(f => f.Key as string == index);
                    return match.Any() ? match.First().Value : null;
                }
                else
                {
                    throw new InvalidOperationException($"This is not a dictionary table");
                }
            }
        }

        /// <summary>
        /// Indexer for numeric fields of the table. Used for array only.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object? this[int index]
        {
            get
            {
                if (Type == TableType.List)
                {
                    // If key exists return it else null.
                    object? res = (index < 0 || index >= _tableFields.Count) ? null : _tableFields[index].Value;
                    return res;
                }
                else
                {
                    throw new InvalidOperationException($"This is not a dictionary table");
                }
            }
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Add a value to the table. Checks consistency on the fly.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddVal(object key, object val)
        {
            string serr = "";

            switch (Type)
            {
                case TableType.Unknown:
                    // New table. Determine type from key.
                    switch (key)
                    {
                        case string _:
                            Type = TableType.Dictionary;
                            _tableFields.Add(new(key, val));
                            break;

                        case int _:
                            Type = TableType.List;
                            _listValueType = val.GetType();
                            _tableFields.Add(new(key, val));
                            break;

                        default:
                            serr = $"Invalid key type {key.GetType()}";
                            break;
                    }
                    break;

                case TableType.List:
                    if (key.GetType() == typeof(int) && val.GetType() == _listValueType)
                    {
                        _tableFields.Add(new(key, val));
                    }
                    else
                    {
                        serr = $"Mismatched table key type:{key.GetType()} for key:{key}";
                    }
                    break;

                case TableType.Dictionary:
                    if (key.GetType() == typeof(string))
                    {
                        _tableFields.Add(new(key, val));
                    }
                    else
                    {
                        serr = $"Mismatched table key type:{key.GetType()} for key:{key}";
                    }
                    break;

                case TableType.Invalid:
                    serr = $"Invalid table";
                    break;
            }

            if (serr.Length > 0)
            {
                Type = TableType.Invalid;
                throw new ArgumentException(serr);
            }
        }

        /// <summary>
        /// Return a readonly list representing the lua table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ReadOnlyCollection<object> AsList()
        {
            if (Type != TableType.List)
            {
                throw new InvalidOperationException($"This is not a list table");
            }

            List<object> list = new();
            _tableFields.ForEach(kv => list.Add(kv.Value));
            var ret = new ReadOnlyCollection<object>(list);
            return ret;
        }

        /// <summary>
        /// Return a readonly dict representing the lua table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ReadOnlyDictionary<string, object> AsDict()
        {
            if (Type != TableType.Dictionary)
            {
                throw new InvalidOperationException($"This is not a dictionary table");
            }

            Dictionary<string, object> dict = new();
            _tableFields.ForEach(f => dict[f.Key.ToString()!] = f.Value);
            var ret = new ReadOnlyDictionary<string, object>(dict);
            return ret;
        }

        /// <summary>
        /// Dump the table into a readable form.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="indent">Indent level for table</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string Format(string tableName, int indent = 0)
        {
            List<string> ls = new();
            var sindent = indent > 0 ? new(' ', 4 * indent) : "";

            switch (Type)
            {
                case TableType.List:
                    List<object> lvals = new();
                    _tableFields.ForEach(f => lvals.Add(f.Value));
                    ls.Add($"{sindent}{tableName}(array):[ {string.Join(", ", lvals)} ]");
                    break;

                case TableType.Dictionary:
                    ls.Add($"{sindent}{tableName}(dict):");
                    sindent += "    ";

                    foreach (var f in _tableFields)
                    {
                        switch (f.Value)
                        {
                            case null:          ls.Add($"{sindent}{f.Key}(null):");      break;
                            case string s:      ls.Add($"{sindent}{f.Key}(string):{s}"); break;
                            case bool b:        ls.Add($"{sindent}{f.Key}(bool):{b}");   break;
                            case int l:        ls.Add($"{sindent}{f.Key}(int):{l}");   break;
                            case double d:      ls.Add($"{sindent}{f.Key}(double):{d}"); break;
                            case DataTableX t:   ls.Add($"{t.Format($"{f.Key}", indent + 1)}");      break; // recursion!
                            default:            throw new InvalidOperationException($"Unsupported type {f.Value.GetType()} for {f.Key}"); // should never happen
                        }
                    }
                    break;

                case TableType.Unknown:
                case TableType.Invalid:
                    ls.Add($"Table is {Type}");
                    break;
            }

            return string.Join(Environment.NewLine, ls);
        }
        #endregion


        //// When you implement IEnumerable, you must also implement IEnumerator.
        //public class DataTableEnumerator : IEnumerator
        //{
        //    List<TableField> _fields;

        //    // Enumerators are positioned before the first element
        //    // until the first MoveNext() call.
        //    int position = -1;

        //    public DataTableEnumerator(List<TableField> list)
        //    {
        //        _fields = list;
        //    }

        //    public bool MoveNext()
        //    {
        //        position++;
        //        return position < _fields.Count;
        //    }

        //    public void Reset()
        //    {
        //        position = -1;
        //    }

        //    object IEnumerator.Current
        //    {
        //        get { return Current; }
        //    }

        //    public TableField Current
        //    {
        //        get
        //        {
        //            if (position < _fields.Count)
        //            {
        //                return _fields[position];
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException();
        //            }
        //        }
        //    }
        //}
    }
}