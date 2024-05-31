# Syndiesis

The most revolutionary syntax visualizer for C#

## Motivation

The project was inspired by the task for applying into a Roslyn internship position regarding C# syntax highlighting at JetBrains:
([project](https://internship.jetbrains.com/projects/1442/) / [task](https://internship.jetbrains.com/applications/19433/))

The main design inspiration is [SharpLab](https://sharplab.io/). The syntax view feature of SharpLab is the main design that the app built upon.

Despite not applying for the internship, I wanted to finish the project and release it into a usable state without entering the depths of feature creep.

## Usage

Download from the [Releases](https://github.com/Rekkonnect/Syndiesis/releases) page.

The program is designed to be cross-platform for desktop (including Windows, Linux and macOS). It's heavily tested to run on Windows 10, and it's moderately tested on Windows 11 and macOS. Please file an issue if platform-specific problems occur.

Check the change log [here](/docs/changelog/README.md).

View the [wiki](https://github.com/Rekkonnect/Syndiesis/wiki) for detailed documentation.

### Demo

> _The video was shot in version 1.1.0_

https://github.com/Rekkonnect/Syndiesis/assets/8298332/268a2205-d2a4-446b-a145-5748f30f85c0

## Stack

- Visual Studio 2022
- .NET 8.0
- C# 12.0
- [Avalonia 11.0](https://github.com/AvaloniaUI/Avalonia)

### Dependencies

- [Roslyn 4.9.2](https://github.com/dotnet/roslyn)
- [AvaloniaEdit](https://github.com/avaloniaUI/AvaloniaEdit), for the code editor

## Features

Below is a short list of features:

- Code editor
  - AvaloniaEdit's features
  - Custom nagivation to outer syntax nodes based on the current selection
- Syntax and semantic analysis visualizer
  - Current caret syntax node highlighting
  - Tree view of nodes
  - Display of property names of syntax objects
  - Colorful display of different analysis list view nodes
  - Navigation to code snippet span of selected node
  - Syntax, symbol and operation analysis
  - Indication of throwing properties per node
  - Loading nodes respects responsiveness of the app

A large portion of the app is built using basic components in Avalonia. The code editor is from [AvaloniaEdit](https://github.com/avaloniaUI/AvaloniaEdit).
Some icons were taken from the free version of [FontAwesome](https://fontawesome.com/).

### Desired features

Desired features are listed in the [issues](https://github.com/Rekkonnect/Syndiesis/issues).

### Ruled-out features

- Auto-complete on text

## Design philosophy

The syntax visualizer should provide detailed information about how Roslyn parses the given C# code snippet, and in a pretty and user-friendly format. SharpLab lacks in readability of the tree, and it also doesn't paint the entire picture as intended.

The properties of the nodes are automatically extracted via reflection. Some are filtered out due to duplication in most cases, and in other cases they were not providing any helpful information.

Each different node type is differently treated to extract the most useful information out of it. If you encounter a node missing critical information, or displaying it weirdly, please file an [issue](https://github.com/Rekkonnect/Syndiesis/issues/new).

With 1.1.0 onwards, the program's direction was shifted more towards explaining and visualizing the expected results by using Roslyn itself. This expands the initial scope of the program into being a handy utility making the usage of Roslyn more predictable, especially when testing analyzers or source generators.
