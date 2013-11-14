using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NetworkSimulator.Bll.Jitterbug;
using NetworkSimulator.Bll.RunSets;
using NetworkSimulator.Bll.Tests;
using NetworkSimulator.Bll.Utilities;
using System.Linq;

namespace NetworkSimulator.Bll
{
    public static class SimController
    {
        #region Declarations

        private const int NUM_TRAINING_BINS = 65536;
        private const string TRAINING_FILE = @"Sam_TrainingSet_10000000.ipd";
        private const decimal IPD_LOWER_BOUND= 0.000000m; 

        #endregion

        #region Public Static Methods

        public static void CalculateEntropy(string setInfoXmlPath, string ipdFilePath)
        {
            SetInfo setInfo = SetInfo.ReadXml(setInfoXmlPath);
            List<decimal> ipds = FileUtilities.ParseIPDFile(ipdFilePath);
            List<decimal> trainingIpds = FileUtilities.ParseIPDFile(setInfo.TrainingFilePath);

            Entropy en = new Entropy(ipds, trainingIpds, setInfo.NumTrainingBins, setInfo.SampleSize);
            List<decimal> entropy = en.DetermineEntropy();

            string path = ipdFilePath + "_EN.txt";

            for (int i = 0; i < entropy.Count; i++)
            {
                File.AppendAllText(path, Convert.ToString(entropy[i])+ Environment.NewLine);
            }
        }

        public static void CreateSimFiles(string setInfoXmlPath, string setName)
        {
            SetInfo setInfo = SetInfo.ReadXml(setInfoXmlPath);
            setInfo.Name = setName;

            SetBase sampleSet;

            if (setInfo.UseCorrelated)
                sampleSet = new CorrelatedSampleSet(setInfo);
            else
                sampleSet = new UniformSampleSet(setInfo);
            
            sampleSet.CreateSet();
        }

        public static void ParseIPDS_TraceFile(string processDir, string traceFileIdent, string outputFileIdent, string sourceIpIdent, string setInfoXmlPath)
        {
            SetInfo setInfo = SetInfo.ReadXml(setInfoXmlPath);

            string[] subDirs = Directory.GetDirectories(processDir);

            Dictionary<string, string> ipdFiles = new Dictionary<string, string>();

            for (int i = 0; i < subDirs.Length; i++)
            {
                string traceFilePath = Path.Combine(subDirs[i], traceFileIdent);

                if (File.Exists(traceFilePath))
                {
                    List<decimal> ipds = FileUtilities.ParseTraceFile(traceFilePath, sourceIpIdent);

                    string outFilePath = Path.Combine(subDirs[i], outputFileIdent);

                    string filePath = FileUtilities.WriteIPDFile(outFilePath, ipds);

                    File.Delete(traceFilePath);

                    string fileName = Path.GetFileName(filePath);
                    string folderName = Path.GetFileName(subDirs[i]);

                    ipdFiles.Add(folderName, fileName);
                }
            }

            CreateCceScript(processDir, ipdFiles, Path.GetFileName(setInfo.TrainingFilePath));
            CreateCceCleanupScript(processDir, ipdFiles);
        }

        public static List<decimal> ParseUncTraceFile(string traceFile)
        {
            List<FileUtilities.PacketHeaderInfo> headers = FileUtilities.GetTraceFileHeaderInfo(traceFile);

            headers = headers.OrderBy(r => r.LineNumber).ToList();

            headers.RemoveAll(r => r.SourceIP.Contains("ssh"));

            var groupedHeaders = headers.GroupBy(r => string.Format("{0} => {1}", r.SourceIP, r.DestIP));
            
            List<string> sources = new List<string>();

            Dictionary<string, List<FileUtilities.PacketHeaderInfo>> flows = new Dictionary<string, List<FileUtilities.PacketHeaderInfo>>();

            List<decimal> ipds = new List<decimal>();

            foreach (IGrouping<string, FileUtilities.PacketHeaderInfo> infos in groupedHeaders)
            {
                string key = infos.Key;
                var testList = infos.ToList();

                sources.Add(key);
                flows.Add(key, testList);

                for (int i = 1; i < testList.Count; i++)
                {
                    decimal ipd = testList[i].TimeStamp - testList[i - 1].TimeStamp;
                    //Console.WriteLine(ipd);

                    if(ipd > IPD_LOWER_BOUND)
                        ipds.Add(ipd);
                }
            }

            return ipds;
        }

