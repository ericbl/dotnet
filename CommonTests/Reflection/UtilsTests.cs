using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Reflection.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void GetPublicGenericPropertiesChangedTest1()
        {
            // Define objects to test
            OrgaFunction func1 = new OrgaFunction { Id = 1, Description = "func1" };
            OrgaFunction func2 = new OrgaFunction { Id = 2, Description = "func2" };
            FunctionAssignment funcAss1 = new FunctionAssignment
            {
                Function = func1,
                Level = 1
            };
            FunctionAssignment funcAss2 = new FunctionAssignment
            {
                Function = func2,
                Level = 2
            };

            // Main test: read properties changed
            var propertiesChanged = Utils.GetPublicGenericPropertiesChanged(funcAss1, funcAss2, null);

            Assert.IsNotNull(propertiesChanged);
            Assert.IsTrue(propertiesChanged.Count == 3);
            Assert.IsTrue(propertiesChanged[0].PropertyName == "FunctionAssignment.OrgaFunction.Description");
            Assert.IsTrue(propertiesChanged[1].PropertyName == "FunctionAssignment.OrgaFunction.Id");
            Assert.IsTrue(propertiesChanged[2].PropertyName == "FunctionAssignment.Level");
        }

        [TestMethod()]
        public void GetPublicGenericPropertiesChangedTest2()
        {
            // Define objects to test
            Department dep1 = new Department { Id = 1, Description = "department1" };
            DepartmentAssignment depAss11 = new DepartmentAssignment { Department = dep1 };
            DepartmentAssignment depAss12 = new DepartmentAssignment { Department = dep1 };

            var propertiesChanged = Utils.GetPublicGenericPropertiesChanged(depAss11, depAss12, null);
            Assert.IsNotNull(propertiesChanged);
            Assert.IsTrue(propertiesChanged.Count == 0);

            Department dep2 = new Department { Id = 2, Description = "department2" };
            DepartmentAssignment depAss2 = new DepartmentAssignment { Department = dep2 };

            propertiesChanged = Utils.GetPublicGenericPropertiesChanged(depAss11, depAss2, null);
            Assert.IsNotNull(propertiesChanged);
            Assert.IsTrue(propertiesChanged.Count == 2);
            Assert.IsTrue(propertiesChanged[0].PropertyName == "DepartmentAssignment.Department.Description");
            Assert.IsTrue(propertiesChanged[1].PropertyName == "DepartmentAssignment.Department.Id");
        }
    }
}