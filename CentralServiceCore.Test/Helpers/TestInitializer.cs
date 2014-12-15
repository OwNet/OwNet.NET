using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Test.Helpers
{
    class TestInitializer
    {
        internal static void Init()
        {
            Data.DataController.DataModelCreator = new TestDataModelCreator();
        }
    }
}
