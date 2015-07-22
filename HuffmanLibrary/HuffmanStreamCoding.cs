using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace HuffmanLibrary
{
    public static class HuffmanStreamCoding
    {
        // Data Members
        static FrequencyTable frequencyTable;

        // Public Methods
        public static void EncodeStream(Stream inStream, Stream outStream, StreamWriter treeStream)
        {
            EncodeStream(inStream, outStream);
            writeHuffmanTreeToTextStream(treeStream);
        }

        public static void EncodeStream(Stream inStream, Stream outStream)
        {
            int i = 0; // counter
            int newFreq;
            // the list of all Huffman Tree nodes
            List<TreeNode<int, byte>> huffmanTree = new List<TreeNode<int, byte>>();
            // the root of Huffman Tree
            TreeNode<int, byte> huffmanTreeRoot = new TreeNode<int, byte>(0, 0);

            frequencyTable = new FrequencyTable();
            /*frequencyTable = new FrequencyEntry[256 ];
            for (i = 0; i < frequencyTable.Length; ++i)
                frequencyTable[i] = new FrequencyEntry();*/

            calculateFrequency(inStream);

            i = 0;
            // construct a forest
            for (i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                    // add a new node to Huffman Tree, where character hits is the key 
                    // and the character itself is the value
                    huffmanTree.Add(new TreeNode<int, byte>(frequencyTable.Hits[i], (byte)i));
            }

            // link the nodes
            while (huffmanTree.Count > 1)
            {
                huffmanTree.Sort(compareTreeNodes);

                newFreq = huffmanTree[0].Key + huffmanTree[1].Key;

                TreeNode<int, byte> node = new TreeNode<int, byte>(newFreq, 0);
                node.Left = huffmanTree[0];
                node.Right = huffmanTree[1];

                node.Left.Parent = node;
                node.Right.Parent = node;

                huffmanTree.RemoveAt(0);
                huffmanTree.RemoveAt(0);

                huffmanTree.Add(node);

                huffmanTreeRoot = node;
            }

            //calculate codewords
            traverseTree(huffmanTreeRoot, "");

            //wrtie table
            writeHuffmanTreeToStream(outStream);

            // 
            outStream.WriteByte((byte)(inStream.Length % 256));

            //wrtie encoded stream
            //string buffer = "", bits = "";
            //string pooledBuffer = "";
            BitBuffer bitBuffer = new BitBuffer(1, outStream);
            int nextChar = 0;

            inStream.Seek(0, SeekOrigin.Begin);
            nextChar = inStream.ReadByte();
            while (nextChar != -1)
            {
                bitBuffer.AddToBuffer(frequencyTable.Codewords[nextChar]);
                //pooledBuffer += frequencyTable.Codewords[nextChar];

                /*if (buffer.Length >= 8)
                {
                    bits = buffer.Substring(0, 8);
                    buffer = buffer.Substring(8);
                    outStream.WriteByte(encodeBitString(bits));
                }*/
                nextChar = inStream.ReadByte();
            }

            bitBuffer.Flush();
            //if (buffer.Length > 0)
            //  outStream.WriteByte(encodeBitString(buffer));
        }

        public static void DecodeStream(Stream inStream, Stream outStream)
        {
            int i = 0; // counter
            byte streamLength = 0, readCharacters = 0;

            // the root of Huffman Tree
            TreeNode<int, byte> huffmanTreeRoot = new TreeNode<int, byte>(-1, 0);
            TreeNode<int, byte> currentNode;

            frequencyTable = new FrequencyTable();

            readHuffmanTreeFromStream(inStream);

            streamLength = (byte)inStream.ReadByte();

            i = 0;
            // construct A Huffman Tree
            for (i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                {
                    currentNode = huffmanTreeRoot;
                    foreach (char c in frequencyTable.Codewords[i])
                    {
                        if (c == '0')
                        {
                            if (currentNode.Left == null)
                                currentNode.Left = new TreeNode<int, byte>(-1, 0);
                            currentNode = currentNode.Left;
                        }
                        else if (c == '1')
                        {
                            if (currentNode.Right == null)
                                currentNode.Right = new TreeNode<int, byte>(-1, 0);
                            currentNode = currentNode.Right;
                        }
                    }
                    currentNode.Key = 1; // this node contains a character (is a leaf)
                    currentNode.Value = (byte)i;
                }
            }

            // Tree is ready, start decoding
            string buffer = "", bits = "";
            //string pooledBuffer = "";
            int nextByte = 0, nextNextByte;
            i = 0;

            nextNextByte = inStream.ReadByte();
            while (nextByte != -1)
            {
                nextByte = nextNextByte;
                nextNextByte = inStream.ReadByte();

                bits = decodeByteToBitString((byte)nextByte);
                if (bits.Length < 8)
                    bits += new string('0', 8 - bits.Length);

                buffer += bits;
                //pooledBuffer += bits;

                currentNode = huffmanTreeRoot; // start from Tree root

                for (i = 0; i < buffer.Length; i++)
                {
                    if (nextNextByte == -1) // end of stream
                    {
                        if (readCharacters == streamLength)
                            break;
                    }

                    if (buffer[i] == '0')
                        currentNode = currentNode.Left;
                    else
                        currentNode = currentNode.Right;

                    if (currentNode.Key == 1) // character found
                    {
                        ++readCharacters;

                        outStream.WriteByte(currentNode.Value);
                        buffer = buffer.Substring(i + 1);

                        i = -1;
                        currentNode = huffmanTreeRoot;
                    }
                }

                //nextByte = inStream.ReadByte();
            }
        }

        // Utility functions
        private static void calculateFrequency(Stream inputStream)
        {
            int nextChar = 0;

            nextChar = inputStream.ReadByte();
            while (nextChar != -1)
            {
                frequencyTable.HitCharacter((byte)nextChar);
                nextChar = inputStream.ReadByte();
            }
        }

        private static int compareTreeNodes(TreeNode<int, byte> node1, TreeNode<int, byte> node2)
        {
            if (node1 == null || node2 == null)
                return 0;

            if (node1.Key == node2.Key)
                return 0;
            else if (node1.Key > node2.Key)
                return 1;
            else
                return -1;
        }

        private static void traverseTree(TreeNode<int, byte> startNode, string codeword)
        {
            if (startNode.Left == null && startNode.Right == null)
            {
                frequencyTable.Codewords[startNode.Value] = codeword;
            }
            else
            {
                if (startNode.Left != null)
                {
                    traverseTree(startNode.Left, codeword + "0");
                }
                if (startNode.Right != null)
                {
                    traverseTree(startNode.Right, codeword + "1");
                }
            }
        }

        private static void writeHuffmanTreeToStream(Stream fileStream)
        {
            BitBuffer bitBuffer = new BitBuffer(1, fileStream);

            fileStream.WriteByte((byte)frequencyTable.TableSize);

            for (int i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                {
                    fileStream.WriteByte((byte)i);
                    fileStream.WriteByte((byte)frequencyTable.Codewords[i].Length);
                }
            }

            for (int i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                {
                    bitBuffer.AddToBuffer(frequencyTable.Codewords[i]);
                }
            }

            bitBuffer.Flush();
        }
        /*
        {
            BinaryFormatter binaryFormater = new BinaryFormatter();
            object[,] headerRows= new object[frequencyTable.TableSize,2];
            int j = 0;
            
            binaryFormater.Serialize(fileStream, frequencyTable.TableSize);
            
            for (int i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                {
                    headerRows[j, 0] = (byte)i;
                    headerRows[j, 1] = frequencyTable.Codewords[i];
                    ++j;
                }
            }

            binaryFormater.Serialize(fileStream, headerRows);
        }
        */

        private static void writeHuffmanTreeToTextStream(StreamWriter treeStream)
        {
            FrequencyEntry e;
            long originalSize = 0;
            long newSize = 0;

            treeStream.WriteLine("Table size: {0}", frequencyTable.TableSize);
            treeStream.WriteLine();

            SortedList<string, FrequencyEntry> sortedTable = new SortedList<string, FrequencyEntry>(frequencyTable.TableSize);
            for (int i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                {
                    sortedTable.Add(frequencyTable.Codewords[i], new FrequencyEntry((byte)i, frequencyTable.Hits[i], frequencyTable.Codewords[i]));
                    originalSize += frequencyTable.Hits[i] * 8;
                    newSize += frequencyTable.Hits[i] * frequencyTable.Codewords[i].Length;
                }
            }

            treeStream.WriteLine("Original size: {0}", originalSize);
            treeStream.WriteLine();
            treeStream.WriteLine("New size: {0}", newSize);
            treeStream.WriteLine();
            treeStream.WriteLine("Compression ratio: {0}%", (double)((long)((1 - (double)newSize / originalSize) * 10000)) / 100);
            treeStream.WriteLine();

            treeStream.WriteLine("   Character   \t   Frequency   \t   Codeword   ");
            treeStream.WriteLine("---------------\t---------------\t--------------");

            for (int i = 0; i < sortedTable.Count; i++)
            {
                e = sortedTable.Values[i];
                treeStream.Write(e.Character + " [" + (char)e.Character + "]\t\t" + e.Frequency + "\t\t" + e.Codeword);
                treeStream.WriteLine();
            }
            treeStream.Flush();
        }

        private static void readHuffmanTreeFromStream(Stream fileStream)
        {
            //BinaryFormatter binaryFormater = new BinaryFormatter();
            //object[,] headerRows = new object[frequencyTable.TableSize, 2];
            int tableSize = 0;
            byte character, codewordLength;

            tableSize = fileStream.ReadByte();

            for (int i = 0; i < tableSize; i++)
            {
                character = (byte)fileStream.ReadByte();
                codewordLength = (byte)fileStream.ReadByte();

                frequencyTable.HitCharacter(character);
                frequencyTable.Codewords[character] = new string(' ', codewordLength);
            }

            string buffer = "", bits = "";
            int nextByte = 0;

            for (int i = 0; i < frequencyTable.MaxTableSize; i++)
            {
                if (frequencyTable.Hits[i] != 0)
                {

                    while (buffer.Length < frequencyTable.Codewords[i].Length)
                    {
                        nextByte = fileStream.ReadByte();
                        bits = decodeByteToBitString((byte)nextByte);
                        if (bits.Length < 8)
                            bits += new string('0', 8 - bits.Length);

                        buffer += bits;
                    }

                    frequencyTable.Codewords[i] = buffer.Substring(0, frequencyTable.Codewords[i].Length);
                    buffer = buffer.Substring(frequencyTable.Codewords[i].Length);
                }

            }
        }
        /*{
            BinaryFormatter binaryFormater = new BinaryFormatter();
            object[,] headerRows = new object[frequencyTable.TableSize, 2];
            int tableSize = 0;

            tableSize = (int)binaryFormater.Deserialize(fileStream);

            headerRows = binaryFormater.Deserialize(fileStream) as object[,];

            for (int i = 0; i < tableSize; i++)
            {
                frequencyTable.HitCharacter((byte)headerRows[i, 0]);
                frequencyTable.Codewords[(byte)headerRows[i, 0]] = (string)headerRows[i, 1];
            }
        }*/

        internal static byte encodeBitString(string bits)
        {
            byte b = 0;
            byte powersOfTwo = 1;

            foreach (char c in bits)
            {
                if (c == '1')
                    b += powersOfTwo;
                powersOfTwo *= 2;
            }
            return b;
        }

        internal static string decodeByteToBitString(byte Byte)
        {
            if (Byte == 0)
                return "0";

            char[] bits = new char[8];
            int i = 0; // counter

            bits[0] = '0';

            while (Byte > 0)
            {
                if (Byte % 2 == 1)
                    bits[i] = '1';
                else
                    bits[i] = '0';


                Byte /= 2;
                ++i;
            }

            return new string(bits, 0, i);
        }
    }
}
