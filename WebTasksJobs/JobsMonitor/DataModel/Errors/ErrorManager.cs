
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Guardian.Menta.JobsPorvider
{
    class ErrorManager
    {

        public ErrorManager()
        {
            
        }
       
        private List<ErrorMessage> errMsgList;
        private object syncRoot = new Object();

        public List<ErrorMessage> ErrMsgList
        {
            get
            {
                if (errMsgList == null)
                {
                    lock (syncRoot)
                    {
                        if (errMsgList == null)
                           errMsgList = GetErrorList();
                    }
                }

                return errMsgList;
            }
        }

        private List<ErrorMessage> GetErrorList()
        {
            
               return new List<ErrorMessage>();
           
        }

        private static string GetErrWebresourceName()
        {
            string result = "";
            result = "new_errormessage";
            return result;
        }
        public static List<ErrorMessage> DeserializeFromXml(string xml)
        {
            List<ErrorMessage> result;
            var ser = new XmlSerializer(typeof(List<ErrorMessage>));
            using (var tr = new StringReader(xml))
            {
                result = (List<ErrorMessage>)ser.Deserialize(tr);
            }
            return result;
        }

       
    }
}
