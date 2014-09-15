using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
//using System.Security.Cryptography;
//using System.IO;
using System.Runtime.InteropServices;


namespace armSim
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            OptionParser options = new OptionParser();
            options.parse(args);
            if (options.getTest() || options.getFile() != null)
            {
                armsim.run(options);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}