        public static void ParseUncTraceFile(string traceFile, StreamWriter writer)
        {
            List<FileUtilities.PacketHeaderInfo> headers = FileUtilities.GetTraceFileHeaderInfo(traceFile);

            headers = headers.OrderBy(r => r.LineNumber).ToList();

            headers.RemoveAll(r => r.SourceIP.Contains("ssh"));

            var groupedHeaders = headers.GroupBy(r => string.Format("{0} => {1}", r.SourceIP, r.DestIP));

            List<string> sources = new List<string>();

            Dictionary<string, List<FileUtilities.PacketHeaderInfo>> flows = new Dictionary<string, List<FileUtilities.PacketHeaderInfo>>();

            foreach (IGrouping<string, FileUtilities.PacketHeaderInfo> infos in groupedHeaders)
            {
                string key = infos.Key;
                var testList = infos.ToList();

                sources.Add(key);
                flows.Add(key, testList);

                for (int i = 1; i < testList.Count; i++)
                {
                    decimal ipd = testList[i].TimeStamp - testList[i - 1].TimeStamp;
                    //Console.WriteLine(ipd);

                    if (ipd > IPD_LOWER_BOUND)
                    {
                        writer.WriteLine(ipd);
                    }
                }
            }
        }

        public static void TestResults(string resultsDir, string captureIpdFileIdent, string setInfoXmlPath)
        {
            string[] subDirs = Directory.GetDirectories(resultsDir);

            SetInfo setInfo = SetInfo.ReadXml(setInfoXmlPath);

            List<decimal> trainingIpds = FileUtilities.ParseIPDFile(setInfo.TrainingFilePath);

            for (int i = 0; i < subDirs.Length; i++)
            {
                Console.WriteLine("In directory {0}", subDirs[i]);

                string[] files = Directory.GetFiles(subDirs[i], captureIpdFileIdent + "*.ipd");

                for (int j = 0; j < files.Length; j++)
                {
                    Console.WriteLine("Testing file: {0}", files[j]);

                    List<decimal> sampleIpds = FileUtilities.ParseIPDFile(files[j]);

                    Console.WriteLine("testing regularity...");
                    //I have to do "sampleIpds.FindAll(r => true)" so that it will copy the list of IPDs
                    List<decimal> regularityResults = RegularityTest.Test(sampleIpds.FindAll(r => true));

                    Console.WriteLine("testing KS...");
                    List<decimal> ksResults = KsTest.Test(sampleIpds, trainingIpds);
                    
                    WriteResultsFile(files[j], regularityResults, ksResults);
                }
            }
        }

        public static void ParseBinFrequencyFile(string resultsDir, string rawBinFile)
        {
            string[] subDirs = Directory.GetDirectories(resultsDir);

            for (int i = 0; i < subDirs.Length; i++)
            {
                string inputFile = Path.Combine(subDirs[i], rawBinFile);

                if (File.Exists(inputFile))
                {
                    Bin[] bins = FileUtilities.ParseBinFrequencyFile(inputFile, NUM_TRAINING_BINS);

                    string binInfoFile = Path.Combine(subDirs[i], Path.GetFileNameWithoutExtension(rawBinFile) + "_BinInfo.csv");

                    FileUtilities.WriteBinInfoFile(bins, binInfoFile);
                }
            }
        }

        public static void CreateTestScript(string setInfoXmlPath, string processDir)
        {
            SetInfo setInfo = SetInfo.ReadXml(setInfoXmlPath);

            string[] files = Directory.GetFiles(processDir);

            Dictionary<string, string> testSets = new Dictionary<string, string>();

            for (int i = 0; i < files.Length; i++)
            {
                string folderName = Path.GetFileNameWithoutExtension(files[i]);
                string folderPath = Path.Combine(processDir, folderName);

                Directory.CreateDirectory(folderPath);

                File.Move(files[i], Path.Combine(folderPath, Path.GetFileName(files[i])));

                testSets.Add(folderName, Path.GetFileName(files[i]));
            }

            string trainingFileName = Path.GetFileName(setInfo.TrainingFilePath);

            CreateCceScript(processDir, testSets, trainingFileName);
            CreateCceCleanupScript(processDir, testSets);
        }

        #endregion

        #region Private Static Methods

        private static void WriteResultsFile(string sampleFile, List<decimal> regResults, List<decimal> ksResults)
        {
            string resultFile_Reg = sampleFile + "_RegResults.txt";
            string resultFile_Ks = sampleFile + "_KsResults.txt";

            string[] results = new string[regResults.Count];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = regResults[i].ToString();
            }

            File.WriteAllLines(resultFile_Reg, results);

            results = new string[ksResults.Count];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = ksResults[i].ToString();
            }

