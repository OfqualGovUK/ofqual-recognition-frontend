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
}