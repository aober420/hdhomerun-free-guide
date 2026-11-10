<p align="center"><img src="assets/banner.png" alt="HDHomeRun Free Guide" width="100%"></p>

# HDHomeRun Free Guide

A Jellyfin 12.x plugin (12.1 or later) that downloads the free two-day HDHomeRun TV guide and keeps it updated. You'll need a physical HDHomeRun tuner. No SiliconDust account or subscription is needed.

## Installation

Add your HDHomeRun under **Dashboard → Live TV → Tuner Devices**.

Open **Dashboard → Plugins → Repositories** and add this repository URL:

```text
https://raw.githubusercontent.com/aober420/hdhomerun-free-guide/main/manifest.json
```

Install **HDHomeRun Free Guide** from the plugin catalog and restart Jellyfin.

The plugin downloads the guide on startup and adds it to Live TV. After that, it updates roughly once a day. After scanning for channels on your HDHomeRun, run **Force Download HDHomeRun Free Guide** from **Dashboard → Scheduled Tasks** to download fresh listings immediately and refresh the guide. Both force downloads and scheduled downloads clear this guide provider's cache before refreshing Jellyfin, so it imports the updated listings. The regular **Download HDHomeRun Free Guide** task follows the automatic schedule. Some channels may need to be mapped in Jellyfin's guide settings.

If Jellyfin can't find your tuner, enter its address under **Plugins → HDHomeRun Free Guide**. Make sure it's also added under Live TV. Disable any other HDHomeRun guide plugin before using this one.

Supports Jellyfin 12.1 and later compatible releases in the 12.x series.

## License

Copyright (c) 2026 aober420 (Aober). Plugin source is licensed under **GPL-3.0-only**. See [LICENSE](LICENSE) and [NOTICE](NOTICE). Third-party components retain their respective licenses.
