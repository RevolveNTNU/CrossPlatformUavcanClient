using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using System.Linq;

namespace CrossPlatformUavcanClient.Models
{
    public class UavcanChannelWData
    {
        public double Value { get; set; }
        public UavcanChannel UavcanChannel { get; set; }

        public UavcanChannelWData(UavcanChannel channel)
        {
            UavcanChannel = channel;
            Value = 0;
        }

        public UavcanChannelWData(UavcanChannel channel, double value)
        {
            UavcanChannel = channel;
            Value = value;
        }
    }
}
