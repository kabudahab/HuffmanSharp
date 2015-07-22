using System;
using System.Collections.Generic;
using System.Text;

namespace HuffmanLibrary
{
    class FrequencyEntry
    {
        byte _character = 0;
        int _frequency = 0;
        string _codeword = "";

        public FrequencyEntry(byte character, int frequency, string codeword)
        {
            _character = character;
            _frequency = frequency;
            _codeword = codeword;
        }

        public byte Character
        {
            get { return _character; }
            set { _character = value; }
        }

        public int Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        public string Codeword
        {
            get { return _codeword; }
            set { _codeword = value; }
        }
    } // class FrequencyEntry
    

    [Serializable]
    class FrequencyTable
    {
        private const int TABLE_MAX_SIZE = 256;

        //private FrequencyEntry[] _table;
        private int _nonZeroEnties = 0;
        private int[] _hits = new int[TABLE_MAX_SIZE];
        private string[] _codeword = new string[TABLE_MAX_SIZE];

        public FrequencyTable()
        {
            /*int i = 0; // counter

            _table = new FrequencyEntry[256 ];
            for (i = 0; i < _table.Length; ++i)
                _table[i] = new FrequencyEntry();*/
        }

        public int MaxTableSize
        {
            get { return TABLE_MAX_SIZE; }
        } 

        public void HitCharacter(byte character)
        {
            if (_hits[character]++ == 0)
                ++_nonZeroEnties;
        }

        public string[] Codewords
        {
            get { return _codeword; }
            set { _codeword = value; }
        }

        public int[] Hits
        {
            get { return _hits; }
        }

         public int TableSize
        {
            get { return _nonZeroEnties; }
        }       
    }

    [Serializable]
    class TreeNode<TKey, TValue>
    {
        TreeNode<TKey,TValue> _parent, _left, _right;
        TKey _key;
        TValue _value;

        public TreeNode(TKey key, TValue value)
        {
            _key = key;
            _value = value;
       }
        
        public TKey Key
        {
            get { return _key; }
            set { _key = value; }
        }
  
        public TValue Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public TreeNode<TKey, TValue> Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public TreeNode<TKey, TValue> Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public TreeNode<TKey, TValue> Right
        {
            get { return _right; }
            set { _right = value; }
        }
    } // class TreeNode

    /*
    [Serializable]
    struct HeaderEntry
    {
        public byte character;
        public char[] codeword;
    }
     * */
}
