using Lykke.Job.SolarCoinCashinGrabber.Settings.JobSettings;
using Lykke.Sdk.Settings;

namespace Lykke.Job.SolarCoinCashinGrabber.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public SolarCoinCashinGrabberJobSettings SolarCoinCashinGrabberJob { get; set; }
    }
}
