using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NestingList
{
    public partial class ImportDialog : Form
    {
        public ImportDialog()
        {
            InitializeComponent();
        }

        public int qty = 0;
        public string palletNo;

        private void btnOK_Click(object sender, EventArgs e)
        {
            int.TryParse(ItemQty.Text, out qty);
            palletNo = PalletNo.Text;
        }

        private void ImportDialog_Load(object sender, EventArgs e)
        {
            PalletNo.Text = palletNo;
            ItemQty.SelectAll();
        }
    }
}