            File.WriteAllLines(resultFile_Ks, results);
        }

        private static void CreateCceScript(string processDir, Dictionary<string, string> ipdFiles)
        {
            CreateCceScript(processDir, ipdFiles, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDir"></param>
        /// <param name="ipdFiles">Key = folder name; Value = fileName.ipd</param>
        private static void CreateCceScript(string processDir, Dictionary<string, string> ipdFiles, string trainingFileName)
        {
            //Checks if the trainingFileName is null
            string trainingFile = trainingFileName ?? TRAINING_FILE;

            string filePath = Path.Combine(processDir, "cceScript");

            using (FileStream outputStream = File.Create(filePath))
            {
                using (StreamWriter streamWriter = new StreamWriter(outputStream))
                {
                    streamWriter.Write("echo Started at:\n");
                    streamWriter.Write("date\n");

                    foreach (KeyValuePair<string, string> pair in ipdFiles)
                    {
                        //Corrected ENTROPY
                        streamWriter.Write("./cce 65536 1 2000 ");
                        streamWriter.Write(pair.Key + "/" + pair.Value);
                        streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        streamWriter.Write(pair.Key + "/Results_CEN.txt\n");

                        //Corrected ENTROPY
                        //streamWriter.Write("./cce 65536 1 1000 ");
                        //streamWriter.Write(pair.Key + "/" + pair.Value);
                        //streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        //streamWriter.Write(pair.Key + "/Results_CEN_t.txt\n");

                        //Corrected Conditional Entropy
                        streamWriter.Write("./cce 5 50 2000 ");
                        streamWriter.Write(pair.Key + "/" + pair.Value);
                        streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        streamWriter.Write(pair.Key + "/Results_CCE.txt\n");

                        //PERCENTAGE OF SINGULAR PATTERNS
                        streamWriter.Write("./cceSingPat 65536 1 2000 ");
                        streamWriter.Write(pair.Key + "/" + pair.Value);
                        streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        streamWriter.Write(pair.Key + "/Results_EN_SingPat.txt\n");
                        
                        //PERCENTAGE OF SINGULAR PATTERNS
                        //streamWriter.Write("./cceSingPat 65536 1 1000 ");
                        //streamWriter.Write(pair.Key + "/" + pair.Value);
                        //streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        //streamWriter.Write(pair.Key + "/Results_EN_SingPat_t.txt\n");

                        //Entropy H_l
                        streamWriter.Write("./cceH 65536 1 2000 ");
                        streamWriter.Write(pair.Key + "/" + pair.Value);
                        streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        streamWriter.Write(pair.Key + "/Results_H.txt\n");

                        ////Entropy H_t
                        //streamWriter.Write("./cceH 65536 1 1000 ");
                        //streamWriter.Write(pair.Key + "/" + pair.Value);
                        //streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        //streamWriter.Write(pair.Key + "/Results_H_t.txt\n");

                        ////BIN FREQUENCY
                        streamWriter.Write("./cceBinPrintRange 65536 1 2000 ");
                        streamWriter.Write(pair.Key + "/" + pair.Value);
                        streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        streamWriter.Write(pair.Key + "/BinFrequencyEN.txt\n");

                        ////BIN FREQUENCY CCE
                        //streamWriter.Write("./cceBinPrintRange 5 1 2000 ");
                        //streamWriter.Write(pair.Key + "/" + pair.Value);
                        //streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        //streamWriter.Write(pair.Key + "/BinFrequencyCCE.txt\n");

                        ////CCE Pattern Print
                        //streamWriter.Write("./ccePrintPattern 5 50 2000 ");
                        //streamWriter.Write(pair.Key + "/" + pair.Value);
                        //streamWriter.Write(string.Format(" 0 {0} > ", trainingFile));
                        //streamWriter.Write(pair.Key + "/PatternCCE.txt\n");
                    }

                    streamWriter.Write("echo Ended at:\n");
                    streamWriter.Write("date\n");
                    streamWriter.Write("NOTIFICATION_EMAIL=rjwalls@sbcglobal.net\n");
                    streamWriter.Write("echo \"=^..^=\" | mail -s \"CCE Finished\" $NOTIFICATION_EMAIL\n");
                    streamWriter.Write("NOTIFICATION_EMAIL=2144771937@tmomail.net\n");
                    streamWriter.Write("echo \"=^..^=\" | mail -s \"CCE Finished\" $NOTIFICATION_EMAIL\n");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDir"></param>
        /// <param name="ipdFiles">Key = folder name; Value = fileName.ipd</param>
        private static void CreateCceCleanupScript(string processDir, Dictionary<string, string> ipdFiles)
        {
            string filePath = Path.Combine(processDir, "cceCleanUpScript");

            using (FileStream outputStream = File.Create(filePath))
            {
                using (StreamWriter streamWriter = new StreamWriter(outputStream))
                {
                    foreach (KeyValuePair<string, string> pair in ipdFiles)
                    {
                        streamWriter.Write("rm -R " + pair.Key + "\n");
                    }
                }
            }
        }

        #endregion
    }
}