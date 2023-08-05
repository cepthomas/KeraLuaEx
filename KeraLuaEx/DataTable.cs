using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;


namespace KeraLuaEx
{
    /// <summary>C# representation of a lua table.
    /// Intended for carrying data only. Supported value types: 
    /// LuaType             C# Type
    /// -----------------   ---------------
    /// LuaType.Nil         null
    /// LuaType.String      string
    /// LuaType.Boolean     bool
    /// LuaType.Number      long or double
    /// LuaType.Table       List or Dictionary
    ///
    /// Lua tables support both array and map types. To be considered an array:
    ///  - all keys must be integers and not sparse.
    ///  - all values must be the same type.
    /// To be considered a map:
    ///  - all keys must be strings and unique.
    ///  - values can be any supported type.
    /// </summary>
    public class DataTable
    {
        #region Types
        /// <summary>What am I?</summary>
        public enum TableType { Unknown, List, Dictionary, Invalid }

        /// <summary>Representation of a lua table field.</summary>
        record TableField(object Key, object Value);
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
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataTable()
        {
        }

        /// <summary>
        /// Construct from a list.
        /// </summary>
        /// <param name="vals"></param>
        public DataTable(List<double> vals)
        {
            vals.ForEach(v => _tableFields.Add(new(vals.Count + 1, v)));
            Type = TableType.List;
        }

        /// <summary>
        /// Construct from a list.
        /// </summary>
        /// <param name="vals"></param>
        public DataTable(List<long> vals)
        {
            vals.ForEach(v => _tableFields.Add(new(vals.Count + 1, v)));
            Type = TableType.List;
        }

        /// <summary>
        /// Construct from a list.
        /// </summary>
        /// <param name="vals"></param>
        public DataTable(List<string> vals)
        {
            vals.ForEach(v => _tableFields.Add(new(vals.Count + 1, v)));
            Type = TableType.List;
        }

