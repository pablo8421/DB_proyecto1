using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace BasesDeDatos_Proyecto1
{
    class ErrorListener : BaseErrorListener 
    {
        public String erroresTotal { get; set; }

        public ErrorListener() : base()
        {
            erroresTotal = "";
        }

        override
        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            String error = msg + " on line " + line + " character " + charPositionInLine + Environment.NewLine;
            erroresTotal += error;
            System.Console.Error.WriteLine("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}
