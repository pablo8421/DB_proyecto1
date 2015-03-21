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

        private void boton_ejecutar_Click(object sender, EventArgs e)
        {
            AntlrInputStream stream = new AntlrInputStream(queryText.Text);
            SqlLexer lexer = new SqlLexer(stream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            SqlParser parser = new SqlParser(tokenStream);
            
            ErrorListener lectorErrores = new ErrorListener();

            lexer.RemoveErrorListeners();
            parser.RemoveErrorListeners();

            parser.AddErrorListener(lectorErrores);

            consolaText.Text = lectorErrores.erroresTotal+ "ALGO";

            //ParseTree tree = parser.full_query();.
            parser.full_query();
            //Aca sistema de tipos :)
        }
    }
}
