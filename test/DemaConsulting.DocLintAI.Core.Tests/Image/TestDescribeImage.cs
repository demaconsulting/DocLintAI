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

using DemaConsulting.DocLintAI.Core.Image;
using OllamaSharp;

namespace DemaConsulting.DocLintAI.Core.Tests.Image;

/// <summary>
///     Tests for the DescribeImage class
/// </summary>
[TestClass]
public class TestDescribeImage
{
    /// <summary>
    ///     Path to the examples
    /// </summary>
    private const string ExamplesPath = @"..\..\..\..\..\examples\images";

    /// <summary>
    ///     This test checks the image of a cat is described adequately.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This test verifies that the DescribeImage.DescribeAsync method can successfully describe an image of a cat.
    ///     </para>
    ///     <para>
    ///         This test requires an Ollama server running with the LLaVA model downloaded.
    ///     </para>
    /// </remarks>
    /// <returns></returns>
    [TestMethod]
    public async Task DescribeImage_Cat()
    {
        // Assemble
        var client = new OllamaApiClient("http://localhost:11434");
        var config = AnalysisConfiguration.Load(ExamplesPath);
        var imageBytes = await File.ReadAllBytesAsync($@"{ExamplesPath}\cat.png");

        // Act
        var description = await DescribeImage.DescribeAsync(client, config, imageBytes);
        
        // Assert
        Assert.Contains("Cat", description, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Grass", description, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     This test check the image of a flowchart is described adequately.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This test verifies that the DescribeImage.DescribeAsync method can successfully describe an image of a flowchart.
    ///     </para>
    ///     <para>
    ///         This test requires an Ollama server running with the LLaVA model downloaded.
    ///     </para>
    /// </remarks>
    /// <returns></returns>
    [TestMethod]
    public async Task DescribeImage_Flowchart()
    {
        // Assemble
        var client = new OllamaApiClient("http://localhost:11434");
        var config = AnalysisConfiguration.Load(ExamplesPath);
        var imageBytes = await File.ReadAllBytesAsync($@"{ExamplesPath}\flowchart.png");

        // Act
        var description = await DescribeImage.DescribeAsync(client, config, imageBytes);

        // Assert
        Assert.Contains("Flowchart", description, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Lamp", description, StringComparison.InvariantCultureIgnoreCase);
    }
}