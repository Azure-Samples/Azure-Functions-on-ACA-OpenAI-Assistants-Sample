﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.OpenAI.Assistants;

/// <summary>
/// Class that defines all the built-in functions for executing CNCF Serverless Workflows.
/// IMPORTANT: Renaming methods in this class is a breaking change!
/// </summary>
class BuiltInFunctionsProvider : IFunctionProvider
{
    /// <inheritdoc/>
    Task<ImmutableArray<FunctionMetadata>> IFunctionProvider.GetFunctionMetadataAsync() =>
        Task.FromResult(this.GetFunctionMetadata().ToImmutableArray());

    // TODO: Not sure what this is for...
    /// <inheritdoc/>
    ImmutableDictionary<string, ImmutableArray<string>> IFunctionProvider.FunctionErrors =>
        new Dictionary<string, ImmutableArray<string>>().ToImmutableDictionary();


    internal static string GetBuiltInFunctionName(string functionName)
    {
        return $"OpenAI::{functionName}";
    }

    /// <summary>
    /// Returns an enumeration of all the function triggers defined in this class.
    /// </summary>
    IEnumerable<FunctionMetadata> GetFunctionMetadata()
    {
        foreach (MethodInfo method in this.GetType().GetMethods())
        {
            if (method.GetCustomAttribute<FunctionNameAttribute>() is not FunctionNameAttribute)
            {
                // Not an Azure Function definition
                continue;
            }

            FunctionMetadata metadata = new()
            {
                // NOTE: We always use the method name and ignore the function name
                Name = GetBuiltInFunctionName(method.Name),
                ScriptFile = $"assembly:{method.ReflectedType.Assembly.FullName}",
                EntryPoint = $"{method.ReflectedType.FullName}.{method.Name}",
                Language = "DotNetAssembly",
            };

            // Scan the parameters for binding attributes and add them to the bindings collection
            // so that we can register them with the Functions runtime.
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.GetCustomAttribute<OpenAIServiceAttribute>() is not null)
                {
                    // NOTE: We assume each OpenAI service function in this file defines the parameter name as "service".
                    metadata.Bindings.Add(BindingMetadata.Create(new JObject(
                        new JProperty("type", "openAIService"),
                        new JProperty("name", "service"),
                        new JProperty("direction", "in"))));
                }
            }

            yield return metadata;
        }
    }
}
