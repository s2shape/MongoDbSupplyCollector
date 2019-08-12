using System;
namespace MongoDbSupplyCollector
{
    public class ConnectionStringPart
    {
        public string Name { get; private set; }
        public bool Required { get; private set; }
        public DataType DataType { get; private set; }

        public ConnectionStringPart(string name, bool required, DataType dataType)
        {
            Name = name;
            Required = required;
            DataType = dataType;
        }
    }

   public enum DataType
    {
        text,
        password,
        number
    }
}
