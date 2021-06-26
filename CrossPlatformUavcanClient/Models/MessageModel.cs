using RevolveUavcan.Dsdl.Fields;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CrossPlatformUavcanClient.Models
{
    public class MessageModel
    {
        public uint SubjectId { get; set; }
        public string NameSpace { get; set; }
        public string Name { get; set; }
        public ObservableCollection<UavcanChannelWData> Channels { get; set; }

        public MessageModel(uint subjectId, string name, List<UavcanChannel> channels)
        {
            SubjectId = subjectId;
            Name = name;
            NameSpace = name.Split('.').First();
            Channels = new ObservableCollection<UavcanChannelWData>(channels.Select(x => new UavcanChannelWData(x)));
        }
    }
}
