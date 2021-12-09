using CommandLineParser.Exceptions;
using System;

namespace ScanProtoMsg
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            ScanOption p = new ScanOption();

            try
            {
                parser.ExtractArgumentAttributes(p);
                parser.ParseCommandLine(args);
                //parser.ShowParsedArguments();

            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
            }

            var scanJob = new ScanJob(p);
            scanJob.Do();
        }
    }
}
