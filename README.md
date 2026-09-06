# TextDiffToHtml
Side by side Text diff to html in C#
---

Originally, this source is the C# conversion of this PowerShell script:

https://github.com/Aiikon/TextDiff

based on the DiffPlex library:

https://github.com/mmanela/diffplex

And also a C# conversion of this LinqPad script:

https://github.com/lassevk/DiffLib/tree/main/Examples

based on the DiffLib library:

https://github.com/lassevk/DiffLib

Then, other algorithms were added:

https://github.com/iyulab/TextDiff

https://github.com/thomashambach/csharpdiff

Aiikon presents two interesting views of the DiffPlex library: a side-by-side view and an inline view. The DiffLib example, on the other hand, presents another view: a compact view. The goal of TextDiffToHtml is therefore to test comparison libraries using some of the best display methods.
```
string left = @"ABC abc
    DEF def
    HIJ
    KLM";

string right = @"ABC abc
    DEF DEF
    KLM
    XYZ";
```

![Side by Side Sample](https://raw.githubusercontent.com/Aiikon/TextDiff/master/Examples/SideBySideSample.png)

![Inline Sample](https://raw.githubusercontent.com/Aiikon/TextDiff/master/Examples/InlineSample.png)

See the [samples](http://patrice.dargenton.free.fr/CodesSources/TextDiffToHtmlSamples.html).

[DiffMatchPatch](https://github.com/google/diff-match-patch) is also used as the tool behind the Track Changes mode of the DiffPlex library. For DiffLib, the Track Changes mode implementation is poor, it should probably be optimized.

# Keywords
TextDiff, Text comparison, DiffPlex, DiffLib, TextDiff.Sharp, CSharpDiff, DiffMatchPatch.

# Table of content
- [Keywords](#keywords)
- [Features](#features)
- [Explanations](#explanations)
    - [Semantic diff using DiffLibLLM](#semantic-diff-using-difflibllm)
    - [Command line arguments](#command-line-arguments)
    - [SendTo menu](#sendto-menu)
- [Dependencies](#dependencies)
- [Versions](#versions)
- [Links](#links)

# Features
- DiffPlex, DiffLib, TextDiff.Sharp and CSharpDiff libraries are available;
- Side by side, Inline, Compact and Track changes display modes are available;
- Using [Vereyon's WebBrowser control](https://github.com/Vereyon/WebBrowser), it is possible to render the html in the Winform screen, before showing it your favorite Web browser. Note: Some HTML display styles does not work in the preview browser, they are only available in the external web browser: for example, maximum HTML column sizes (max-width: 100vw);
- Semantic diff using [DiffLibLLM](https://github.com/PatriceDargenton/DiffLibLLM): Compare texts with their translation.

# Explanations

## Semantic diff using DiffLibLLM
- Download and install [Ollama](https://ollama.com/)
  - Download some Ollama embedding models:
  ```
  Ollama pull all-minilm
  Ollama pull nomic-embed-text
  Ollama pull ...
  ```
  - Configure TextDiffToHtml.dll.config with them: TextDiffLLMModels: all-minilm;nomic-embed-text;... and TextDiffLLMConfigured: True
  - See [Sample2](http://patrice.dargenton.free.fr/ai/DiffLibLLM/Semantic_Sample2_using_embeddinggemma.html), [Sample3](http://patrice.dargenton.free.fr/ai/DiffLibLLM/Semantic_Sample3_using_embeddinggemma.html), [Sample4](http://patrice.dargenton.free.fr/ai/DiffLibLLM/Semantic_Sample4_using_embeddinggemma.html) and [Sample5](http://patrice.dargenton.free.fr/ai/DiffLibLLM/Semantic_Sample5_using_mxbai-embed-large.html).
- Semantic matching is sensitive to tokenization (see Sample5: "The midnight hour is close at hand"), yet it generally performs quite well, despite being somewhat slow, and the matching is visible thanks to a color gradient within the sentence. It is evident that tokenization is merely a first step, albeit one that already accomplishes part of the task. A subsequent step would involve using an LLM—specifically for translation, rather than just vectorization; the matching algorithm should leverage increasingly powerful tools (tokenization, vectorization, translation) to refine results, while remaining mindful of the required computational time. Tokenization is an imperfect process, as operations on semantic vectors are not entirely transitive (see the [DiffLib](https://github.com/PatriceDargenton/DiffLibLLM) examples); nevertheless, vectorization alone already yields interesting results. Moreover, we know that with the most powerful LLMs, this is already sufficient to generate powerful intelligence.

## Command line arguments

- 2 arguments: Full file path of the first file, Full file path of the second file

- 3 arguments: DisplayMode (SideBySide/Inline/Compact/TrackChanges), Full file path of the first file, Full file path of the second file

- 4 arguments: ShowIdenticalLines/HideIdenticalLines, DisplayMode, Full file path of the first file, Full file path of the second file

- 5 arguments: Library (DiffPlex/DiffLib/TextDiffSharp/CSharpDiff), ShowIdenticalLines/HideIdenticalLines, DisplayMode, Full file path of the first file, Full file path of the second file

## SendTo menu
Put a shortcut to TextDiffToHtml.exe into the SendTo menu and then select two files to compare and send them to this shortcut.

The SendTo folder is located there:

C:\Users\[Your profile]\AppData\Roaming\Microsoft\Windows\SendTo

Note: AppData is a hidden folder, but you can still type and view the folder in the File Explorer.

# Dependencies

This project relies on the following NuGet packages:

- [DiffPlex](https://www.nuget.org/packages/DiffPlex/) ![NuGet](https://img.shields.io/nuget/v/DiffPlex.svg)
- [DiffLib](https://www.nuget.org/packages/DiffLib/) ![NuGet](https://img.shields.io/nuget/v/DiffLib.svg)
- [TextDiff.Sharp](https://www.nuget.org/packages/TextDiff.Sharp/) ![NuGet](https://img.shields.io/nuget/v/TextDiff.Sharp.svg)
- [CSharpDiff](https://www.nuget.org/packages/CSharpDiff/) ![NuGet](https://img.shields.io/nuget/v/CSharpDiff.svg)
- [DiffMatchPatch](https://www.nuget.org/packages/DiffMatchPatch/) ![NuGet](https://img.shields.io/nuget/v/DiffMatchPatch.svg)
- [DiffLibLLM](https://www.nuget.org/packages/DiffLibLLM/) ![NuGet](https://img.shields.io/nuget/v/DiffLibLLM.svg)
- [Vereyon.Windows.WebBrowser](https://www.nuget.org/packages/Vereyon.Windows.WebBrowser/) ![NuGet](https://img.shields.io/nuget/v/Vereyon.Windows.WebBrowser.svg)
- [Enums.Net](https://www.nuget.org/packages/Enums.Net/) ![NuGet](https://img.shields.io/nuget/v/Enums.Net.svg)

# Versions

See [Changelog.md](Changelog.md)

# Links

See also:

- Semantic diff using [DiffLibLLM](https://github.com/PatriceDargenton/DiffLibLLM): Compare texts with their translation

- [DocToText](https://github.com/PatriceDargenton/DocToText): MS-Word .docx & .doc converter to plain text (.txt) and Markdown (.md) in C#

- [TextDiffOptions](https://github.com/PatriceDargenton/TextDiffOptions): Options interface for TextDiffToHtml (or WinMerge) comparator in C#

- [VBWinDiff](https://github.com/PatriceDargenton/VBWinDiff) (french): same utility in french and VB.Net