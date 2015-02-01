using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GeomertyNetworkWorker
{
    public class CommonJunction
    {
        [XmlElement("eid")]
        public int EID
        {
            get { return _eid; }
            set {_eid = value;}
        }
        
        [XmlElement("classid")]
        public int ClassID
        {
            get { return _classID; }
            set { _classID = value; }
        }

        public CommonJunction()
        {
        }

        public override int GetHashCode()
        {
            return this.EID.GetHashCode();
        }

        public CommonJunction(int EID, int ClassID, string Str)
        {
            
            _eid = EID;
            _Str = Str;
            _classID = ClassID;
        }

        private int _eid = 0;
        private int _classID = 0;
        private string _Str = "";     
    }
}
