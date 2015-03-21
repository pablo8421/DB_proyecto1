using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace BasesDeDatos_Proyecto1
{
    class ErrorListener : IAntlrErrorListener<IToken>
    {
        public String erroresTotal { get; set; }

        public ErrorListener() : base()
        {
            erroresTotal = "";
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            String error = msg + " on line " + line + " character " + charPositionInLine + Environment.NewLine;
            erroresTotal += error;
        }
    }
}
