using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ConversorGts7Geomat
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
            foreach (string fileName in listBox1.Items)
            {
                Console.WriteLine(fileName);
                Queue<Gts7Line> queue = new Queue<Gts7Line>();
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 512))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        Gts7Line gg = Gts7Converter.processLine(line);
                        if (gg != null) queue.Enqueue(gg);
                    }
                }

                string coordenada = Gts7Converter.coordenadaGenerator(queue);
                string caderneta = Gts7Converter.cadernetaGenerator(queue);
                string newFileName = fileName.Replace(".gts7", "");
                newFileName = newFileName.Replace(".GTS7", "");
                newFileName = newFileName.Replace(".GT7", "");
                newFileName = newFileName.Replace(".gt7", "");
                string cadFileName = newFileName + "_CADERN.DAT";
                string cooFileName = newFileName + "_COORDE.DAT";
                File.WriteAllText(cadFileName, caderneta);
                File.WriteAllText(cooFileName, coordenada);
            }
        }
    }
}
