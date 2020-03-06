using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidSynthesis
{
    class InvalidUserParametersException:Exception
    {
        public InvalidUserParametersException() { }
        public InvalidUserParametersException(string msg) : base(msg) { }
    }
}
