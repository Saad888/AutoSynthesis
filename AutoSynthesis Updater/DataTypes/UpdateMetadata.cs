using System.Collections.Generic;

namespace AutoSynthesis_Updater
{
    class UpdateMetadata
    {
        public string CurrentVersion { get; set; }
        public string UpdateVersion { get; set; }
        public Dictionary<string, string> UpdateFiles { get; set; }
    }

}
