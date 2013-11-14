using System;
using System.Collections.Generic;
using System.Threading;
using NetworkSimulator.Bll;
using NetworkSimulator.Bll.RunSets;
using NetworkSimulator.Bll.Utilities;

namespace NetworkSimulator.Con
{
    class Program
    {
        #region Declarations

        public delegate void CommandDelegate(string[] args);

        private struct Command
        {
            public string Description;
            public CommandDelegate Method;
        }

        private static readonly Dictionary<string, Command> Commands = new Dictionary<string, Command>
                                                                           {
                                                                               { "-k", new Command { Description = "Concat", Method = Concat} },
                                                                               { "-r", new Command { Description = "Round", Method = Round} },
                                                                               { "-p", new Command { Description = "ParseIPDs", Method = ParseIPDS } },
                                                                               { "-pt", new Command { Description = "ParseIPDs for a trace file by source ip", Method = ParseIPDS_TraceFile } },
                                                                               { "-s", new Command { Description = "CreateSimFiles", Method = CreateSimFiles} },
                                                                               { "-pb", new Command { Description = "Parse Bin Frequency File", Method = ParseBinFrequencyFile } },
                                                                               { "-h", new Command { Description = "Prints help information", Method = PrintHelpInformation } },
                                                                               { "-H", new Command { Description = "Prints help information", Method = PrintHelpInformation } },
                                                                               { "?", new Command { Description = "Prints help information", Method = PrintHelpInformation } },
                                                                               { "-re", new Command { Description = "Removes all ipds about the passed threshold", Method = RemoveIpds } },
                                                                               { "-t", new Command { Description = "Truncates the ipd file to only include the first [count]", Method = Truncate } },
                                                                               { "-ct", new Command { Description = "Creates the Test Script", Method = CreateTestScript } },
                                                                               { "-tr", new Command { Description = "Performs the KS-Test and Regularity test on the results", Method = TestResults} },
                                                                               { "-en", new Command { Description = "Entropy", Method = CalculateEntropy} }
                                                                           };

        #endregion

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0 || !Commands.ContainsKey(args[0]))
            {
                Console.WriteLine("Type '-h' for help.");
                return;
            }

#if DEBUG
            Console.WriteLine("Pausing for 10 seconds to allow the attachment of a debugger...");
            Thread.Sleep(10000);
            Console.WriteLine("Starting.");
#endif

