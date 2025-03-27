
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Tests.TestData;

public static class SessionServiceTestCases
{
    public class TestData
    {
        public string? Name { get; set; }
    }

    public static IEnumerable<object[]> SetInSessionTestCases()
    {
        yield return new object[] { "key1", new TestData { Name = "Sample" } };
        yield return new object[] { "key2", new TestData { Name = "" } };
        yield return new object[] { "key3", new TestData { Name = "With special chars !@#$%^&*" } };
        yield return new object[] { "key4", new TestData { Name = null } };
    }

    public static IEnumerable<object[]> GetFromSessionTestCases()
    {
        yield return new object[] { "key1", new TestData { Name = "Sample" }, false };
        yield return new object[] { "key2", new TestData { Name = "" }, false };
        yield return new object[] { "key3", new TestData { Name = null }, false };
        yield return new object[] { "key4", null, false };
        yield return new object[] { "key5", null, true };
    }

    public static IEnumerable<object[]> TaskStatusUpdateCases()
    {
        yield return new object[] { TaskStatusEnum.CannotStartYet, TaskStatusEnum.Completed };
        yield return new object[] { TaskStatusEnum.InProgress, TaskStatusEnum.CannotStartYet };
    }
}