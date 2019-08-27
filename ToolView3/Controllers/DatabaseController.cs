using EntityFramework.BulkInsert.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using ToolView3.DAL;
using ToolView3.Models;
using Z.EntityFramework.Plus;

namespace ToolView3.Controllers {
    public class DatabaseController {
        public void CreateDatabase() {
            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Database.Create();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 95345" + e);
                }
                finally {
                    db.Dispose();
                }
            }
        }

        public void DeleteDatabase() {
            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Database.Delete();
                }
                catch (Exception) {
                    Debug.WriteLine("Error: 95447");
                }
                finally {
                    db.Dispose();
                }
            }
        }

        public int GetFirstHostIDNumber() {
            using (ToolViewContext db = new ToolViewContext()) {
                db.Configuration.AutoDetectChangesEnabled = false;
                int result = 0;

                try {
                    if (db.Hosts.Any()) {
                        result = db.Hosts.Max(x => x.ID);
                    }
                    else {
                        result = 1;
                    }
                }
                catch (Exception) {
                    Debug.WriteLine("Error: 95458");
                }
                finally {
                    db.Dispose();
                }

                return result;
            }
        }

        public void PopulateDatabaseFromNessus(List<Host> hostList, List<Vulnerability> vulnList) {
            using (ToolViewContext db = new ToolViewContext()) {

                try {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Database.CommandTimeout = 0;
                    db.BulkInsert(hostList);
                    db.BulkInsert(vulnList);
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine("Error: 965784");
                }
                finally {
                    db.Dispose();
                }
            }
        }

        public void RemoveDuplicatesFromDatabase() {
            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    db.Configuration.AutoDetectChangesEnabled = false;

                    var hostDuplicates = from h in db.Hosts
                                         group h by new { h.HostIP, h.NetBiosName, h.FQDN, h.OperatingSystem }
                                                into g
                                         where g.Count() > 1
                                         select g;

                    foreach (var g in hostDuplicates) {
                        List<Host> duplicates = g.Skip(1).ToList();

                        foreach (Host record in duplicates) {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            db.Hosts.Remove(record);
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception) {
                    Debug.WriteLine("Error: 965777");
                }
                finally {
                    db.Dispose();
                }
            }
        }

        public void RemoveSelectedVulnerability(List<int> pluginIDList) {
            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    foreach (int pluginID in pluginIDList) {
                        db.Vulnerabilities.Where(v => v.PluginID == pluginID).Delete();
                    }

                    db.SaveChanges();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 923657" + ": " + e);
                }
                finally {
                    db.Dispose();
                }
            }
        }

        public void MergeSelectedVulnerabilities(List<int> pluginIDList, string newIssueTitle, string pluginNames, string severityRating, string riskFactor) {
            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    List<Vulnerability> vulnsToMerge = new List<Vulnerability>();
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    int newPluginID = GetFreePluginID();

                    foreach (int pluginID in pluginIDList) {
                        db.Vulnerabilities.Where(v => v.PluginID == pluginID).Update(x => new Vulnerability() {
                            PluginID = newPluginID,
                            PluginName = newIssueTitle,
                            Description = pluginNames,
                            Solution = "",
                            Severity = severityRating,
                            RiskFactor = riskFactor
                        });
                    }

                    db.SaveChanges();
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 923843" + ": " + e);
                }
                finally {
                    db.Dispose();
                }
            }
        }

        private int GetFreePluginID() {
            using (ToolViewContext db = new ToolViewContext()) {
                int newPluginID = new Random().Next(100000, 999999);

                try {
                    while (db.Vulnerabilities.Any(v => v.PluginID == newPluginID)) {
                        newPluginID = new Random().Next(100000, 999999);
                    }
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 922324" + ": " + e);
                }
                finally {
                    db.Dispose();
                }

                return newPluginID;
            }
        }

        public void ClearDatabase() {
            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    db.Database.ExecuteSqlCommand("DELETE FROM Host");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT(Host, RESEED, 1)");
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 965333 : " + e);
                }
                finally {
                    db.Dispose();
                }
            }
        }

        public bool IsTablesPopulated() {
            bool result = false;

            using (ToolViewContext db = new ToolViewContext()) {
                try {
                    if (db.Hosts.Any() && db.Vulnerabilities.Any()) {
                        result = true;
                    }
                }
                catch (Exception e) {
                    Debug.WriteLine("Error: 330913 : " + e);
                }
                finally {
                    db.Dispose();
                }
            }
            return result;
        }
    }
}
