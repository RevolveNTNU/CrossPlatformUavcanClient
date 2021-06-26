using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Dsdl.Types;
using System.Linq;

namespace CrossPlatformUavcanClient.Models
{
    public class UavcanChannelWData : UavcanChannel
    {
        public double Value { get; set; }
        public string ShortName => FieldName.Split('.').Last();
        public UavcanChannelWData(BaseType basetype, int size, string fieldName) : base(basetype, size, fieldName)
        {
            Value = 0;
        }

        public UavcanChannelWData(UavcanChannel channel) : base(channel.Basetype, channel.Size, channel.FieldName)
        {
            Value = 0;
        }
    }
}
