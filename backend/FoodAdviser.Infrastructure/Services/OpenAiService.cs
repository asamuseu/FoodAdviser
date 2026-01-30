using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// OpenAI service implementation for generating recipe suggestions.
/// </summary>
public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiService> _logger;
    private readonly OpenAiOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiService"/> class.
    /// </summary>
    public OpenAiService(
        HttpClient httpClient,
        ILogger<OpenAiService> logger,
        IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Recipe>> GenerateRecipesAsync(
        IReadOnlyList<FoodItem> availableItems,
        DishType dishType,
        int numberOfPersons,
        int recipeCount,
        CancellationToken ct = default)
    {
        var prompt = BuildPrompt(availableItems, dishType, numberOfPersons, recipeCount);

        _logger.LogDebug("Sending recipe generation request to OpenAI with prompt length {Length}", prompt.Length);

        var request = new OpenAiChatRequest
        {
            Model = _options.Model,
            Messages = new List<OpenAiMessage>
            {
                new()
                {
                    Role = "system",
                    Content = "You are a professional chef assistant that creates recipes based on available ingredients. " +
                              "You always respond with valid JSON only, without any additional text, markdown formatting, or code blocks."
                },
                new()
                {
                    Role = "user",
                    Content = prompt
                }
            },
            Temperature = 0.7,
            MaxTokens = 4000
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, JsonOptions, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(JsonOptions, ct);

            if (result?.Choices == null || result.Choices.Count == 0)
            {
                _logger.LogWarning("OpenAI returned no choices in response");
                return Array.Empty<Recipe>();
            }

            var content = result.Choices[0].Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("OpenAI returned empty content");
                return Array.Empty<Recipe>();
            }

            _logger.LogDebug("Received OpenAI response: {Content}", content);

            // Clean up the response - remove any markdown code blocks if present
            content = CleanJsonResponse(content);

            var recipes = JsonSerializer.Deserialize<List<OpenAiRecipeResponse>>(content, JsonOptions);

            if (recipes == null || recipes.Count == 0)
            {
                _logger.LogWarning("Failed to deserialize recipes from OpenAI response");
                return Array.Empty<Recipe>();
            }

            return recipes.Select(r => new Recipe
            {
                Id = Guid.NewGuid(),
                Title = r.Name ?? string.Empty,
                Description = r.Description ?? string.Empty,
                DishType = dishType,
                Ingredients = r.Ingredients?.Select(i => new Ingredient
                {
                    Name = i.Name ?? string.Empty,
                    Quantity = i.Quantity,
                    Unit = i.Unit ?? string.Empty
                }).ToList() ?? new List<Ingredient>()
            }).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling OpenAI API");
            throw new InvalidOperationException("Failed to communicate with OpenAI service. Please try again later.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI response as JSON");
            return Array.Empty<Recipe>();
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != ct)
        {
            _logger.LogError(ex, "OpenAI request timed out");
            throw new InvalidOperationException("Recipe generation request timed out. Please try again.", ex);
        }
    }

    private static string BuildPrompt(
        IReadOnlyList<FoodItem> availableItems,
        DishType dishType,
        int numberOfPersons,
        int recipeCount)
    {
        var ingredientsList = new StringBuilder();
        foreach (var item in availableItems)
        {
            ingredientsList.AppendLine($"- {item.Name}: {item.Quantity} {item.Unit}");
        }

        return $@"Based on the following available ingredients and their quantities, generate exactly {recipeCount} {dishType} recipe(s) for {numberOfPersons} person(s).

Available ingredients:
{ingredientsList}

Requirements:
1. Only use ingredients from the list above
2. Respect the available quantities - do not exceed what is available
3. Each recipe must be suitable for {numberOfPersons} person(s)
4. Each recipe must be a {dishType}

Return ONLY a valid JSON array with no additional text. Each recipe object must have this exact structure:
[
  {{
    ""name"": ""Recipe Name"",
    ""description"": ""Detailed cooking instructions and description"",
    ""ingredients"": [
      {{
        ""name"": ""Ingredient name"",
        ""quantity"": 100,
        ""unit"": ""g""
      }}
    ]
  }}
]

If you cannot create any valid recipes with the available ingredients, return an empty JSON array: []

Remember: Return ONLY the JSON array, no explanations, no markdown, no code blocks.";
    }

    private static string CleanJsonResponse(string content)
    {
        content = content.Trim();

        // Remove markdown code blocks if present
        if (content.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            content = content.Substring(7);
        }
        else if (content.StartsWith("```"))
        {
            content = content.Substring(3);
        }

        if (content.EndsWith("```"))
        {
            content = content.Substring(0, content.Length - 3);
        }

        return content.Trim();
    }

    #region OpenAI API DTOs

    private class OpenAiChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<OpenAiMessage> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    private class OpenAiMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class OpenAiChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAiChoice> Choices { get; set; } = new();
    }

    private class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage? Message { get; set; }
    }

    private class OpenAiRecipeResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("ingredients")]
        public List<OpenAiIngredientResponse>? Ingredients { get; set; }
    }

    private class OpenAiIngredientResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }
    }

    #endregion
}
