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
using System.Xml;
using System.Xml.Linq;

namespace NestingList
{

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private string fileName;

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = "c:\\tpacad\\product\\nestcad\\";
            openFileDlg.Title = "Import";
            openFileDlg.Filter = "Nesting TpaCAD (*.ncad)|*.ncad";

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openFileDlg.FileName;
                ImportDialog input = new ImportDialog();

                if (input.ShowDialog() == DialogResult.OK)
                {

                    var xml = XDocument.Load(selectedFileName);
                    var items = xml.Descendants("row");
                    foreach (var item in items)
                    {
                        string name = item.Attribute("name").Value;
                        string height = item.Attribute("dimh").Value;
                        string length = item.Attribute("diml").Value;
                        string thick = item.Attribute("dims").Value;
                        string material = "Generic";
                        if (item.Attribute("material") != null)
                        {
                            material = item.Attribute("material").Value;
                        }

                        int qty = 1;
                        Int32.TryParse(item.Attribute("items").Value, out qty);

                        int multiply = input.qty;
                        qty = qty * multiply;

                        var row = new ListViewItem(new[] { name, length, height, thick, qty.ToString(), material });
                        PartsList.Items.Add(row);

                    }
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = "c:\\tpacad\\product\\nestcad\\";
            openFileDlg.Filter = "Nesting TpaCAD (*.ncad)|*.ncad";

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDlg.SafeFileName;
                PartsList.Items.Clear();
                MainForm.ActiveForm.Text = "Nesting List - " + openFileDlg.FileName;

                var xml = XDocument.Load(openFileDlg.FileName);
                var items = xml.Descendants("row");
                foreach (var item in items)
                {
                    string name = item.Attribute("name").Value;
                    string height = item.Attribute("dimh").Value;
                    string length = item.Attribute("diml").Value;
                    string thick = item.Attribute("dims").Value;
                    string qty = item.Attribute("items").Value;
                    string material = "Generic";
                    if (item.Attribute("material") != null)
                    {
                        material = item.Attribute("material").Value;
                    }
                    
                    var row = new ListViewItem(new[] { name, length, height, thick, qty, material });
                    PartsList.Items.Add(row);

                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDlg = new SaveFileDialog();
            saveFileDlg.Filter = "Nesting TpaCAD (*.ncad)|*.ncad";
            saveFileDlg.FileName = fileName;

            if (saveFileDlg.ShowDialog() == DialogResult.OK)
            {

                String spc = System.IO.Path.GetFileName(saveFileDlg.FileName).Replace(".ncad","");
                String material = PartsList.Items[0].SubItems[5].Text;
                String thick = PartsList.Items[0].SubItems[3].Text;
                int grain = 0;

                DialogResult dialogResult = MessageBox.Show("  Ar medžiaga su tekstūra?", "Material Grain", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dialogResult == DialogResult.Yes) grain = 1;

                String xml = $@"<?xml version='1.0' encoding='UTF-8'?>
                <update version='1.0'>
                  <params>
                    <param name='refOrder' value='{spc}' />
                    <param name='refProduct' value='{material}' />
                    <param name='bLeft' value='10' />
                    <param name='bRight' value='10' />
                    <param name='bBottom' value='10' />
                    <param name='bTop' value='10' />
                  </params>
                  <sheets>
                    <sheet en='1' name='Sheet_1' diml='2070' dimh='2800' dims='{thick}' items='100' type='0' grain='{grain + grain * grain}' />
                  </sheets>
                  <rows>
                  </rows>
                </update>";

                XDocument doc = XDocument.Parse(xml);
                var rows = doc.Descendants("rows").First();
                foreach (ListViewItem item in PartsList.Items)
                {
                    var row = new XElement("row",
                        new XAttribute("en","1"),
                        new XAttribute("name", item.SubItems[0].Text),
                        new XAttribute("diml", item.SubItems[1].Text),
                        new XAttribute("dimh", item.SubItems[2].Text),
                        new XAttribute("dims", item.SubItems[3].Text),
                        new XAttribute("items", item.SubItems[4].Text),
                        new XAttribute("ang", "1"),
                        new XAttribute("grain", grain.ToString()),
                        new XAttribute("material", item.SubItems[5].Text)
                    );
                    if (row.Attribute("material").Value != material) material = "";
                    rows.Add(row);
                }

                if (material == "")
                {
                    var product = (from param in doc.Descendants("param")
                                   where param.Attribute("name").Value == "refProduct"
                                   select param).FirstOrDefault();

                    product.SetAttributeValue("value", "");
                }

                doc.Save(saveFileDlg.FileName);
                MainForm.ActiveForm.Text = "Nesting List - " + saveFileDlg.FileName;
                fileName = System.IO.Path.GetFileName(saveFileDlg.FileName);
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Process.Start("c:\\tpacad\\bin\\tpacad.exe");
            this.Close();
        }

        private void PartsList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && PartsList.SelectedIndices.Count > 0)
            {
                foreach (int item in PartsList.SelectedIndices)
                {
                    PartsList.Items.RemoveAt(PartsList.SelectedIndices[0]);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (PartsList.SelectedIndices.Count > 0)
            {
                foreach (int item in PartsList.SelectedIndices)
                {
                    PartsList.Items.RemoveAt(PartsList.SelectedIndices[0]);
                }
            }
        }

        private void PartsList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //Console.WriteLine("column" + e.Column.ToString());
        }


    }
}
