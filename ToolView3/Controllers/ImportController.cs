using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using ToolView3.Models;

namespace ToolView3.Controllers {
    public class ImportController {
        public List<string> getImportFilenames() {
            OpenFileDialog openFileDialog1 = new OpenFileDialog {
                Multiselect = true,
                Filter = "Nessus Files|*.nessus"
            };

            DialogResult result = openFileDialog1.ShowDialog();
            List<string> filenames = new List<string>();

            if (result == DialogResult.OK) {
                foreach (string file in openFileDialog1.FileNames) {
                    filenames.Add(file);
                }
            }

            return filenames;
        }

        public void ImportFromNessusFile(List<string> filenames) {
            DatabaseController db = new DatabaseController();

            try {
                int lastHostID = db.GetFirstHostIDNumber();
                List<Host> hostsList = new List<Host>();
                List<Vulnerability> vulnList = new List<Vulnerability>();

                foreach (string file in filenames) {
                    NessusParser parser = new NessusParser(file);
                    ParseReport report = parser.Run();

                    foreach (ParseReportHost host in report.Hosts) {
                        string hostname = host.Properties.NetBiosName;
                        string fullqualname = host.Properties.HostFqdn;

                        if (string.IsNullOrEmpty(hostname)) {
                            hostname = "unknown";
                        }

                        if (string.IsNullOrEmpty(fullqualname)) {
                            fullqualname = "unknown";
                        }

                        hostsList.Add(new Host {
                            ID = lastHostID,
                            HostIP = host.Properties.HostIp,
                            FQDN = fullqualname,
                            NetBiosName = hostname,
                            OperatingSystem = host.Properties.OperatingSystem
                        });

                        List<ParseReportHostItem> vulnItems = host.Items;

                        foreach (ParseReportHostItem vuln in vulnItems) {
                            vulnList.Add(new Vulnerability {
                                PluginID = vuln.PluginId,
                                PluginName = vuln.PluginName,
                                PluginType = vuln.PluginType,
                                RiskFactor = vuln.RiskFactor,
                                Severity = vuln.Severity,
                                Description = vuln.Description,
                                Solution = vuln.Solution,
                                Port = vuln.Port.ToString(),
                                Protocol = vuln.Protocol,
                                Synopsis = vuln.Synopsis,
                                PluginOutput = vuln.PluginOutput,
                                HostID = lastHostID
                            });
                        }
                        ++lastHostID;
                    }
                }
                db.PopulateDatabaseFromNessus(hostsList, vulnList);
                db.RemoveDuplicatesFromDatabase();
            }
            catch (Exception) {
                Debug.WriteLine("Error: 96589");
            }
        }
    }
}
