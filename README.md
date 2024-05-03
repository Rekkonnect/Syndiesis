# Syndiesis

The most revolutionary syntax visualizer for C#

## Motivation

The project was inspired by the task for applying into a Roslyn position regarding C# syntax highlighting at JetBrains:
([project](https://internship.jetbrains.com/projects/1442/) / [task](https://internship.jetbrains.com/applications/19433/))

The main design inspiration is [SharpLab](https://sharplab.io/). The syntax view feature of SharpLab is the main design that the app built upon.

Despite not applying, I wanted to finish the project and release it into a usable state without entering the depths of feature creep. The initial intend was to build a complete project for once in my life.

## Usage preview

https://github.com/Rekkonnect/Syndiesis/assets/8298332/4d754285-c9d9-4637-a885-88c78ce3484b

## Stack

- Visual Studio 2022
- .NET 8.0
- C# 12.0
- Avalonia 11.0

## Features

- Code editor
  - Text editing
  - Text selection
  - Scrolling
  - Copy/paste text
  - Navigation with keybinds
- Syntax visualizer
  - Current cursor syntax node highlighting
  - Tree view of nodes
  - Display of property names of syntax objects
  - Colorful display of different syntax list view nodes

The entirety of the app is built using basic components in Avalonia. Everything is built from scratch without external dependencies for UI.

### Desired features

Features that someday could be implemented without drifting off the scope of the project.

- Syntax highlighting on code view
- Animations on navigation through the syntax tree via code editor cursor
- Indentation reduction with Shift+Tab
- Indentation adjustments on selected lines when multiple lines are selected
- Multiple cursors for editing simultaneously

### Ruled-out features

- Auto-complete on text

## Usage documentation

### Node type legend

- `N` - SyntaxNode
- `T` - SyntaxToken
- `D` - Display value (`.ValueText` of SyntaxToken)
- `SL` - [Separated]SyntaxList
- `TL` - SyntaxTokenList
- `_` - Whitespace trivia
- `\n` - End of line trivia
- `/*` - Comment trivia
- `#` - Preprocessor directive trivia
- `~` - Disabled text (text that is not active due to conditional preprocessor directives)

### Keybinds

- Navigation
  - `Up` / `Down` / `Left` / `Right` - Move cursor by one character or line
  - `Ctrl+Left` / `Ctrl+Right` - Move to next word left or right
- Manipulation
  - `Back` - Delete one character left
  - `Delete` - Delete one character right
  - `Ctrl+Back` - Delete one word left
  - `Ctrl+Delete` - Delete one word right
  - `Ctrl+C` - Copy current selection
  - `Ctrl+V` - Paste current content on clipboard
  - `Ctrl+Shift+V` - Paste current content on clipboard and replace entire snippet with pasted content
  - `Ctrl+A` - Select all
  - `Tab` - Insert up to 4 spaces to fill a 4-character section within the line

## Design philosophy

The syntax visualizer should provide detailed information about how Roslyn parses the given C# code snippet, and in a pretty and user-friendly format. SharpLab lacks in readability of the tree, and it also doesn't paint the entire picture as intended.

The properties of the nodes are automatically extracted via reflection. Some are filtered out due to duplication in most cases, and in other cases they were not providing any helpful information.

Each different node type is differently treated to extract the most useful information out of it. If you encounter a node missing critical information, or displaying it weirdly, please file an [issue](https://github.com/Rekkonnect/Syndiesis/issues/new).
