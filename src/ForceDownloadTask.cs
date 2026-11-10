using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Tasks;

namespace FreeGuide;

public sealed class ForceDownloadTask(IConfigurationManager configuration, IApplicationPaths paths, ITaskManager tasks, IListingsManager listings) : IScheduledTask {
 public string Name => "Force Download HDHomeRun Free Guide";
 public string Key => "ForceDownloadHDHomeRunFreeGuide";
 public string Description => "Downloads fresh guide data immediately, clears this guide provider's cache, and refreshes Jellyfin's guide.";
 public string Category => "Live TV";
 public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => [];
 public Task ExecuteAsync(IProgress<double> progress,CancellationToken cancellationToken) =>
  new DownloadTask(configuration,paths,tasks,listings).ExecuteDownloadAsync(progress,cancellationToken,true);
}
