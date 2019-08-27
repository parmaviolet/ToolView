using System.Collections.Generic;
using System.Xml.Linq;

namespace ToolView3 {
    public class ParseReportHostItem {
        public int Port { get; set; }
        public string SvcName { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string RiskFactor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Synopsis { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public string PluginType { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public int PluginId { get; set; }
        public string PluginOutput { get; set; } = string.Empty;
    }

    public class ParseReportHost {
        public string Name { get; set; } = string.Empty;
        public List<ParseReportHostItem> Items { get; set; }
        public ParseHostProperties Properties { get; set; }

        public long NumOpenPorts { get; set; }
        public long NumVulnCritical { get; set; }
        public long NumVulnHigh { get; set; }
        public long NumVulnMedium { get; set; }
        public long NumVulnLow { get; set; }
        public long NumVulnNone { get; set; }
    }

    public class ParseReport {
        public List<ParseReportHost> Hosts { get; set; }
    }

    public class ParseHostProperties {
        public string HostStart { get; set; } = string.Empty;
        public string HostEnd { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string HostIp { get; set; } = string.Empty;
        public string HostFqdn { get; set; } = string.Empty;
        public string NetBiosName { get; set; } = string.Empty;
    }

    public class RiskFactor {
        public const string Critical = "Critical";
        public const string High = "High";
        public const string Medium = "Medium";
        public const string Low = "Low";
        public const string None = "Info";
    }

    public class NessusParser {
        private readonly string path;

        public NessusParser(string path) {
            this.path = path;
        }

        public ParseReport Run() {
            ParseReport parseReport = new ParseReport {
                Hosts = new List<ParseReportHost>()
            };

            XElement element = XElement.Load(path);
            XElement report = element.Element("Report");
            foreach (XElement host in report.Elements("ReportHost")) {
                ParseReportHost parseHost = new ParseReportHost {
                    Items = new List<ParseReportHostItem>(),

                    Name = (string)host.Attribute("name")
                };
                if (string.IsNullOrEmpty(parseHost.Name)) {
                    continue;
                }

                foreach (XElement item in host.Elements("ReportItem")) {
                    ParseReportHostItem parseReportHostItem = new ParseReportHostItem {
                        Port = (int)item.Attribute("port"),
                        SvcName = (string)item.Attribute("svc_name"),
                        Protocol = (string)item.Attribute("protocol"),
                        Severity = (string)item.Attribute("severity"),
                        PluginId = (int)item.Attribute("pluginID"),
                        PluginName = (string)item.Attribute("pluginName"),
                        Synopsis = (string)item.Element("synopsis"),
                        Description = (string)item.Element("description"),
                        RiskFactor = (string)item.Element("risk_factor")
                    };
                    switch (parseReportHostItem.RiskFactor) {
                        case RiskFactor.Critical:
                            parseHost.NumVulnCritical++;
                            break;
                        case RiskFactor.High:
                            parseHost.NumVulnHigh++;
                            break;
                        case RiskFactor.Medium:
                            parseHost.NumVulnMedium++;
                            break;
                        case RiskFactor.Low:
                            parseHost.NumVulnLow++;
                            break;
                        default:
                            parseHost.NumVulnNone++;
                            break;
                    }

                    parseReportHostItem.Solution = (string)item.Element("solution");
                    parseReportHostItem.PluginType = (string)item.Element("plugin_type");
                    parseReportHostItem.PluginOutput = (string)item.Element("plugin_output");
                    parseHost.Items.Add(parseReportHostItem);
                }

                ParseHostProperties parseHostProps = new ParseHostProperties();

                foreach (XElement hostProp in host.Elements("HostProperties")) {
                    foreach (XElement prop in hostProp.Elements()) {
                        string propName = (string)prop.Attribute("name");
                        if (propName == "HOST_START") {
                            parseHostProps.HostStart = prop.Value;
                        }
                        else if (propName == "HOST_END") {
                            parseHostProps.HostEnd = prop.Value;
                        }
                        else if (propName == "operating-system") {
                            parseHostProps.OperatingSystem = prop.Value;
                        }
                        else if (propName == "host-ip") {
                            parseHostProps.HostIp = prop.Value;
                        }
                        else if (propName == "host-fqdn") {
                            parseHostProps.HostFqdn = prop.Value;
                        }
                        else if (propName == "netbios-name") {
                            parseHostProps.NetBiosName = prop.Value;
                        }
                    }
                    parseHost.Properties = parseHostProps;
                }
                parseHost.NumOpenPorts = 0;

                if (!string.IsNullOrEmpty(parseHost.Properties.HostIp)) {
                    parseReport.Hosts.Add(parseHost);
                }
            }
            return parseReport;
        }
    }
}