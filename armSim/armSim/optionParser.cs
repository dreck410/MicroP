﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armSim
{

    public class OptionParser
    {

        public string file = "";
        public int memSize = 32768;
        public bool test = false;
        public bool valid;


        //-----------------Getters
        public string getFile()
        {
            return file;
        }
        public int getMemSize()
        {
            return memSize;
        }
        public bool getTest()
        {
            return test;
        }
        public void setTest(bool newTest)
        {
            test = newTest;
        }
        public void setFile(string newFile)
        {
            file = newFile;
        }
        public void setMemSize(int newMemSize)
        {
            memSize = newMemSize;
        }

        public void getError(string inpu)
        {
            string output = inpu;
            output += "\nValid input is: armsim --load elf-file [ --mem memory-size ] [ --test]";
            Console.WriteLine(output);
        }

        public bool parse(string[] inpu)
        {
            file = null;
            test = false;
            valid = true;
            memSize = 32768;
            for (int i = 0; i < inpu.Length; i++)
            {
                switch (inpu[i])
                {
                    case "--load":
                        i++;
                        file = inpu[i];
                        break;

                    case "--mem":
                        i++;
                        memSize = Convert.ToInt32(inpu[i]);
                        if (memSize > 1048576)
                        {
                            getError("Memsize is too large, cannot be over 1048576 bytes.");
                            valid = false;
                        }
                        break;

                    case "--test":
                        test = true;
                        break;

                    default:
                        //this can be the helper instructions
                        getError(inpu[i] + " is an invalid option.");
                        valid = false;
                        break;
                }
            }
            if (file == null)
            {
                valid = false;
                getError("No file specified.");
            }
            return valid;
        }
    }

}
