using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using HuffmanLibrary;

namespace Huffman
{
    class Program
    {
        static void Main(string[] args)
        {
            string command = "", inFilePath, outFilePath, reportSwitch = ""; // hold command line arguments

            /*#region debug
            
            //int mode = 0;
            //int mode = 1;
            int mode = 2;
            if (mode == 0||mode==2)
            {
                Stream inputFileStream1 = new FileStream("test.txt", FileMode.Open);
                Stream outputFileStream1 = new FileStream("test.tx_", FileMode.Create);
                HuffmanStreamCoding.EncodeStream(inputFileStream1, outputFileStream1, new StreamWriter("test.txt_report.txt"));
                outputFileStream1.Close();
            }
            if (mode==1||mode==2)
            {
                Stream inputFileStream1 = new FileStream("test.tx_", FileMode.Open);
                Stream outputFileStream1 = new FileStream("test2.txt", FileMode.Create);

                HuffmanStreamCoding.DecodeStream(inputFileStream1, outputFileStream1);
            }

            if( mode!=-1)
                return;
            
            #endregion*/

            if (args.Length == 1)
            {
                command = args[0].ToUpper();
                if (command == "/?" || command == "/HELP")
                {
                    showHelpMessage();
                    return;
                }
            }

            if (args.Length < 3)
            {
                showInvalidUseMessage();
                return;
            }
            
            command = args[0].ToUpper();

            if (args.Length>3)
            {
                reportSwitch = args[3].ToUpper();
            }

            inFilePath = args[1];
            outFilePath = args[2];
             
            if (!File.Exists(inFilePath))
            {
                Console.WriteLine("Cannot find the file specified '" + inFilePath + "'\n");
                return;
            }

            Stream inputFileStream = new FileStream(inFilePath, FileMode.Open);
            Stream outputFileStream = new FileStream(outFilePath, FileMode.Create);

            if (command == "ENCODE") // Encode inputfile
            {
                if (reportSwitch == "/REPORT")
                    HuffmanStreamCoding.EncodeStream(inputFileStream, outputFileStream, new StreamWriter(inFilePath+"_report.txt"));
                else
                    HuffmanStreamCoding.EncodeStream(inputFileStream, outputFileStream );
            }
            else if (command == "DECODE") // Encode inputfile
            {
                HuffmanStreamCoding.DecodeStream(inputFileStream, outputFileStream);
            }
            else
            {
                showInvalidUseMessage();
                return;
            }
        }

        private static void showHelpMessage()
        {
            Console.WriteLine();
            Console.WriteLine("The syntax of this command is: ");
            Console.WriteLine();
            Console.WriteLine("Usage: Huffman [ encode | decode ]  <input file>  <output file>");
            Console.WriteLine("               [ /report ]");
            Console.WriteLine("");
        }

        private static void showInvalidUseMessage()
        {
            Console.WriteLine("Invalid use. Type /? or /help.");
        }
    }
}