            Commands[args[0]].Method.Invoke(args);
        }

        #region Private Static Methods

        private static void TestResults(string[] args)
        {
            string resultsDir, captureIpdFileIdent, setInfoXmlPath;

            try
            {
                setInfoXmlPath = args[1];
                resultsDir = args[2];
                captureIpdFileIdent = args[3];
                
            }
            catch (Exception)
            {
                Console.WriteLine("-tr <setInfoXmlPath> <resultsDirectory> <captureIpdFileIdentifier>");
                return;
            }

            SimController.TestResults(resultsDir, captureIpdFileIdent, setInfoXmlPath);
        }

        private static void CreateTestScript(string[] args)
        {
            string processDir, setInfoXmlPath;

            try
            {
                setInfoXmlPath = args[1];
                processDir = args[2];
            }
            catch (Exception)
            {
                Console.WriteLine("-ct <setInfoXmlPath> <processDir>");
                return;
            }

            SimController.CreateTestScript(setInfoXmlPath, processDir);
        }

        private static void PrintHelpInformation(string[] args)
        {
            foreach (var command in Commands)
            {
                Console.WriteLine("{0}:\t{1}", command.Key, command.Value.Description);
            }
        }

        private static void ParseBinFrequencyFile(string[] args)
        {
            string resultsDir, rawBinFreqFile;

            try
            {
                resultsDir = args[1];
                rawBinFreqFile = args[2];
            }
            catch (Exception)
            {
                Console.WriteLine("-pb <resultsDir> <rawBinFreqFile>");
                return;
            }

            SimController.ParseBinFrequencyFile(resultsDir, rawBinFreqFile);
        }

        private static void CalculateEntropy(string[] args)
        {
            string setInfoXmlPath, ipdFilePath;

            try
            {
                setInfoXmlPath = args[1];
                ipdFilePath = args[2];
            }
            catch
            {
                Console.WriteLine("-en <setInfoXmlPath> <ipdFilePath>");
                return;
            }

            SimController.CalculateEntropy(setInfoXmlPath, ipdFilePath);
        }

        private static void CreateSimFiles(string[] args)
        {
            string setInfoXmlPath, setName;

            try
            {
                setInfoXmlPath = args[1];
                setName = args[2];
            }
            catch
            {
                Console.WriteLine("-s <setInfoXmlPath> <name>");
                return;
            }

            SimController.CreateSimFiles(setInfoXmlPath, setName);
        }

        private static void ParseIPDS_TraceFile(string[] args)
        {
            string processDir, traceFileIdent, outputFileIdent, sourceIpIdent, setInfoXmlPath;

            try
            {
                processDir = args[1];
                traceFileIdent = args[2];
                outputFileIdent = args[3];
                sourceIpIdent = args[4];
                setInfoXmlPath = args[5];
            }
            catch (Exception)
            {
                Console.WriteLine("-pt <processDir> <traceFileIdentifier> <outputFileIdentifier> <sourceIpIdent> <setInfoXmlPath>");
                return;
            }

            SimController.ParseIPDS_TraceFile(processDir, traceFileIdent, outputFileIdent, sourceIpIdent, setInfoXmlPath);
        }

        private static void ParseIPDS(string[] args)
        {
            string type, inputFile, outputFileIdent, sourceIp;

            try
            {
                type = args[1].ToLower();
                inputFile = args[2];
                outputFileIdent = args[3];
                sourceIp = args[4];
            }
            catch (Exception)
            {
                Console.WriteLine("usage: ParseIPDs <type packet/timing/trace> <inputFile> <outputFileIdentifier> <sourceIP>");
                return;
            }

            List<decimal> ipds;

            if (type == "timing")
                ipds = FileUtilities.ParseTimingFile(inputFile);
            else if (type == "trace")
                ipds = FileUtilities.ParseTraceFile(inputFile, sourceIp);
            else
                ipds = FileUtilities.ParsePacketHeaderFile(inputFile);

            FileUtilities.WriteIPDFile(outputFileIdent, ipds);
        }

        private static void Round(string[] args)
        {
            string inputFile, outputFile;
            int precision;

            try
            {
                inputFile = args[0];
                precision = Convert.ToInt32(args[1]);
                outputFile = args[2];
            }
            catch (Exception)
            {
                Console.WriteLine("Round <inputFile> <precision> <outputFile>");
                return;
            }

            FileUtilities.RoundFile(inputFile, precision, outputFile);
        }

        private static void Concat(string[] args)
        {
            string outputFile;
            List<string> inputFiles = new List<string>();
            long numIpds;

            try
            {
                if (args.Length < 3)
                    throw new ApplicationException();

                outputFile = args[0];
                numIpds = Convert.ToInt64(args[1]);

                for (int i = 2; i < args.Length; i++)
                {
                    inputFiles.Add(args[i]);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Concat <outputFile> <numIpds> <inputfile1> ... <inputfileN>");
                return;
            }

            FileUtilities.ConcatFiles(inputFiles, outputFile, numIpds);
        }

        private static void RemoveIpds(string[] args)
        {
            string inputFile;
            string outputFile;
            decimal threshold, minThreshold;

            try
            {
                inputFile = args[1];
                outputFile = args[2];
                threshold = Convert.ToDecimal(args[3]);
                minThreshold = Convert.ToDecimal(args[4]);
            }
            catch (Exception)
            {
                Console.WriteLine("-re <inputFile> <outputFile> <maxThreshold> <minThreshold>");
                return;
            }

            FileUtilities.RemoveIpds(inputFile, outputFile, threshold, minThreshold);
        }

        private static void Truncate(string[] args)
        {
            string inputFile;
            string outputFile;
            int count;

            try
            {
                inputFile = args[1];
                outputFile = args[2];
                count = Convert.ToInt32(args[3]);
            }
            catch (Exception)
            {
                Console.WriteLine("-t <inputFile> <outputFile> <count>");
                return;
            }

            FileUtilities.Truncate(inputFile, outputFile, count);
        }

        #endregion
    }
}