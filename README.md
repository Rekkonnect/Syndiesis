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

### Preview

> _The video was shot in version 1.0.0_

https://github.com/Rekkonnect/Syndiesis/assets/8298332/4d754285-c9d9-4637-a885-88c78ce3484b

## Stack

- Visual Studio 2022
- .NET 8.0
- C# 12.0
- [Avalonia 11.0](https://github.com/AvaloniaUI/Avalonia)
- [Roslyn 4.9.2](https://github.com/dotnet/roslyn)

## Features

Below is a short list of features:

- Code editor
  - Text editing
  - Text selection
  - Scrolling
  - Copy/paste text
  - Navigation with keybinds
  - Smart indentation
- Syntax and semantic analysis visualizer
  - Current cursor syntax node highlighting
  - Tree view of nodes
  - Display of property names of syntax objects
  - Colorful display of different analysis list view nodes
  - Navigation to code snippet span of selected node
  - Syntax, symbol and operation analysis
  - Indication of throwing properties per node

The entirety of the app is built using basic components in Avalonia. Everything is built from scratch without external library dependencies for UI.
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
