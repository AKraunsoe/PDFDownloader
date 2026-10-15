using System.Diagnostics;

class ProgressBar : IDisposable
{
	private const int BarWidth = 20;
	private readonly Stopwatch elapsed = Stopwatch.StartNew();
	private readonly object consoleLock = new();
	private Timer? timer;
	private int displayedProcessedCount;
	private int displayedDownloadedCount;
	private int displayedTotalCount;
	private int lastLineLength;
	private bool isComplete;

	public void StartTimer()
	{
		timer = new Timer(_ => Render(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
	}

	public void Dispose()
	{
		timer?.Dispose();
		elapsed.Stop();
	}

	public void Update(int processedCount, int downloadedCount, int totalCount)
	{
		lock (consoleLock)
		{
			if (totalCount == 0)
			{
				Console.WriteLine($"[{new string('-', BarWidth)}] 100% | Nothing to download");
				return;
			}

			displayedProcessedCount = processedCount;
			displayedDownloadedCount = downloadedCount;
			displayedTotalCount = totalCount;
			isComplete = processedCount >= totalCount;
			RenderLocked();
		}
	}

	private void Render()
	{
		lock (consoleLock)
		{
			if (displayedTotalCount > 0 && !isComplete)
			{
				RenderLocked();
			}
		}
	}

	private void RenderLocked()
	{
		int percent = displayedProcessedCount * 100 / displayedTotalCount;
		int filledWidth = displayedProcessedCount * BarWidth / displayedTotalCount;
		string bar = new string('#', filledWidth) + new string('-', BarWidth - filledWidth);
		TimeSpan duration = elapsed.Elapsed;
		string elapsedText = duration.TotalSeconds < 60
			? $"{(int)duration.TotalSeconds}s"
			: $"{(int)duration.TotalMinutes}m {duration.Seconds:00}s";
		string line = $"[{bar}] {percent,3}% | {displayedDownloadedCount}/{displayedTotalCount} Downloaded {displayedProcessedCount}/{displayedTotalCount} Processed | {elapsedText}";
		Console.Write("\r" + line.PadRight(lastLineLength));
		lastLineLength = Math.Max(lastLineLength, line.Length);

		if (isComplete)
		{
			Console.WriteLine();
			timer?.Change(Timeout.Infinite, Timeout.Infinite);
		}
	}
}
