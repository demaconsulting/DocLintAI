# DocLintAI

This project contains DocLintAI - an AI document linting tool using document
comprehension and prompts to perform document quality checks.


## Installation

At this time DocLintAI requires:

- [DotNet 8](https://dotnet.microsoft.com/en-us/download)
- [Ollama](https://www.ollama.com) installed on the local PC with:
  - The llama3 model for document processing (run `ollama pull llama3`)
  - The llava model for image processing (run `ollama pull llava`)


## Usage

To use DocLintAI:

1. Ensure the ollama server is running (run `ollama serve`)
2. Run DocLintAI specifying the PDF document and the quality sets to apply to it


## Configuration

DocLintAI uses a hierarchical configuration YAML format to specify settings and contextual information.

```yaml
root: true
documentModel: llama3
imageModel: llava
context:
- This documentation is for the XXX project.
- The XXX project is used to do YYY.
```

When DocLintAI runs, it searches for `doclintai.yaml` files - starting in the current folder and
moving up the folder structure until it hits a file marked with `root: true`. This allows projects
to put a root `doclintai.yaml` at the top of the folder structure with basic project information,
and then for sub-folders to include extra details and/or override the ML models used for analysis.


## Quality sets

DocLintAI uses document quality sets specified in YAML format to perform quality checks on documents.

```yaml
checks:
- title: Spelling Check
  prompt: Are there any obvious spelling errors in the document (Yes or No)?
  pass: No

- title: Grammar Check
  prompt: Are there any obvious grammar mistakes in the document (Yes or No)?
  pass: No

- title: Ambiguity Check
  prompt: Are there any bad ambiguities in the document that could confuse a reader (Yes or No)?
  pass: No

- title: Heading Check
  prompt: Are all section headings well named and reflect the content of the sections (Yes or No)?
  pass: Yes

- title: Description
  prompt: Describe the document.
```

The DocLintAI tool can run multiple quality checks on a document.


## Internals

DocLintAI first parses the document to an internal representation. For example when given a PDF
document, it analyzes every page of the PDF extracting both text and images in reading-order and
uses an image ML model to create a textual description of the images.

DocLintAI then runs every quality check by:

1. Creating a new ML Chat
2. Seeding the chat with project contextual information
3. Seeding the chat with the document contents
4. Running the quality check prompt through the chat
5. If the quality check has a pass criteria then the chat-response is checked for the passing criteria


## Security Considerations

The use of [Ollama](https://www.ollama.com) on the local PC, and the use of separate ML chats ensures
that no intellectual property leaves the PC, and that no intellectual property bleeds through from one
document to the next.
