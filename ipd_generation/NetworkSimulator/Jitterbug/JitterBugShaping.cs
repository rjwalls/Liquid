using System;
using System.Collections.Generic;
using NetworkSimulator.Bll.Tests;

namespace NetworkSimulator.Bll.Jitterbug
{
    internal class JitterBugShaping : JitterBugBase
    {
        #region Declarations

        private List<Bin> _bins;
        private readonly Random _random = new Random();
        private readonly Entropy _entropyClass;

        #endregion

        #region Instantiation and Setup


        public JitterBugShaping(string binaryMessage, List<decimal> ipds, JitterBugInfo info, List<decimal> trainingIpds, int numBins)
            : base(binaryMessage, ipds, info)
        {
            _entropyClass = new Entropy(null, trainingIpds, numBins, 0);
    
            SetupBins(numBins);
            CreateFlow();
        }

        private void SetupBins(int numBins)
        {
            _bins = new List<Bin>();

            //We have to add one since the bins are numbered 1-BIN_SIZE
            for (int i = 0; i < numBins + 1; i++)
            {
                _bins.Add(new Bin(i));
            }
        }

        #endregion

        #region Private Methods

        private void CreateFlow()
        {
            int index = 1;

            while (index < Packets.Count)
            {
                int K = GetK();
                int L = GetL();

                EmbedJBug(index, K);

                //index += K + 1;
                index += K;

                EmbedShaping(index, L);

                //We have to add 1 so that it will skip a packet and not interfere with the JBug flow.
                //index += L + 1;
                index += L;
            }
        }

        private void EmbedJBug(int startIndex, int K)
        {
            //We start at index 1 because we do not want to delay the very first packet
            for (int i = startIndex; (i < K + startIndex) && (i < Packets.Count); i++)
            {
                //Get the current bit of the message
                string bit = Convert.ToString(_binaryMessage[i % _binaryMessage.Length]);

                //Get the current interpacket delay
                decimal interPacketDelay = Packet.GetInterPacketDelay(Packets[i - 1], Packets[i]);

                int rotateModifier = RotateSequence[i % RotateSequence.Count];

                //Get the additional delay that needs to be added
                decimal additionalDelay = (!_info.RoundIpd) ? DetermineAdditionalDelay(interPacketDelay, bit, rotateModifier) : DetermineAdditionalDelay_Rnd(interPacketDelay, bit, rotateModifier);
                
                //Delay the packet and update the flow
                DelayPacketAndUpdateFlow(i, additionalDelay);

                CheckIfCorrect(i, bit, rotateModifier);

                //Get the new ipd
                interPacketDelay = Packet.GetInterPacketDelay(Packets[i - 1], Packets[i]);

                int binIndex = _entropyClass.DetermineBin(interPacketDelay);

                //Add the ipd to the correct bin
                _bins[binIndex].AddValue(interPacketDelay);
            }
        }

        private void EmbedShaping(int startIndex, int L)
        {
            for (int i = startIndex; (i < startIndex + L) && (i < Packets.Count); i++)
            {
                int minPenalty = int.MaxValue;
                int bestBinIndex = int.MaxValue;

                //Get the IPD
                decimal ipd = Packet.GetInterPacketDelay(Packets[i - 1], Packets[i]);

                //Find the best bin to put the IPD into
                for (decimal offset = 0; offset < _info.MaxShapingDelaySeconds; offset += _info.ShapingIncrement)
                {
                    decimal newIpd = ipd + offset;

                    int binIndex = _entropyClass.DetermineBin(newIpd);

                    int newPenalty = (_info.PenaltyForDist * (int)(offset * 1000)) + _info.PenaltyForBin * _bins[binIndex].Count;

                    if (newPenalty < minPenalty || minPenalty == int.MaxValue)
                    {
                        bestBinIndex = binIndex;
                        minPenalty = newPenalty;
                    }
                }

                decimal randIpd = DetermineNewIpd(bestBinIndex, ipd);
                decimal newDelay = randIpd - ipd;

                if (randIpd < ipd)
                    throw new ApplicationException("The new IPD is less than the old ipd! We cannot send the packet back in time.");

                if ((newDelay) > _info.MaxShapingDelaySeconds)
                    throw new ApplicationException("The new IPD is greater than the old ipd by more than the MAX_DELAY");

                int count = _bins[bestBinIndex].Count;

                if(count > 0)
                {
                    
                }

                _bins[bestBinIndex].AddValue(randIpd);
                DelayPacketAndUpdateFlow(i, newDelay);
            }
        }

