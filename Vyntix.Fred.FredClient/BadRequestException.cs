using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.Vyntix.Fred.FredClient;
internal class BadRequestException : Exception
{
    public BadRequestException()
    {
        
    }

    public BadRequestException(string msg) : base(msg)
    {
        
    }
}
