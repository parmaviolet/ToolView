using System.Collections.Generic;

namespace ToolView3.Models {
    public class AffectedHostsMulti {
        public string HostIP { get; set; }
        public string NetBiosName { get; set; }
        public string FQDN { get; set; }
        public string Ports { get; set; }
        public string Vulns { get; set; }
    }
}
