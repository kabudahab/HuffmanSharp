using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HuffmanLibrary
{
    class BitBuffer
    {
        private int _bits;
        private Stream _outStream;

        private string buffer;

        public BitBuffer(int Bytes, Stream outStream)
        {
            _bits = Bytes * 8;
            _outStream = outStream;

            buffer = "";
        }

        /// <summary>
        /// Flush buuffer contents to output stream when object is disposed
        /// </summary>
        ~BitBuffer()
        {
            if (buffer.Length > 0)
                _outStream.WriteByte(HuffmanStreamCoding.encodeBitString(buffer));
        }

        /// <summary>
        /// Buffer a string of bits 
        /// </summary>
        /// <param name="stringOfBits">String of bits (0's or 1's)</param>
        public void AddToBuffer( string stringOfBits )
        {
            buffer += stringOfBits;

            if (buffer.Length >= _bits)
            {
                string bits = buffer.Substring(0, _bits);
                buffer = buffer.Substring(_bits);
                _outStream.WriteByte(HuffmanStreamCoding.encodeBitString(bits));
            }
        }

        /// <summary>
        /// Flush buuffer contents to output stream
        /// </summary>
        public void Flush()
        {
            if (buffer.Length > 0)
            {
                _outStream.WriteByte(HuffmanStreamCoding.encodeBitString(buffer));
                buffer = "";
            }
        }
    }
}
