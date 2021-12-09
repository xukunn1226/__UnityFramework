using CommandLineParser.Arguments;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScanProtoMsg
{
    public class ScanOption
    {
        [ValueArgument(typeof(string), 's', "src", Description = "Set source folder")]
        public string src;

        [ValueArgument(typeof(string), 'd', "dst", Description = "Set destination folder")]
        public string dst;
    }
}
