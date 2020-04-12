using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSynthesis
{
    class InvalidUserParametersException:Exception
    {
        public InvalidUserParametersException() { }
        public InvalidUserParametersException(string msg) : base(msg) { }
    }
}
