using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace Syndiesis.Controls;

public static class AsyncClickHelpers
{
    public static void AttachAsyncClick(this Button button, Action action)
    {
        button.Click += HandleClickAsync;

        void HandleClickAsync(object? sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;

            Dispatcher.UIThread.InvokeAsync(HandleActionExecution);
        }

        async Task HandleActionExecution()
        {
            await Task.Run(action);
            await Dispatcher.UIThread.InvokeAsync(ReenableButton);
        }

        void ReenableButton()
        {
            button.IsEnabled = true;
        }
    }
}
