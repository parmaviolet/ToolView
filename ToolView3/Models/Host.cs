using System.Collections.Generic;

namespace ToolView3.Models {
    public class Host {
        public int ID { get; set; }
        public string HostIP { get; set; }
        public string NetBiosName { get; set; }
        public string FQDN { get; set; }
        public string OperatingSystem { get; set; }

        public virtual ICollection<Vulnerability> Vulnerabilities { get; set; }
    }
}
