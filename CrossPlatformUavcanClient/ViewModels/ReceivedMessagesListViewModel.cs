using CrossPlatformUavcanClient.Models;
using ReactiveUI;
using RevolveUavcan.Communication.DataPackets;
using RevolveUavcan.Uavcan;
using RevolveUavcan.Uavcan.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace CrossPlatformUavcanClient.ViewModels
{
    public class ReceivedMessagesListViewModel : ViewModelBase
    {
        private SynchronizationContext _syncContext = SynchronizationContext.Current;

        private ObservableCollection<MessageModel> messages;
        public ObservableCollection<MessageModel> Messages { get => messages; set => this.RaiseAndSetIfChanged(ref messages, value); }
        private readonly IUavcanParser uavcanParser;

        public ReceivedMessagesListViewModel(IUavcanParser parser)
        {
            uavcanParser = parser;
            uavcanParser.UavcanMessageParsed += AddReceivedMessageToList;
            messages = new ObservableCollection<MessageModel>();
        }

        public void ClearAllCommand()
        {
            Messages.Clear();
        }

        private void AddReceivedMessageToList(object? sender, UavcanDataPacket e)
        {
            var data = e.ParsedDataDict;
            var fields = new List<UavcanChannelWData>();

            foreach (var keyValPair in data)
            {
                fields.Add(new UavcanChannelWData(keyValPair.Key, keyValPair.Value));
            }

            var name = e.UavcanFrame.IsServiceNotMessage ?
                ((UavcanSerializationRulesGenerator)uavcanParser.UavcanSerializationRulesGenerator).GetServiceNameFromSubjectId(e.UavcanFrame.SubjectId) :
                ((UavcanSerializationRulesGenerator)uavcanParser.UavcanSerializationRulesGenerator).GetMessageNameFromSubjectId(e.UavcanFrame.SubjectId);

            _syncContext.Send(x =>
            {
                Messages.Add(new MessageModel(e.UavcanFrame.SubjectId, name, DateTime.Now, e.UavcanFrame.SourceNodeId, fields));
                this.RaisePropertyChanging(nameof(Messages));
            }, null);
        }
    }
}
