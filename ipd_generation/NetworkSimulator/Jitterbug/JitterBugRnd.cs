using System;
using System.Collections.Generic;

namespace NetworkSimulator.Bll.Jitterbug
{
    internal class JitterBugRnd : JitterBugBase
    {
        public JitterBugRnd(string binaryMessage, List<decimal> ipds, JitterBugInfo info)
            : base(binaryMessage, ipds, info)
        {
            EmbedMessage();
        }

        /// <summary>
        /// This method is detectable
        /// </summary>
        /// <param name="interPacketDelay"></param>
        /// <param name="bit"></param>
        /// <param name="rotateModifier"></param>
        /// <returns></returns>
        protected override decimal DetermineAdditionalDelay(decimal interPacketDelay, string bit, int rotateModifier)
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
    }
}