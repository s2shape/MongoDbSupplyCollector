using System;
using System.Collections.Generic;

namespace MongoDbSupplyCollector
{
    public interface IBuildConnectionStrings
    {
        string BuildConnectionString(Dictionary<string, string> connectionStringValues);
    }
}
