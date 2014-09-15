using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace armSim
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

        private void runButton_Click(object sender, EventArgs e)
        {
            OptionParser options = new OptionParser();
            OutputLabel.Text = "Running";
            ErrorLabel.Text = "";
            try
            {
                if (memSizeBox.Text != "" && fileChosenBox.Text != "")
                {
                    int memSize = Convert.ToInt32(memSizeBox.Text);
                    options.setMemSize(memSize);
                    options.setTest(testRB.Checked);
                    options.setFile(fileChosenBox.Text);


                }
                else
                {

                    ErrorLabel.Text = "Please provide input";
                    OutputLabel.Text = "";
                }
                //options entered

            }
            catch
            {
                ErrorLabel.Text = "Invalid Input";
                OutputLabel.Text = "";
            }
            
        }

      
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Executable Files|*.exe|All Files| *.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileChosenBox.Text = openFileDialog1.FileName;
                
            }
        }
    }
}
