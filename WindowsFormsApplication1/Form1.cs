using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }
       
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            listBox1.ClearSelected();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(openFileDialog1.FileNames);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
