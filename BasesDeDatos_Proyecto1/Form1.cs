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
        String databaseActual;

        public Form1()
        {
            InitializeComponent();
            databaseActual = "";

            this.dataGridView1.SortCompare += new DataGridViewSortCompareEventHandler(this.costum_SortCompare);
        }

        private void costum_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            throw new NotImplementedException();
            try
            {
                if (DBNull.Value.Equals(e.CellValue1) || DBNull.Value.Equals(e.CellValue2))
                {
                    if (DBNull.Value.Equals(e.CellValue1) || e.CellValue1.Equals(null))
                    {
                        e.SortResult = 1;
                    }
                    else if (DBNull.Value.Equals(e.CellValue2) || e.CellValue2.Equals(null))
                    {
                        e.SortResult = -1;
                    }
                }
                else
                {
                    //if (e.CellValue1 is Int32)
                    //{
                    //    e.SortResult = ((Int32)e.CellValue1).CompareTo((Int32)e.CellValue1);
                    //}
                    e.SortResult = (e.CellValue1 as IComparable).CompareTo(e.CellValue2 as IComparable);
                }
                //e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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

                text = text.Replace("\r\n", "\n");
                text = text.Replace("\n", Environment.NewLine);

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
            //Lista de palabras reservadas para cambiarlas a mayusculas
            String[] palabrasReservadas = { "CREATE", "DATABASE", "DATABASES", "ALTER", "RENAME", "TO", "DROP",
                                            "SHOW", "USE", "TABLE", "CONSTRAINT", "PRIMARY", "FOREIGN", "KEY",
                                            "REFERENCES", "ADD", "COLUMN", "COLUMNS", "TABLES", "FROM", "INSERT", "INTO", 
                                            "VALUES", "UPDATE", "SET", "WHERE", "DELETE", "SELECT", "ORDER", "NULL", 
                                            "BY", "ASC", "DESC", "INT", "FLOAT", "DATE", "CHAR", "AND", "OR", "NOT", "CHECK" };

            for (int i = 0; i < palabrasReservadas.Length; i++)
            {
                String palabra = palabrasReservadas[i];
                Regex expRegular = new Regex(@"\b" + palabra + @"\b", RegexOptions.IgnoreCase);
                input = expRegular.Replace(input, palabra);
            }

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
            parser.AddErrorListener(lectorErrores); 
            Antlr4.Runtime.Tree.IParseTree tree = parser.full_query();

            consolaText.Text = lectorErrores.erroresTotal;

            TypeSystem sistemaTipos = new TypeSystem(databaseActual);
            sistemaTipos.resultados = dataGridView1;
                
            if(lectorErrores.noHayError){
                String valorFinal = sistemaTipos.Visit(tree);
                if (!valorFinal.Equals("Error"))
                    consolaText.Text = sistemaTipos.mensajes;
                else
                    consolaText.Text = sistemaTipos.mensajes + sistemaTipos.errores;
            }

            databaseActual = sistemaTipos.BDenUso;
        }
    }
}
