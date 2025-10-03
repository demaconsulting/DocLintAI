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
using System.Text;
using DemaConsulting.DocLintAI.Core.Image;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace DemaConsulting.DocLintAI.Core.Document.Pdf;

/// <summary>
///     PDF Document Parser
/// </summary>
public class PdfDocumentParser : IDocumentParser
{
    /// <inheritdoc />
    public async Task<DocumentInfo> ParseAsync(IChatClient client, AnalysisConfiguration config, string filePath)
    {
        return await ParseDocumentAsync(client, config, filePath);
    }

    /// <summary>
    ///     Parse a PDF document
    /// </summary>
    /// <param name="client">Chat client</param>
    /// <param name="config">Analysis configuration</param>
    /// <param name="filePath">File path</param>
    /// <returns>Document information</returns>
    public static async Task<DocumentInfo> ParseDocumentAsync(IChatClient client, AnalysisConfiguration config, string filePath)
    {
        // Construct the document information
        var docInfo = new DocumentInfo();

        // Open the PDF document
        using var pdfDoc = PdfDocument.Open(filePath);

        // Add the metadata for the document file name
        docInfo.Metadata["fileName"] = Path.GetFileName(filePath);

        // Add the title if available
        if (!string.IsNullOrWhiteSpace(pdfDoc.Information.Title))
            docInfo.Metadata["title"] = pdfDoc.Information.Title;

        // Add the author if available
        if (!string.IsNullOrWhiteSpace(pdfDoc.Information.Author))
            docInfo.Metadata["author"] = pdfDoc.Information.Author;

        // Add the subject if available
        if (!string.IsNullOrWhiteSpace(pdfDoc.Information.Subject))
            docInfo.Metadata["subject"] = pdfDoc.Information.Subject;

        // Process every page
        foreach (var pdfPage in pdfDoc.GetPages())
        {
            // Get the words and group them into text blocks
            var pdfWords = pdfPage.GetWords(NearestNeighbourWordExtractor.Instance);
            var pdfBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(pdfWords);

            // Construct the list of items (text blocks and images)
            var pdfItems = new List<PdfItem>();
            pdfItems.AddRange(pdfBlocks.Select(b => new PdfTextBlock(b)));
            pdfItems.AddRange(pdfPage.GetImages().Select(i => new PdfImage(i)));

            // Iterate through the items in order
            var builder = new StringBuilder();
            foreach (var pdfItem in Order(pdfItems))
            {
                if (pdfItem is PdfTextBlock textBlock)
                {
                    // Append text blocks to the text builder
                    builder.AppendLine(textBlock.Text.Text);
                }
                else if (pdfItem is PdfImage image)
                {
                    // If we have text accumulated, add it as a new element
                    if (builder.Length > 0)
                    {
                        docInfo.Elements.Add(
                            new DocumentElement
                            {
                                Type = "text",
                                Position = $"Page {pdfPage.Number}",
                                Text = builder.ToString()
                            });
                        builder.Clear();
                    }

                    // Emit the description of the image
                    var description = await DescribeImage.DescribeAsync(client, config, image.Bytes);
                    docInfo.Elements.Add(
                        new DocumentElement
                        {
                            Type = "image",
                            Position = $"Page {pdfPage.Number}",
                            Text = description
                        });
                }
            }

            // Emit any remaining text on this page
            if (builder.Length > 0)
            {
                docInfo.Elements.Add(
                    new DocumentElement
                    {
                        Type = "text",
                        Position = $"Page {pdfPage.Number}",
                        Text = builder.ToString()
                    });
                builder.Clear();
            }
        }

        // Return the document information
        return docInfo;
    }


    /// <summary>
    ///     Return the PDF items in reading order using a topological sort.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static IEnumerable<PdfItem> Order(IReadOnlyList<PdfItem> items, double t = 5)
    {
        // Build the directed graph
        var graph = BuildGraph(items, t);

        // Remove the items in topological order
        while (graph.Count > 0)
        {
            var maxCount = graph.Max(kvp => kvp.Value.Count);
            var (index, _) = graph.FirstOrDefault(kvp => kvp.Value.Count == maxCount);
            graph.Remove(index);

            foreach (var g in graph)
            {
                g.Value.Remove(index);
            }

            var block = items[index];
            yield return block;
        }
    }

    /// <summary>
    ///     Construct a directed graph representing the "before" relation between text blocks.
    /// </summary>
    /// <param name="items">PDF Items</param>
    /// <param name="t">Tolerance</param>
    /// <returns>Directed graph</returns>
    private static Dictionary<int, List<int>> BuildGraph(IReadOnlyList<PdfItem> items, double t)
    {
        var graph = new Dictionary<int, List<int>>();

        for (var i = 0; i < items.Count; i++)
        {
            graph.Add(i, []);
        }

        for (var i = 0; i < items.Count; i++)
        {
            var a = items[i];
            for (var j = 0; j < items.Count; j++)
            {
                if (i == j) continue;
                var b = items[j];

                if (PdfItem.GetBefore(a, b, t))
                {
                    graph[i].Add(j);
                }
            }
        }

        return graph;
    }

    /// <summary>
    ///     PDF Item abstract class
    /// </summary>
    private abstract class PdfItem
    {
        /// <summary>
        ///     Get the bounding box of the item
        /// </summary>
        public abstract PdfRectangle BoundingBox { get; }

        /// <summary>
        ///     Get the "before" relation between two items
        /// </summary>
        /// <param name="a">First item</param>
        /// <param name="b">Second item</param>
        /// <param name="t">Visual tolerance</param>
        /// <returns>True if a comes before b</returns>
        public static bool GetBefore(PdfItem a, PdfItem b, double t)
        {
            var xRelation = IntervalRelationsHelper.GetRelationX(a.BoundingBox, b.BoundingBox, t);
            var yRelation = IntervalRelationsHelper.GetRelationY(a.BoundingBox, b.BoundingBox, t);

            return xRelation == IntervalRelations.Precedes ||
                   yRelation == IntervalRelations.Precedes ||
                   xRelation == IntervalRelations.Meets ||
                   yRelation == IntervalRelations.Meets ||
                   xRelation == IntervalRelations.Overlaps ||
                   yRelation == IntervalRelations.Overlaps;
        }
    }

    /// <summary>
    ///     PDF Text Block item
    /// </summary>
    /// <param name="text">Text block</param>
    private sealed class PdfTextBlock(TextBlock text) : PdfItem
    {
        public TextBlock Text { get; } = text;

        public override PdfRectangle BoundingBox => Text.BoundingBox;
    }

    /// <summary>
    ///     PDF Image item
    /// </summary>
    /// <param name="image">Image</param>
    private sealed class PdfImage(IPdfImage image) : PdfItem
    {
        private IPdfImage Image { get; } = image;

        public override PdfRectangle BoundingBox => Image.Bounds;

        public byte[] Bytes => Image.TryGetPng(out var pngBytes) ? pngBytes : Image.RawBytes.ToArray();
    }
}