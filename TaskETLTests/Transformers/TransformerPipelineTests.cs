using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using TaskETL.Transformers;
using TaskETLTests.Mock;

namespace TaskETLTests.Transformers
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TransformerPipelineTests
    {
        [TestMethod]
        public void TestInitializesWithoutErrors()
        {
            ITransformer<BasicModel, AnotherModel> transformer = null;
        }
    }
}
