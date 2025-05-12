# Syndiesis

The most revolutionary syntax visualizer and more for C# and Visual Basic

## Legal Dislcaimers

This project is not endorsed by any third-party and does not intend to violate any rights of their respective holders.
The software distributes other third-party software as follows:
- Icons from Microsoft's **Visual Studio 2022 Image Library**
- Software libraries mentioned below, with compatible distribution policies

## Motivation

The project was inspired by the task for applying into a Roslyn internship position regarding C# syntax highlighting at JetBrains
(possibly dead links: [project](https://internship.jetbrains.com/projects/1442/) / [task](https://internship.jetbrains.com/applications/19433/))

The main design inspiration is [SharpLab](https://sharplab.io/). The syntax view feature of SharpLab is the main design that the app built upon.

Despite not applying for the internship, I wanted to finish the project and release it into a usable state without entering the depths of feature creep.

## Usage

For Windows, download from the [Releases](https://github.com/Rekkonnect/Syndiesis/releases) page. For macOS and Linux, you have to manually compile the program (refer to the section below).

The program is designed to be cross-platform for desktop (including Windows, Linux and macOS). It's heavily tested to run on Windows 10, and it's moderately tested on Windows 11 and macOS. Please file an issue if platform-specific problems occur.

Check the change log [here](/docs/changelog/README.md).

View the [wiki](https://github.com/Rekkonnect/Syndiesis/wiki) for detailed documentation.

### Compiling

To compile this program you will need an IDE like Visual Studio 2022, or JetBrains Rider 2024.3+.
Load the solution file (`Syndiesis.sln`) from the IDE of your choice and build the project (recommended to switch to *Release mode*).

### Demo

> _The video was shot in version 1.2.0_

https://github.com/user-attachments/assets/19821a70-e020-4929-9662-584d1afb6416

## Stack

- Visual Studio 2022
- .NET 9.0
- C# 13.0
- [Avalonia 11.0](https://github.com/AvaloniaUI/Avalonia)

### Dependencies

- [Roslyn 4.12.0](https://github.com/dotnet/roslyn)
- [AvaloniaEdit](https://github.com/avaloniaUI/AvaloniaEdit), for the code editor
- [jamarino/IntervalTree](https://github.com/jamarino/IntervalTree), for the diagnostics

## Features

Below is a short list of features:

- Code editor
  - AvaloniaEdit's features
  - Syntax and semantic colorization
  - Go to definition using F12
  - Custom nagivation to outer syntax nodes based on the current selection
  - Diagnostics display
  - Automatic recognition of the snippet's language (C# or VB)
  - Selection of any available language version
- Syntax and semantic analysis visualizer
  - Current caret syntax node highlighting
  - Tree view of nodes
  - Display of property names of syntax objects
  - Colorful display of different analysis list view nodes
  - Navigation to code snippet span of selected node
  - Syntax, symbol, operation and attribute analysis
  - Node details view
  - Indication of throwing properties per node
  - Loading nodes respects responsiveness of the app

A large portion of the app is built using basic components in Avalonia. The code editor is from [AvaloniaEdit](https://github.com/avaloniaUI/AvaloniaEdit).
Some icons were taken from the free version of [FontAwesome](https://fontawesome.com/).

### Bugs and issues

Any issues regarding the code editor are most likely to be reported in [AvaloniaEdit](https://github.com/avaloniaUI/AvaloniaEdit). This includes behavior not specific to Syndiesis. Issues will be closed if they are specific to AvaloniaEdit, and must be reported there.

Syndiesis exposes data retrieved from Roslyn itself with minimal intervention for readability purposes. If you encounter misrepresented data, it is probably a Roslyn bug, but feel free to report it regardless. Examples of known Roslyn bugs include:
- Fixed buffer size expression has no symbol info ([dotnet/roslyn#75113](https://github.com/dotnet/roslyn/issues/75113))

### Desired features

Desired features among other issues are listed in the [issues](https://github.com/Rekkonnect/Syndiesis/issues).

## Design philosophy

The syntax visualizer should provide detailed information about how Roslyn parses the given C# code snippet, and in a pretty and user-friendly format. SharpLab lacks in readability of the tree, and it also doesn't paint the entire picture as intended.

The properties of the nodes are automatically extracted via reflection. Some are filtered out due to duplication in most cases, and in other cases they were not providing any helpful information.

Each different node type is differently treated to extract the most useful information out of it. If you encounter a node missing critical information, or displaying it weirdly, please file an [issue](https://github.com/Rekkonnect/Syndiesis/issues/new).

With 1.1.0 onwards, the program's direction was shifted more towards explaining and visualizing the expected results by using Roslyn itself. This expands the initial scope of the program into being a handy utility making the usage of Roslyn more predictable, especially when testing analyzers or source generators.

On 1.2.0 the node details view was added, focusing more on giving direct feedback for the specified node. This also helps with troubleshooting issues and discovering potential bugs in Roslyn itself.
