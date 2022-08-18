using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Kaenx.Creator.Models.Dynamic
{
    public class DynSeparator : IDynItems, INotifyPropertyChanged
    {
        [JsonIgnore]
        public IDynItems Parent { get; set; }

        private int _id = -1;
        public int Id
        {
            get { return _id; }
            set { _id = value; Changed("Id"); }
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { _name = value; Changed("Name"); }
        }

        private SeparatorHint _hint = SeparatorHint.None;
        public SeparatorHint Hint
        {
            get { return _hint; }
            set { _hint = value; Changed("Hint"); }
        }

        public ObservableCollection<Translation> Text {get;set;} = new ObservableCollection<Translation>();

        private bool _transText = false;
        public bool TranslationText
        {
            get { return _transText; }
            set { _transText = value; Changed("TranslationText"); }
        }
        
        private bool _useTextParam = false;
        public bool UseTextParameter
        {
            get { return _useTextParam; }
            set { _useTextParam = value; Changed("UseTextParameter"); }
        }


        private ParameterRef _textRefObject;
        [JsonIgnore]
        public ParameterRef TextRefObject
        {
            get { return _textRefObject; }
            set { _textRefObject = value; Changed("TextRefObject"); }
        }

        [JsonIgnore]
        public int _textRef;
        public int TextRef
        {
            get { return TextRefObject?.UId ?? -1; }
            set { _textRef = value; }
        }

        public string Cell { get; set; }
        
        public ObservableCollection<IDynItems> Items { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        public object Copy()
        {
            return this.MemberwiseClone();;
        }
    }

    public enum SeparatorHint
    {
        None,
        HorizontalRuler,
        Headline,
        Information,
        Error
    }
}
