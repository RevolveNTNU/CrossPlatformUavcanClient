using CrossPlatformUavcanClient.Models;
using ReactiveUI;
using RevolveUavcan.Uavcan.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RevolveUavcan.Communication;

namespace CrossPlatformUavcanClient.ViewModels
{
    public class MessageListViewModel : ViewModelBase
    {
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

        public MessageListViewModel(IUavcanParser parser, IUavcanCommunicationModule commMod)
        {
            uavcanParser = parser;
            uavcanCommunicationModule = commMod;
            Messages = new ObservableCollection<MessageModel>();
            AllMessages = new List<MessageModel>();
            NameSpaces = new ObservableCollection<string>();
            InitMessageList();
            SelectedNameSpace = NameSpaces.First();
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
    }
}
