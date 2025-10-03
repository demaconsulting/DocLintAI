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

using DemaConsulting.DocLintAI.Core.Document;
using DemaConsulting.DocLintAI.Core.Document.Pdf;
using DemaConsulting.DocLintAI.Core.Lint;
using OllamaSharp;

namespace DemaConsulting.DocLintAI.Core.Tests.Document;

/// <summary>
///     Unit tests for linting <see cref="DocumentInfo"/>
/// </summary>
[TestClass]
public class TestDocumentInfoLint
{
    /// <summary>
    ///     Path to the examples Pdf
    /// </summary>
    private const string ExamplesPdfPath = @"..\..\..\..\..\examples\pdf";

    /// <summary>
    ///     Path to the examples quality checks
    /// </summary>
    private const string ExamplesQualityPath = @"..\..\..\..\..\examples\quality";

    /// <summary>
    ///     Test linting the sample1.pdf document.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Sample1_Lint()
    {
        // Assemble
        var client = new OllamaApiClient("http://localhost:11434");
        var config = AnalysisConfiguration.Load(ExamplesQualityPath);
        var qualitySet = QualitySet.Load($@"{ExamplesQualityPath}\sample1.yaml");

        // Act
        var docInfo = await PdfDocumentParser.ParseDocumentAsync(client, config, $@"{ExamplesPdfPath}\sample1.pdf");
        var results = await docInfo.LintAsync(client, config, [qualitySet]);

        // Assert
        Assert.HasCount(3, results);
        Assert.AreEqual("Sample PDF Check", results[0].Title);
        Assert.IsTrue(results[0].Pass);
        Assert.AreEqual("Latin Check", results[1].Title);
        Assert.IsTrue(results[1].Pass);
        Assert.AreEqual("Description", results[2].Title);
        Assert.IsNull(results[2].Pass);
        Assert.Contains("Sample PDF File", results[2].Text, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     Test linting the sample2.pdf document.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Sample2_Lint()
    {
        // Assemble
        var client = new OllamaApiClient("http://localhost:11434");
        var config = AnalysisConfiguration.Load(ExamplesQualityPath);
        var qualitySet = QualitySet.Load($@"{ExamplesQualityPath}\sample2.yaml");

        // Act
        var docInfo = await PdfDocumentParser.ParseDocumentAsync(client, config, $@"{ExamplesPdfPath}\sample2.pdf");
        var results = await docInfo.LintAsync(client, config, [qualitySet]);

        // Assert
        Assert.HasCount(3, results);
        Assert.AreEqual("Minecraft Player Customization", results[0].Title);
        Assert.IsTrue(results[0].Pass);
        Assert.AreEqual("Character Skins Check", results[1].Title);
        Assert.IsTrue(results[1].Pass);
        Assert.AreEqual("Description", results[2].Title);
        Assert.IsNull(results[2].Pass);
        Assert.Contains("Player customization", results[2].Text, StringComparison.InvariantCultureIgnoreCase);
    }
}