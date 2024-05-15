# Changelog

## v1.0.3

### Features

- Code editor
  - Improved double-click word selection
- Syntax tree view
  - Detailed property display of display values
  - Moving cursor by right-clicking on node
  - Display `GetStructure()` on structured syntax trivia
- General
  - Include usage reference
  - Logging
  - Support running on macOS (thanks @elvodqa)
  - Better scroll handling

### Bugfixes

- Code editor
  - Extra retained line after deleting multiline content
  - Clicking outside code editor content (i.e. scroll bars) handling text selection
  - Handle mouse dragging to select text outside of bounds of code editor
  - Improve cursor icon persistence
- Syntax tree view
  - Disabled text line display correctly displays affected lines

### Performance

- Removed unused `Avalonia.ReactiveUI` library
- Improved hovering on node lines in deep nodes
- Faster syntax object property value retrieval

## v1.0.2

### Features

- Code editor
  - Hovering mouse sets the `Ibeam` cursor instead of the default
  - No background fill on selected line if text is selected
  - Dragging mouse selects text
  - Double-click engages word selection mode
  - Allow customizing indentation width in spaces
- Syntax tree view
  - Missing tokens with trivia are now displayed
  - Larger spacing between token value display and kind
- Settings
  - Settings are now stored in `appsettings.json`

### Bugfixes

- Code editor
  - Clear selection upon setting source
  - Capture preferred cursor character position
    - On text deletion
- Syntax tree view
  - Support `PreprocessingMessageTrivia`

## v1.0.1

### Features

- Code editor
  - Adjust indentation with Tab and Shift+Tab
  - Smart indentation on new line
  - Cut text
  - Copy/cut/paste text of single line without selecting
  - Select word with `Ctrl+W`
  - Select outer node contents with `Ctrl+U`
- Syntax tree view
  - Reduce left padding of child nodes
  - Reduce size of expansion +/- indicator
- Main view
  - Drill transition on entering or leaving settings
  - Enter settings with `Ctrl+S`
  - Reset code with `Ctrl+R`

### Bugfixes

- Code editor
  - Active cursor line number highlight
  - Capture preferred cursor character position
    - On text input
    - On pointer click
  - Do not delete extra character with `Backspace` or `Delete` if text is selected

### Performance

- Some async operations are now performed outside the UI thread to avoid hiccups.

## v1.0.0

Initial release.
