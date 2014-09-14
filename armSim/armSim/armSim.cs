using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;


namespace armSim
{
    public class armsim
    {

        //Comment for commit
        public static StreamWriter log;
        public static Options optionParser;

        public static void printArray(byte[] array)
        {
            string output = "";

            output = "The Array\n";
            for (int i = 0; i < array.Length; i++)
            {
                output += array[i].ToString("X2");
                i++;
                output += array[i].ToString("X2") + " ";
                if ((i + 1) % 16 == 0)
                {
                    output += "\n";
                }
            }
            log.WriteLine(output);

        }

        /*
         * 
         * Unused might be good in the future
        public static byte[] stringToByteArray(string input){

            byte[] bA = new byte[input.Length * sizeof(char)];

            char[] inputArray = input.ToCharArray ();
            System.Buffer.BlockCopy(inputArray, 0, bA, 0, bA.Length);
            return bA;
        }
        */

        public static int run(string[] args)
        {

            optionParser = new Options();



            try
            {
                Console.WriteLine("We are here");
                log = new StreamWriter("log.txt");
                log.WriteLine("Log: Start");

                if (optionParser.parse(args))
                {
                    if (optionParser.getTest())
                    {
                        TestRam.RunTests(log);
                        TestArmSim.RunTests(log);
                    }

                    log.WriteLine("Log: MemSize " + optionParser.getMemSize());
                    log.WriteLine("Log: File " + optionParser.getFile());
                    log.WriteLine("Log: Little Endian " + BitConverter.IsLittleEndian);


                    ELFReader e = new ELFReader();

                    byte[] elfArray = File.ReadAllBytes(optionParser.getFile());

                    e.ReadHeader(elfArray);



                    log.WriteLine("ELF: Header Position " + e.elfHeader.e_phoff);
                    log.WriteLine("ELF: Header Size " + e.elfHeader.e_ehsize);
                    log.WriteLine("ELF: Entry Position " + e.elfHeader.e_entry.ToString("X4"));
                    log.WriteLine("ELF: Number of program headers " + e.elfHeader.e_phnum);

                    for (int i = 1; i <= e.elfHeader.e_phnum; i++)
                    {
                        log.WriteLine("ELF: Program Header {0}, Offset = {1}, Size = {2}",
                                       i,
                                       e.elfHeader.e_phoff,
                                       e.elfHeader.e_phentsize);
                    }

                    //ignore the entry point for a loader
                    //its for (executing)going into the ram after it's loaded

                    ramSim ram = new ramSim(optionParser.getMemSize());
                    writeElfToRam(e, elfArray, ref ram);


                    log.WriteLine("Log: Ram Hash " + ram.getHash());


                    printArray(ram.getArray());

                }
            }
            catch
            {

                log.WriteLine("Log: Something went wrong");

                log.Close();
                return -1;
            }

            //all is good in the world,the sky is blue, the tank is clean...THE TANK IS CLEAN!!
            log.WriteLine("Log: Program Finished");
            log.WriteLine("-----------------------------");
            log.Close();

            return 0;
        }

        public static void writeElfToRam(ELFReader e, byte[] elfArray, ref ramSim ram)
        {

            log.WriteLine("RAM: Size {0}", ram.getSize());


            for (int prog = 0; prog < e.elfHeader.e_phnum; prog++)
            {
                int ramAddress = e.elfphs[prog].p_vaddr;
                log.WriteLine("RAM: Writing to {0} ", ramAddress);

                int elfOffSet = (int)e.elfphs[prog].p_offset;
                log.WriteLine("ELF: Reading from {0}", e.elfphs[prog].p_offset);

                log.WriteLine("ELF: Size of Segment {0}", e.elfphs[prog].p_filesz);
                int ii = ramAddress;
                int j = elfOffSet;
                for (; j < elfArray.Length && ii < e.elfphs[prog].p_filesz + ramAddress; ii++, j++)
                {
                    ram.WriteByte(ii, elfArray[j]);
                }


            }

        }

    }

  
  


}