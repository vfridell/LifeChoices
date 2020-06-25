using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameOfLifeLib.Parsers;

namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //RuleFactory.GetRuleFromFile("wireworld_test.table");
            //RuleFactory.GetRuleFromFile("smalltest.table");
            RuleFactory.GetRuleFromFile("tinytest.table");
        }
    }
}
