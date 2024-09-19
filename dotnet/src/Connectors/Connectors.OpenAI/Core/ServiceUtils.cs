// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.SemanticKernel.Connectors.OpenAI.Core;

internal static class ServiceUtils
{
    internal static string GetModelId(PromptExecutionSettings? executionSettings, string defaultModelId)
        => string.IsNullOrWhiteSpace(executionSettings?.ModelId)
            ? defaultModelId
            : executionSettings!.ModelId!;
}
