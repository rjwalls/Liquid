using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NetworkSimulator.Bll;
using NetworkSimulator.Bll.Tests;
using NetworkSimulator.Bll.Utilities;

namespace TestConsole1
{
    class Program
    {
        static void Main(string[] args)
        {
            AverageIPDs();
            //CalcBER();
            //CleanData();
            //string fileName = @"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\TestSets\052609_02\JBugShaping_052609_200000\Results_EN.txt";
            //decimal percentile = 17.10622739m;

            //List<decimal> results = File.ReadAllLines(fileName).Select(r => Convert.ToDecimal(r)).ToList();

            //var res2 = results.Where(r => r <= percentile).ToList();

            //int count = res2.Count; 

            //decimal truePositive = ((decimal)count) / results.Count;

            //Console.WriteLine(truePositive);

            //Console.ReadKey(true);

            //SetInfo setInfo = new SetInfo
            //                      {
            //                          MessageToEncode = "Robert Walls",
            //                          SampleFilePath = @"C:\test.ipd",
            //                          TrainingFilePath = @"C:\training.ipd",
            //                          NumTrainingBins = 65536,
            //                          SetSize = 200000,
            //                          SampleSize = 2000,
            //                          IpdThresholdMax = 120m,
            //                          IpdThresholdMin = 0.001m,
            //                          MasterSetDirectory =
            //                              @"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\TestSets",
            //                          MasterScriptDirectory =
            //                              @"C:\Documents and Settings\Wallsr\My Documents\Research\Scripts",
            //                          UseCorrelated = true,
            //                          CreateDriverScript = true,
            //                          JBugInfo = new JitterBugInfo()
            //                                         {
            //                                             TimingWindowMs = 20,
            //                                             RotateSequenceLength = 2000,
            //                                             ShapingIncrement = 0.00001m,
            //                                             KMax = 120,
            //                                             KMin = 90,
            //                                             LMax = 120,
            //                                             LMin = 90,
            //                                             PenaltyForBin = 1,
            //                                             PenaltyForDist = 0,
            //                                             MaxShapingDelaySeconds = 0.020m
            //                                         }
            //  };

            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(SetInfo));
            //TextWriter writer = new StreamWriter("SetInfo.xml");
            //xmlSerializer.Serialize(writer, setInfo);

            //string[] files = Directory.GetFiles(@"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\RawTraceFiles\041909");

            ////List<decimal> ipds = new List<decimal>();

            //string path =
            //    @"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\Sampled\Lowerbound\TrainingSet_0s_" + DateTime.Now.ToString("MMddyy");

            //using (StreamWriter writer = new StreamWriter(path, true))
            //{
            //    foreach (string file in files)
            //    {
            //        SimController.ParseUncTraceFile(file, writer);
            //    }
            //}

            //FileUtilities.WriteIPDFile("masterSet_0s", @"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\Sampled\Lowerbound", ipds);
        }

        public static void CleanData()
        {
            string sampleFilePath = @"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\Sampled\Lowerbound\SampleSet_0s_060909.ipd";
            string trainFilePath = @"C:\Documents and Settings\Wallsr\My Documents\Research\Samples\IPDs\Sampled\Lowerbound\TrainingSet_0s_060909.ipd";
            int samSize = 2000;
            int numBin = 65536;

            List<decimal> samIpds = FileUtilities.ParseIPDFile(sampleFilePath);
            List<decimal> trainIpds = FileUtilities.ParseIPDFile(trainFilePath);
            
            Entropy entropy = new Entropy(samIpds, trainIpds, numBin, samSize);
            List<decimal> ce = entropy.DetermineEntropy();
        }

        public static void CalcBER()
        {
            string actFile = @"C:\Documents and Settings\Wallsr\Desktop\BER\actual.ipd";
            string expFile = @"C:\Documents and Settings\Wallsr\Desktop\BER\expected.ipd";
            decimal threshold = 0.005m;

            int errorCount = 0;

            List<decimal> actIpds = FileUtilities.ParseIPDFile(actFile);
            List<decimal> expIpds = FileUtilities.ParseIPDFile(expFile);

            for (int i = 0; i < actIpds.Count && i < actIpds.Count; i++)
            {
                decimal diff = Math.Abs(actIpds[i] - expIpds[i]);

                if(diff >= threshold)
                {
                    errorCount++;
                }
            }
        }

        public static void AverageIPDs()
        {
             string actFile = @"C:\Documents and Settings\Wallsr\Desktop\BER\actual.ipd";
            List<decimal> ipds = FileUtilities.ParseIPDFile(actFile);
            decimal average = ipds.Average();
        }
    }
}
