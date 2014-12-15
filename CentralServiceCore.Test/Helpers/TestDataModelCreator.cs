using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Test.Helpers
{
    class TestDataModelCreator : CentralServiceCore.Data.IDataModelCreator
    {
        DataModelContainerMock container = new DataModelContainerMock();

        public CentralServiceCore.Data.ICentralDataModelContainer CreateDataModelContainer()
        {
            return container;
        }
    }
}
