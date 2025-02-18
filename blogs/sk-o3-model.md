# Using OpenAI’s o3-mini Reasoning Model in Semantic Kernel

OpenAI’s **o3-mini** is a newly released **small reasoning model** (launched January 2025) that delivers advanced problem-solving capabilities at a fraction of the cost of previous models. It excels in STEM domains (science, math, coding) while maintaining **low latency and cost** similar to the earlier o1-mini model ([OpenAI o3-mini | OpenAI](https://openai.com/index/openai-o3-mini/#:~:text=We%E2%80%99re%20releasing%20OpenAI%20o3,mini)).

This model is also available as [Azure OpenAI Service](https://azure.microsoft.com/en-us/blog/announcing-the-availability-of-the-o3-mini-reasoning-model-in-microsoft-azure-openai-service), emphasizing its **efficiency gains and new features** like reasoning effort control and tool use.

Throughout this post We'll explore how to use `o3-mini` and other reasoning models with Semantic Kernel in both C# and Python.

[**Key Features of OpenAI o3-mini:**](https://azure.microsoft.com/en-us/blog/announcing-the-availability-of-the-o3-mini-reasoning-model-in-microsoft-azure-openai-service)

- **Reasoning Effort Control:** Adjust the model’s “thinking” level (low, medium, high) to balance response **speed vs depth**. This parameter lets the model spend more time on complex queries when set to high, [using additional hidden reasoning tokens](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/reasoning#:~:text=the%20message%20response%20content%20but,reasoning_tokens) for a more thorough answer.
- **Structured Outputs:** Supports JSON Schema-based output constraints, enabling the model to produce well-defined JSON or other structured formats for downstream automation.
- **Function and Tool Integration:** Natively calls functions and external tools (similar to previous OpenAI models), making it easier to build AI agents that perform actions or calculations as part of their responses.
- **Developer Messages:** Introduces a new `"developer"` role (replacing the old system role) for instructions, allowing more flexible and explicit system prompts. (Azure OpenAI ensures backward compatibility by mapping legacy system messages to this new role.)
- **Enhanced STEM Performance:** Improved abilities in coding, mathematics, and scientific reasoning, outperforming earlier models on many technical benchmarks.

- **Performance & Efficiency:** Early evaluations show that o3-mini provides **more accurate reasoning and faster responses** than its predecessors. OpenAI’s internal testing reported _39% fewer major errors_ on challenging questions compared to the older o1-mini, while also delivering answers about _24% faster_. In fact, with medium effort, o3-mini matches the larger o1 model’s performance on tough math and science problems, and at **high effort it can even outperform the full o1 model** on certain tasks ([OpenAI o3-mini | OpenAI](https://openai.com/index/openai-o3-mini/#:~:text=Mathematics%3A%20With%20low%20reasoning%20effort%2C,consensus%29%20with%2064%20samples)). These gains come with substantial cost savings: o3-mini is roughly _63% cheaper to use than_ o1-mini, thanks to optimizations that dramatically reduce the per-token pricing.

- **Pricing:** One of o3-mini’s biggest appeals is its [**cost-effectiveness**](https://openai.com/index/openai-o3-mini/). According to OpenAI’s pricing, o3-mini usage is billed at about **$1.10 per million input tokens** and **$4.40 per million output tokens**

  - [Open AI Pricing](https://platform.openai.com/docs/pricing)
  - [Azure OpenAI Service Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/)

  _You can get also a 50% discount for cached or batched tokens, further lowering effective costs in certain scenarios_.

Now, let’s see **how to use o3-mini in Semantic Kernel**. Because o3-mini follows the same OpenAI Chat Completion API format, we can plug it into SK using the existing OpenAI connector.

We’ll demonstrate minimal code in C# and Python to send a prompt to the o3-mini model and get a response. We’ll also show how to configure the important `reasoning_effort` setting to get the best results from the model.

#### In .NET (C#)

For a C# project using Semantic Kernel, you can add o3-mini as an OpenAI chat completion service. Make sure you have your OpenAI API key (or Azure OpenAI endpoint and key if using Azure). Using the SK connectors, we create a chat completion service pointing to the o3-mini model and (optionally) specify a high reasoning effort for more complex queries:

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable SKEXP0010 // Reasoning effort is still in preview for OpenAI SDK.

 // Initialize the OpenAI chat completion service with the o3-mini model.
 var chatService = new OpenAIChatCompletionService(
     modelId: "o3-mini",  // OpenAI API endpoint
     apiKey: "YOUR_OPENAI_API_KEY"  // Your OpenAI API key
 );

 // (If using Azure OpenAI Service, use AzureOpenAIChatCompletionService
 // with your endpoint URI, API key, and deployed model name instead.)

 // Create a new chat history and add a user message to prompt the model.
 ChatHistory chatHistory = [];
 chatHistory.AddUserMessage("Hello, how are you?");

 // Configure reasoning effort for the chat completion request.
 var settings = new OpenAIPromptExecutionSettings { ReasoningEffort = "high" };

 // Send the chat completion request to o3-mini
 var reply = await chatService.GetChatMessageContentAsync(chatHistory, settings);
 Console.WriteLine("o3-mini reply: " + reply);
```

_Note:_ If you’re using **Azure OpenAI**, the setup is very similar – you would use `AzureOpenAIChatCompletionService` instead providing the "`<deployment-name>`", "`<https://your-endpoint>`", "{`<api-key>`"). The `reasoning_effort` parameter is [supported in the Azure OpenAI Chat Completion API](https://azure.microsoft.com/en-us/blog/announcing-the-availability-of-the-o3-mini-reasoning-model-in-microsoft-azure-openai-service/#:~:text=,%E2%80%9Cdeveloper%E2%80%9D%20attribute%20replaces%20the%20system) as well, but make sure you have the latest Azure OpenAI SDK set to the latest REST API version (2024-12-01-preview or later) that includes this parameter.

#### Python

Using o3-mini in Python with Semantic Kernel is just as straightforward. We can utilize the SK OpenAI connector classes to call the model. Below is an example how to use Semantic Kernel targeting o3-mini and enabling high reasoning effort:

```python
import asyncio
from semantic_kernel.contents import ChatHistory
from semantic_kernel.connectors.ai.open_ai import OpenAIChatCompletion, OpenAIChatPromptExecutionSettings, AsyncOpenAI

# OpenAI API key (or Azure OpenAI key)
OPENAI_API_KEY = "YOUR_OPENAI_API_KEY"

async def main():
    # Initialize the OpenAI chat completion with o3-mini model
    chat_service = OpenAIChatCompletion(
        ai_model_id="o3-mini",
        async_client=AsyncOpenAI(api_key=OPENAI_API_KEY)
        # For Azure OpenAI: AsyncOpenAI(api_key=AZURE_API_KEY, base_url="https://<your-resource>.openai.azure.com/")
    )

    # Start a chat history and add a user prompt
    chat_history = ChatHistory()
    chat_history.add_user_message("Hello, how are you?")

    # Create settings and set high reasoning effort for a more detailed response
    settings = OpenAIChatPromptExecutionSettings()
    settings.reasoning_effort = "high"  # Ask o3-mini to use high reasoning mode

    # Get the model's response
    response = await chat_service.get_chat_message_content(chat_history, settings)
    print("o3-mini reply:", response)

# Run the async main function
asyncio.run(main())
```

Just like that, with only a few lines of code in C# or Python, you can **start leveraging o3-mini’s reasoning capabilities** within your Semantic Kernel applications. Whether you’re building an AI agent that needs rigorous problem-solving or a chat assistant that can handle complex queries, o3-mini provides a powerful yet cost-efficient option. The `reasoning_effort` knob gives you fine control – for example, use high effort for difficult questions where accuracy matters most, and medium or low effort for casual or time-sensitive interactions ([OpenAI o3-mini | OpenAI](https://openai.com/index/openai-o3-mini/#:~:text=window%29%20platform,when%20latency%20is%20a%20concern)).

We encourage you to experiment with o3-mini in your SK workflows. Its combination of **advanced reasoning skills, developer-friendly features, and low operational cost** makes it an exciting addition to the toolkit. With Semantic Kernel abstracting away much of the integration hassle, swapping in o3-mini is seamless. Give it a try and see how it **elevates your AI-driven applications** – whether you’re generating code, solving math problems, or orchestrating complex multi-step AI tasks. Happy building!

**References:**

- OpenAI Blog – _“OpenAI o3-mini: Pushing the frontier of cost-effective reasoning.”_ (Jan 31, 2025) ([OpenAI o3-mini | OpenAI](https://openai.com/index/openai-o3-mini/#:~:text=We%E2%80%99re%20releasing%20OpenAI%20o3,mini)) ([OpenAI o3-mini | OpenAI](https://openai.com/index/openai-o3-mini/#:~:text=window%29%20platform,when%20latency%20is%20a%20concern))
- Microsoft Azure AI Blog – _“Announcing the availability of the o3-mini reasoning model in Azure OpenAI Service.”_ ([Announcing the availability of the o3-mini reasoning model in Microsoft Azure OpenAI Service | Microsoft Azure Blog](https://azure.microsoft.com/en-us/blog/announcing-the-availability-of-the-o3-mini-reasoning-model-in-microsoft-azure-openai-service)

- OpenAI API Documentation – _Reasoning models and `reasoning_effort` parameter_ ([Open AI | API Reference](https://platform.openai.com/docs/api-reference/chat/create#chat-create-reasoning_effort))

- Performance Insights – ([OpenAI o3-mini | OpenAI](https://openai.com/index/openai-o3-mini/#:~:text=Mathematics%3A%20With%20low%20reasoning%20effort%2C,consensus%29%20with%2064%20samples))

- Pricing Details – ([Open AI Pricing](https://platform.openai.com/docs/pricing)), ([Azure OpenAI Service Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/))
