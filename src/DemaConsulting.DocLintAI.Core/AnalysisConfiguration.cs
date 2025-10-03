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

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DemaConsulting.DocLintAI.Core;

/// <summary>
///     Analysis Configuration class
/// </summary>
public class AnalysisConfiguration
{
    /// <summary>
    ///     Document Analysis Model name (e.g. "llama3")
    /// </summary>
    public string DocumentModel { get; set; } = string.Empty;

    /// <summary>
    ///     Image Analysis Model name (e.g. "llava")
    /// </summary>
    public string ImageModel { get; set; } = string.Empty;
    
    /// <summary>
    ///     Context information
    /// </summary>
    public List<string> Context { get; set; } = [];

    /// <summary>
    ///     Load analysis configuration from the specified path
    /// </summary>
    /// <param name="path">Analysis path (file or folder)</param>
    /// <param name="inclusive">Allow path-walking for configuration</param>
    /// <returns>Analysis Configuration instance</returns>
    public static AnalysisConfiguration Load(string? path = null, bool inclusive = true)
    {
        // Handle null path as the current directory
        path ??= Directory.GetCurrentDirectory();
        
        // Start with empty analysis information
        var analysis = new AnalysisFile();

        // Process the path
        if (File.Exists(path))
        {
            // Read the file and then move to the parent folder
            analysis = AnalysisFile.Read(path);
            path = Ascend(Path.GetDirectoryName(path));
        }
        else if (Directory.Exists(path))
        {
            // Process the folder and then move to the parent folder
            analysis = ProcessFolder(analysis, path);
            path = Ascend(path);
        }
        else
        {
            // Unsupported path
            throw new FileNotFoundException("The specified analysis configuration path is invalid", path);
        }

        // Loop walking up to the root
        while (!analysis.Root && inclusive && !string.IsNullOrEmpty(path))
        {
            analysis = ProcessFolder(analysis, path);
            path = Ascend(path);
        }

        // Return the configuration
        return new AnalysisConfiguration
        {
            DocumentModel = analysis.DocumentModel ?? "llama3",
            ImageModel = analysis.ImageModel ?? "llava",
            Context = analysis.Context
        };
    }

    /// <summary>
    ///     Process the specified folder for configuration
    /// </summary>
    /// <param name="current">Current configuration</param>
    /// <param name="folder">Folder to process</param>
    /// <returns>Updated analysis file information</returns>
    private static AnalysisFile ProcessFolder(AnalysisFile current, string folder)
    {
        // Construct the proposed file name
        var configPath = Path.Combine(folder, "doclintai.yaml");
        if (!File.Exists(configPath))
            return current;
        
        // Process the file
        var file = AnalysisFile.Read(configPath);
        file.Merge(current);
        return file;
    }

    /// <summary>
    ///     Ascend one folder in the path
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns>Path ascended one folder</returns>
    private static string Ascend(string? path)
    {
        return Directory.GetParent(path ?? string.Empty)?.FullName ?? string.Empty;
    }

    /// <summary>
    ///     Analysis File class
    /// </summary>
    private sealed class AnalysisFile
    {
        /// <summary>
        ///     Document Analysis Model name
        /// </summary>
        public string? DocumentModel { get; set; }
        
        /// <summary>
        ///     Image Analysis Model name
        /// </summary>
        public string? ImageModel { get; set; }
        
        /// <summary>
        ///     Context information
        /// </summary>
        public List<string> Context { get; set; } = [];

        /// <summary>
        ///     Root information
        /// </summary>
        public bool Root { get; set; } = false;

        /// <summary>
        ///     Read an AnalysisFile from the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>AnalysisFile instance</returns>
        public static AnalysisFile Read(string fileName)
        {
            // Read the file contents
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<AnalysisFile>(File.ReadAllText(fileName));
        }

        /// <summary>
        ///     Merge analysis information with child
        /// </summary>
        /// <param name="child">Child analysis file</param>
        public void Merge(AnalysisFile child)
        {
            // Override with the child's document model if specified
            if (!string.IsNullOrEmpty(child.DocumentModel))
                DocumentModel = child.DocumentModel;

            // Override with the child's image model if specified
            if (!string.IsNullOrEmpty(child.ImageModel))
                ImageModel = child.ImageModel;

            // Append the child's context information
            Context = [..Context.Union(child.Context)];
        }
    }
}