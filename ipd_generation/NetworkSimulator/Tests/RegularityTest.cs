using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator.Bll.Tests
{
    internal class RegularityTest
    {
        #region Declarations

        public const int DEFAULT_SAMPLE_SIZE = 2000;
        public const int DEFAULT_WINDOW_SIZE = 100;

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Uses the default sample and window sizes
        /// </summary>
        /// <param name="ipds"></param>
        public static List<decimal> Test(List<decimal> ipds)
        {
            return Test(ipds, DEFAULT_SAMPLE_SIZE, DEFAULT_WINDOW_SIZE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipds"></param>
        /// <param name="sampleSize"></param>
        /// <param name="windowSize"></param>
        public static List<decimal> Test(List<decimal> ipds, int sampleSize, int windowSize)
        {
            List<decimal> results = new List<decimal>();

            while (ipds.Count > 0)
            {
                List<decimal> sampleIpds;

                if (ipds.Count > sampleSize)
                {
                    sampleIpds = ipds.GetRange(0, sampleSize);
                    ipds.RemoveRange(0, sampleSize);
                }
                else
                {
                    sampleIpds = ipds.GetRange(0, ipds.Count);
                    ipds.Clear();
                }

                decimal result = CalculateRegularity(sampleIpds, windowSize);
                
                results.Add(result);
            }

            return results;
        }

        #endregion

        #region Private Static Methods

        private static decimal CalculateRegularity(List<decimal> sampleIpds, int windowSize)
        {
            List<decimal> windowStdDevs = new List<decimal>();

            //Calculate the std dev for all windows
            while (sampleIpds.Count > 0)
            {
                List<decimal> windowIpds;

                if (sampleIpds.Count > windowSize)
                {
                    windowIpds = sampleIpds.GetRange(0, windowSize);
                    sampleIpds.RemoveRange(0, windowSize);
                }
                else
                {
                    windowIpds = sampleIpds.GetRange(0, sampleIpds.Count);
                    sampleIpds.Clear();
                }

                decimal currentWindowStdDev = ComputeStdDev(windowIpds);
                
                windowStdDevs.Add(currentWindowStdDev);
            }

            List<decimal> pairwiseDifs = new List<decimal>();

            //Calculate the pairwise differences
            for (int i = 0; i < windowStdDevs.Count - 1; i++)
            {
                for (int j = i + 1; j < windowStdDevs.Count; j++)
                {
                    decimal iStdDev = windowStdDevs[i];
                    decimal jStdDev = windowStdDevs[j];

                    decimal difference = Math.Abs(iStdDev - jStdDev)/iStdDev;
                    pairwiseDifs.Add(difference);
                }
            }

            if (pairwiseDifs.Count == 0)
                return 0;

            return ComputeStdDev(pairwiseDifs);
        }

        private static decimal ComputeStdDev(List<decimal> values)
        {
            decimal average = values.Average();
            int n = values.Count;

            decimal variance = values.Sum(x => (x - average)*(x - average)/(n - 1));

            double stdDev = Math.Sqrt(Convert.ToDouble(variance));

            return Convert.ToDecimal(stdDev);
        }

        #endregion
    }
}
