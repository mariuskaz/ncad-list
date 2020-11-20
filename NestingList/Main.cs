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

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 15, 332, 13);
            textBox.SetBounds(12, 31, 332, 36);
            buttonOk.SetBounds(188, 72, 75, 28);
            buttonCancel.SetBounds(269, 72, 75, 28);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(356, 115);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            textBox.SelectAll();

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = "c:\\tpacad\\nestcad\\";
            openFileDlg.Title = "Importas";
            openFileDlg.Filter = "Nesting TpaCAD (*.ncad)|*.ncad";

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openFileDlg.FileName;
                string value = "1";
                if (InputBox("Multiply", "Quantity:", ref value) == DialogResult.OK)
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

                        int multiply = 1;
                        Int32.TryParse(value, out multiply);

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
            openFileDlg.InitialDirectory = "c:\\tpacad\\nestcad\\";
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
                String grain = "0";
                String rotate = "ang";

                DialogResult dialogResult = MessageBox.Show("  Ar medžiaga su tekstūra?", "Material Grain", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dialogResult == DialogResult.Yes)
                {
                    grain = "1";
                    rotate = "grain";
                }

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
                    <sheet en='1' name='Sheet_1' diml='2800' dimh='2070' dims='{thick}' items='100' type='0' grain='{grain}' />
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
                        new XAttribute(rotate, "1"),
                        new XAttribute("material", item.SubItems[5].Text)
                    );
                    rows.Add(row);
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
