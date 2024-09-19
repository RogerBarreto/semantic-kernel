// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;
using Moq;

namespace SemanticKernel.Connectors.AzureOpenAI.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="AzureOpenAIAudioToTextService"/> class.
/// </summary>
public sealed class AzureOpenAIAudioToTextServiceTests : IDisposable
{
    private readonly HttpMessageHandlerStub _messageHandlerStub;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public AzureOpenAIAudioToTextServiceTests()
    {
        this._messageHandlerStub = new HttpMessageHandlerStub();
        this._httpClient = new HttpClient(this._messageHandlerStub, false);
        this._mockLoggerFactory = new Mock<ILoggerFactory>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConstructorWithApiKeyWorksCorrectly(bool includeLoggerFactory)
    {
        // Arrange & Act
        var service = includeLoggerFactory ?
            new AzureOpenAIAudioToTextService("deployment", "https://endpoint", "api-key", "model-id", loggerFactory: this._mockLoggerFactory.Object) :
            new AzureOpenAIAudioToTextService("deployment", "https://endpoint", "api-key", "model-id");

        // Assert
        Assert.Equal("model-id", service.Attributes[AIServiceExtensions.ModelIdKey]);
        Assert.Equal("deployment", service.Attributes[AzureClientCore.DeploymentNameKey]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConstructorWithTokenCredentialWorksCorrectly(bool includeLoggerFactory)
    {
        // Arrange & Act
        var credentials = DelegatedTokenCredential.Create((_, _) => new AccessToken());
        var service = includeLoggerFactory ?
            new AzureOpenAIAudioToTextService("deployment", "https://endpoint", credentials, "model-id", loggerFactory: this._mockLoggerFactory.Object) :
            new AzureOpenAIAudioToTextService("deployment", "https://endpoint", credentials, "model-id");

        // Assert
        Assert.Equal("model-id", service.Attributes[AIServiceExtensions.ModelIdKey]);
        Assert.Equal("deployment", service.Attributes[AzureClientCore.DeploymentNameKey]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConstructorWithOpenAIClientWorksCorrectly(bool includeLoggerFactory)
    {
        // Arrange & Act
        var client = new AzureOpenAIClient(new Uri("http://host"), "key");
        var service = includeLoggerFactory ?
            new AzureOpenAIAudioToTextService("deployment", client, "model-id", loggerFactory: this._mockLoggerFactory.Object) :
            new AzureOpenAIAudioToTextService("deployment", client, "model-id");

        // Assert
        Assert.Equal("model-id", service.Attributes[AIServiceExtensions.ModelIdKey]);
        Assert.Equal("deployment", service.Attributes[AzureClientCore.DeploymentNameKey]);
    }

    [Fact]
    public void ItThrowsIfDeploymentNameIsNotProvided()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new AzureOpenAIAudioToTextService(" ", "http://host", "apikey"));
        Assert.Throws<ArgumentException>(() => new AzureOpenAIAudioToTextService(" ", azureOpenAIClient: new(new Uri("http://host"), "apikey")));
        Assert.Throws<ArgumentException>(() => new AzureOpenAIAudioToTextService("", "http://host", "apikey"));
        Assert.Throws<ArgumentException>(() => new AzureOpenAIAudioToTextService("", azureOpenAIClient: new(new Uri("http://host"), "apikey")));
        Assert.Throws<ArgumentNullException>(() => new AzureOpenAIAudioToTextService(null!, "http://host", "apikey"));
        Assert.Throws<ArgumentNullException>(() => new AzureOpenAIAudioToTextService(null!, azureOpenAIClient: new(new Uri("http://host"), "apikey")));
    }

    [Theory]
    [MemberData(nameof(ExecutionSettings))]
    public async Task GetTextContentWithInvalidSettingsThrowsExceptionAsync(OpenAIAudioToTextExecutionSettings? settings, Type expectedExceptionType)
    {
        // Arrange
        var service = new AzureOpenAIAudioToTextService("deployment", "https://endpoint", "api-key", "model-id", this._httpClient);
        this._messageHandlerStub.ResponseToReturn = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test audio-to-text response")
        };

        // Act
        var exception = await Record.ExceptionAsync(() => service.GetTextContentsAsync(new AudioContent(new BinaryData("data"), mimeType: null), settings));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType(expectedExceptionType, exception);
    }

    [Theory]
    [InlineData("verbose_json")]
    [InlineData("json")]
    [InlineData("vtt")]
    [InlineData("srt")]
    public async Task ItRespectResultFormatExecutionSettingAsync(string format)
    {
        // Arrange
        var service = new AzureOpenAIAudioToTextService("deployment", "https://endpoint", "api-key", httpClient: this._httpClient);
        this._messageHandlerStub.ResponseToReturn = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test audio-to-text response")
        };

        // Act
        var settings = new OpenAIAudioToTextExecutionSettings("file.mp3") { ResponseFormat = format };
        var result = await service.GetTextContentsAsync(new AudioContent(new BinaryData("data"), mimeType: null), settings);

        // Assert
        Assert.NotNull(this._messageHandlerStub.RequestContent);
        Assert.NotNull(result);

        var multiPartData = Encoding.UTF8.GetString(this._messageHandlerStub.RequestContent!);
        var multiPartBreak = multiPartData.Substring(0, multiPartData.IndexOf("\r\n", StringComparison.OrdinalIgnoreCase));

        Assert.Contains($"{format}\r\n{multiPartBreak}", multiPartData);
    }

    [Fact]
    public async Task GetTextContentByDefaultWorksCorrectlyAsync()
    {
        // Arrange
        var service = new AzureOpenAIAudioToTextService("deployment-name", "https://endpoint", "api-key", "model-id", this._httpClient);
        this._messageHandlerStub.ResponseToReturn = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test audio-to-text response")
        };

        // Act
        var result = await service.GetTextContentsAsync(new AudioContent(new BinaryData("data"), mimeType: null), new OpenAIAudioToTextExecutionSettings("file.mp3"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test audio-to-text response", result[0].Text);
    }

    [Theory]
    [InlineData(null, "deployment-name")] // Defaults to service definition
    [InlineData("", "deployment-name")]  // Defaults to service definition
    [InlineData(" ", "deployment-name")]  // Defaults to service definition
    [InlineData("gpt-4o", "gpt-4o")] // Uses provided model id
    [InlineData("gpt-35-turbo", "gpt-35-turbo")] // Uses provided model id
    public async Task GetTextContentsUseDeploymentNameFromSettingsAsync(string? providedDeploymentName, string expectedDeploymentName)
    {
        // Arrange
        var service = new AzureOpenAIAudioToTextService("deployment-name", "https://endpoint", "api-key", "model-id", this._httpClient);
        this._messageHandlerStub.ResponseToReturn = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("Test audio-to-text response")
        };

        // Act
        await service.GetTextContentsAsync(new AudioContent(new BinaryData("data"), mimeType: null), new AzureOpenAIAudioToTextExecutionSettings("file.mp3") { DeploymentName = providedDeploymentName });

        var requestUri = this._messageHandlerStub.RequestUri!;
        Assert.Contains($"openai/deployments/{expectedDeploymentName}/audio/transcriptions", requestUri.ToString());

        var requestBody = Encoding.UTF8.GetString(this._messageHandlerStub.RequestContent!);
        Assert.Contains($"Content-Disposition: form-data; name=model\r\n\r\n{expectedDeploymentName}", requestBody);
    }

    public void Dispose()
    {
        this._httpClient.Dispose();
        this._messageHandlerStub.Dispose();
    }

    public static TheoryData<OpenAIAudioToTextExecutionSettings?, Type> ExecutionSettings => new()
    {
        { new OpenAIAudioToTextExecutionSettings(""), typeof(ArgumentException) },
        { new OpenAIAudioToTextExecutionSettings("file"), typeof(ArgumentException) }
    };
}
