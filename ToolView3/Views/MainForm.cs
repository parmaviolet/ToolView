using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using ToolView3.Controllers;
using ToolView3.Models;
using ToolView3.Views;

namespace ToolView3 {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1) {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            else {
                DatabaseController controller = new DatabaseController();
                //controller.CreateDatabase();
                ClearScreen();

                ToolStripMenuItem deleteLabel = new ToolStripMenuItem {
                    Text = "Delete"
                }; deleteLabel.Click += new EventHandler(RemoveSelectedVulnerabilities);
                ToolStripMenuItem mergeLabel = new ToolStripMenuItem {
                    Text = "Merge"
                }; mergeLabel.Click += new EventHandler(MergeSelectedVulnerabilities);
                contextMenuStrip1.Items.AddRange(new ToolStripMenuItem[] { deleteLabel, mergeLabel });

                label5.Text = "Affected Hosts: ";
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
            DatabaseController controller = new DatabaseController();
            controller.DeleteDatabase();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e) {
            DatabaseController controller = new DatabaseController();
            controller.DeleteDatabase();
            controller.CreateDatabase();
            ClearScreen();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) {
            DatabaseController controller = new DatabaseController();
            controller.DeleteDatabase();
            Application.Exit();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show("Something to go here...");
        }

        private void ImportFromNessusToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadingForm loadingForm = new LoadingForm();
            ClearScreenAfter();
            loadingForm.ShowDialog();

            DataController dataCont = new DataController();
            DatabaseController databaseCont = new DatabaseController();

