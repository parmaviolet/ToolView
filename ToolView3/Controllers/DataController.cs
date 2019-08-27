using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using ToolView3.DAL;
using ToolView3.Models;

namespace ToolView3.Controllers {
    public class DataController {
        public List<Vulnerability> GetVulnList() {
            using (ToolViewContext db = new ToolViewContext()) {
                List<Vulnerability> vulnList = null;
                try {
                    vulnList = db.Vulnerabilities.ToList();
                }
                catch (Exception) {
                    Debug.WriteLine("Error: 921234");
                }
                return vulnList;
            }

        }

        public Vulnerability GetSelectedVuln(int pluginID) {
            using (ToolViewContext db = new ToolViewContext()) {
                Vulnerability vuln = new Vulnerability();

                try {
                    vuln = db.Vulnerabilities.Where(v => v.PluginID == pluginID).FirstOrDefault();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 92360" + ": " + e);
                }

                return vuln;
            }
        }

        public List<AffectedHosts> GetAffectedHosts(int pluginID) {
            using (ToolViewContext db = new ToolViewContext()) {
                List<AffectedHosts> affectedHosts = new List<AffectedHosts>();

                try {
                    var query = (from h in db.Hosts
                                 join v in db.Vulnerabilities.Where(vuln => vuln.PluginID == pluginID)
                                 on h.ID equals v.HostID
                                 into result
                                 select new {
                                     h.HostIP,
                                     h.NetBiosName,
                                     h.FQDN,
                                     Vulns = result,
                                 }).OrderBy(o => o.HostIP);

                    foreach (var host in query) {
                        if (host.Vulns.Any()) {
                            List<string> portList = new List<string>();

                            foreach (Vulnerability vuln in host.Vulns) {
                                portList.Add(vuln.Port + "/" + vuln.Protocol);
                            }

                            AffectedHosts hst = new AffectedHosts {
                                HostIP = host.HostIP,
                                FQDN = host.NetBiosName,
                                Ports = string.Join(", ", portList.Distinct())
                            };
                            affectedHosts.Add(hst);
                        }
                    }
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 92121" + ": " + e);
                }

                var DistinctItems = affectedHosts.GroupBy(x => x.HostIP).Select(y => y.First());
                List<AffectedHosts> newList = new List<AffectedHosts>();

                foreach (var item in DistinctItems) {
                    newList.Add(item);
                }

                return newList;
            }
        }

        public void SetAppendixPluginID(int pluginID) {
            using (ToolViewContext db = new ToolViewContext()) {
                AppendixData appData = new AppendixData();

                try {
                    if (db.Appendix.Any()) {
                        AppendixData recordDelete = db.Appendix.FirstOrDefault();
                        db.Appendix.Remove(recordDelete);
                        db.SaveChanges();
                    }

                    appData.pluginID = pluginID;
                    db.Appendix.Add(appData);
                    db.SaveChanges();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 921234" + e);
                }
            }
        }

        public AppendixData GetAppendixPluginID() {
            using (ToolViewContext db = new ToolViewContext()) {
                AppendixData appData = new AppendixData();

                try {
                    appData = db.Appendix.FirstOrDefault();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 921234" + e);
                }

                return appData;
            }
        }

        public List<Vulnerability> GetAppendixData(string hostIP, int pluginID) {
            using (ToolViewContext db = new ToolViewContext()) {
                Host host = new Host();
                List<Vulnerability> vuln = new List<Vulnerability>();

                try {
                    host = db.Hosts.Where(h => h.HostIP == hostIP).FirstOrDefault();
                    vuln = db.Vulnerabilities.Where(v => v.HostID == host.ID && v.PluginID == pluginID).ToList();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 9213214" + e);
                }

                return vuln;
            }
        }

        public List<AffectedPatchHosts> GetAffectedPatchHosts(int pluginID) {
            using (ToolViewContext db = new ToolViewContext()) {
                List<AffectedPatchHosts> hosts = new List<AffectedPatchHosts>();

                try {
                    var query = (from h in db.Hosts
                                 join v in db.Vulnerabilities.Where(vuln => vuln.PluginID == pluginID)
                                 on h.ID equals v.HostID
                                 into result
                                 select new {
                                     h.HostIP,
                                     NetBiosName = h.NetBiosName,
                                     FQDN = h.FQDN,
                                     Vulns = result
                                 }).OrderBy(o => o.HostIP);

                    foreach (var host in query) {
                        if (host.Vulns.Any()) {
                            AffectedPatchHosts hst = new AffectedPatchHosts {
                                HostIP = host.HostIP,
                                NetBiosName = host.NetBiosName,
                                FQDN = host.FQDN,
                                Vulns = host.Vulns.ToList()
                            };
                            hosts.Add(hst);
                        }
                    }
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 945660" + ": " + e);
                }

                return hosts;
            }
        }

        public List<VulnXMLHosts> GetVulnXMLData() {
            using (ToolViewContext db = new ToolViewContext()) {
                List<VulnXMLHosts> result = new List<VulnXMLHosts>();

                try {
                    var query = (from h in db.Hosts
                                 join v in db.Vulnerabilities
                                 on h.ID equals v.HostID
                                 into output
                                 select new {
                                     h.HostIP,
                                     h.NetBiosName,
                                     h.FQDN,
                                     Vulns = output
                                 }).OrderBy(o => o.HostIP);

                    foreach (var item in query) {
                        if (item.Vulns.Any()) {
                            VulnXMLHosts host = new VulnXMLHosts {
                                HostIP = item.HostIP,
                                NetBiosName = item.NetBiosName,
                                FQDN = item.FQDN,
                                Vulns = item.Vulns.ToList()
                            };

                            result.Add(host);
                        }
                    }
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 9445860" + ": " + e);
                }

                return result;
            }
        }

        public List<AffectedHostsMulti> GetAffectedHostsMulti(List<int> pluginIDList) {
            using (ToolViewContext db = new ToolViewContext()) {

                List<AffectedHostsMulti> affectedHosts = new List<AffectedHostsMulti>();

                try {
                    var query = (from h in db.Hosts
                        join v in db.Vulnerabilities.Where(vuln => pluginIDList.Contains(vuln.PluginID))
                        on h.ID equals v.HostID
                           into result
                           select new {
                               h.HostIP,
                               NetBiosName = h.NetBiosName,
                               FQDN = h.FQDN,
                               Vulns = result
                           }).OrderBy(o => o.HostIP);

                    foreach (var host in query) {
                        if (host.Vulns.Any()) {
                            List<string> portList = new List<string>();

                            foreach (Vulnerability vuln in host.Vulns) {
                                portList.Add(vuln.Port + "/" + vuln.Protocol);
                            }

                            AffectedHostsMulti hst = new AffectedHostsMulti {
                                HostIP = host.HostIP,
                                FQDN = host.FQDN,
                                Ports = string.Join(", ", portList.Distinct()),
                                Vulns = string.Join(System.Environment.NewLine, host.Vulns.Select(x => x.PluginName))
                            };
                            affectedHosts.Add(hst);
                        }
                    }
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 92121" + ": " + e);
                }

                return affectedHosts;
            }
        }
    }
}
