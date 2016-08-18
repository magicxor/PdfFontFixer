using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using System.IO;
using System.Web.Script.Serialization;
using NLog;

namespace PdfFontFixer
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void FixSingleFile(string srcFile, FontFixer fontFixer)
        {
            logger.Info("Start processing of the file: {0}", srcFile);
            if (File.Exists(srcFile))
            {
                var backupFileName = srcFile + ".backuppf";
                File.Move(srcFile, backupFileName);
                fontFixer.Fix(backupFileName, srcFile);
                if (File.Exists(srcFile) && (new System.IO.FileInfo(srcFile).Length > 0))
                {
                    File.Delete(backupFileName);
                }
                else
                {
                    logger.Error("Something went wrong. File is not processed: {0}", backupFileName);
                }
            }
            else
            {
                logger.Error("File not exists: {0}", srcFile);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                // patch Aspose.Pdf (v11.4.0-11.5.0 and maybe higer...)
                LicenseHelper.ModifyInMemory.ActivateMemoryPatching();

                // parse command line arguments
                var cmdLineArgs = new CmdOptions();
                var cmdLineParsed = new Parser(new ParserSettings
                {
                    MutuallyExclusive = true,
                    CaseSensitive = true,
                    HelpWriter = Console.Error,
                    IgnoreUnknownArguments = false,
                }).ParseArguments(args, cmdLineArgs) &&
                ((!string.IsNullOrWhiteSpace(cmdLineArgs.File)) ||
                (!string.IsNullOrWhiteSpace(cmdLineArgs.Directory)));

                // load settings
                var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (exeDir == null) { throw new DirectoryNotFoundException("Exe directory not found: {0}"); }
                var settingsPath = Path.Combine(exeDir, "settings.json");
                Dictionary<string, string> settings;
                if (File.Exists(settingsPath))
                {
                    settings =
                        (new JavaScriptSerializer()).Deserialize<Dictionary<string, string>>(
                            File.ReadAllText(settingsPath));
                }
                else
                {
                    throw new FileNotFoundException("Settings not found", settingsPath);
                }

                // do work
                if (cmdLineParsed)
                {
                    var fontFixer = new FontFixer() { Substitutes = settings };

                    if (!string.IsNullOrWhiteSpace(cmdLineArgs.File))
                    {
                        FixSingleFile(cmdLineArgs.File, fontFixer);
                    }
                    else if (!string.IsNullOrWhiteSpace(cmdLineArgs.Directory))
                    {
                        foreach (string file in Directory.EnumerateFiles(cmdLineArgs.Directory, "*.pdf", SearchOption.AllDirectories))
                        {
                            FixSingleFile(file, fontFixer);
                        }
                    }
                }
                else
                {
                    var argsLine = string.Join(" ", args);
                    logger.Error("Wrong arguments: {0}", argsLine);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
