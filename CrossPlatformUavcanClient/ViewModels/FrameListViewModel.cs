using CrossPlatformUavcanClient.Models;
using ReactiveUI;
using RevolveUavcan.Uavcan.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RevolveUavcan.Communication;
using RevolveUavcan.Dsdl.Fields;
using System;

namespace CrossPlatformUavcanClient.ViewModels
{
    public class FrameListViewModel : ViewModelBase
    {
        public enum FRAME_TYPE
        {
            MESSAGE,
            REQUEST,
            RESPONSE
        }


        private ObservableCollection<MessageModel> messages;

        public List<MessageModel> AllMessages { get; set; }
        public ObservableCollection<MessageModel> Messages { get => messages; set => this.RaiseAndSetIfChanged(ref messages, value); }
        public ObservableCollection<string> NameSpaces { get; set; }

        private string selectedNameSpace = "";
        public string SelectedNameSpace
        {
            get
            {
                return selectedNameSpace;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedNameSpace, value);
                Messages = new ObservableCollection<MessageModel>(AllMessages.FindAll(x => x.NameSpace == value).ToList());
            }
        }

        private readonly IUavcanParser uavcanParser;
        private readonly IUavcanCommunicationModule uavcanCommunicationModule;

        public FrameListViewModel(IUavcanParser parser, IUavcanCommunicationModule commMod, FRAME_TYPE frame_type)
        {
            uavcanParser = parser;
            uavcanCommunicationModule = commMod;
            Messages = new ObservableCollection<MessageModel>();
            AllMessages = new List<MessageModel>();
            NameSpaces = new ObservableCollection<string>();
            InitFrameList(frame_type);
            SelectedNameSpace = NameSpaces.First();
        }

        private void InitFrameList(FRAME_TYPE frame_type)
        {
            if (frame_type == FRAME_TYPE.MESSAGE)
            {
                InitMessageList();
            }
            else
            {
                InitServiceList(frame_type == FRAME_TYPE.REQUEST);
            }
        }

        private void InitMessageList()
        {
            foreach (var keyValPair in uavcanParser.UavcanSerializationRulesGenerator.MessageSerializationRules)
            {
                var message = new MessageModel(keyValPair.Key.Item1, keyValPair.Key.Item2, keyValPair.Value.FindAll(x => x.Basetype != RevolveUavcan.Dsdl.Types.BaseType.VOID), uavcanCommunicationModule, uavcanParser);
                AllMessages.Add(message);

                if (!NameSpaces.Contains(message.NameSpace))
                {
                    NameSpaces.Add(message.NameSpace);
                }
            }
        }

        private void InitServiceList(bool requestNotResponse)
        {
            foreach (var keyValPair in uavcanParser.UavcanSerializationRulesGenerator.ServiceSerializationRules)
            {
                var fields = requestNotResponse ? keyValPair.Value.RequestFields : keyValPair.Value.ResponseFields;
                var message = new MessageModel(keyValPair.Key.Item1, keyValPair.Key.Item2, fields.FindAll(x => x.Basetype != RevolveUavcan.Dsdl.Types.BaseType.VOID), uavcanCommunicationModule, uavcanParser);
                AllMessages.Add(message);

                if (!NameSpaces.Contains(message.NameSpace))
                {
                    NameSpaces.Add(message.NameSpace);
                }
            }
        }
    }
}
