using System;
using System.Collections.Generic;
using System.Text;
using NetworkSimulator.Bll.Utilities;

namespace NetworkSimulator.Bll.Jitterbug
{
    public abstract class JitterBugBase : NetworkFlow
    {
        #region Declarations 

        protected string _binaryMessage;
        protected List<decimal> _interPacketDelaysModWindow;

        //TODO: ***Can this sequence be used like a one-time pad? Or could we use this sequence as the manner to shape the traffic?
        protected List<int> _rotateSequence;
        protected JitterBugInfo _info;

        #endregion

        #region Instantiation and Setup

        protected JitterBugBase(string binaryMessage, List<decimal> ipds, JitterBugInfo info)
            : base(ipds)
        {
            _info = info;
            _binaryMessage = binaryMessage;

            SetupRandomSequence();
        }

        /// <summary>
        /// This method will embed the binary message into the flow
        /// </summary>
        protected void EmbedMessage()
        {
            //We start at index 1 because we do not want to delay the very first packet
            for (int i = 1; i < Packets.Count; i++)
            {
                //Get the current bit of the message
                string bit = Convert.ToString(_binaryMessage[i % _binaryMessage.Length]);

                //Get the current interpacket delay
                decimal interPacketDelay = Packet.GetInterPacketDelay(Packets[i-1], Packets[i]);
                
                int rotateModifier = RotateSequence[i % RotateSequence.Count];

                //Get the additional delay that needs to be added
                decimal delay = DetermineAdditionalDelay(interPacketDelay, bit, rotateModifier);

                //Delay the packet and update the flow
                DelayPacketAndUpdateFlow(i, delay);

                CheckIfCorrect(i, bit, rotateModifier);
            }
        }

        /// <summary>
        /// In this simulation we assume that this random sequence is know by both the sender and the receiver.
        /// The sequence consists of random numbers in the range of 0, _timingWindowMs - 1
        /// </summary>
        private void SetupRandomSequence()
        {
            for (int i = 0; i < _info.RotateSequenceLength; i++)
            {
                RotateSequence.Add(NumberUtilities.RandomGenerator.Next(0, _info.TimingWindowMs - 1));
            }
        }

        #endregion

        #region Public Methods

        public static JitterBugBase Create(string message, List<decimal> ipds, JitterBugType type, List<decimal> trainingIpds, int numBins, JitterBugInfo jBugInfo)
        {
            string binaryMessage = ConvertToBinary(message);

            JitterBugBase jitterBase;

            switch(type)
            {
                case JitterBugType.Rnd:
                    jitterBase = new JitterBugRnd(binaryMessage, ipds, jBugInfo);
                    break;
                case JitterBugType.NonRnd:
                    jitterBase = new JitterBugNonRnd(binaryMessage, ipds, jBugInfo);
                    break;
                case JitterBugType.Shaping:
                    jitterBase = new JitterBugShaping(binaryMessage, ipds, jBugInfo, trainingIpds, numBins);
                    break;
                default:
                    throw new ApplicationException("Unknown Jitterbug type!");
            }

            return jitterBase;
        }

        #endregion

        #region Protected Methods

        protected static string ConvertToBinary(string message)
        {
            Byte[] messageBytes = Encoding.Unicode.GetBytes(message);

            StringBuilder binaryMessage = new StringBuilder();

            for (int i = 0; i < messageBytes.Length; i++)
            {
                string binary = Convert.ToString(messageBytes[i], 2);

                if (binary == "0")
                    binary = "0000000";

                binaryMessage.Append(binary);
            }

            return binaryMessage.ToString();
        }

        /// <summary>
        /// This method will check to see if the packet's ipd correctly encodes the desired bit given a known rotation modifier.
        /// </summary>
        /// <param name="packetIndex"></param>
        /// <param name="bit">The desired bit</param>
        /// <param name="rotateModifier"></param>
        protected void CheckIfCorrect(int packetIndex, string bit, int rotateModifier)
        {
            //The units need to all be in milliseconds

            //Determine the IPD
            decimal ipdMilli = Packet.GetInterPacketDelay(Packets[packetIndex - 1], Packets[packetIndex]) * 1000;
            int sigma = _info.TimingWindowMs / 4;
            decimal dividend = (ipdMilli - rotateModifier);

            if (dividend < 0)
                dividend += _info.TimingWindowMs;

            decimal result = (dividend) % _info.TimingWindowMs;

            string resultBit;

            int windowHalf = _info.TimingWindowMs / 2;

            if ((result >= (windowHalf - sigma)) && (result <= (windowHalf + sigma)))
                resultBit = "1";
            else
                resultBit = "0";

            if (resultBit != bit)
                throw new ApplicationException("The actual encoded bit does not match the intended bit!");
        }

        /// <summary>
        /// This method will take the currently known interPacketDelay and determine the
        /// amount of time, in seconds, that the packet needs to be delayed in order to encode the desired bit.
        /// </summary>
        /// <param name="originalIpd">The current IPD in seconds</param>
        /// <param name="bit">The bit to encode. "1" or "0"</param>
        /// <param name="rotateModifier">A number in the range 0..(TimingWindow-1)</param>
        /// <returns>the amount of time, in seconds, that the packet needs to be delayed in order to encode the desired bit.</returns>
        protected abstract decimal DetermineAdditionalDelay(decimal originalIpd, string bit, int rotateModifier);

        /// <summary>
        /// This method will take a negative dividend and find the smallest equivalent positive dividend
        /// </summary>
        /// <param name="dividend">The dividend of the mod operation. Example dividend % divisor</param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        protected static int MakeDividendPositive(int dividend, int divisor)
        {
            while (dividend < 0)
                dividend += divisor;

            return dividend;
        }

        #endregion

        #region Property Accessors

        /// <summary>
        /// This the size of the TimingWindow in seconds.
        /// </summary>
        public decimal TimingWindowSec
        {
            get
            {
                return (decimal)_info.TimingWindowMs / 1000;
            }
        }

        public List<int> RotateSequence
        {
            get
            {
                if (_rotateSequence == null)
                    _rotateSequence = new List<int>();

                return _rotateSequence;
            }
        }

        public string BinaryMessage
        {
            get { return _binaryMessage; }
        }

        #endregion
    }
}