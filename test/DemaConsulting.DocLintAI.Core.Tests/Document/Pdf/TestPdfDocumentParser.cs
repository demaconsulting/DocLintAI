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

using DemaConsulting.DocLintAI.Core.Document.Pdf;
using OllamaSharp;

namespace DemaConsulting.DocLintAI.Core.Tests.Document.Pdf;

/// <summary>
///     Unit tests for <see cref="PdfDocumentParser"/>
/// </summary>
[TestClass]
public class TestPdfDocumentParser
{
    /// <summary>
    ///     Path to the examples
    /// </summary>
    private const string ExamplesPath = @"..\..\..\..\..\examples\pdf";

    /// <summary>
    ///     Test parsing the sample1.pdf document.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task PdfDocumentParser_Sample1()
    {
        // Assemble
        var client = new OllamaApiClient("http://localhost:11434");
        var config = AnalysisConfiguration.Load(ExamplesPath);
        
        // Act
        var docInfo = await PdfDocumentParser.ParseDocumentAsync(client, config, $@"{ExamplesPath}\sample1.pdf");
        
        // Assert
        Assert.AreEqual("sample1.pdf", docInfo.Metadata["fileName"]);
        Assert.AreEqual("sample", docInfo.Metadata["title"]);
        Assert.HasCount(1, docInfo.Elements);
        Assert.AreEqual("text", docInfo.Elements[0].Type);
        Assert.AreEqual("Page 1", docInfo.Elements[0].Position);
        Assert.StartsWith("Sample PDF", docInfo.Elements[0].Text);
    }

    /// <summary>
    ///     Test parsing the sample2.pdf document.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task PdfDocumentParser_Sample2()
    {
        // Assemble
        var client = new OllamaApiClient("http://localhost:11434");
        var config = AnalysisConfiguration.Load(ExamplesPath);

        // Act
        var docInfo = await PdfDocumentParser.ParseDocumentAsync(client, config, $@"{ExamplesPath}\sample2.pdf");

        // Assert
        Assert.AreEqual("sample2.pdf", docInfo.Metadata["fileName"]);
        Assert.HasCount(3, docInfo.Elements);
        Assert.AreEqual("image", docInfo.Elements[0].Type);
        Assert.AreEqual("Page 1", docInfo.Elements[0].Position);
        Assert.Contains("Adventure Mode", docInfo.Elements[0].Text, StringComparison.InvariantCultureIgnoreCase);
        Assert.AreEqual("text", docInfo.Elements[1].Type);
        Assert.AreEqual("Page 1", docInfo.Elements[1].Position);
        Assert.Contains("Player Customization", docInfo.Elements[1].Text, StringComparison.InvariantCultureIgnoreCase);
        Assert.AreEqual("image", docInfo.Elements[2].Type);
        Assert.AreEqual("Page 1", docInfo.Elements[2].Position);
        Assert.Contains("Skins", docInfo.Elements[2].Text, StringComparison.InvariantCultureIgnoreCase);
    }
}