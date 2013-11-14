

namespace NetworkSimulator.Bll
{
    public class Packet
    {
        /// <summary>
        /// The send time in seconds from the start of the flow
        /// </summary>
        public decimal SendTime;

        /// <summary>
        /// The inter-packet delay in seconds. Packet 2 should have a send time greater
        /// than that of packet 1.
        /// </summary>
        /// <param name="pack1"></param>
        /// <param name="pack2"></param>
        /// <returns></returns>
        public static decimal GetInterPacketDelay(Packet pack1, Packet pack2)
        {
            return pack2.SendTime - pack1.SendTime;
        }

        public override string ToString()
        {
            return string.Format("Sendtime={0}", SendTime);
        }
    }
}