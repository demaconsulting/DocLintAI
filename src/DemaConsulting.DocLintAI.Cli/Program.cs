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

using DemaConsulting.DocLintAI.Core;
using DemaConsulting.DocLintAI.Core.Document.Pdf;
using DemaConsulting.DocLintAI.Core.Lint;
using OllamaSharp;

namespace DemaConsulting.DocLintAI.Cli;

/// <summary>
///     DocLintAI console application class
/// </summary>
public static class Program
{
    /// <summary>
    ///     Basic application entry point
    /// </summary>
    /// <param name="args">Application arguments</param>
    public static async Task Main(string[] args)
    {
        // Ensure we have at least the input document argument and one quality set.
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: DocLintAI <input-document> [quality-sets]");
            Environment.ExitCode = 1;
            return;
        }

        // Construct the client
        var client = new OllamaApiClient("http://localhost:11434");
        
        // Construct the analysis configuration
        var config = AnalysisConfiguration.Load();
        
        // Load the quality sets.
        var qualitySets = args[1..].Select(QualitySet.Load).ToList();

        // Parse the document
        Console.WriteLine($"Parsing {args[0]}...");
        var docInfo = await PdfDocumentParser.ParseDocumentAsync(client, config, args[0]);

        // Lint the document
        Console.WriteLine($"Linting {args[0]}...");
        var results = await docInfo.LintAsync(client, config, qualitySets);

        // Report the results
        foreach (var result in results)
        {
            Console.WriteLine($"# {result.Title}");

            // Process the result
            switch (result.Pass)
            {
                case null:
                    // No pass/fail - just report the result
                    Console.WriteLine(result.Text);
                    Console.WriteLine();
                    break;
                
                case true:
                    // Passed
                    Console.WriteLine("   Pass");
                    Console.WriteLine();
                    break;
                
                default:
                    // Failed
                    Console.WriteLine("    Fail");
                    Console.WriteLine();
                    Environment.ExitCode = 1;
                    break;
            }
        }
    }
}