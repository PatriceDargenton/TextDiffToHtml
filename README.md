# TextDiffToHtml
Side by side Text diff to html in C#
---

This source is the C# conversion of this PowerShell script:

https://github.com/Aiikon/TextDiff

based on the DiffPlex library:

https://github.com/mmanela/diffplex

And also a C# conversion of this LinqPad script:

https://github.com/lassevk/DiffLib/tree/main/Examples

based on the DiffLib library:

https://github.com/lassevk/DiffLib

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

[DiffMatchPatch](https://github.com/google/diff-match-patch) was also used as the tool behind the Track Changes mode of the DiffPlex library. For DiffLib, the Track Changes mode implementation is poor, it should probably be optimized.

# Keywords
TextDiff, Text comparison, DiffPlex, DiffLib, DiffMatchPatch.

# Table of content
- [Keywords](#keywords)
- [Features](#features)
- [Explanations](#explanations)
    - [Command line arguments](#command-line-arguments)
    - [SendTo menu](#sendto-menu)
- [Dependencies](#dependencies)
- [Versions](#versions)
- [Links](#links)

# Features
- DiffPlex and DiffLib libraries are available;
- Side by side, Inline, Compact and Track changes display modes are available;
- Using [Vereyon's WebBrowser control](https://github.com/Vereyon/WebBrowser), it is possible to render the html in the Winform screen, before showing it your favorite Web browser. Note: Some HTML display styles does not work in the preview browser, they are only available in the external web browser: for example, maximum HTML column sizes (max-width: 100vw).

# Explanations

## Command line arguments

- 2 arguments: Full file path of the first file, Full file path of the second file

- 3 arguments: DisplayMode (SideBySide/Inline/Compact/TrackChanges), Full file path of the first file, Full file path of the second file

- 4 arguments: ShowIdenticalLines/HideIdenticalLines, DisplayMode, Full file path of the first file, Full file path of the second file

- 5 arguments: Library (DiffPlex/DiffLib), ShowIdenticalLines/HideIdenticalLines, DisplayMode, Full file path of the first file, Full file path of the second file

## SendTo menu
Put a shortcut to TextDiffToHtml.exe into the SendTo menu and then select two files to compare and send them to this shortcut.

The SendTo folder is located there:

C:\Users\[Your profile]\AppData\Roaming\Microsoft\Windows\SendTo

Note: AppData is a hidden folder, but you can still type and view the folder in the File Explorer.

# Dependencies

This project relies on the following NuGet packages:

- [DiffPlex](https://www.nuget.org/packages/DiffPlex/) ![NuGet](https://img.shields.io/nuget/v/DiffPlex.svg)
- [DiffLib](https://www.nuget.org/packages/DiffLib/) ![NuGet](https://img.shields.io/nuget/v/DiffLib.svg)
- [DiffMatchPatch](https://www.nuget.org/packages/DiffMatchPatch/) ![NuGet](https://img.shields.io/nuget/v/DiffMatchPatch.svg)
- [Vereyon.Windows.WebBrowser](https://www.nuget.org/packages/Vereyon.Windows.WebBrowser/) ![NuGet](https://img.shields.io/nuget/v/Vereyon.Windows.WebBrowser.svg)
- [Enums.Net](https://www.nuget.org/packages/Enums.Net/) ![NuGet](https://img.shields.io/nuget/v/Enums.Net.svg)

# Versions

See [Changelog.md](Changelog.md)

# Links

See also: [VBWinDiff](https://github.com/PatriceDargenton/VBWinDiff) (french)