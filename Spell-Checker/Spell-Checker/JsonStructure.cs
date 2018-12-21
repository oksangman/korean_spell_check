using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spell_Checker.Response
{
    public class Temp
    {
        public Message message;
    }
    public class Result
    {
        public int errata_count;
        public string origin_html;
        public string html;
        public string notag_html;
    }

    public class Message
    {
        public Result result;
    }
}
