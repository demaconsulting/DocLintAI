// Copyright (c) 2025 DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.DocLintAI.Core.Lint;
using Microsoft.Extensions.AI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DemaConsulting.DocLintAI.Core.Document;

/// <summary>
///     Document Information class
/// </summary>
public class DocumentInfo
{
    /// <summary>
    ///     Document metadata (title, author, etc.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = [];

    /// <summary>
    ///     Document elements
    /// </summary>
    public List<DocumentElement> Elements { get; set; } = [];

    /// <summary>
    ///     Load document information from the specified YAML file
    /// </summary>
    /// <param name="yamlFile">YAML file</param>
    /// <returns>Document information</returns>
    public static DocumentInfo Load(string yamlFile)
    {
        // Read the file contents
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<DocumentInfo>(File.ReadAllText(yamlFile));
    }

    /// <summary>
    ///     Save document information to the specified YAML file
    /// </summary>
    /// <param name="yamlFile">YAML file</param>
    public void Save(string yamlFile)
    {
        // Write the file contents
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        File.WriteAllText(yamlFile, serializer.Serialize(this));
    }

    /// <summary>
    ///     Lint the document with the specified quality sets
    /// </summary>
    /// <param name="client">Chat client</param>
    /// <param name="config">Analysis Configuration</param>
    /// <param name="qualitySets">Quality sets</param>
    /// <returns>Results</returns>
    public async Task<Result[]> LintAsync(IChatClient client, AnalysisConfiguration config, IEnumerable<QualitySet> qualitySets)
    {
        // Iterate over every check
        var results = new List<Result>();
        foreach (var check in qualitySets.SelectMany(s => s.Checks))
        {
            // Construct the chat options
            var chatOptions = new ChatOptions { ModelId = config.DocumentModel };

            // Construct the messages seeded with the context
            var chatMessages = config.Context.Select(c => new ChatMessage(ChatRole.System, c)).ToList();
            
            // Construct the messages to provide the document information
            string? position = null;
            foreach (var element in Elements)
            {
                var order = (element.Position != position) ? "first" : "next";
                var context = (element.Position != null) ? $" on {element.Position}" : "";
                position = element.Position;
                var location = $"The {order} item{context} of the document";

                switch (element.Type)
                {
                    case "text":
                        chatMessages.Add(new ChatMessage(ChatRole.System, $"{location} is the text containing the following: {element.Text}"));
                        break;

                    case "image":
                        chatMessages.Add(new ChatMessage(ChatRole.System, $"{location} is an image containing the following: {element.Text}"));
                        break;

                    default:
                        chatMessages.Add(new ChatMessage(ChatRole.System, $"{location} is an element containing the following: {element.Text}"));
                        break;
                }
            }

            // Construct the prompt message
            var queryMessage = new ChatMessage(ChatRole.User, check.Prompt);
            chatMessages.Add(queryMessage);

            // Get the response
            var response = await client.GetResponseAsync(chatMessages, chatOptions);

            // Return the description
            var responseText = string.Concat(response.Messages.Select(m => m.Text));

            // Determine whether the response constitutes a pass
            bool? pass = check.Pass != null
                ? responseText.Contains(check.Pass, StringComparison.InvariantCultureIgnoreCase)
                : null;
            
            // Add the result
            results.Add(new Result(check.Title, responseText, pass));
        }

        // Return the results
        return [..results];
    }
}