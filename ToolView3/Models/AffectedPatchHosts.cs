using System.Collections.Generic;

namespace ToolView3.Models {
    public class AffectedPatchHosts {
        public string HostIP { get; set; }
        public string NetBiosName { get; set; }
        public string FQDN { get; set; }

        public virtual ICollection<Vulnerability> Vulns { get; set; }
    }
}
