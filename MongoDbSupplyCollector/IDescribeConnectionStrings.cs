using System;
using System.Collections.Generic;

namespace MongoDbSupplyCollector
{
    public interface IDescribeConnectionStrings
    {
         List<ConnectionStringPart> GetConnectionStringParts();
    }
}
