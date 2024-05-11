# Changelog

## 1.0.1

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

## 1.0.0

Initial release.
