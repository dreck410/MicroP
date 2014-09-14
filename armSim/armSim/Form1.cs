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

        string[] options;
        public Form1(string[] args)
        {
            options = args;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OutputLabel.Text = "";
            OutputLabel.Text = "Running Please Wait. . .";

            string[] finalOptions = parseOptions(options);
            if (armsim.run(finalOptions) == 0)
            {
                ErrorLabel.Text = "";
                OutputLabel.Text = "Read the ELF file into Ram";
            }
            else
            {
                OutputLabel.Text = "";
                ErrorLabel.Text = "Something went Wrong. See the Log File";
            }
            
        }

        public string[] parseOptions(string[] options)
        {
            int x = 4;
            if (testRB.Checked)
            {
                x++;
            }
            string[] output = new string[x];
         /*   Array.Copy(options, output, options.Length);
            for (int i = 0; i < output.Length; i++)
            {
                switch (output[i])
                {
                    case "--length":

                        break;
                    case "--mem":

                        break;
                    case "--test":

                        break;

                    default:
                        break;
                }

            }
          */
            string memSize = "32768";
            try
            {
                int number = Convert.ToInt32(memSizeBox.Text);
                if (number > 1048576 || number < 1)
                {
                    OutputLabel.Text = "";
                    ErrorLabel.Text = "Number must be in the range, 1 -> 1048576";
                }
                else
                {
                    memSize = number.ToString();
                }


                output[0] = "--load";
                if (fileChosenBox.Text == "")
                {
                    output[1] = "test1.exe";
                }
                else
                {
                    output[1] = fileChosenBox.Text;
                }
                output[2] = "--mem";
                output[3] = memSize;
                if (testRB.Checked)
                {
                    output[4] = "--test";
                }
            }
            catch
            {
                OutputLabel.Text = "";
                ErrorLabel.Text = "Invalid input";
            }



            return output;

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
