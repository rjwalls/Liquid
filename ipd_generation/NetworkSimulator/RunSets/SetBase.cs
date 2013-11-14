#region Copyright 2009, Tenaska Power Services, Co.
//
// All rights are reserved. Reproduction or transmission in whole or in 
// part, in any form or by any means, electronic, mechanical or otherwise, 
// is prohibited without the prior written consent of the copyright owner.
//
// Filename: SetBase.cs
// Created: 4/15/2009 12:31:33 PM
// Author: TPS\wallsr
//
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetworkSimulator.Bll.Jitterbug;
using NetworkSimulator.Bll.Utilities;

namespace NetworkSimulator.Bll.RunSets
{
    public abstract class SetBase
    {
        #region Declarations

        public string _legitFilePath;
        public string _jBugRndFilePath;
        public string _jBugNonRndFilePath;
        public string _jBugShapingFilePath;

        protected List<decimal> _legitIpds;
        protected List<decimal> _jBugRndIpds;
        protected List<decimal> _jBugNonRndIpds;
        protected List<decimal> _jBugShapingIpds;
        protected readonly List<decimal> _masterSampleIpds;
        protected readonly List<decimal> _trainingIpds;
        protected readonly SetInfo _setInfo;

        #endregion

        #region Instantiation & Setup

        protected SetBase(SetInfo setInfo)
        {
            _setInfo = setInfo;

            //Read the input file IPD discarding all IPDs that are not within the given threshold.
            _masterSampleIpds = FileUtilities.RemoveIpds(setInfo.SampleFilePath, setInfo.IpdThresholdMax, setInfo.IpdThresholdMin);

            //Get the list of training IPDs
            _trainingIpds = FileUtilities.ParseIPDFile(setInfo.TrainingFilePath);
        }

        #endregion

        #region Protected Methods

        protected abstract List<decimal> SampleIpds();

        /// <summary>
        /// This method will create a jitterbug flow using the passing in IPDs as the base.
        /// </summary>
        /// <param name="ipdsToModify">Note, these ipds will be cloned and thus the passed reference will remain unmodified</param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected List<decimal> CreateJitterBugFlow(List<decimal> ipdsToModify, JitterBugType type)
        {
            //This will clone the passed in IPDs so that we do not modify the original list.
            List<decimal> ipds = ipdsToModify.FindAll(r => true);

            List<decimal> jBugIpds = new List<decimal>();

            while (ipds.Count > 0)
            {
                List<decimal> sampleIpds;

                if (ipds.Count > _setInfo.SampleSize)
                {
                    sampleIpds = ipds.GetRange(0, _setInfo.SampleSize);
                    ipds.RemoveRange(0, _setInfo.SampleSize);
                }
                else
                {
                    sampleIpds = ipds.GetRange(0, ipds.Count);
                    ipds.Clear();
                }

                JitterBugBase jBug = JitterBugBase.Create(_setInfo.MessageToEncode, sampleIpds, type, _trainingIpds, _setInfo.NumTrainingBins, _setInfo.JBugInfo);

                jBugIpds.AddRange(jBug.GetIPDs());
            }

            return jBugIpds;
        }

        #endregion

        #region Public Methods

        public void CreateSet()
        {
            _legitIpds = SampleIpds();
            _jBugRndIpds = CreateJitterBugFlow(_legitIpds, JitterBugType.Rnd);
            _jBugNonRndIpds = CreateJitterBugFlow(_legitIpds, JitterBugType.NonRnd);
            _jBugShapingIpds = CreateJitterBugFlow(_legitIpds, JitterBugType.Shaping);

            Write();
        }

        /// <summary>
        /// This method will write the test set to the local file system.
        /// </summary>
        private void Write()
        {
            if (!Directory.Exists(_setInfo.SetPath))
                Directory.CreateDirectory(_setInfo.SetPath);

            Dictionary<string, string> ipdFiles = new Dictionary<string, string>();

            string ipdFileName;
            //Write the files
            ipdFileName = FileUtilities.WriteIPDFile_ForTestSets("JBugRnd", _setInfo.SetPath, _jBugRndIpds);
            ipdFiles.Add(ipdFileName, "JBugRnd");

            ipdFileName = FileUtilities.WriteIPDFile_ForTestSets("JBugNonRnd", _setInfo.SetPath, _jBugNonRndIpds);
            ipdFiles.Add(ipdFileName, "JBugNonRnd");

            ipdFileName = FileUtilities.WriteIPDFile_ForTestSets("JBugShaping", _setInfo.SetPath, _jBugShapingIpds);
            ipdFiles.Add(ipdFileName, "JBugShaping");

            ipdFileName = FileUtilities.WriteIPDFile_ForTestSets("Legit", _setInfo.SetPath, _legitIpds);
            ipdFiles.Add(ipdFileName, "Legit");

            if(_setInfo.CreateDriverScript)
                CreateDriverScript(ipdFiles);

            CreateSetInfoFile();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method is used to write the information used for the set to a file in the set directory.
        /// </summary>
        private void CreateSetInfoFile()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Created on " + DateTime.Now);
            builder.AppendLine(_setInfo.ToString());

            File.AppendAllText(_setInfo.SetPath + @"\SetInfo.txt", builder.ToString());
        }

        private void CreateDriverScript(Dictionary<string, string> ipdFiles)
        {
            if (!Directory.Exists(_setInfo.ScriptPath))
                Directory.CreateDirectory(_setInfo.ScriptPath);

            string filePath = Path.Combine(_setInfo.ScriptPath, "Driver");

            using (FileStream outputStream = File.Create(filePath))
            {
                using (StreamWriter streamWriter = new StreamWriter(outputStream))
                {
                    streamWriter.Write("echo Started at:\n");
                    streamWriter.Write("date\n");

                    foreach (KeyValuePair<string, string> pair in ipdFiles)
                    {
                        streamWriter.Write("echo **********************************************************************\n");
                        streamWriter.Write("echo Starting " + pair.Value + "\n");
                        streamWriter.Write("DELAY_INPUT_FILE=/home/wallsr/Documents/Research/Samples/IPDs/TestSets/" + _setInfo.Name + "/" + pair.Key + "\n");
                        streamWriter.Write("export DELAY_INPUT_FILE\n");
                        streamWriter.Write("echo $DELAY_INPUT_FILE\n");
                        streamWriter.Write(".././Master/Start_End_Tcpdump_Remote\n");
                        streamWriter.Write("echo **********************************************************************\n");
                    }

                    streamWriter.Write("echo Ended at:\n");
                    streamWriter.Write("date\n");
                    streamWriter.Write("NOTIFICATION_EMAIL=2144771937@tmomail.net\n");
                    streamWriter.Write("echo \"=^..^=\" | mail -s \"Experiment Finished\" $NOTIFICATION_EMAIL\n");
                }
            }
        }

        #endregion

        #region System.Object Methods

        #endregion

        #region Operator Overrides

        #endregion

        #region Property Accessors

        #endregion
    }
}