        /// <summary>
        /// Construct from a dictionary.
        /// </summary>
        /// <param name="vals"></param>
        public DataTable(Dictionary<string, object> vals)
        {
            vals.ToList().ForEach(kv => _tableFields.Add(new(kv.Key, kv.Value)));
            Type = TableType.Dictionary;
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

                        case long _:
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
                    if (key.GetType() == typeof(long) && val.GetType() == _listValueType)
                    {
                        _tableFields.Add(new(key, val));
                    }
                    else
                    {
                        serr = $"Mismatched table key type:{key.GetType()}";//TODO1 needs more info - table name
                    }
                    break;

                case TableType.Dictionary:
                    if (key.GetType() == typeof(string))
                    {
                        _tableFields.Add(new(key, val));
                    }
                    else
                    {
                        serr = $"Mismatched table key type:{key.GetType()} for key:{key}";//TODO1 needs more info - table name
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
        /// Return a list representing the lua table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public List<object> AsList()
        {
            // Convert and return.
            List<object> ret = new();

            if (Type == TableType.List)
            {
                foreach (var v in _tableFields)
                {
                    ret.Add(v.Value);
                }
            }
            else
            {
                throw new InvalidOperationException($"This is not a list table");
            }

            return ret;
        }

        /// <summary>
        /// Return a dict representing the lua table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Dictionary<string, object> AsDict()
        {
            // Clone and return.
            Dictionary<string, object> ret = new();

            if (Type == TableType.Dictionary)
            {
                _tableFields.ForEach(f => ret[f.Key.ToString()!] = f.Value);
            }
            else
            {
                throw new InvalidOperationException($"This is not a dictionary table");
            }

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

            switch (Type)
            {
                case TableType.List:
                    List<object> lvals = new();
                    _tableFields.ForEach(f => lvals.Add(f.Value));
                    ls.Add($"{Indent(indent)}{tableName}(array):[ {string.Join(", ", lvals)} ]");
                    break;

                case TableType.Dictionary:
                    ls.Add($"{Indent(indent)}{tableName}(dict):");
                    indent += 1;

                    foreach (var f in _tableFields)
                    {
                        switch (f.Value)
                        {
                            case null:     ls.Add($"{Indent(indent)}{f.Key}(null):");      break;
                            case string s: ls.Add($"{Indent(indent)}{f.Key}(string):{s}"); break;
                            case bool b:   ls.Add($"{Indent(indent)}{f.Key}(bool):{b}");   break;
                            case int i:    ls.Add($"{Indent(indent)}{f.Key}(int):{i}");    break;
                            case long l:   ls.Add($"{Indent(indent)}{f.Key}(long):{l}");   break;
                            case double d: ls.Add($"{Indent(indent)}{f.Key}(double):{d}"); break;
                            case DataTable t:
                                //ls.Add($"{Indent(indent)}{key.ToString()}(dict):");
                                ls.Add($"{t.Format($"{f.Key}", indent)}");
                                break; // recursion!
                            default: throw new InvalidOperationException($"Unsupported type {f.Value.GetType()} for {f.Key}"); // should never happen
                        }
                    }
                    break;

                case TableType.Unknown:
                case TableType.Invalid:
                    ls.Add($"Table is {Type}");
                    break;
            }

            static string Indent(int indent)
            {
                return indent > 0 ? new(' ', 4 * indent) : "";
            }

            return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// Create a table from json.
        /// </summary>
        /// <param name="sjson">Json string</param>
        /// <returns>New DataTable</returns>
        public static DataTable FromJson(string sjson)//TODO1
        {
            DataTable table = new();

            // Uses Utf8JsonReader directly. https://marcroussy.com/2020/08/17/deserialization-with-system-text-json/

            var options = new JsonReaderOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
            var bytes = Encoding.ASCII.GetBytes(sjson);
            var reader = new Utf8JsonReader(bytes, options);
            while (reader.Read())
            {
                //Debug.Write(reader.GetString());
                Debug.WriteLine($"{reader.TokenType}:{reader.TokenStartIndex}");

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        var str = reader.GetString();
                        Debug.WriteLine($"PropertyName({reader.TokenStartIndex}):{str}");
                        break;

                    case JsonTokenType.String:
                        str = reader.GetString();
                        Debug.WriteLine($"String({reader.TokenStartIndex}):{str}");
                        //Console.Write(text);
                        break;

                    case JsonTokenType.Number:
                        if (reader.TryGetInt64(out long value))
                        {
                            Debug.WriteLine($"Long({reader.TokenStartIndex}):{value}");
                        }
                        else
                        {
                            double dblValue = reader.GetDouble();
                            Debug.WriteLine($"Double({reader.TokenStartIndex}):{dblValue}");
                        }
                        break;

                        // etc....
                        // None = 0,
                        //    There is no value (as distinct from System.Text.Json.JsonTokenType.Null). This is the default token type if no data has been read.
                        // StartObject = 1,
                        //    The token type is the start of a JSON object.
                        // EndObject = 2,
                        //    The token type is the end of a JSON object.
                        // StartArray = 3,
                        //    The token type is the start of a JSON array.
                        // EndArray = 4,
                        //    The token type is the end of a JSON array.
                        // PropertyName = 5,
                        //    The token type is a JSON property name.
                        // Comment = 6,
                        //    The token type is a comment string.
                        // String = 7,
                        //    The token type is a JSON string.
                        // Number = 8,
                        //    The token type is a JSON number.
                        // True = 9,
                        //    The token type is the JSON literal true.
                        // False = 10,
                        //    The token type is the JSON literal false.
                        // Null = 11
                        //    The token type is the JSON literal null.
                }
            }

            return table;
        }

        /// <summary>
        /// Create json from table.
        /// </summary>
        /// <returns>Json string</returns>
        public string ToJson()
        {
            return "TODO1";
        }        
        #endregion
    }
}