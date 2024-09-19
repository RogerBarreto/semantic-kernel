// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToAudio;
using SemanticKernel.IntegrationTests.TestSettings;
using Xunit;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using SemanticKernel.UnitTests;
using Microsoft.Extensions.Logging;

namespace SemanticKernel.IntegrationTests.Connectors.AzureOpenAI;

public sealed class AzureOpenAITextToAudioTests
{
    private readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .AddJsonFile(path: "testsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile(path: "testsettings.development.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<AzureOpenAITextToAudioTests>()
        .Build();

    private readonly HttpMessageHandlerStub _messageHandlerStub;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public AzureOpenAITextToAudioTests()
    {
        this._messageHandlerStub = new HttpMessageHandlerStub();
        this._httpClient = new HttpClient(this._messageHandlerStub, false);
        this._mockLoggerFactory = new Mock<ILoggerFactory>();
    }

    [Fact]
    public async Task AzureOpenAITextToAudioTestAsync()
    {
        // Arrange
        AzureOpenAIConfiguration? azureOpenAIConfiguration = this._configuration.GetSection("AzureOpenAITextToAudio").Get<AzureOpenAIConfiguration>();
        Assert.NotNull(azureOpenAIConfiguration);

        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAITextToAudio(
                deploymentName: azureOpenAIConfiguration.DeploymentName,
                endpoint: azureOpenAIConfiguration.Endpoint,
                credential: new AzureCliCredential())
            .Build();

        var service = kernel.GetRequiredService<ITextToAudioService>();

        // Act
        var result = await service.GetAudioContentAsync("The sun rises in the east and sets in the west.");

        // Assert
        var audioData = result.Data!.Value;
        Assert.False(audioData.IsEmpty);
    }

    [Theory]
    [InlineData(null, "model-id")] // Defaults to service definition
    [InlineData("", "model-id")]  // Defaults to service definition
    [InlineData(" ", "model-id")]  // Defaults to service definition
    [InlineData("gpt-4o", "gpt-4o")] // Uses provided model id
    [InlineData("gpt-35-turbo", "gpt-35-turbo")] // Uses provided model id
    public async Task GetAudioContentsUseModelIdFromSettingsAsync(string? providedModelId, string expectedModelId)
    {
        // Arrange
        byte[] expectedByteArray = [0x00, 0x00, 0xFF, 0x7F];

        var service = new AzureOpenAITextToAudioService("model-id", "api-key", "organization", this._httpClient);
        using var stream = new MemoryStream(expectedByteArray);

        this._messageHandlerStub.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(stream)
        };

        // Act
        await service.GetAudioContentsAsync("Some text", new AzureOpenAITextToAudioExecutionSettings() { DeploymentName = providedModelId });

        var requestBody = Encoding.UTF8.GetString(this._messageHandlerStub.RequestContent!);
        Assert.Contains($"\"model\":\"{expectedModelId}\"", requestBody);
    }

}
