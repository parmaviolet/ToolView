using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolView3.Controllers;
using ToolView3.Models;

namespace ToolView3.Views
{
    public partial class PluginOutputForm : Form
    {
        public PluginOutputForm(){
            InitializeComponent();
        }

        private void PluginOutputForm_Load(object sender, EventArgs e) {
            PopulateTreeView();
        }

        private void PopulateTreeView() {
            DataController dataCont = new DataController();
            List<Vulnerability> vulnList = dataCont.GetVulnList();

            List<TreeNode> nodeList1 = new List<TreeNode>();
            TreeNode node1 = null;

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
        }

        private void button1_Click(object sender, EventArgs e) {

            DataController dataCont = new DataController();
            dataGridView1.DataSource = dataCont.GetAffectedHostsMulti(GetSelectedNodes());
        }

        private List<int> GetSelectedNodes() {
            List<int> checkednodes = new List<int>();

            foreach (TreeNode node in treeView1.Nodes) {
                if (node.Checked) {
                    checkednodes.Add(Convert.ToInt32(node.Name));
                }
            }
            return checkednodes;
        }
    }
}
