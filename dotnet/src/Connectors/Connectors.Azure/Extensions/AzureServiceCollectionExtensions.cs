﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Azure;
using Microsoft.SemanticKernel.Http;

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0039 // Use local function

namespace Microsoft.SemanticKernel;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> and related classes to configure OpenAI and Azure OpenAI connectors.
/// </summary>
public static class AzureServiceCollectionExtensions
{
    #region Chat Completion

    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to augment.</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <returns>The same instance as <paramref name="services"/>.</returns>
    public static IServiceCollection AddAzureChatCompletion(
        this IServiceCollection services,
        string deploymentName,
        Uri endpoint,
        string apiKey,
        string? serviceId = null,
        string? modelId = null)
    {
        Verify.NotNull(services);
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNull(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        Func<IServiceProvider, object?, AzureChatCompletionService> factory = (serviceProvider, _) =>
        {
            OpenAIClient client = CreateAzureClient(
                endpoint,
                new AzureKeyCredential(apiKey),
                HttpClientProvider.GetHttpClient(serviceProvider));

            return new(deploymentName, client, modelId, serviceProvider.GetService<ILoggerFactory>());
        };

        services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return services;
    }

    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to augment.</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credentials">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <returns>The same instance as <paramref name="services"/>.</returns>
    public static IServiceCollection AddAzureChatCompletion(
        this IServiceCollection services,
        string deploymentName,
        Uri endpoint,
        TokenCredential credentials,
        string? serviceId = null,
        string? modelId = null)
    {
        Verify.NotNull(services);
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNull(endpoint);
        Verify.NotNull(credentials);

        Func<IServiceProvider, object?, AzureChatCompletionService> factory = (serviceProvider, _) =>
        {
            OpenAIClient client = CreateAzureClient(
                endpoint,
                credentials,
                HttpClientProvider.GetHttpClient(serviceProvider));

            return new(deploymentName, client, modelId, serviceProvider.GetService<ILoggerFactory>());
        };

        services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return services;
    }

    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to augment.</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="openAIClient"><see cref="OpenAIClient"/> to use for the service. If null, one must be available in the service provider when this service is resolved.</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <returns>The same instance as <paramref name="services"/>.</returns>
    public static IServiceCollection AddAzureChatCompletion(
        this IServiceCollection services,
        string deploymentName,
        OpenAIClient? openAIClient = null,
        string? serviceId = null,
        string? modelId = null)
    {
        Verify.NotNull(services);
        Verify.NotNullOrWhiteSpace(deploymentName);

        Func<IServiceProvider, object?, AzureChatCompletionService> factory = (serviceProvider, _) =>
            new(deploymentName, openAIClient ?? serviceProvider.GetRequiredService<OpenAIClient>(), modelId, serviceProvider.GetService<ILoggerFactory>());

        services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return services;
    }

    #endregion

    private static OpenAIClient CreateAzureClient(Uri endpoint, AzureKeyCredential credentials, HttpClient? httpClient) =>
        new(endpoint, credentials, ClientCore.GetOpenAIClientOptions(httpClient));

    private static OpenAIClient CreateAzureClient(Uri endpoint, TokenCredential credentials, HttpClient? httpClient) =>
        new(endpoint, credentials, ClientCore.GetOpenAIClientOptions(httpClient));
}