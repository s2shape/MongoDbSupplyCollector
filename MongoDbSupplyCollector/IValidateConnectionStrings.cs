using System;
using System.Collections.Generic;

namespace MongoDbSupplyCollector
{
    public interface IValidateConnectionStrings
    {
        bool IsValidConnectionString(Dictionary<string, string> connectionStringValues);
    }
}