        /// <summary>
        /// This method will get the next value in the rotating sequence
        /// </summary>
        /// <returns></returns>
        private int GetK()
        {
            return _random.Next(_info.KMin, _info.KMax);
        }

        /// <summary>
        /// This method will get the next value in the rotating sequence
        /// </summary>
        /// <returns></returns>
        private int GetL()
        {
            return _random.Next(_info.LMin, _info.LMax);
        }

        protected override decimal DetermineAdditionalDelay(decimal originalIpd, string bit, int rotateModifier)
        {
            // NOTE:  The mod function is defined as the amount by which a number exceeds the 
            // largest integer multiple of the divisor that is not greater than that number. This is slightly different
            // than how it is implemented in C. Using this definition (-10) % 20 = 10. In c: (-10) % 20 = -10. For Jitterbug,
            // we would like to use the former implementation. For example, Given a current ipd of 0 ms, rotate modifier of 17 ms, desired bit "1",
            // we want to send the packet with an ipd of 7ms ( [7-17] % 20 = 10 ); however, using the c implementation we would end up sending with
            // an ipd of 27 ms.
            int origIpdInMs = (int)Math.Round(originalIpd * 1000);
            int newIpdInMs = origIpdInMs;
            int target;

            //These target values are defined by the Jitterbug paper
            switch (bit)
            {
                case "0":
                    target = 0;
                    break;
                case "1":
                    target = Convert.ToInt32(Math.Floor((decimal)_info.TimingWindowMs / 2));
                    break;
                default:
                    throw new Exception("Unknown bit: " + bit);
            }

            //TODO: Check if the ipdInSec - rotMod is negative. If so, run new logic
            int dividend = (newIpdInMs - rotateModifier);

            //We have to call this method so that the mod function will act in a more appropriate manner. I.e. -10 mod 20 = 10. 
            dividend = MakeDividendPositive(dividend, _info.TimingWindowMs);

            while ((dividend % _info.TimingWindowMs) != target)
            {
                newIpdInMs++;
                dividend = (newIpdInMs - rotateModifier);
                //We have to call this method so that the mod function will act in a more appropriate manner. I.e. -10 mod 20 = 10. 
                dividend = MakeDividendPositive(dividend, _info.TimingWindowMs);
            }

            //We have to divide by 1000 to convert it back to seconds
            decimal additionalDelaySecs = Convert.ToDecimal(newIpdInMs - origIpdInMs) / 1000;

            //This situation may occur due to the rotating sequence numbers example. IPD = 0; bit = 1; rotMod = 17 ==> 27 ms However, It should pick 7ms
            //I am trying to fix this problem. See Note above.
            if (additionalDelaySecs > TimingWindowSec)
                throw new ApplicationException("They JBug delay is too large!");

            return additionalDelaySecs;
        }

        /// <summary>
        /// This method is detectable
        /// </summary>
        /// <param name="interPacketDelay"></param>
        /// <param name="bit"></param>
        /// <param name="rotateModifier"></param>
        /// <returns></returns>
        protected decimal DetermineAdditionalDelay_Rnd(decimal interPacketDelay, string bit, int rotateModifier)
        {
            decimal target, delay = Math.Round(interPacketDelay * 1000);

            switch (bit)
            {
                case "0":
                    target = 0;
                    break;
                case "1":
                    target = Convert.ToInt32(Math.Floor((decimal)_info.TimingWindowMs / 2));
                    break;
                default:
                    throw new Exception("Unknown bit: " + bit);
            }

            while (((delay - rotateModifier) % _info.TimingWindowMs) != target)
            {
                delay++;
            }

            return (delay / 1000 - interPacketDelay);
        }

        #endregion

        #region Protected Methods

        protected decimal DetermineNewIpd(int binIndex, decimal currentIpd)
        {
            int maxValue = (int)(_entropyClass._trainingBins[binIndex].MaxValue * 1000000);
            int minValue = (int)(_entropyClass._trainingBins[binIndex].MinValue * 1000000);

            int currentIpdInt = (int)(currentIpd * 1000000);

            int maxIpdInt = (int)((currentIpd + _info.MaxShapingDelaySeconds) * 1000000);

            ///We cannot send the packet back in time. :)
            if (minValue < currentIpdInt)
                minValue = currentIpdInt;

            if (maxValue > maxIpdInt)
                maxValue = maxIpdInt;

            int nextValue = _random.Next(minValue, maxValue);

            return (decimal)nextValue / 1000000;
        }

        #endregion
    }
}
