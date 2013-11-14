using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator.Bll.Tests
{
    /// <summary>
    /// http://www.physics.csbsju.edu/stats/KS-test.html
    /// </summary>
    internal class KsTest
    {
        #region Declarations

        public const int DEFAULT_SAMPLE_SIZE = 2000;
        
        public const decimal STEP = 0.0001m;

        #endregion

        #region Public Static Methods

        public static List<decimal> Test(List<decimal> testSetIpds, List<decimal> trainingIpds)
        {
            return Test(testSetIpds, trainingIpds, DEFAULT_SAMPLE_SIZE);
        }

        private static List<decimal> Test(List<decimal> testSetIpds, List<decimal> trainingIpds, int sampleSize)
        {
            List<decimal> results = new List<decimal>();

            while (testSetIpds.Count > 0)
            {
                List<decimal> sampleIpds;

                if (testSetIpds.Count > sampleSize)
                {
                    sampleIpds = testSetIpds.GetRange(0, sampleSize);
                    testSetIpds.RemoveRange(0, sampleSize);
                }
                else
                {
                    sampleIpds = testSetIpds.GetRange(0, testSetIpds.Count);
                    testSetIpds.Clear();
                }

                decimal result = CalculateKs(sampleIpds, trainingIpds);

                results.Add(result);
            }

            return results;
        }

        private static decimal CalculateKs(List<decimal> testSetIpds, List<decimal> trainingIpds)
        {
            testSetIpds.Sort();
            trainingIpds.Sort();

            decimal min = Math.Min(testSetIpds[0], trainingIpds[0]);
            decimal max = Math.Max(testSetIpds[testSetIpds.Count - 1], trainingIpds[trainingIpds.Count - 1]);

            decimal maxDiff = decimal.MinValue;

            int trainingIndex = 0;
            int testSetIndex = 0;

            decimal cumulativeFrac_testSet;
            decimal cumulativeFrac_training; 

            for (decimal i = min; i <= max; i+= STEP)
            {
                decimal ipd = i;

                if(testSetIndex >= 0)
                    testSetIndex = testSetIpds.FindIndex(testSetIndex, r => r > ipd);

                if (testSetIndex != -1)
                    cumulativeFrac_testSet = (decimal)(testSetIndex) / testSetIpds.Count;
                else
                    cumulativeFrac_testSet = 1m;

                if(trainingIndex >= 0)
                    trainingIndex = trainingIpds.FindIndex(trainingIndex, r => r > ipd);

                if (trainingIndex != -1)
                    cumulativeFrac_training = (decimal)(trainingIndex) / trainingIpds.Count;
                else
                    cumulativeFrac_training = 1m;

                decimal diff = Math.Abs(cumulativeFrac_testSet - cumulativeFrac_training);

                if (diff > maxDiff)
                    maxDiff = diff;
            }

            return maxDiff;
        }

        #endregion
    }
}
