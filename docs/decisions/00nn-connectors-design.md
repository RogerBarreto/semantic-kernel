---
# These are optional elements. Feel free to remove any of them.
status: proposed
contact: rogerbarreto
date: 2024-08-12
deciders: rogerbarreto, sergeymenshykh, markwallace-microsoft, dmytrostruk, westey-m
---

# .Net Connector Best Practices

This document outlines the best practices for developing .Net connectors for Semantic Kernel.

## Project Configuration

Project solution file should have similar configuration `csproj`:

- Rename `{ProviderName}` with the provider name, e.g., `OpenAI`.
- Rename `{Modality 1, 2, 3}` with the modality name, e.g., `TextGeneration`, `ChatCompletion`.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>Microsoft.SemanticKernel.Connectors.{ProviderName}</AssemblyName>
    <RootNamespace>$(AssemblyName)</RootNamespace>
    <TargetFrameworks>net8.0;netstandard2.0</TargetFrameworks>
    <VersionSuffix>alpha</VersionSuffix>
    <NoWarn>$(NoWarn);SKEXP0070</NoWarn>
  </PropertyGroup>

  <Import Project="$(RepoRoot)/dotnet/nuget/nuget-package.props" />
  <Import Project="$(RepoRoot)/dotnet/src/InternalUtilities/src/InternalUtilities.props" />

  <PropertyGroup>
    <Title>Semantic Kernel - {ProviderName} Connectors</Title>
    <Description>Semantic Kernel connectors for {ProviderName} generation models. Contains {Modality 1, 2, 3} services.</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\SemanticKernel.Abstractions\SemanticKernel.Abstractions.csproj" />
    <ProjectReference Include="..\..\SemanticKernel.Core\SemanticKernel.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="SemanticKernel.Connectors.{ProviderName}.UnitTests" />
  </ItemGroup>

</Project>
```

## Project Namespace

- **Root namespace** should be `Microsoft.SemanticKernel.Connectors.{ProviderName}`.
- All public classes should be set at the **Root namespace** regardless of subfolder.

## Solution Configuration

This step is required to enable the project to be Published in nuget once it is merged in `main`.

Do do that, open the `.sln` file:

Get the {YourProjectGUID} by searching for `Connectors.{ProviderName}` in the `.sln` file.
and you should find a line like below:

> Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Connectors.{ProviderName}", "src\Connectors\Connectors.Google\Connectors.{ProviderName}.csproj", **"{YourProjectGUID should be here}"**
> EndProject

Grab **YourProjectGuid** in hands and now find and update the following lines, this will ensure that the Publish configuration is set to `Publish` instead of `Debug`.

```diff
-   {YourProjectGUID}.Publish|Any CPU.ActiveCfg = Debug|Any CPU
-   {YourProjectGUID}.Publish|Any CPU.Build.0 = Debug|Any CPU
+   {YourProjectGUID}.Publish|Any CPU.ActiveCfg = Publish|Any CPU
+   {YourProjectGUID}.Publish|Any CPU.Build.0 = Publish|Any CPU
```

## Project Structure

The project structure should be organized in a way that makes it easy to understand and maintain.

### 1. Internal Logic: Core/

The `Core` folder should contain all the internal logic and components of the connector such as:

- Clients
- Internal Extensions
- Internal Helper classes

#### 1.1 Clients

> [!IMPORTANT]
> Clients should be internal to the connector and only used internally by the services.

Clients are a necessary decoupling strategy from the services, which also allow us to quickly update our services when the provider launches a new SDK or updates an existing one. Custom HttpClient implementations are necessary if no .NET SDKs are available from the Provider.

> [!NOTE]
> When possible move the validation responsibility to the client keeping the Service layer minimal, avoiding unnecessary checks in the Service or in any of the `IKernelBuilder` or `IServiceCollection` extension methods.

#### 1.2 Internal Models: `Core/Models/`

Intended for internal models only.

- HttpClient Request models
- HttpClient Response models

### 2. AI Services: `Services/`

Services are `public` and **must** implement `SemanticKernel.Abstractions` service interfaces.
Not all connectors will implement all the services, but the services that are implemented should be implemented in a way that is consistent with the other connectors.

### 3. Execution Settings: `Settings/`

Execution settings should be implemented as a `public` class that extends `PromptExecutionSettings` and should be passed to the service APIs that requires it.

#### 3.1 Naming

Provider as prefix for the execution settings class name, e.g., `ProviderChatExecutionSettings`, `ProviderPromptExecutionSettings`.

#### 3.2 Shared naming problem

⚠️ Shared execution settings for different modalities should be avoided. Each modality should have its own execution settings.
Currently Text/Chat share `ProviderPromptExecutionSettings` which limits some of the settings that only exists for `TextGeneration` or `ChatCompletion`.

> [!NOTE]
> Open AI differences between [TextGeneration](https://platform.openai.com/docs/api-reference/completions/create) and [ChatCompletion](https://platform.openai.com/docs/api-reference/chat/create).

#### 3.3 Modalities Naming

- ChatCompletion
  - `ProviderChatExecutionSettings` ⚠️ TBD
  - `ProviderPromptExecutionSettings`. (current naming)
- TextGeneration
  - `ProviderTextExecutionSettings` ⚠️ TBD
  - `ProviderPromptExecutionSettings`. (current naming)

#### 3.4 Freeze Behavior

- Should be immutable after using `Freeze()` method.

#### 3.5 Properties

- Should have independent properties
- Should all reference to the `ExtendedData` dictionary

### 4. Extension Methods

dfddfs
