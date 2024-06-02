using Avalonia.Controls;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class LoadingTreeListNode : UserControl
{
    public LoadingTreeListNode()
    {
        InitializeComponent();
    }

    public void SetProgress(ProgressInfo progress)
    {
        if (!progress.IsValid)
        {
            progressRun.Text = string.Empty;
            return;
        }

        progressRun.Text = $"{progress.RealValue}/{progress.Maximum} ({(int)(progress.Rate * 100)}%)";
    }
}
