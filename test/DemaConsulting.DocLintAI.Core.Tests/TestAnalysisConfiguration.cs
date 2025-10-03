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

namespace DemaConsulting.DocLintAI.Core.Tests;

/// <summary>
///     Unit tests for the <see cref="AnalysisConfiguration"/> class.
/// </summary>
[TestClass]
public sealed class TestAnalysisConfiguration
{
    /// <summary>
    ///     Path to the examples
    /// </summary>
    private const string ExamplesPath = @"..\..\..\..\..\examples";

    /// <summary>
    ///     This test checks reading the config1 analysis configuration file.
    /// </summary>
    [TestMethod]
    public void Config1_ReadSuccess()
    {
        // Act
        var config1 = AnalysisConfiguration.Load($@"{ExamplesPath}\config1\custom-name-with-root.yaml");
        
        // Assert
        Assert.AreEqual("config1-document-model", config1.DocumentModel);
        Assert.AreEqual("config1-image-model", config1.ImageModel);
        Assert.HasCount(2, config1.Context);
        Assert.AreEqual("This is an example context", config1.Context[0]);
        Assert.AreEqual("It just has config1 information", config1.Context[1]);
    }

    /// <summary>
    ///     This test checks reading the config2 analysis configuration file.
    /// </summary>
    [TestMethod]
    public void Config2_ReadSuccess()
    {
        // Act
        var config2 = AnalysisConfiguration.Load($@"{ExamplesPath}\config2\custom-name.yaml");

        // Assert
        Assert.AreEqual("config2-document-model", config2.DocumentModel);
        Assert.AreEqual("llava", config2.ImageModel);
        Assert.HasCount(2, config2.Context);
        Assert.AreEqual("This is a DocLintAI example document", config2.Context[0]);
        Assert.AreEqual("This is the config2 context", config2.Context[1]);
    }

    /// <summary>
    ///     This test checks reading the config3 analysis configuration file.
    /// </summary>
    [TestMethod]
    public void Config3_ReadSuccess()
    {
        // Act
        var config3 = AnalysisConfiguration.Load($@"{ExamplesPath}\config3\child");

        // Assert
        Assert.AreEqual("llama3", config3.DocumentModel);
        Assert.AreEqual("config3-image-model", config3.ImageModel);
        Assert.HasCount(2, config3.Context);
        Assert.AreEqual("This is a DocLintAI example document", config3.Context[0]);
        Assert.AreEqual("This is the config3 context", config3.Context[1]);
    }
}