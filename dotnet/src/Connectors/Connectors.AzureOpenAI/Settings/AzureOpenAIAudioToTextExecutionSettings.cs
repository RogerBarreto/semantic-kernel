// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI;

/// <summary>
/// Execution settings for an AzureOpenAI completion request.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class AzureOpenAIAudioToTextExecutionSettings : OpenAIAudioToTextExecutionSettings
{
    /// <summary>
    /// Creates an instance of <see cref="AzureOpenAIAudioToTextExecutionSettings"/>.
    /// </summary>
    public AzureOpenAIAudioToTextExecutionSettings(string fileName) : base(fileName)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="AzureOpenAIAudioToTextExecutionSettings"/> class with default filename - "file.mp3".
    /// </summary>
    public AzureOpenAIAudioToTextExecutionSettings()
    {
    }

    /// <summary>
    /// The name of the deployment to use for the completion request.
    /// </summary>
    /// <remarks>
    /// Azure API's doesn't make a distinction between the modelId and the deploymentName.
    /// </remarks>
    [Experimental("SKEXP0010")]
    public string? DeploymentName
    {
        get => this.ModelId;
        set => this.ModelId = value;
    }
}
