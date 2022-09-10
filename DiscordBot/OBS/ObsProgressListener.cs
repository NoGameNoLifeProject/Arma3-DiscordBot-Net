extern alias obsjava;
using System;
using ProgressListener = obsjava::com.obs.services.model.ProgressListener;
using ProgressStatus = obsjava::com.obs.services.model.ProgressStatus;

namespace DiscordBot.OBS;

public class ObsProgressListener: ProgressListener
{
    public event Action<ProgressStatus> Status;
    public void progressChanged(ProgressStatus ps)
    {
        Status.Invoke(ps);
    }
}