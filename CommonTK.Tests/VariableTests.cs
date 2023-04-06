using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    public class VariableTests
    {
        [Fact]
        public void GeneratorTest()
        {
            Assert.Equal("global.variable.process.test", Variable.VariableActionGroup("test", EnvironmentVariableTarget.Process));
        }

        [Fact]
        public void ProcessVariableTest()
        {
            Assert.Null(Variable.GetVariable("test"));
            Assert.Throws<KeyNotFoundException>(() => new Variable("test", false));
            var test = new Variable("test");
            Assert.Null(Variable.GetVariable("test"));
            test.Value = "test";
            Assert.Equal("test", Variable.GetVariable("test"));
            Assert.Equal("test", test.Value);
        }
    }
}
