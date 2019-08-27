using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ToolView3.Controllers;
using ToolView3.Models;

namespace ToolView3.Views {
    public partial class AppendixForm : Form {
        public AppendixForm() {
            InitializeComponent();
        }

        private void AppendixForm_Load(object sender, EventArgs e) {
            mwTreeView1.ImageList = imageList1;

            PopulateHostsList();
        }

        private void PopulateHostsList() {
            DataController dataCont = new DataController();
            List<TreeNode> nodeList1 = new List<TreeNode>();
            List<AffectedHosts> appData = new List<AffectedHosts>();
            TreeNode node1 = null;

            int pluginID = dataCont.GetAppendixPluginID().pluginID;
            appData = dataCont.GetAffectedHosts(pluginID);

            foreach (var host in appData) {
                node1 = new TreeNode(host.HostIP);
                node1.Name = host.HostIP;
                node1.ImageIndex = 0;
                node1.SelectedImageIndex = 0;
                nodeList1.Add(node1);
            }

            TreeNode[] nodes1 = nodeList1.ToArray();
            mwTreeView1.Nodes.Clear();
            mwTreeView1.BeginUpdate();
            mwTreeView1.Nodes.AddRange(nodes1);
            mwTreeView1.EndUpdate();
        }

        private void MwTreeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            DataController dataCont = new DataController();
            List<Vulnerability> vulns = new List<Vulnerability>();
            string hostIP = mwTreeView1.SelectedNode.Text;
            int pluginID = dataCont.GetAppendixPluginID().pluginID;

            vulns = dataCont.GetAppendixData(hostIP, pluginID);
            string[] outputList = new string[vulns.Count];
            int listNum = 0;

            foreach (var vuln in vulns) {
                outputList[listNum] = outputList[listNum] + hostIP + " : " + vuln.Port.ToString() + "/" + vuln.Protocol
                                        + Environment.NewLine
                                        + vuln.PluginOutput
                                        + Environment.NewLine;
            }

            string seperator = Environment.NewLine + System.Environment.NewLine;
            richTextBox1.Text = string.Join(seperator, outputList);
        }

        private void Button1_Click(object sender, EventArgs e) {
            List<AffectedHosts> hosts = new List<AffectedHosts>();
            DataController dataCont = new DataController();
            int pluginID = dataCont.GetAppendixPluginID().pluginID;

            hosts = dataCont.GetAffectedHosts(pluginID);
            string output = "";

            foreach (AffectedHosts host in hosts) {
                string hostIP = host.HostIP;
                List<Vulnerability> vulns = new List<Vulnerability>();
                vulns = dataCont.GetAppendixData(hostIP, pluginID);

                foreach (Vulnerability vuln in vulns) {
                    output = output + host.HostIP + " : " + vuln.Port + "/" + vuln.Protocol
                                + Environment.NewLine
                                + vuln.PluginOutput
                                + Environment.NewLine
                                + Environment.NewLine;
                }
            }

            richTextBox1.Text = output;
        }
    }
}
