using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkSimulator.Bll
{
    /// <summary>
    /// Note: I do not think that is class has ever been used for anything.
    /// </summary>
    [Obsolete]
    public sealed class Results
    {
        #region Declarations

        private readonly List<string> _resultOutput;
        private DateTime _startTime;
        private DateTime _endTime;
        private readonly string _resultName;
        private readonly List<string> _notes;

        #endregion

        #region Instantiation & Setup

        public Results(string resultName)
        {
            _resultName = resultName;
            _resultOutput = new List<string>();
            _startTime = DateTime.Now;
            _endTime = DateTime.Now;
            _notes = new List<string>();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Result Name: ");
            builder.Append(_resultName);
            builder.Append(Environment.NewLine);
            builder.Append("Start Time: ");
            builder.Append(_startTime);
            builder.Append(Environment.NewLine);
            builder.Append("End Time: ");
            builder.Append(_endTime);
            builder.Append(Environment.NewLine);
            
            builder.Append(Environment.NewLine);
            builder.Append("Notes:");
            builder.Append(Environment.NewLine);

            for (int i = 0; i < _notes.Count; i++)
            {
                builder.Append(_notes[i]);
                builder.Append(Environment.NewLine);
            }

            builder.Append(Environment.NewLine);
            builder.Append("Output:");
            builder.Append(Environment.NewLine);

            for (int i = 0; i < _resultOutput.Count; i++)
            {
                builder.Append(_resultOutput[i]);
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        #endregion

        #region Operator Overrides

        #endregion

        #region Property Accessors

        public List<string> ResultOutput
        {
            get { return _resultOutput; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime EndTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public List<string> Notes
        {
            get { return _notes; }
        }

        #endregion
    }
}