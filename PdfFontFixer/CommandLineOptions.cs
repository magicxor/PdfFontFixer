using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace PdfFontFixer
{
    class CmdOptions
    {
        [Option('f', "file", HelpText = "Any PDF file.", MutuallyExclusiveSet = "pdf-source")]
        public string File { get; set; }

        [Option('d', "directory", HelpText = "Any directory with PDF files.", MutuallyExclusiveSet = "pdf-source")]
        public string Directory { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}