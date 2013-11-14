using System.Collections.Generic;

namespace NetworkSimulator.Bll
{
    public class NetworkFlow
    {
        private List<Packet> _packets;
        /// <summary>
        /// This is the time at which the very first packet will be sent.
        /// </summary>
        public const int SEND_TIME_START = 0;

        public NetworkFlow(List<decimal> ipds)
        {
            SetupPackets(ipds);
        }

        private void SetupPackets(List<decimal> ipds)
        {
            //TODO: Transform these ipds into a set of packets with correct send times.

            //Create the initial packet 0. Sent at the correct initial start time.
            _packets = new List<Packet> {new Packet {SendTime = SEND_TIME_START}};

            Packet previousPacket = _packets[0];

            foreach (decimal ipd in ipds)
            {
                decimal nextSendTime = previousPacket.SendTime + ipd;
                Packet nextPacket = new Packet{SendTime = nextSendTime};

                _packets.Add(nextPacket);
                previousPacket = nextPacket;
            }
        }

        /// <summary>
        /// Sets the send time of all the packets that end up getting buffered to be equal to the send time of the delayed packet.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="delay"></param>
        protected void DelayPacketAndUpdateFlow(int index, decimal delay)
        {
#if OUTPUT
            Console.WriteLine("Index: {0} Delay: {1}", index, delay);
            Console.WriteLine("Before {0}", _packets[index]);
#endif
            _packets[index].SendTime += delay;

#if OUTPUT
            Console.WriteLine("After {0}", _packets[index]);
#endif

            //Set the send time of all the packets that end up getting buffered to be equal to the
            //send time of the delayed packet.
            for (int i = index+1; i < _packets.Count; i++)
            {
                if(_packets[i].SendTime > _packets[index].SendTime)
                    break;

                _packets[i].SendTime = _packets[index].SendTime;
            }
        }

        public List<decimal> GetIPDs()
        {
            List<decimal> ipds = new List<decimal>();

            for (int i = 1; i < _packets.Count; i++)
            {
                decimal ipd = Packet.GetInterPacketDelay(_packets[i - 1], _packets[i]);

                ipds.Add(ipd);
            }

            return ipds;
        }

        protected List<Packet> Packets
        {
            get { return _packets; }
        }
    }
}