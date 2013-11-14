using System;
using System.Collections.Generic;
using System.IO;

namespace NetworkSimulator.Bll.Tests
{
    public sealed class Entropy

    {
        #region Declarations

        public List<Bin> _trainingBins;
        private List<Bin> _bins;
        private readonly List<decimal> _trainingIpds;
        private readonly List<decimal> _ipds;


        /// <summary>
        /// The number of training bins to use when trying to bin the sample data.
        /// </summary>
        private int _numBins;

        private int _sampleSize;


        #endregion

        #region Instantiation and Setup

        public Entropy(List<decimal> ipds, List<decimal> trainingIpds, int numBins, int sampleSize)
        {
            _ipds = ipds;
            _sampleSize = sampleSize;
            _numBins = numBins;
            _trainingIpds = trainingIpds;
            _trainingIpds.Sort();

            SetupBins();
        }

        private void SetupBins()
        {
            _trainingBins = new List<Bin>();
            _bins = new List<Bin>();

            //We have to add one since the bins are numbered 1-BIN_SIZE
            for (int i = 0; i < _numBins + 1; i++)
            {
                _trainingBins.Add(new Bin(i));
                _bins.Add(new Bin(i));
            }
        }

        #endregion

        #region Public Methods

        public List<decimal> DetermineEntropy()
        {
            List<decimal> entropy = new List<decimal>();

            int loopCount = 0;

            while(_ipds.Count > 0)
            {
                SetupBins();
                int numUniquePatterns = 0;

                //Bin the values
                for(int i=0; i < _sampleSize && i < _ipds.Count; i++)
                {
                    int binNumber = DetermineBin(_ipds[i]);

                    File.AppendAllText("bin.txt", string.Format("{0} {1}\n", _ipds[i], binNumber));

                    _bins[binNumber].AddValue(_ipds[i]);
                }

                double sampleEntropy = 0d;
                
                for (int i = 0; i < _bins.Count; i++)
                {
                    double binProb = (double)_bins[i].Count/_sampleSize;

                    if(binProb != 0)
                    {
                        if (_bins[i].Count == 1)
                            numUniquePatterns++;

                        sampleEntropy += -1*binProb*Math.Log(binProb, 2d);   
                    }
                }

                double percentageUnique = ((double) numUniquePatterns)/_sampleSize;
                double EN = sampleEntropy + sampleEntropy*percentageUnique;

                entropy.Add(Convert.ToDecimal(EN));

                //TODO: Remove this
                if(EN < 12d)
                {
                    Console.WriteLine("Starting index = {0}", loopCount * _sampleSize);
                }

                if(_ipds.Count > _sampleSize)
                    _ipds.RemoveRange(0, _sampleSize);
                else
                    _ipds.Clear();

                loopCount++;
            }

            return entropy;
        }

        /// <summary>
        /// This method will bin the current ipd based on the method of equiprobable bins using the training data.
        /// The implementation is based almost entirely on the C code provided by Steven.
        /// </summary>
        /// <param name="ipd">The current interpacket delay in seconds</param>
        /// <returns>The bin number.</returns>
        public int DetermineBin(decimal ipd)
        {
            int min = 0;
            int max = _trainingIpds.Count - 1; //Possibly different
            int i = (min + max) / 2;
            int oldi;
            int binNumber;


            while (true)
            {
                oldi = i;

                if (_trainingIpds[i] > ipd)  //The indexing could be different
                    max = i - 1;
                else if (_trainingIpds[i] < ipd)
                    min = i + 1;
                else
                    break;

                i = (min + max) / 2;

                if (oldi == i)
                    break;
            }

            //Find the first training IPD larger than the current ipd
            if (_trainingIpds[i] <= ipd)
                while (i <= _trainingIpds.Count - 1 && _trainingIpds[i] <= ipd)
                    i++;
            else //Find the first training IPD smaller than the current ipd
                while (i > 0 && _trainingIpds[i] > ipd)
                    i--;


            //if (i == _trainingIpds.Count)
            //    binNumber = _numBins - 1;
            //else
            binNumber = (int)((Convert.ToDouble(i) / _trainingIpds.Count) * _numBins);

            _trainingBins[binNumber].AddValue(ipd);

            return binNumber;
        }

        #endregion 
    }
}
