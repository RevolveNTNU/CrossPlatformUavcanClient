using RevolveUavcan.Dsdl.Fields;
using RevolveUavcan.Communication;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RevolveUavcan.Uavcan;
using RevolveUavcan.Uavcan.Interfaces;
using System;

namespace CrossPlatformUavcanClient.Models
{
    public class MessageModel
    {

        private const byte TRANSFER_ID_START = 0;
        private const byte TRANSFER_ID_END = 31;

        public uint SubjectId { get; set; }
        public string NameSpace { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public uint SourceNodeId { get; set; }
        public ObservableCollection<UavcanChannelWData> Channels { get; set; }

        private readonly IUavcanCommunicationModule? uavcanCommunicationModule;
        private readonly IUavcanParser? uavcanParser;
        private byte transferId = 0;

        public MessageModel(uint subjectId, string name, List<UavcanChannel> channels, IUavcanCommunicationModule commMod, IUavcanParser parser)
        {
            SubjectId = subjectId;
            Name = name;
            NameSpace = name.Split('.').First();
            uavcanCommunicationModule = commMod;
            uavcanParser = parser;
            Channels = new ObservableCollection<UavcanChannelWData>(channels.Select(x => new UavcanChannelWData(x)));
        }

        public MessageModel(uint subjectId, string name, DateTime timestamp, uint sourceNodeId, List<UavcanChannelWData> channels)
        {
            SubjectId = subjectId;
            Name = name;
            NameSpace = name.Split('.').First();
            Timestamp = timestamp;
            SourceNodeId = sourceNodeId;
            Channels = new ObservableCollection<UavcanChannelWData>(channels);
        }

        public void SendThisMessageCommand()
        {
            if (uavcanCommunicationModule == null || uavcanParser == null)
            {
                return;
            }

            var uavcanFrame = new UavcanFrame()
            {
                SourceNodeId = SourceNodeId,
                Priority = 1,
                IsCompleted = true,
                ToggleBit = true,
                IsStartOfTransfer = true,
                IsEndOfTransfer = true,
                IsRequestNotResponse = true,
                IsServiceNotMessage = false,
                SubjectId = SubjectId,
                TransferId = transferId
            };


            if (uavcanParser.UavcanSerializationRulesGenerator.TryGetSerializationRuleForMessage(SubjectId, out var uavcanChannels))
            {
                uavcanFrame = UavcanSerializer.SerializeUavcanData(uavcanChannels, Channels.Select(x => x.Value).ToList(), uavcanFrame);

                uavcanCommunicationModule.WriteUavcanFrame(uavcanFrame);

                if (transferId == TRANSFER_ID_END)
                {
                    transferId = TRANSFER_ID_START;
                }
                else
                {
                    transferId++;
                }
            }
        }
    }
}