            if (databaseCont.IsTablesPopulated()) {
                PopulateTreeView(dataCont.GetVulnList());
            }
        }

        private void ExportToVulnXMLToolStripMenuItem_Click(object sender, EventArgs e) {
            DatabaseController db = new DatabaseController();

            if (db.IsTablesPopulated()) {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog {
                    Filter = "XML File|*.xml",
                    Title = "Save an XML File",
                    DefaultExt = "xml"
                };
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "") {
                    System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                    fs.Close();
                    CreateVulnXML(saveFileDialog1.FileName);
                    MessageBox.Show("Save Complete.");
                }
            }
            else {
                MessageBox.Show("No records in the database. Try importing some results first.");
            }
        }

        private void PluginOutputTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!treeView1.Nodes.Count.Equals(0)){
                PluginOutputForm pluginForm = new PluginOutputForm();
                pluginForm.ShowDialog();                                 
            }
            else
            {
                MessageBox.Show("Please import Nessus reults first.");
            }
        }

        private void CreateVulnXML(string filename) {
            DataController dataCont = new DataController();
            List<Vulnerability> vulns = new List<Vulnerability>();
            List<VulnXMLHosts> hosts = new List<VulnXMLHosts>();

            vulns = dataCont.GetVulnList();

            if (vulns.Any()) {

                vulns = vulns.GroupBy(v => new { v.PluginID, v.PluginName })
                                        .Select(v => v.First())
                                        .OrderByDescending(v => v.Severity)
                                        .ThenBy(v => v.PluginName).ToList();

                XmlDocument doc = new XmlDocument();

                XmlNode results = doc.CreateNode(XmlNodeType.Element, "Results", null);
                XmlNode date = doc.CreateAttribute("Date");
                date.Value = DateTime.Now.ToString();
                XmlNode tool = doc.CreateAttribute("Tool");
                results.Attributes.SetNamedItem(date);
                results.Attributes.SetNamedItem(tool);
                doc.AppendChild(results);

                foreach (Vulnerability vuln in vulns) {
                    XmlNode vulnsMain = doc.CreateNode(XmlNodeType.Element, "Vulns", null);
                    XmlNode vulnitem = doc.CreateNode(XmlNodeType.Element, "Vuln", null);
                    XmlNode group = doc.CreateAttribute("Group");
                    XmlNode id = doc.CreateAttribute("id");
                    id.Value = Convert.ToString(vuln.PluginID);
                    vulnitem.Attributes.SetNamedItem(group);
                    vulnitem.Attributes.SetNamedItem(id);

                    XmlNode title = doc.CreateNode(XmlNodeType.Element, "Title", null);
                    title.InnerText = vuln.PluginName;
                    XmlNode description = doc.CreateNode(XmlNodeType.Element, "Description", null);
                    description.InnerText = "\\\\" + Environment.NewLine +
                                            "\\\\" + Environment.NewLine +
                                            "\\\\";
                    XmlNode recommendation = doc.CreateNode(XmlNodeType.Element, "Recommendation", null);
                    recommendation.InnerText = "\\\\" + Environment.NewLine +
                                               "\\\\" + Environment.NewLine +
                                               "\\\\";
                    XmlNode references = doc.CreateNode(XmlNodeType.Element, "References", null);
                    XmlNode refer = doc.CreateNode(XmlNodeType.Element, "Ref", null);
                    refer.InnerText = "\\\\" + Environment.NewLine +
                                      "\\\\";
                    XmlNode severity = doc.CreateNode(XmlNodeType.Element, "Severity", null);
                    severity.InnerText = vuln.Severity;

                    results.AppendChild(vulnsMain);
                    vulnsMain.AppendChild(vulnitem);
                    vulnitem.AppendChild(title);
                    vulnitem.AppendChild(description);
                    vulnitem.AppendChild(recommendation);
                    vulnitem.AppendChild(references);
                    references.AppendChild(refer);
                    vulnitem.AppendChild(severity);
                }

                hosts = dataCont.GetVulnXMLData();

                XmlNode hostsMain = doc.CreateNode(XmlNodeType.Element, "Hosts", null);
                results.AppendChild(hostsMain);

                foreach (VulnXMLHosts h in hosts) {
                    XmlNode host = doc.CreateNode(XmlNodeType.Element, "Host", null);
                    XmlNode dnsname = doc.CreateAttribute("dnsname");
                    XmlNode hostname = doc.CreateAttribute("hostname");
                    XmlNode ipv6 = doc.CreateAttribute("ipv6");
                    XmlNode ipv4 = doc.CreateAttribute("ipv4");

                    dnsname.Value = h.FQDN;
                    hostname.Value = h.NetBiosName;
                    ipv4.Value = h.HostIP;

                    host.Attributes.SetNamedItem(dnsname);
                    host.Attributes.SetNamedItem(hostname);
                    host.Attributes.SetNamedItem(ipv6);
                    host.Attributes.SetNamedItem(ipv4);

                    hostsMain.AppendChild(host);

                    XmlNode vulnsHost = doc.CreateNode(XmlNodeType.Element, "Vulns", null);
                    host.AppendChild(vulnsHost);

                    //
                    //
                    //  Consider using the Get Affected hosts option to return each hosts ports to then insert into XML >>>>
                    //
                    //

                    foreach (Vulnerability item in h.Vulns) {
                        item.Port = item.Port + "/" + item.Protocol;
                    }

                    var result = from item in h.Vulns
                                 group item by item.PluginID into grp
                                 select new {
                                     PluginID = grp.Key,
                                     Ports = string.Join(", ", grp.Select(p => p.Port).Distinct()),
                                     HostIP = grp.Select(x => x.HostID).First()
                                 };

                    foreach (var v in result) {
                        XmlNode vuln1 = doc.CreateNode(XmlNodeType.Element, "Vuln", null);
                        XmlNode testphase = doc.CreateAttribute("TestPhase");
                        XmlNode id1 = doc.CreateAttribute("id");

                        id1.InnerText = v.PluginID.ToString();
                        vuln1.Attributes.SetNamedItem(testphase);
                        vuln1.Attributes.SetNamedItem(id1);

                        XmlNode data = doc.CreateNode(XmlNodeType.Element, "Data", null);
                        XmlNode type = doc.CreateAttribute("Type");
                        XmlNode encoding = doc.CreateAttribute("encoding");

                        type.InnerText = "afh:Port(s)";
                        data.Attributes.SetNamedItem(type);
                        data.Attributes.SetNamedItem(encoding);
                        data.InnerText = v.Ports;
                        vuln1.AppendChild(data);
                        vulnsHost.AppendChild(vuln1);
                    }
                }

                doc.Save(filename);
            }
            else {
                MessageBox.Show("No vulnerabilities in database to export.");
            }
        }

        private void ExportToMissingPatchesToolStripMenuItem_Click(object sender, EventArgs e) {
            DataController dataCont = new DataController();
            string output = "";
            int pluginID = 38153;

            output = CreateMissingPatchOutput(dataCont.GetAffectedPatchHosts(pluginID));

            if (output.Any()) {
                SaveMissingPatchListFile(output);
            }
            else {
                MessageBox.Show("This function relies on the presence of Nessus Plugin ID 38153. Either the Vuln list is empty or the plugin was not present in the scan results.");
            }
        }

        private string CreateMissingPatchOutput(List<AffectedPatchHosts> affectedHosts) {
            string output = "";

            foreach (AffectedPatchHosts host in affectedHosts) {
                output = output + host.HostIP + " : " + host.FQDN
                            + System.Environment.NewLine
                            + host.Vulns.Select(x => x.PluginOutput).FirstOrDefault()
                            + System.Environment.NewLine;
            }

            return output;
        }

        private void SaveMissingPatchListFile(string outputPatchList) {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog {
                Filter = "Text File|*.txt",
                Title = "Save an Text File",
                DefaultExt = "txt"
            };
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "") {

                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                string filename = saveFileDialog1.FileName;
                fs.Close();
                System.IO.File.WriteAllText(filename, outputPatchList);
                MessageBox.Show("Save Complete");
            }
        }

        private void PopulateTreeView(List<Vulnerability> vulnList) {
            treeView1.Nodes.Clear();
            List<TreeNode> nodeList1 = new List<TreeNode>();
            TreeNode node1 = null;

            if (vulnList != null) {

                vulnList = vulnList.GroupBy(v => new { v.PluginID, v.PluginName })
                                        .Select(v => v.First())
                                        .OrderByDescending(v => v.Severity)
                                        .ThenBy(v => v.PluginName).ToList();

                foreach (Vulnerability vuln in vulnList) {
                    if (vuln != null) {
                        node1 = new TreeNode(vuln.PluginName + " - " + vuln.RiskFactor) {
                            Name = vuln.PluginID.ToString(),
                            ImageIndex = int.Parse(vuln.Severity),
                            SelectedImageIndex = int.Parse(vuln.Severity)
                        };
                        nodeList1.Add(node1);
                    }
                }

                TreeNode[] nodes1 = nodeList1.ToArray();
                treeView1.BeginUpdate();
                treeView1.Nodes.AddRange(nodes1);
                treeView1.EndUpdate();

                PopulateSearchList();
            }
        }

        private void ClearScreen() {
            treeView1.Nodes.Clear();
            dataGridView1.DataSource = null;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            label5.Text = "Affected Hosts: ";
        }

        private void ClearScreenAfter() {
            dataGridView1.DataSource = null;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            label5.Text = "Affected Hosts: ";
        }

        private void MergeSelectedVulnerabilities(object sender, EventArgs e) {
            List<int> vulnsToMerge = new List<int>();
            List<TreeNode> nodesToMerge = GetSelectedNodeList();

            if (nodesToMerge.Count != 0 && nodesToMerge.Count != 1) {
                string newIssueTitle = GetNewIssueTitle();

                if (newIssueTitle != "")
                {

                    foreach (TreeNode node in nodesToMerge)
                    {
                        vulnsToMerge.Add(int.Parse(node.Name));
                    }

                    DatabaseController controller = new DatabaseController();

                    string pluginNames = GetPluginNameList(nodesToMerge);
                    string severityRating = nodesToMerge[0].SelectedImageIndex.ToString();
                    string riskFactor = nodesToMerge[0].Text.Split('-').Last().Replace(" ", "");

                    controller.MergeSelectedVulnerabilities(vulnsToMerge, newIssueTitle, pluginNames, severityRating, riskFactor);

                    DataController dataCont = new DataController();
                    PopulateTreeView(dataCont.GetVulnList());
                    ClearScreenAfter();
                }
            }
        }

        private void RemoveSelectedVulnerabilities(object sender, EventArgs e) {
            List<int> vulnsToRemove = new List<int>();
            List<TreeNode> nodesToDelete = GetSelectedNodeList();

            if (nodesToDelete.Count != 0) {
                foreach (TreeNode node in nodesToDelete) {
                    vulnsToRemove.Add(int.Parse(node.Name));
                }

                DatabaseController controller = new DatabaseController();
                controller.RemoveSelectedVulnerability(vulnsToRemove);

                foreach (TreeNode node in nodesToDelete) {
                    treeView1.Nodes.Remove(node);
                }

                DataController dataCont = new DataController();
                PopulateTreeView(dataCont.GetVulnList());
                ClearScreenAfter();
            }
            else {
                MessageBox.Show("No Vulnerabilities selected to delete.");
            }
        }

        private List<TreeNode> GetSelectedNodeList() {
            List<TreeNode> checkedNodeList = new List<TreeNode>();

            foreach (TreeNode node in treeView1.Nodes) {
                if (node.BackColor == SystemColors.Highlight && node.ForeColor == SystemColors.HighlightText) {
                    checkedNodeList.Add(node);
                }

                if (node.IsSelected) {
                    checkedNodeList.Add(node);
                }
            }
            return checkedNodeList;
        }

        private string GetNewIssueTitle() {
            string newIssueTitle = null;
            newIssueTitle = Interaction.InputBox("Please enter a new Issue Title:", "Issue Rename");
            return newIssueTitle;
        }

        private string GetPluginNameList(List<TreeNode> nodesToMerge) {
            string pluginNames = "";

            foreach (TreeNode node in nodesToMerge) {
                pluginNames = pluginNames + "+ " + node.Text + System.Environment.NewLine;
            }

            return pluginNames;
        }

        public List<TreeNode> backupSearchList = new List<TreeNode>();

        private void TextBox1_TextChanged(object sender, EventArgs e) {
            string searchString = textBox1.Text;
            List<TreeNode> searchList = new List<TreeNode>();

            if (searchString.Contains(',')) {
                searchList = DoMultipleSearch(searchString);
            }
            else {
                searchList = DoSearch(searchString);
            }

            treeView1.Nodes.Clear();

            foreach (TreeNode fnode in searchList) {
                treeView1.Nodes.Add((TreeNode)fnode.Clone());
            }
        }

        private List<TreeNode> DoMultipleSearch(string searchString) {
            List<TreeNode> treeNodeList = new List<TreeNode>();
            List<string> searchList = new List<string>();
            searchList = searchString.Split(',').ToList();

            foreach (string item in searchList) {
                treeNodeList.AddRange(DoSearch(item));
            }

            return treeNodeList;
        }

        private List<TreeNode> DoSearch(string searchString) {
            List<TreeNode> searchList = new List<TreeNode>();

            string nodeTitle = "";

            foreach (TreeNode node in backupSearchList) {
                nodeTitle = node.Text;

                if (nodeTitle.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) {
                    searchList.Add(node);
                }
            }

            return searchList;


        }

        private void PopulateSearchList() {
            ClearSeachList();

            foreach (TreeNode node in treeView1.Nodes) {
                backupSearchList.Add((TreeNode)node.Clone());
            }
        }

        private void ClearSeachList() {
            backupSearchList.Clear();
        }

        /*    private void removeFromBackupSearchList(List<TreeNode> checkedNodeList)
            {
                for (int x = backupSearchList.Count - 1; x >= 0; x--)
                {
                    string matchedNodeName = backupSearchList[x].Name;

                    for (int y = checkedNodeList.Count - 1; y >= 0; y--)
                    {
                        TreeNode node = new TreeNode();
                        node = checkedNodeList[y];

                        string checkedNodeName = node.Name;

                        if (matchedNodeName.IndexOf(checkedNodeName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            backupSearchList.Remove(backupSearchList[x]);
                        }
                    }
                }
            }*/

        private void DataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (!sender.Equals(null)) {
                AppendixForm appendixForm = new AppendixForm();
                appendixForm.ShowDialog();
            }
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            if (sender != null) {
                DataController dataCont = new DataController();
                Vulnerability selectedVuln = new Vulnerability();
                selectedVuln = dataCont.GetSelectedVuln(int.Parse(treeView1.SelectedNode.Name));

                textBox2.Text = "Plugin ID: " + selectedVuln.PluginID + "\r\n\r\n" +
                                    "Plugin Name: " + selectedVuln.PluginName + "\r\n\r\n" +
                                    "Risk Factor: " + selectedVuln.RiskFactor;
                textBox3.Text = selectedVuln.Description;
                textBox4.Text = selectedVuln.Solution;

                dataGridView1.DataSource = dataCont.GetAffectedHosts(selectedVuln.PluginID);
                dataCont.SetAppendixPluginID(selectedVuln.PluginID);

                label5.Text = "Affected Hosts: " + dataGridView1.RowCount;
            }
        }
    }
}
