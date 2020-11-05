using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            textBox.SetBounds(12, 31, 332, 30);
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
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\tpacad\\nestcad\\";
            openFileDialog1.Title = "Pasirinkite";
            openFileDialog1.Filter = "TPA nesting files (*.ncad)|*.ncad";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openFileDialog1.FileName;
                string value = "1";
                if (InputBox("Multiply", "Quantity:", ref value) == DialogResult.OK)
                {

                    // https://stackoverflow.com/questions/642293/how-do-i-read-and-parse-an-xml-file-in-c

                    XmlDocument doc = new XmlDocument();
                    doc.Load(selectedFileName);

                    var xml = XDocument.Load(@selectedFileName);
                    var items = xml.Descendants("row");
                    foreach (var item in items)
                    {
                        string name = item.Attribute("name").Value;
                        string height = item.Attribute("dimh").Value;
                        string width = item.Attribute("diml").Value;
                        string thick = item.Attribute("dims").Value;
                        string sheet = item.Attribute("en").Value;
                        string qty = item.Attribute("items").Value;
                        var row = new ListViewItem(new[] { name, height, width, thick, qty });
                        PartsList.Items.Add(row);
                    }
                }
            }
        }
    }
}
