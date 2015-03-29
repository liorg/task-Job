using Lior.Job.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    [Export(typeof(IDal))]
    public class A : IDal
    {
        public string message;

        public A()
        {
            //  This constructor would not be called. 
            //  Since we use a ImportingConstructor attribute
        }

        [ImportingConstructor]
        public A([Import("Msg")]string str)
        {
            message = str;
        }

        
        public string show()
        {
            return "Welcome" +message;
        }
    }
}
