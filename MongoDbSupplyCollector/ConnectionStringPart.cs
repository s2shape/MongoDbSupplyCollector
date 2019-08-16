using System;
namespace MongoDbSupplyCollector
{
    public class ConnectionStringPart
    {
        public string Name { get; private set; }
        public bool Required { get; private set; }
        public CnnnectionStringDataType DataType { get; private set; }

        public ConnectionStringPart(string name, bool required, CnnnectionStringDataType dataType)
        {
            Name = name;
            Required = required;
            DataType = dataType;
        }
    }

   public enum CnnnectionStringDataType
    {
        text,
        password,
        number
    }
}
