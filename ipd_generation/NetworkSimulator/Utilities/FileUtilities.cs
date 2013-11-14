using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetworkSimulator.Bll.Utilities
{
    public sealed class FileUtilities
    {
        #region Public Static Methods

        public static void Truncate(string inputFile, string outputFile, int count)
        {
            List<decimal> ipds = ParseIPDFile(inputFile);

            List<decimal> ipdsTrun = ipds.GetRange(0, count);

            WriteIPDFile(outputFile, ipdsTrun);
        }

        public static List<decimal> RemoveIpds(string inputFile, decimal maxThreshold, decimal minThreshold)
        {
            return ParseIPDFile(inputFile).FindAll(r => r <= maxThreshold && r >= minThreshold);
        }

        public static void RemoveIpds(string inputFile, string outputFile, decimal maxThreshold, decimal minThreshold)
        {
            List<decimal> ipds = RemoveIpds(inputFile, maxThreshold, minThreshold);

            WriteIPDFile(outputFile, ipds);
        }

        public static void WriteBinInfoFile(Bin[] bins, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile, false))
            {
                for (int i = 1; i < bins.Length; i++)
                {
                    if (bins[i].Count > 0)
                        writer.WriteLine(i + "," + bins[i].Count + "," + bins[i].MinValue + "," + bins[i].MaxValue);
                }
            }
        }

        public static Bin[] ParseBinFrequencyFile(string inputFile, int numBins)
        {
            Bin[] bins = new Bin[numBins + 1];

            for (int i = 0; i < bins.Length; i++)
            {
                bins[i] = new Bin(i);
            }

            string[] lines = File.ReadAllLines(inputFile);

            string[] raw;

            for (int i = 0; i < lines.Length; i++)
            {

                raw = lines[i].Split(' ');

                decimal value = Convert.ToDecimal(raw[0]);
                int index = Convert.ToInt32(raw[1].Split('\0')[0]);

                bins[index].AddValue(value);
            }

            return bins;
        }

        public static string GetFileName(string fileId, int count)
        {
            return fileId + "_" + DateTime.Now.ToString("MMddyy") + "_" + count + ".ipd";
        }

        public static void ConcatFiles(List<string> inputFiles, string outputFile, long numIpds)
        {
            long count = 0;

            using (FileStream outFile = new FileStream(outputFile, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(outFile))
                {
                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        using (StreamReader reader = new StreamReader(inputFiles[i]))
                        {
                            while (!reader.EndOfStream)
                            {
                                if (count >= numIpds)
                                    break;
                                
                                count++;

                                writer.WriteLine(reader.ReadLine());
                            }
                        }
                    }

                }
            }
        }

        public static void ConcatFiles(List<string> inputFiles, string outputFile)
        {
            using (FileStream outFile = new FileStream(outputFile, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(outFile))
                {
                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        using (StreamReader reader = new StreamReader(inputFiles[i]))
                        {
                            while (!reader.EndOfStream)
                            {
                                writer.WriteLine(reader.ReadLine());
                            }
                        }
                    }

                }
            }
        }

        public static void RoundFile(string inputFile, int precision, string outputFile)
        {

            string[] lines = File.ReadAllLines(inputFile);

            for (int i = 0; i < lines.Length; i++)
            {
                decimal ipd = Convert.ToDecimal(lines[i]);

                lines[i] = Math.Round(ipd, precision).ToString();
            }

            File.WriteAllLines(outputFile, lines);
        }

        public static void TestFile(string inputFile)
        {
            string[] lines = File.ReadAllLines(inputFile);

            for (int i = 0; i < lines.Length; i++)
            {
                Convert.ToDecimal(lines[i]);
            }
        }

        /// <summary>
        /// This method will parse a packet timing info file
        /// and return the interpacket delays for those packets.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public static List<decimal> ParseTimingFile(string inputFile)
        {
            string[] lines = File.ReadAllLines(inputFile);
            string[] currentLine;

            List<decimal> packetTimes = new List<decimal>();

            for (int i = 0; i < lines.Length; i++)
            {
                currentLine = lines[i].Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                //Remove the 'E' if it exists
                currentLine = currentLine[0].Split('E');
                packetTimes.Add(Convert.ToDecimal(currentLine[0]));
            }

            List<decimal> ipds = new List<decimal>();

            //We have to use packtimes.Count - 1 so that we do not error out on the last packet
            for (int i = 0; i < packetTimes.Count - 1; i++)
            {
                ipds.Add(packetTimes[i + 1] - packetTimes[i]);
            }

            return ipds;
        }

        public static List<decimal> ParseIPDFile(string inputFile)
        {
            string[] fileLines = File.ReadAllLines(inputFile);
            List<decimal> ipds = new List<decimal>();

            for (int i = 0; i < fileLines.Length; i++)
            {
                ipds.Add(Convert.ToDecimal(fileLines[i]));
            }

            return ipds;
        }

        public static List<decimal> ParsePacketHeaderFile(string inputFile)
        {
            List<PacketHeaderInfo> headers = new List<PacketHeaderInfo>();

            using( FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            {
                using( StreamReader reader = new StreamReader(fileStream))
                {
                    

                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            string line = reader.ReadLine().Trim();
                            string[] lineParts = line.Split(' ');

                            decimal timeStamp = ParseTime(lineParts[0]);
                            string sourceIP = lineParts[2];
                            string destIP = lineParts[4];

                            headers.Add(new PacketHeaderInfo()
                                            {TimeStamp = timeStamp, SourceIP = sourceIP, DestIP = destIP});
                        }
                        catch(Exception ex)
                        {
                            //Do nothing...
                        }
                    }
                }
            }

            Dictionary<string, List<PacketHeaderInfo>> headerDict = new Dictionary<string, List<PacketHeaderInfo>>();
            
            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers[i].SourceIP + "_" + headers[i].DestIP;

                if(!headerDict.ContainsKey(key))
                    headerDict.Add(key, new List<PacketHeaderInfo>());

                headerDict[key].Add(headers[i]);
            }

            List<decimal> ipds = new List<decimal>();

            foreach (var entry in headerDict)
            {
                for (int i = 0; i < entry.Value.Count - 1; i++)
                {
                    //break;
                    decimal ipdCount = entry.Value[i + 1].TimeStamp - entry.Value[i].TimeStamp;
                    ipds.Add(ipdCount);
                }
            }

            return ipds;
        }

        public static List<decimal> ParseTraceFile(string inputFile, string sourceIp)
        {
            //If the set starts running on day 1 and finishes on day 2. The first packet
            //sent on day 2 will have a time stamp < the last packet sent on day 1.
            //There should only be at most one of these. If we get more than one, we have a problem.
            bool negativeIpdFound = false;

            List<PacketHeaderInfo> headers = new List<PacketHeaderInfo>();

            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    int lineNumber = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();

                        try
                        {
                            string[] lineParts = line.Split(' ');

                            decimal timeStamp = ParseTime(lineParts[0]);
                            string sourceIP = lineParts[2];
                            string destIP = lineParts[4];

                            headers.Add(new PacketHeaderInfo() { TimeStamp = timeStamp, SourceIP = sourceIP, DestIP = destIP, LineNumber = lineNumber});
                        }
                        catch (Exception ex)
                        {
                            //Do nothing...
                        }

                        lineNumber++;
                    }
                }
            }

            List<PacketHeaderInfo> headersFiltered = headers.FindAll(r => r.SourceIP.StartsWith(sourceIp));

            //TODO: Sort by line #?
            headersFiltered = headersFiltered.OrderBy(r => r.LineNumber).ToList();

            List<decimal> ipds = new List<decimal>();

            for (int i = 0; i < headersFiltered.Count - 1; i++)
            {
                decimal ipd = headersFiltered[i + 1].TimeStamp - headersFiltered[i].TimeStamp;

                if (ipd < 0 && !negativeIpdFound)
                {
                    negativeIpdFound = true;
                    continue;
                }
                
                if(ipd <0 && negativeIpdFound)
                    throw new ApplicationException("The ipd is negative?! That does not make sense!");

                ipds.Add(ipd);
            }

            return ipds;
        }

        public static List<int> ParseRotSequence(string inputFile)
        {
            string[] fileLines = File.ReadAllLines(inputFile);
            List<int> rotSeg= new List<int>();

            for (int i = 0; i < fileLines.Length; i++)
            {
                rotSeg.Add(Convert.ToInt32(fileLines[i]));
            }

            return rotSeg;
        }

        public static string WriteIPDFile_ForTestSets(string outputFileIdentifier, List<decimal> ipds)
        {
            return WriteIPDFile_ForTestSets(outputFileIdentifier, "", ipds);
        }

        public static string WriteIPDFile_ForTestSets(string outputFileIdentifier, string directory, List<decimal> ipds)
        {
            outputFileIdentifier = GetFileName(outputFileIdentifier, ipds.Count);

            string path = (directory != "") ? Path.Combine(directory, outputFileIdentifier) : outputFileIdentifier;

            using (FileStream outputStream = File.Create(path))
            {
                using (StreamWriter streamWriter = new StreamWriter(outputStream))
                {
                    streamWriter.WriteLine(ipds.Count);

                    for (int i = 0; i < ipds.Count; i++)
                    {
                        streamWriter.Write(ipds[i]);
                        streamWriter.Write('\n');
                        //streamWriter.WriteLine(ipds[i]);
                    }
                }
            }

            return outputFileIdentifier;
        }

        public static string WriteIPDFile(string outputFileIdentifier, List<decimal> ipds)
        {
            return WriteIPDFile(outputFileIdentifier, "", ipds);
        }
        
        /// <summary>
        /// Returns the file name
        /// </summary>
        /// <param name="outputFileIdentifier"></param>
        /// <param name="ipds"></param>
        /// <returns></returns>
        public static string WriteIPDFile(string outputFileIdentifier, string directory, List<decimal> ipds)
        {
            outputFileIdentifier = GetFileName(outputFileIdentifier, ipds.Count);

            string path = (directory != "") ? Path.Combine(directory, outputFileIdentifier) : outputFileIdentifier;

            using (FileStream outputStream = File.Create(path))
            {
                using (StreamWriter streamWriter = new StreamWriter(outputStream))
                {
                    for (int i = 0; i < ipds.Count; i++)
                    {
                        streamWriter.Write(ipds[i]);
                        streamWriter.Write('\n');
                        //streamWriter.WriteLine(ipds[i]);
                    }
                }
            }

            return outputFileIdentifier;
        }

        /// <summary>
        /// This method should return the header information from a txt file that was created by tcpdump
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public static List<PacketHeaderInfo> GetTraceFileHeaderInfo(string inputFile)
        {
            List<PacketHeaderInfo> headers = new List<PacketHeaderInfo>();

            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    int lineNumber = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();

                        try
                        {
                            string[] lineParts = line.Split(' ');

                            decimal timeStamp = ParseTime(lineParts[0]);
                            string sourceIP = lineParts[2];
                            string destIP = lineParts[4];

                            headers.Add(new PacketHeaderInfo() { TimeStamp = timeStamp, SourceIP = sourceIP, DestIP = destIP, LineNumber = lineNumber, Raw = line});
                        }
                        catch (Exception ex)
                        {
                            //Do nothing...
                        }

                        lineNumber++;
                    }
                }
            }

            return headers;
        }

        #endregion

        #region Private Methods

        private static decimal ParseTime(string raw)
        {
            string[] rawSplit = raw.Split(':');
            string hour = rawSplit[0];
            string min = rawSplit[1];

            decimal timeSeconds = Convert.ToDecimal(rawSplit[2]);

            timeSeconds += Convert.ToDecimal(min)*60;
            timeSeconds += Convert.ToDecimal(hour)*60*60;

            return timeSeconds;
        }

        #endregion

        public class PacketHeaderInfo
        {
            public decimal TimeStamp;
            public string SourceIP;
            public string DestIP;
            public int LineNumber;
            public string Raw;

            public override string ToString()
            {
                return string.Format("{0} {1} => {2}", TimeStamp, SourceIP, DestIP);
            }
        }
    }
}