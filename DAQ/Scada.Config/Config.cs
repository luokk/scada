using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Config
{
    public enum ReadLineResult
    {
        Null,
        Error,
        OK
    }

    public enum SectionType
    {
        None,
        Spaces,
        Title,
        Comment,
        KeyWithStringValue,
        KeyWithArrayValue,
        KeyWithDictValue,
    }

    public enum ScadaValueType
    {
        Null,
        String,
        Array,
        Dict
    }

    public interface IValue
    {
        ScadaValueType Type { get; }
    }

    public class NullValue : IValue
    {
        public static NullValue Null = new NullValue();

        public ScadaValueType Type
        {
            get { return ScadaValueType.Null; }
        }
    }

    public class StringValue : IValue
    {
        private string value = null;

        public StringValue(string value)
        {
            this.value = value;
        }

        public ScadaValueType Type
        {
            get { return ScadaValueType.String; }
        }

        public override string ToString()
        {
            return this.value;
        }

        public static implicit operator string(StringValue sv)
        {
            return (sv != null) ? sv.ToString() : null;
        }

        public static implicit operator int(StringValue sv)
        {
            return (sv != null) ? int.Parse(sv.ToString()) : int.MaxValue;
        }

        public static implicit operator double(StringValue sv)
        {
            return (sv != null) ? double.Parse(sv.ToString()) : double.MaxValue;
        }
    }

    public class ArrayValue : IValue
    {
        private List<string> list = new List<string>();
        public void Add(string item)
        {
            list.Add(item);
        }
        public int Count
        {
            get { return list.Count; }
        }
        public string Item(int index)
        {
            return list[index];
        }

        public ScadaValueType Type
        {
            get { return ScadaValueType.Array; }
        }
    }

    public class DictValue : IValue
    {
        public ScadaValueType Type
        {
            get { return ScadaValueType.Dict; }
        }
    }

}
