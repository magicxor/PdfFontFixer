using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Pdf.Text.TextOptions;
using NLog;

namespace PdfFontFixer
{
    class FontFixer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public Dictionary<string, string> Substitutes { get; set; }

        public void Fix(string srcFile, string destFile)
        {
            // load existing PDF file
            Document pdfDocument = new Document(srcFile);
            // Search text fragments and set edit option as remove unused fonts
            TextFragmentAbsorber absorber = new TextFragmentAbsorber(new TextEditOptions(TextEditOptions.FontReplace.RemoveUnusedFonts));

            //accept the absorber for all the pages
            pdfDocument.Pages.Accept(absorber);
            // traverse through all the TextFragments
            foreach (TextFragment textFragment in absorber.TextFragments)
            {
                string substituteFontName;
                if (Substitutes.TryGetValue(textFragment.TextState.Font.FontName, out substituteFontName))
                {
                    textFragment.TextState.Font = FontRepository.FindFont(substituteFontName);
                }
                else
                {
                    logger.Error("Substitution font is not found for {0}", textFragment.TextState.Font.FontName);
                }
            }
            // save the updated document
            pdfDocument.Save(destFile);
        }
    }

}