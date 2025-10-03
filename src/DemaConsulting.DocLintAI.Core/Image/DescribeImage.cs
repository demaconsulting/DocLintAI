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

using Microsoft.Extensions.AI;

namespace DemaConsulting.DocLintAI.Core.Image;

/// <summary>
///     Helper class to get an image description
/// </summary>
public static class DescribeImage
{
    /// <summary>
    ///     Describe an image
    /// </summary>
    /// <param name="client">Chat client</param>
    /// <param name="config">Analysis Configuration</param>
    /// <param name="imageBytes">Image bytes</param>
    /// <param name="imageType">Image type</param>
    /// <returns>Image description</returns>
    public static async Task<string> DescribeAsync(IChatClient client, AnalysisConfiguration config, byte[] imageBytes, string imageType = "image/png")
    {
        // Construct the chat options
        var chatOptions = new ChatOptions { ModelId = config.ImageModel };
        
        // Construct the messages seeded with the context
        var chatMessages = config.Context.Select(c => new ChatMessage(ChatRole.System, c)).ToList();

        // Construct the query message with the image
        var queryMessage = new ChatMessage(ChatRole.User, "Please describe the contents of this image.");
        queryMessage.Contents.Add(new DataContent(imageBytes, imageType));
        chatMessages.Add(queryMessage);

        // Get the response
        var response = await client.GetResponseAsync(chatMessages, chatOptions);

        // Return the description
        return string.Concat(response.Messages.Select(m => m.Text));
    }
}