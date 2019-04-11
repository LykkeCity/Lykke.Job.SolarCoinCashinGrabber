using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.SolarCoinCashinGrabber.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        
        public string MongoConnString { get; set; }
        
        [AzureTableCheck]
        public string AzureQueueConnString { get; set; }
    }
}
