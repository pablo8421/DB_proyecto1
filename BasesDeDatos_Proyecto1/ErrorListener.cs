/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que genera los errores sintácticos de las queries
*/
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
        public String erroresTotal { get; set; }    //Variable que almacena errores
        public bool noHayError { get; set; }        //Variable que dice si hay o no errores
        
        //Constructor
        public ErrorListener() : base()
        {
            erroresTotal = "";
            noHayError = true;
        }

        //Método que genera los errores sintácticos
        override
        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            noHayError = false;
            String error = msg + " on line " + line + " character " + charPositionInLine + Environment.NewLine;
            erroresTotal += error;
            System.Console.Error.WriteLine("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}
