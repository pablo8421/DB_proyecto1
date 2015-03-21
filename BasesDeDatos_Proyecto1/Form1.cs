using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Text.RegularExpressions;

namespace BasesDeDatos_Proyecto1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void boton_cargar_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.sql)|*.sql";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = false;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string filePath = choofdlog.FileName;
                StreamReader streamReader = new StreamReader(filePath);
                string text = streamReader.ReadToEnd();
                streamReader.Close();

                text.Replace("\n", Environment.NewLine);

                queryText.Text = text;
            }


        }

        private void boton_guardar_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "All Files (*.sql)|*.sql";
            saveFileDialog1.Title = "Guardar como";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                File.WriteAllText(saveFileDialog1.FileName, queryText.Text);
            }
        }

        private String arreglarCaseSensitive(String input)
        {
            String palabra = "CREATE";

            String regex = ("\b(?i)" + palabra + "(?i-)");
            Regex expRegular = new Regex(@"\b" + palabra + @"\b", RegexOptions.IgnoreCase);
            input = expRegular.Replace(input, palabra);
            Console.WriteLine(input);
            return input;
        }

        private void boton_ejecutar_Click(object sender, EventArgs e)
        {
            AntlrInputStream inputStream = new AntlrInputStream(arreglarCaseSensitive(queryText.Text));

            SqlLexer lexer = new SqlLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            SqlParser parser = new SqlParser(commonTokenStream);

            parser.RemoveErrorListeners();
            ErrorListener lectorErrores = new ErrorListener();
            parser.AddErrorListener(lectorErrores); // add ours

            parser.full_query();

            consolaText.Text = lectorErrores.erroresTotal;


            //Aca sistema de tipos :)
        }
    }
}
