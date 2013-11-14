using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using NetworkSimulator.Bll.Jitterbug;

namespace NetworkSimulator.Bll.RunSets
{
    /// <summary>
    /// string setName, string sampleFile, string trainingFile, int testSetSize, int sampleSize, decimal ipdMaxThreshold, decimal ipdMinThreshold)
    /// </summary>
    [Serializable]
    public sealed class SetInfo
    {
        #region Public Static Methods

        public static SetInfo ReadXml(string xmlPath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SetInfo));

            using (TextReader reader = new StreamReader(xmlPath))
            {
                return (SetInfo)xmlSerializer.Deserialize(reader);
            }
        }

        #endregion

        #region Operator Overrides

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            PropertyInfo[] properties = GetType().GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                string name = properties[i].Name;
                string value = Convert.ToString(properties[i].GetValue(this, null));

                builder.AppendLine(name + " : " + value);
            }

            return builder.ToString();
        }

        #endregion

        #region Property Accessors

        public string MessageToEncode { get; set; }
        public string Name { get; set; }
        public string SampleFilePath { get; set; }
        public string TrainingFilePath { get; set; }
        public int NumTrainingBins { get; set; }
        public int SetSize { get; set; }
        public int SampleSize { get; set; }
        public decimal IpdThresholdMax { get; set; }
        public decimal IpdThresholdMin { get; set; }
        public string MasterSetDirectory { get; set; }
        public string MasterScriptDirectory { get; set; }
        public string SetPath 
        { 
            get { return Path.Combine(MasterSetDirectory, Name); }
        }
        public string ScriptPath
        {
            get { return Path.Combine(MasterScriptDirectory, Name); }
        }
        public bool UseCorrelated { get; set; }
        public bool CreateDriverScript { get; set; }
        public JitterBugInfo JBugInfo { get; set; }

        #endregion
    }
}