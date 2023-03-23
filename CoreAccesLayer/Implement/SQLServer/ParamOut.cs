using System;

namespace CoreAccesLayer.Implement.SQLServer
{
    public class ParamOut
    {
        public enum Type
        {
            Out,
            InOut,
            In
        }

        private Object _value;

        public Object Valor { get { return _value; } set { _value = value; } }

        public Type InOut { get; set; }

        public int Size { get; set; }

        public ParamOut(Object value)
        {
            _value = value;
        }
    }
}