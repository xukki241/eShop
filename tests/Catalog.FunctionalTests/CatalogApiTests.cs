using System.Net.Http.Json;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Http;
using eShop.Catalog.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace eShop.Catalog.FunctionalTests;

public sealed class CatalogApiTests : IClassFixture<CatalogApiFixture>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public CatalogApiTests(CatalogApiFixture fixture)
    {
        _webApplicationFactory = fixture;
    }

    private HttpClient CreateHttpClient(ApiVersion apiVersion, string? token = TestAuthHandler.AdminToken)
    {
        var handler = new ApiVersionHandler(new QueryStringApiVersionWriter(), apiVersion);
        var client = _webApplicationFactory.CreateDefaultClient(handler);

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private static CatalogItem CreateCatalogItem(int id) => new("AuthZ test item")
    {
        Id = id,
        Description = "AuthZ test description",
        Price = 12.34m,
        PictureFileName = null,
        CatalogTypeId = 8,
        CatalogType = null,
        CatalogBrandId = 13,
        CatalogBrand = null,
        AvailableStock = 10,
        RestockThreshold = 1,
        MaxStockThreshold = 20,
        OnReorder = false
    };

    private async Task<CatalogItem> CreateTemporaryCatalogItemAsync(HttpClient client, int id)
    {
        var createResponse = await client.PostAsJsonAsync("/api/catalog/items", CreateCatalogItem(id), TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        return CreateCatalogItem(id);
    }

    private static void AssertProblemDetailsPayload(string body, int expectedStatus, string expectedDetail)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc9457", root.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()));
        Assert.Equal(expectedStatus, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedDetail, root.GetProperty("detail").GetString());
        Assert.True(root.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemsRespectsPageSize(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = await _httpClient.GetAsync("/api/catalog/items?pageIndex=0&pageSize=5", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<PaginatedItems<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert 103 total items (101 seeded + 2 added by AddCatalogItem tests) with 5 retrieved from index 0
        Assert.Equal(103, result.Count);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(5, result.PageSize);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task UpdateCatalogItemWorksWithoutPriceUpdate(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act - 1
        var response = await _httpClient.GetAsync("/api/catalog/items/1", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var itemToUpdate = JsonSerializer.Deserialize<CatalogItem>(body, _jsonSerializerOptions);

        // Act - 2
        var priorAvailableStock = itemToUpdate.AvailableStock;
        itemToUpdate.AvailableStock -= 1;
        response = version switch
        {
            1.0 => await _httpClient.PutAsJsonAsync("/api/catalog/items", itemToUpdate, TestContext.Current.CancellationToken),
            2.0 => await _httpClient.PutAsJsonAsync($"/api/catalog/items/{itemToUpdate.Id}", itemToUpdate, TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
        response.EnsureSuccessStatusCode();

        // Act - 3
        response = await _httpClient.GetAsync("/api/catalog/items/1", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var updatedItem = JsonSerializer.Deserialize<CatalogItem>(body, _jsonSerializerOptions);

        // Assert - 1
        Assert.Equal(itemToUpdate.Id, updatedItem.Id);
        Assert.NotEqual(priorAvailableStock, updatedItem.AvailableStock);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task UpdateCatalogItemWorksWithPriceUpdate(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act - 1
        var response = await _httpClient.GetAsync("/api/catalog/items/1", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var itemToUpdate = JsonSerializer.Deserialize<CatalogItem>(body, _jsonSerializerOptions);

        // Act - 2
        var priorAvailableStock = itemToUpdate.AvailableStock;
        itemToUpdate.AvailableStock -= 1;
        itemToUpdate.Price = 1.99m;
        response = version switch
        {
            1.0 => await _httpClient.PutAsJsonAsync("/api/catalog/items", itemToUpdate, TestContext.Current.CancellationToken),
            2.0 => await _httpClient.PutAsJsonAsync($"/api/catalog/items/{itemToUpdate.Id}", itemToUpdate, TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
        response.EnsureSuccessStatusCode();

        // Act - 3
        response = await _httpClient.GetAsync("/api/catalog/items/1", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var updatedItem = JsonSerializer.Deserialize<CatalogItem>(body, _jsonSerializerOptions);

        // Assert - 1
        Assert.Equal(itemToUpdate.Id, updatedItem.Id);
        Assert.Equal(1.99m, updatedItem.Price);
        Assert.NotEqual(priorAvailableStock, updatedItem.AvailableStock);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemsbyIds(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = await _httpClient.GetAsync("/api/catalog/items/by?ids=1&ids=2&ids=3", TestContext.Current.CancellationToken);

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<List<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert 3 items
        Assert.Equal(3, result.Count);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemWithId(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = await _httpClient.GetAsync("/api/catalog/items/2", TestContext.Current.CancellationToken);

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<CatalogItem>(body, _jsonSerializerOptions);

        // Assert
        Assert.Equal(2, result.Id);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemWithExactName(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = version switch
        {
            1.0 => await _httpClient.GetAsync("api/catalog/items/by/Wanderer%20Black%20Hiking%20Boots?PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            2.0 => await _httpClient.GetAsync("api/catalog/items?name=Wanderer%20Black%20Hiking%20Boots&PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<PaginatedItems<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Count);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(5, result.PageSize);
        Assert.Equal("Wanderer Black Hiking Boots", result.Data.ToList().FirstOrDefault().Name);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemWithPartialName(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = version switch
        {
            1.0 => await _httpClient.GetAsync("api/catalog/items/by/Alpine?PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            2.0 => await _httpClient.GetAsync("api/catalog/items?name=Alpine&PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<PaginatedItems<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Count);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(5, result.PageSize);
        Assert.Contains("Alpine", result.Data.ToList().FirstOrDefault().Name);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemPicWithId(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        var id = version switch
        {
            1.0 => 22001,
            2.0 => 22002,
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        var bodyContent = CreateCatalogItem(id);
        bodyContent.PictureFileName = "1.webp";

        var createResponse = await _httpClient.PostAsJsonAsync("/api/catalog/items", bodyContent, TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _httpClient.GetAsync($"api/catalog/items/{id}/pic", TestContext.Current.CancellationToken);

        // Arrange
        response.EnsureSuccessStatusCode();
        var result = response.Content.Headers.ContentType.MediaType;

        // Assert
        Assert.Equal("image/webp", result);

        var cleanupResponse = await _httpClient.DeleteAsync($"/api/catalog/items/{id}", TestContext.Current.CancellationToken);
        cleanupResponse.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemWithsemanticrelevance(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = version switch
        {
            1.0 => await _httpClient.GetAsync("api/catalog/items/withsemanticrelevance/Wanderer?PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            2.0 => await _httpClient.GetAsync("api/catalog/items/withsemanticrelevance?text=Wanderer&PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<PaginatedItems<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(5, result.PageSize);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetCatalogItemWithTypeIdBrandId(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = version switch
        {
            1.0 => await _httpClient.GetAsync("api/catalog/items/type/3/brand/3?PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            2.0 => await _httpClient.GetAsync("api/catalog/items?type=3&brand=3&PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<PaginatedItems<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Count);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(3, result.Data.ToList().FirstOrDefault().CatalogTypeId);
        Assert.Equal(3, result.Data.ToList().FirstOrDefault().CatalogBrandId);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetAllCatalogTypeItemWithBrandId(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = version switch
        {
            1.0 => await _httpClient.GetAsync("api/catalog/items/type/all/brand/3?PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            2.0 => await _httpClient.GetAsync("api/catalog/items?brand=3&PageSize=5&PageIndex=0", TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<PaginatedItems<CatalogItem>>(body, _jsonSerializerOptions);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(11, result.Count);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(3, result.Data.ToList().FirstOrDefault().CatalogBrandId);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetAllCatalogTypes(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = await _httpClient.GetAsync("api/catalog/catalogtypes", TestContext.Current.CancellationToken);

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<List<CatalogType>>(body, _jsonSerializerOptions);

        // Assert
        Assert.Equal(8, result.Count);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task GetAllCatalogBrands(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        // Act
        var response = await _httpClient.GetAsync("api/catalog/catalogbrands", TestContext.Current.CancellationToken);

        // Arrange
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<List<CatalogBrand>>(body, _jsonSerializerOptions);

        // Assert
        Assert.Equal(13, result.Count);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task AddCatalogItem(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        var id = version switch
        {
            1.0 => 10015,
            2.0 => 10016,
            _ => 0
        };

        // Act - 1
        var bodyContent = new CatalogItem("TestCatalog1")
        {
            Id = id,
            Description = "Test catalog description 1",
            Price = 11000.08m,
            PictureFileName = null,
            CatalogTypeId = 8,
            CatalogType = null,
            CatalogBrandId = 13,
            CatalogBrand = null,
            AvailableStock = 100,
            RestockThreshold = 10,
            MaxStockThreshold = 200,
            OnReorder = false
        };
        var response = await _httpClient.PostAsJsonAsync("/api/catalog/items", bodyContent, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Act - 2
        response = await _httpClient.GetAsync($"/api/catalog/items/{id}", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var addedItem = JsonSerializer.Deserialize<CatalogItem>(body, _jsonSerializerOptions);

        // Assert - 1
        Assert.Equal(bodyContent.Id, addedItem.Id);

    }

    [Fact]
    public async Task CreateCatalogItemWithoutTokenReturns401()
    {
        var httpClient = CreateHttpClient(new ApiVersion(1.0), token: null);
        var response = await httpClient.PostAsJsonAsync("/api/catalog/items", CreateCatalogItem(21001), TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCatalogItemWithUserTokenReturns403()
    {
        var httpClient = CreateHttpClient(new ApiVersion(1.0), TestAuthHandler.UserToken);
        var response = await httpClient.PostAsJsonAsync("/api/catalog/items", CreateCatalogItem(21002), TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCatalogItemWithAdminTokenReturnsSuccess()
    {
        var httpClient = CreateHttpClient(new ApiVersion(1.0));
        var response = await httpClient.PostAsJsonAsync("/api/catalog/items", CreateCatalogItem(21003), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var cleanupResponse = await httpClient.DeleteAsync("/api/catalog/items/21003", TestContext.Current.CancellationToken);
        cleanupResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetMissingCatalogItemReturnsProblemDetailsWithTraceId()
    {
        var httpClient = CreateHttpClient(new ApiVersion(1.0), token: null);

        var response = await httpClient.GetAsync("/api/catalog/items/99999", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        AssertProblemDetailsPayload(body, StatusCodes.Status404NotFound, "Item with id 99999 was not found.");
    }

    [Fact]
    public async Task UpdateCatalogItemWithoutTokenReturns401()
    {
        var httpClient = CreateHttpClient(new ApiVersion(2.0), token: null);
        var response = await httpClient.PutAsJsonAsync("/api/catalog/items/1", CreateCatalogItem(1), TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCatalogItemWithUserTokenReturns403()
    {
        var httpClient = CreateHttpClient(new ApiVersion(2.0), TestAuthHandler.UserToken);
        var response = await httpClient.PutAsJsonAsync("/api/catalog/items/1", CreateCatalogItem(1), TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCatalogItemWithAdminTokenReturnsSuccess()
    {
        var httpClient = CreateHttpClient(new ApiVersion(2.0));
        var response = await httpClient.PutAsJsonAsync("/api/catalog/items/1", CreateCatalogItem(1), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteCatalogItemWithoutTokenReturns401()
    {
        var httpClient = CreateHttpClient(new ApiVersion(2.0), token: null);
        var response = await httpClient.DeleteAsync("/api/catalog/items/1", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCatalogItemWithUserTokenReturns403()
    {
        var httpClient = CreateHttpClient(new ApiVersion(2.0), TestAuthHandler.UserToken);
        var response = await httpClient.DeleteAsync("/api/catalog/items/1", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCatalogItemWithAdminTokenReturnsSuccess()
    {
        var httpClient = CreateHttpClient(new ApiVersion(2.0));
        var temporaryItem = await CreateTemporaryCatalogItemAsync(httpClient, 21004);

        var response = await httpClient.DeleteAsync($"/api/catalog/items/{temporaryItem.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public async Task DeleteCatalogItem(double version)
    {
        var _httpClient = CreateHttpClient(new ApiVersion(version));

        var id = version switch
        {
            1.0 => 5,
            2.0 => 6,
            _ => 0
        };

        //Act - 1
        var response = await _httpClient.DeleteAsync($"/api/catalog/items/{id}", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Act - 2
        var response1 = await _httpClient.GetAsync($"/api/catalog/items/{id}", TestContext.Current.CancellationToken);
        var responseStatus = response1.StatusCode;

        // Assert - 1
        Assert.Equal("NoContent", response.StatusCode.ToString());
        Assert.Equal("NotFound", responseStatus.ToString());
    }
}
