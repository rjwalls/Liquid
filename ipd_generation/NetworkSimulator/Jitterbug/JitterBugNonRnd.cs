using System;
using System.Collections.Generic;

namespace NetworkSimulator.Bll.Jitterbug
{
    internal class JitterBugNonRnd : JitterBugBase
    {
        public JitterBugNonRnd(string binaryMessage, List<decimal> ipds, JitterBugInfo info)
            : base(binaryMessage, ipds, info)
        {
            EmbedMessage();
        }

        protected override decimal DetermineAdditionalDelay(decimal originalIpd, string bit, int rotateModifier)
        {
            //NOTE:  The mod function is defined as the amount by which a number exceeds the 
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
    }
}