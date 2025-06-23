using Ofqual.Recognition.Frontend.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Services;

public class MemoryCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = new FakeSession("mock-session-id");

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        _cacheService = new MemoryCacheService(_memoryCache, _httpContextAccessorMock.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Set_And_Get_Should_Return_Same_Object()
    {
        // Arrange
        string key = "testKey";
        string expectedValue = "Hello, Cache!";

        // Act
        _cacheService.Set(key, expectedValue);
        var result = _cacheService.Get<string>(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HasInMemoryCache_Should_Return_True_When_Key_Exists()
    {
        // Arrange
        string key = "exists";

        // Act
        _cacheService.Set(key, "exists!");
        var exists = _cacheService.HasInMemoryCache(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Remove_Should_Clear_Cache_Entry()
    {
        // Arrange
        string key = "removeKey";
        _cacheService.Set(key, "to be removed");

        // Act
        _cacheService.Remove(key);
        var exists = _cacheService.HasInMemoryCache(key);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AddOrAppendToList_Should_Append_Item_To_List()
    {
        // Arrange
        string key = "listKey";

        // Act
        _cacheService.AddOrAppendToList(key, "item1");
        _cacheService.AddOrAppendToList(key, "item2");
        var result = _cacheService.Get<List<string>>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("item1", result);
        Assert.Contains("item2", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveFromList_Should_Remove_Matching_Item()
    {
        // Arrange
        string key = "listKeyToRemove";
        _cacheService.AddOrAppendToList(key, "ofqual1");
        _cacheService.AddOrAppendToList(key, "ofqual2");

        // Act
        _cacheService.RemoveFromList<string>(key, s => s == "ofqual1");
        var result = _cacheService.Get<List<string>>(key);

        // Assert
        Assert.Single(result!);
        Assert.DoesNotContain("ofqual1", result!);
        Assert.Contains("ofqual2", result!);
    }
}
