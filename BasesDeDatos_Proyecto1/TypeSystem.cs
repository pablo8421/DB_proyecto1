﻿/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que realiza el análisis semántico e implementa las funcionalidades de cada instruccion.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
//COMENTARIO TONTO
namespace BasesDeDatos_Proyecto1
{
    class TypeSystem : SqlBaseVisitor<String>
    {
        public String errores;                      //Variable que almacena los mensajes de error
        public String mensajes;                     //Variable que almacena los mensajes del verbose y éxito
        public DataGridView resultados;             //Variable que representa la tabla para mostrar los datos
        public String BDenUso;                      //Variable que almacena la base de datos que está en uso
        public Boolean hayVerbose;                  //Variable que permite decidir si hacer o no el verbose
        private MasterTabla masterTabla;            //Variable que guarda la data de las tablas de la base de datos en uso
        private MasterBD masterBD;                  //Variable que guarda la data de las bases de datos
        private List<Tabla> ListaTablas;            //Variable que permite el paso de información de datos de tablas entre los visitors
        private List<Object> datosUpdate;           //Variable que permite el paso de información de datos de las tablas a actualizar entre los visitors
        private List<String> columnasUpdate;        //Variable que permite el paso de información de las columnas de las tablas entre los visitors
        private List<FilaTabla> datosTablas;        //Variable que permite el paso de información de los registros de una tabla entre los visitor
        private int cantInserts;                    //Variable que cuenta la cantidad de inserts que se realizan en una ejecución
        private List<String[]> ordenDeColumnas;     //Variable que permite saber como realizar el order by en un select y pasa la información entre los visitors 

        //Constructor
        public TypeSystem() {
            errores = "";
            mensajes = "";
            BDenUso = "";
            resultados = new DataGridView();
            masterTabla = new MasterTabla();
            ListaTablas = new List<Tabla>();
            datosUpdate = null;
            columnasUpdate = null;
            cantInserts = 0;
        }

        //Constructor con una base de datos personalizada
        public TypeSystem(String BDenUso)
        {
            if (BDenUso == null)
            {
                BDenUso = "";
            }
            errores = "";
            mensajes = "";
            this.BDenUso = BDenUso;
            resultados = new DataGridView();
            ListaTablas = new List<Tabla>();
            datosUpdate = null;
            columnasUpdate = null;

            //Deserealizar masterBD
            masterBD = deserializarMasterBD();

            if (!BDenUso.Equals(""))
            {
                //Deserealizar masterTabla
                masterTabla = deserializarMasterTabla();
                cargarDatosTablas();
            }
        }

        //Visita id_completo definido dentro de la gramática
        override
        public string VisitId_completo(SqlParser.Id_completoContext context)
        {
            bool agregado = false;
            if (context.ChildCount == 1)
            {
                //Verificar si el ID esta, de que tabla es y si esta duplicado
                String retorno = "";
                String col = context.GetChild(0).GetText();
                foreach(Tabla tabla in ListaTablas){
                    if (tabla.columnas.Contains(col))
                    {
                        if (!agregado)
                        {
                            retorno = tabla.nombre + "." + col;
                            agregado = true;
                        }
                        else
                        {
                            String tablas = "(" + retorno.Replace("." + col, "") + "," + tabla.nombre + ")";
                            errores += "Error en línea " + context.start.Line +
                                       ": Existe mas de una posible referencia a la columna '" + col +
                                       " " + tablas + "'." + Environment.NewLine;
                            return "";
                        }
                    }
                    else
                    {
                        errores += "Error en línea " + context.start.Line +
                                       ": No existe la columna '" + col +
                                       "' en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                        return "";
                    }
                }
                return retorno;
            }
            else
            {
                //Verificar si el ID esta, de que tabla es y si esta duplicado, para el hijo
                String retornoHijo = Visit(context.GetChild(0));
                if (retornoHijo.Equals(""))
                {
                    return "";
                }

                //Verificar si el ID esta, de que tabla es y si esta duplicado
                String retornoID = "";
                String col = context.GetChild(2).GetText();
                foreach (Tabla tabla in ListaTablas)
                {
                    if (tabla.columnas.Contains(col))
                    {
                        if (!agregado)
                        {
                            retornoID = tabla.nombre + "." + col;
                            agregado = true;
                        }
                        else
                        {
                            String tablas = "(" + retornoID.Replace("." + col, "") + "," + tabla.nombre + ")";
                            errores += "Error en línea " + context.start.Line +
                                       ": Existe mas de una posible referencia a la columna '" + col +
                                       " " + tablas + "'." + Environment.NewLine;
                            return "";
                        }
                    }
                    else
                    {
                        errores += "Error en línea " + context.start.Line +
                                       ": No existe la columna '" + col +
                                       "' en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                        return "";
                    }
                }
                return retornoHijo + "," + retornoID;
            }
            throw new NotImplementedException();
        }

        //Visita asignacion definido dentro de la gramática
        override
        public string VisitAsignacion(SqlParser.AsignacionContext context)
        {
            if (context.ChildCount == 3)
            {
                String tipoValor = "";
                String nColumna = context.GetChild(0).GetText();
                String valor = context.GetChild(2).GetText();
                Tabla tActual = ListaTablas[0];
                if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.STRING)
                {
                    if (isDate(context.STRING().GetText()))
                    {
                        tipoValor = "DATE";
                    }
                    else
                    {
                        tipoValor = "CHAR";
                    }
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.FLOAT)
                {
                    tipoValor = "FLOAT";
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.INT)
                {
                    tipoValor = "INT";
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.NULL)
                {
                    tipoValor = "NULL";
                }
                else
                {
                    throw new NotImplementedException();
                }
                int indice = tActual.columnas.IndexOf(nColumna);
                if (indice == -1) //La columna no existe en la tabla
                {
                    errores += "Error en línea "+context.start.Line+": La columna '"+nColumna+"' no existe en la tabla '"+tActual.nombre+"'."+Environment.NewLine;
                    return "Error";
                }

                String tipoColumna = tActual.tipos_columnas[indice];
                
                //Verificacion de tipos
                if (!(tipoValor.Equals(tipoColumna)
                    || (tipoValor.Equals("NULL"))
                    || (tipoValor.Equals("DATE") && tipoColumna.StartsWith("CHAR"))
                    || (tipoValor.Equals("INT") && tipoColumna.Equals("FLOAT"))
                    || (tipoValor.Equals("FLOAT") && tipoColumna.Equals("INT"))
                    || (tipoValor.StartsWith("CHAR") && tipoColumna.StartsWith("CHAR"))))
                {
                    errores += "Error en línea " + context.start.Line +
                                ": El tipo del valor '" + tipoValor +
                                "' no concuerda con el tipo de la columna '" + tipoColumna +
                                "' (" + tipoValor +
                                "," + tipoColumna +
                                ")." + Environment.NewLine;
                    return "Error";

                }

                if (tipoValor.Equals("DATE") && tipoColumna.StartsWith("CHAR"))
                {
                    tipoValor = "CHAR";
                }

                if (columnasUpdate.Contains(nColumna)) { //Error, no se pueden repetir columnas en la asignacion
                    errores += "Error en línea " + context.start.Line + ": No se puede colocar la columna '" + nColumna + "' más de una vez." + Environment.NewLine;
                    return "Error";
                }

                if (tipoValor.StartsWith("NULL"))
                    datosUpdate.Add(null);
                else if (tipoColumna.StartsWith("CHAR"))
                {
                    int largo = Convert.ToInt32(tipoColumna.Substring(5,tipoColumna.Length-6));
                    //Revisar si no se pasa del tamaño establecido por la columna
                    if ((valor.Length-2) >= largo)
                        valor = valor.Substring(1, largo);
                    else
                        valor = valor.Substring(1, valor.Length - 2);
                    datosUpdate.Add((String)valor);
                }
                else if (tipoColumna.StartsWith("INT"))
                    datosUpdate.Add(Convert.ToInt32(valor));
                else if (tipoColumna.StartsWith("FLOAT"))
                    datosUpdate.Add(Convert.ToSingle(valor));
                else
                {
                    valor = valor.Substring(1, valor.Length - 2);
                    datosUpdate.Add((String)valor);
                }
                columnasUpdate.Add(nColumna);
                return "void";
            }
            else { //Varias asignaciones
                String exp = Visit(context.GetChild(0));
                if (exp.Equals("Error")) { 
                    //Mensaje de error
                    return "Error";
                }

                String tipoValor = "";
                String nColumna = context.GetChild(2).GetText();
                String valor = context.GetChild(4).GetText();
                Tabla tActual = ListaTablas[0];
                if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(4)).symbol.Type == SqlParser.STRING)
                {
                    if (isDate(context.STRING().GetText()))
                    {
                        tipoValor = "DATE";
                    }
                    else
                    {
                        tipoValor = "CHAR";
                    }
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(4)).symbol.Type == SqlParser.FLOAT)
                {
                    tipoValor = "FLOAT";
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(4)).symbol.Type == SqlParser.INT)
                {
                    tipoValor = "INT";
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(4)).symbol.Type == SqlParser.NULL)
                {
                    tipoValor = "NULL";
                }
                else
                {
                    throw new NotImplementedException();
                }
                int indice = tActual.columnas.IndexOf(nColumna);
                if (indice == -1) //La columna no existe en la tabla
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + nColumna + "' no existe en la tabla '" + tActual.nombre + "'." + Environment.NewLine;
                    return "Error";
                }

                String tipoColumna = tActual.tipos_columnas[indice];

                //Verificacion de tipos
                if (!(tipoValor.Equals(tipoColumna)
                    || (tipoValor.Equals("NULL")) 
                    || (tipoValor.Equals("DATE") && tipoColumna.StartsWith("CHAR"))
                    || (tipoValor.Equals("INT") && tipoColumna.Equals("FLOAT"))
                    || (tipoValor.Equals("FLOAT") && tipoColumna.Equals("INT"))
                    || (tipoValor.StartsWith("CHAR") && tipoColumna.StartsWith("CHAR"))))
                {
                    errores += "Error en línea " + context.start.Line +
                                ": El tipo del valor '" + tipoValor +
                                "' no concuerda con el tipo de la columna '" + tipoColumna +
                                "' (" + tipoValor +
                                "," + tipoColumna +
                                ")." + Environment.NewLine;
                    return "Error";

                }

                if (tipoValor.Equals("DATE") && tipoColumna.StartsWith("CHAR"))
                {
                    tipoValor = "CHAR";
                }

                if (columnasUpdate.Contains(nColumna))
                { //Error, no se pueden repetir columnas en la asignacion
                    errores += "Error en línea " + context.start.Line + ": No se puede colocar la columna '" + nColumna + "' más de una vez." + Environment.NewLine;
                    return "Error";
                }
                if (tipoValor.StartsWith("NULL"))
                    datosUpdate.Add(null);
                else if (tipoColumna.StartsWith("CHAR"))
                {
                    valor = valor.Substring(1, valor.Length - 2);
                    datosUpdate.Add((String)valor);
                }
                else if (tipoColumna.StartsWith("INT"))
                    datosUpdate.Add(Convert.ToInt32(valor));
                else if (tipoColumna.StartsWith("FLOAT"))
                    datosUpdate.Add(Convert.ToSingle(valor));
                else
                {
                    valor = valor.Substring(1, valor.Length - 2);
                    datosUpdate.Add((String)valor);
                }
                columnasUpdate.Add(nColumna);
                return "void";
            }
        }

        //Visita tipo definido dentro de la gramática
        override
        public string VisitTipo(SqlParser.TipoContext context)
        {
            String tipo = context.GetText();
            if (tipo.StartsWith("CHAR"))
            {
                int num = Convert.ToInt32(context.GetChild(2).GetText());
                if (num <= 0)
                {
                    return "Error";
                }
            }
            return tipo;
        }

        //Visita multi_exp definido dentro de la gramática
        override
        public string VisitMulti_exp(SqlParser.Multi_expContext context)
        {
            if(context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                //Obtener el primer hijo
                String multi = Visit(context.GetChild(0));
                String tipo_multi = multi.Substring(0,5).Trim();
                multi = multi.Substring(5);
                
                //Obtener el segundo hijo
                String and = Visit(context.GetChild(2));
                String tipo_and = and.Substring(0, 5).Trim();
                and = and.Substring(5);

                if (tipo_and.Equals("BOOL") && tipo_multi.Equals("BOOL"))
                {
                    return "BOOL " + multi + " " + and + " " + "OR";
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Los tipos de los elementos no concuerdan en un 'OR'." + Environment.NewLine;
                    return "ERROR" + multi + " " + and + " " + "OR";
                }
            }
        }

        //Visita select_where definido dentro de la gramática
        override
        public string VisitSelect_where(SqlParser.Select_whereContext context)
        {
            if (context.ChildCount == 0)
            {
                return "";
            }
            else
            {
                String exp = Visit(context.GetChild(1));
                if(exp.StartsWith("BOOL")){
                    return exp.Substring(5);
                }
                else
                {
                    errores += "Error en línea " + context.start.Line +
                               ": El resutado de la expresion no es booleana." + Environment.NewLine;
                    return "Error";
                }
            }
        }

        //Visita id_tablas definido dentro de la gramática
        override
        public string VisitId_tablas(SqlParser.Id_tablasContext context)
        {
            if (context.ChildCount == 1)
            {
                Tabla tabla = masterTabla.getTable(context.GetChild(0).GetText());
                if (tabla != null)
                {
                    ListaTablas.Add(tabla);
                    return "void";
                }
                else
                {
                    errores += "Error en línea " + context.start.Line +
                               ": No se encontro la tabla '" + context.GetChild(0).GetText() +
                               "'." + Environment.NewLine;
                    return "Error";
                }
            }
            else
            {
                String resto = Visit(context.GetChild(0));
                String propio;
                Tabla tabla = masterTabla.getTable(context.GetChild(2).GetText());

                if (tabla != null)
                {
                    ListaTablas.Add(tabla);
                    propio = "void";
                }
                else
                {
                    errores += "Error en línea " + context.start.Line +
                               ": No se encontro la tabla '" + context.GetChild(2).GetText() +
                               "'." + Environment.NewLine;
                    propio = "Error";
                }
                if (propio.Equals("void") && resto.Equals("void"))
                {
                    return "void";
                }
                else
                {
                    return "Error";
                }
            }
        }

        //Visita select_orderBy definido dentro de la gramática
        override
        public string VisitSelect_orderBy(SqlParser.Select_orderByContext context)
        {
            if (context.ChildCount == 3)
            {
                return Visit(context.GetChild(2));
            }
            else
            {
                return "";
            }
        }

        //Realiza el producto cartesiano entre tablas
        private FilaTabla juntarTablas(List<Tabla> listaTablas)
        {
            //Cargar los datos a usar
            List<FilaTabla> datosTabla = new List<FilaTabla>();
            foreach (Tabla tabla in listaTablas)
            {
                FilaTabla nueva = getFilaTabla(tabla);
                datosTabla.Add(nueva);
            }

            //Inicializar listas a usar
            List<String> columnas = new List<String>();
            List<String> tipos_columnas = new List<String>();

            //Obtener columnas y tipos para la tabla de resultados
            foreach (Tabla tabla in listaTablas)
            {
                foreach (String col in tabla.columnas)
                {
                    columnas.Add(tabla.nombre+"."+col);
                }
                foreach (String tipo in tabla.tipos_columnas)
                {
                    tipos_columnas.Add(tipo);
                }
            }
            //Crear tabla
            Tabla tResultado = new Tabla("Resultados", 0, columnas, tipos_columnas, null);

            FilaTabla fResultado = new FilaTabla(tResultado, "");

            //Primera tabla
            foreach(List<Object> fila in datosTabla[0].datos.elementos){
                fResultado.datos.elementos.Add(fila);
            }

            //Para cada tabla a partir de la segunda
            for (int i = 1; i < datosTabla.Count; i++ )
            {
                //Nueva lista de filas que sera el resultado final de este
                //producto cartesiano
                List<List<Object>> nuevaLista = new List<List<Object>>();
                //Obtener datos sobre los cuales hacer el producto cartesiano
                FilaTabla filasActuales = datosTabla[i];
                //Para cada fila
                foreach (List<Object> fila in fResultado.datos.elementos)
                {
                    //Para cada fila en la tabla que se esta trabajando
                    foreach (List<Object> filaActual in filasActuales.datos.elementos)
                    {
                        //Nueva fila, con los datos ya cargados
                        List<Object> nuevo = new List<Object>(fila);

                        //Se agrega cada elemento a una fila nueva
                        nuevo.AddRange(filaActual);

                        //Se agrega la fila nueva al resultado
                        nuevaLista.Add(nuevo);
                    }
                }
                //Se asigna el resultado
                fResultado.datos.elementos = nuevaLista;
            }

            return fResultado;
        }

        //Visita select definido dentro de la gramática. Realiza la instruccion select en base a lo ingresado.
        override
        public string VisitSelect(SqlParser.SelectContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso para poder hacer el select..." + Environment.NewLine;
            }
            //Verificar si hay base de datos en uso
            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay base de datos en uso por lo que no se puede alterar la tabla.";
                return "Error";
            }

            if (hayVerbose)
            {
                mensajes += "Verificando que las tablas del from existan..." + Environment.NewLine;
            }
            //Obtener tablas sobre las cuales se va a trabajar
            ListaTablas = new List<Tabla>();
            //El Visit llena ListaTablas
            if(Visit(context.GetChild(3)).Equals("Error"))
            {
                return "Error";
            }
            if (hayVerbose)
            {
                mensajes += "Obteniendo la expresion del where, si hay o no una..." + Environment.NewLine;
            }
            //Obtener expresion del where
            String postfix = Visit(context.GetChild(4));
            if (postfix.Equals("Error"))
            {
                return "Error";
            }

            //Tabla de resultados
            FilaTabla resultado = juntarTablas(ListaTablas);

            if (!postfix.Equals(""))
            {
                if (hayVerbose)
                {
                    mensajes += "Filtrando los datos segun la condicion del where..." + Environment.NewLine;
                }
                List<List<Object>> datosFiltrados = new List<List<Object>>();
                //Se evalua cada fila de la tabla de resultados
                foreach (List<Object> fila in resultado.datos.elementos)
                {
                    //Si cumple la condicion, se agrega a la lista de datos
                    if (cumpleCondicion(fila, resultado.tabla, postfix))
                    {
                        datosFiltrados.Add(fila);
                    }
                }
                //Se reasigna los datos de la tabla
                resultado.datos.elementos = datosFiltrados;
            }
            //Generar la lista de listas de Objetos a ordenar
            List<object[]> resultadoQuery = new List<object[]>();
            for (int i = 0; i < resultado.datos.elementos.Count; i++)
            {
                object[] fila = resultado.obtenerFila(i);
                resultadoQuery.Add(fila);
            }
            //Obtener el orderBy
            if (hayVerbose)
            {
                mensajes += "Obteniendo el orden pedido, si se pidio..." + Environment.NewLine;
            }
            String columnasAOrdenar = Visit(context.GetChild(5));
            if (columnasAOrdenar.StartsWith("ERROR"))
            {
                return "Error";
            }
            //Se ordenan, si se pidio que se ordenara
            if (!columnasAOrdenar.Equals(""))
            {
                if (hayVerbose)
                {
                    mensajes += "Ordenando los datos..." + Environment.NewLine;
                }
                
                List<String> listaAOrdenar = new List<String>(columnasAOrdenar.Split(','));
                ordenDeColumnas = new List<String[]>();
                foreach (String columna in listaAOrdenar)
                {
                    String[] colActual = { columna.Substring(0, 3)  //Si es orden ASC o DES
                                         , columna.Substring(3)     //El nombre de la columna
                                         , resultado.tabla.columnas.IndexOf(columna.Substring(3)).ToString() //El indice de la columna
                                         , resultado.tabla.tipos_columnas[resultado.tabla.columnas.IndexOf(columna.Substring(3))] }; //El tipo de la columna
                    ordenDeColumnas.Add(colActual);
                }
                QuickSort_Recursive(resultadoQuery, 0, resultadoQuery.Count - 1);
            }

            if (hayVerbose)
            {
                mensajes += "Obteniendo las columnas que se desean mostrar..." + Environment.NewLine;
            }
            String columnasAMostrar = context.GetChild(1).GetText();
            if (!columnasAMostrar.Equals("*"))
                columnasAMostrar = Visit(context.GetChild(1));
            if (columnasAMostrar.Equals("Error"))
                return "Error";

            if (hayVerbose)
            {
                mensajes += "Preparando los datos en un formato para mostrarlos..." + Environment.NewLine;
            }
            //Generar la data a linkear con el DataGridView
            DataTable dt = new DataTable();

            //Llenar las columnas
            for (int i = 0; i < resultado.tabla.columnas.Count; i++)
            {
                dt.Columns.Add(resultado.tabla.columnas[i].Substring(0, resultado.tabla.columnas[i].IndexOf(".")) + " (" + resultado.tabla.columnas[i].Substring(resultado.tabla.columnas[i].IndexOf(".") + 1) + ")" + " [" + resultado.tabla.tipos_columnas[i] + "]");
            }

            //Llenar los datos
            foreach (object[] fila in resultadoQuery)
            {
                dt.Rows.Add(fila);
            }
            if (!columnasAMostrar.Equals("*"))
            {
                List<String> colMostrar = new List<string>(columnasAMostrar.Split(','));
                for (int i = 0; i < resultado.tabla.columnas.Count; i++)
                {
                    String c = resultado.tabla.columnas[i];
                    if (!colMostrar.Contains(c))
                    {
                        //Hacer algo para no mostrarla
                        resultados.Columns[i].Visible = false;
                    }
                }
            }

            //Preaprar el datagridview
            resultados.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.None);
            resultados.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            resultados.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            resultados.RowHeadersVisible = false;
            resultados.AllowUserToAddRows = false;
            DataView dataView = new DataView(dt);
            
            //Hacer el binding de datos
            resultados.DataSource = dataView;
            //Hacerlo notSortable
            for (int i = 0; i < resultados.ColumnCount; i++)
            {
                resultados.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            
            int cant = dt.Rows.Count;
            if (cant < 0)
                cant = 0;
            resultados.RowHeadersVisible = true;
            mensajes += "Se ha realizado select con exito, retornó " + cant + " valores."+ Environment.NewLine; 
            return "void";
        }

        //Visita valor_completo definido dentro de la gramática.
        override
        public string VisitValor_completo(SqlParser.Valor_completoContext context)
        {
            if (context.ChildCount == 1)
            {
                if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(0)).symbol.Type == SqlParser.STRING)
                {
                    if( isDate(context.STRING().GetText()))
                    {
                        return "DATE " + context.GetChild(0).GetText();
                    }
                    else
                    {
                        return "CHAR " + context.GetChild(0).GetText();
                    }
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(0)).symbol.Type == SqlParser.FLOAT)
                {
                    return "FLOAT" + context.GetChild(0).GetText();
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(0)).symbol.Type == SqlParser.INT)
                {
                    return "INT  " + context.GetChild(0).GetText();
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(0)).symbol.Type == SqlParser.NULL)
                {
                    return "NULL " + context.GetChild(0).GetText();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                String propio;
                if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.STRING)
                {
                    if (isDate(context.GetChild(2).GetText()))
                    {
                        propio = "DATE " + context.GetChild(2).GetText();
                    }
                    else
                    {
                        propio = "CHAR " + context.GetChild(2).GetText();
                    }
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.FLOAT)
                {
                    propio = "FLOAT" + context.GetChild(2).GetText();
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.INT)
                {
                    propio = "INT  " + context.GetChild(2).GetText();
                }
                else if (((Antlr4.Runtime.Tree.TerminalNodeImpl)context.GetChild(2)).symbol.Type == SqlParser.NULL)
                {
                    propio =  "NULL " + context.GetChild(2).GetText();
                }
                else
                {
                    throw new NotImplementedException();
                }
                
                return Visit(context.GetChild(0)) + "," + propio;
            }
            
        }

        //Visita accion_rename definido dentro de la gramática. Renombra una tabla.
        override
        public string VisitAccion_rename(SqlParser.Accion_renameContext context)
        {
            String nombre = ListaTablas.ElementAt(0).nombre;
            String nuevoNombre = context.GetChild(2).GetText();

            if (hayVerbose)
            {
                mensajes += "Verificando si no existe una base da datos con el mismo nombre..." + Environment.NewLine;
            }
            if (masterTabla.containsTable(nuevoNombre)) {
                errores += "Error en linea " + context.start.Line + ": Ya existe una tabla con el nombre '" + nuevoNombre + "' por lo que no se le puede cambiar de nombre a la tabla '" + nombre + "'.\r\n";
                return "Error";
            }

            foreach (Tabla t in masterTabla.tablas)
            {
                if (t.nombre.Equals(nombre))
                {
                    t.nombre = nuevoNombre;
                    if (hayVerbose)
                    {
                        mensajes += "Creando los archivos..." + Environment.NewLine;
                    }
                    String pathViejo = "Databases\\" + BDenUso + "\\" + nombre + ".dat";
                    String pathNuevo = "Databases\\" + BDenUso + "\\" + nuevoNombre + ".dat";
                    System.IO.File.Move(pathViejo, pathNuevo);

                    break;
                }
            }

            foreach (Tabla t in masterTabla.tablas)
            {
                foreach (Restriccion r in t.restricciones)
                {
                    if (r.tabla.Equals(nombre))
                        r.tabla = nuevoNombre;
                }   
            }

            mensajes += "Se ha renombrado la tabla '" + nombre + "' a '" + nuevoNombre + "' con éxito.\r\n";

            return "void";
        }

        //Visita botar_bd definido dentro de la gramática. Elimina una base de datos.
        override
        public string VisitBotar_BD(SqlParser.Botar_BDContext context)
        {
            String nombre = context.GetChild(2).GetText();
            if (hayVerbose)
            {
                mensajes += "Verificando que la base de datos a eliminar exista..." + Environment.NewLine;
            }
            if (masterBD.containsBD(nombre)) 
            {
                DialogResult resultado = MessageBox.Show("Seguro que desea botar " + nombre + " con " + masterBD.getRegistros(nombre) + " registros", "Confirmación", MessageBoxButtons.YesNo);

                if (resultado == DialogResult.Yes)
                {
                    masterBD.borrarBD(nombre);

                    if (nombre.Equals(BDenUso))
                        BDenUso = "";

                    //Aqui se serializaba masterBD

                    String path = "Databases\\" + nombre;
                    System.IO.Directory.Delete(path, true);
                    mensajes += "Se ha borrado la base de datos '" + nombre + "' con éxito.\r\n";
                }
                else
                {
                    mensajes += "No se ha borrado la base de datos '" + nombre + "' con éxito."+Environment.NewLine;
                }
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' no existe en el DBMS.\r\n";
                return "Error";
            }
        }

        //Método encargado de verificar ciertas restricciones de una PK
        private bool verificarRestriccionPK(FilaTabla datos, int nLinea)
        {
            foreach (Restriccion restriccion in datos.tabla.restricciones)
            {
                //Si es llave primaria
                if (restriccion.tipo.Equals("PK"))
                {
                    //Obtener los indices de la llave primaria
                    List<int> indices = new List<int>();
                    foreach (String columna in restriccion.columnasPropias)
                    {
                        indices.Add(datos.tabla.columnas.IndexOf(columna));
                    }
                    //Revisar para cada fila en la tabla
                    for (int num = 0; num < datos.datos.elementos.Count; num++)
                    {
                        for (int num2 = num + 1; num2 < datos.datos.elementos.Count; num2++)
                        {
                            Object[] fila = datos.obtenerFila(num);
                            Object[] row = datos.obtenerFila(num2);
                            bool yaExistePK = true;
                            int i = 0;
                            //Mientras los datos sean los mismos, se siguen evaluando los datos
                            //El ciclo para cuando se analizan todos o se encuentra uno distinto
                            while (yaExistePK && i < indices.Count)
                            {
                                Object enTabla = fila[indices[i]];
                                Object porAgregar = row[indices[i]];

                                if (porAgregar == null)
                                {
                                    errores += "Error en la línea " + nLinea +
                                               ": La llave primaria '" +
                                               restriccion.nombre + "' no puede contener valores 'NULL'.\r\n";
                                    return false;
                                }

                                if (datos.tabla.tipos_columnas[indices[i]].Equals("INT"))
                                {
                                    if (!(((Int32)enTabla)
                                 .Equals(((Int32)porAgregar))))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                else if (datos.tabla.tipos_columnas[indices[i]].Equals("FLOAT"))
                                {
                                    if (!(compareSingle(((Single)enTabla),
                                 ((Single)porAgregar))==0))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                else if (datos.tabla.tipos_columnas[indices[i]].Equals("DATE"))
                                {
                                    if (!(((String)enTabla)
                                 .Equals(((String)porAgregar))))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                else if (datos.tabla.tipos_columnas[indices[i]].StartsWith("CHAR"))
                                {
                                    if (!(((String)enTabla)
                                 .Equals(((String)porAgregar))))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                i++;
                            }
                            //Si ya existe la llave Primaria, no se puede agregar
                            if (yaExistePK)
                            {
                                errores += "Error en la línea " + nLinea + ": Llave duplicada viola restricción de unicidad '" + restriccion.nombre + "'.\r\n";
                                return false;
                            }
                        }
                    }

                }
            }
            return true;
        }

        //Método encargado de verificar restricciones de PK, FK, CHECK
        private bool verificarRestricciones(FilaTabla datos, List<Object> row, int nLinea, int indiceFila)
        {
            //Por cada restriccion
            foreach (Restriccion restriccion in datos.tabla.restricciones)
            {
                //Si es llave primaria
                if (restriccion.tipo.Equals("PK"))
                {
                    if (indiceFila == -1)
                    {
                        //Obtener los indices de la llave primaria
                        List<int> indices = new List<int>();
                        foreach (String columna in restriccion.columnasPropias)
                        {
                            indices.Add(datos.tabla.columnas.IndexOf(columna));
                        }
                        //Revisar para cada fila en la tabla
                        for (int num = 0; num < datos.datos.elementos.Count; num++)
                        {
                            Object[] fila = datos.obtenerFila(num);
                            bool yaExistePK = true;
                            int i = 0;
                            //Mientras los datos sean los mismos, se siguen evaluando los datos
                            //El ciclo para cuando se analizan todos o se encuentra uno distinto
                            while (yaExistePK && i < indices.Count)
                            {
                                Object enTabla = fila[indices[i]];
                                Object porAgregar = (Object)row[indices[i]];

                                if (porAgregar == null)
                                {
                                    errores += "Error en la línea " + nLinea +
                                               ": La llave primaria '" +
                                               restriccion.nombre + "' no puede contener valores 'NULL'.\r\n";
                                    return false;
                                }

                                if (datos.tabla.tipos_columnas[indices[i]].Equals("INT"))
                                {
                                    if (!(((Int32)enTabla)
                                 .Equals(((Int32)porAgregar))))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                else if (datos.tabla.tipos_columnas[indices[i]].Equals("FLOAT"))
                                {
                                    if (!(compareSingle(((Single)enTabla),
                                 ((Single)porAgregar)) == 0))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                else if (datos.tabla.tipos_columnas[indices[i]].Equals("DATE"))
                                {
                                    if (!(((String)enTabla)
                                 .Equals(((String)porAgregar))))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                else if (datos.tabla.tipos_columnas[indices[i]].StartsWith("CHAR"))
                                {
                                    if (!(((String)enTabla)
                                 .Equals(((String)porAgregar))))
                                    {
                                        yaExistePK = false;
                                    }
                                }
                                i++;
                            }
                            //Si ya existe la llave Primaria, no se puede agregar
                            if (yaExistePK)
                            {
                                errores += "Error en la línea " + nLinea + ": Llave duplicada viola restricción de unicidad '" + restriccion.nombre + "'.\r\n";
                                return false;
                            }

                        }
                    }
                }
                //Si es llave foranea
                else if (restriccion.tipo.Equals("FK"))
                {
                    String nTablaF = restriccion.tabla;
                    Tabla tForanea = masterTabla.getTable(nTablaF);
                    if (tForanea == null)
                    {
                        errores += "Error en línea " + nLinea + ": La tabla '" + restriccion.tabla + "' no existe en la base de datos '" + BDenUso + "'.\r\n";
                        return false;
                    }
                    FilaTabla fTabla = getFilaTabla(tForanea);

                    for(int cindex = 0; cindex<restriccion.columnasPropias.Count;cindex++){
                        bool banderaExiste = false;
                        String cP = restriccion.columnasPropias.ElementAt(cindex);
                        String cF = restriccion.columnasForaneas.ElementAt(cindex);
                        int index1 = fTabla.tabla.columnas.IndexOf(cF);
                        int index2 = datos.tabla.columnas.IndexOf(cP);
                        for (int j = 0; j < fTabla.getTamanio(); j++) {
                            if (row.ElementAt(index2) == null || fTabla.getRowElement(j, index1).Equals(row.ElementAt(index2)))
                            {
                                banderaExiste = true;
                                break;
                            }
                        }
                        if (!banderaExiste)
                        {
                            errores += "Error en línea " + nLinea + ": Inserción en la tabla '"+restriccion.tabla+"' viola la llave foránea '"+restriccion.nombre+"'.\r\n";
                            return false;
                        }
                    }
                }
                //Si es Check
                else if (restriccion.tipo.Equals("CH"))
                {
                    Stack<String> stack = new Stack<String>();
                    List<String> expresiones = new List<String>(Regex.Split(restriccion.restriccionCheck, " (?=(?:[^']*'[^']*')*[^']*$)"));
                    foreach (String e in expresiones)
                    {
                        if (e.Equals("OR"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();

                            if (uno.Equals("FALSE ") && dos.Equals("FALSE "))
                            {
                                stack.Push("FALSE ");
                            }
                            else
                            {
                                stack.Push("TRUE ");
                            }
                        }
                        else if (e.Equals("AND"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();

                            if (uno.Equals("TRUE ") && dos.Equals("TRUE "))
                            {
                                stack.Push("TRUE ");
                            }
                            else
                            {
                                stack.Push("FALSE ");
                            }
                        }
                        else if (e.Equals("<>"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                datoUno = row[indexUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                datoDos = row[indexDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                                  && datoDos is Int32)
                            {
                                if(((Int32) datoUno).Equals(((Int32) datoDos))){
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos)))==0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) == 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) == 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).Equals(((String)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                        }
                        else if (e.Equals("="))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                datoUno = row[indexUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                datoDos = row[indexDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                                  && datoDos is Int32)
                            {
                                if (!((Int32)datoUno).Equals(((Int32)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno), (((Single)datoDos))) != 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) != 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) != 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else
                            {
                                if (!((String)datoUno).Equals(((String)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                        }
                        else if (e.Equals(">="))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                datoUno = row[indexUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                datoDos = row[indexDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                                  && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {

                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals("<="))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                datoUno = row[indexUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                datoDos = row[indexDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                                  && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals(">"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                datoUno = row[indexUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                datoDos = row[indexDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                                  && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals("<"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                datoUno = row[indexUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                datoDos = row[indexDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                                  && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals("NOT"))
                        {
                            String uno = stack.Pop();
                            if (uno.Equals("TRUE "))
                            {
                                stack.Push("FALSE ");
                            }
                            else
                            {
                                stack.Push("TRUE ");
                            }
                        }
                        //Es el nombre de la columna o un dato
                        else
                        {
                            if (e.Equals("NULL"))
                            {
                                stack.Push("NULL ");
                            }
                            else
                            {
                                stack.Push(e);
                            }
                        }
                    }
                    if (stack.Pop().Equals("FALSE "))
                    {
                        errores += "Error en línea " + nLinea + ": Inserción en la tabla '" + datos.tabla.nombre + "' viola la revisión '" + restriccion.nombre + "'." + Environment.NewLine;
                        return false;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return true;
        }

        //Visita insert definido dentro de la gramática. Realiza la funcionalidad del insert según lo ingresado.
        override
        public string VisitInsert(SqlParser.InsertContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso para poder hacer la insercesion..." + Environment.NewLine;
            }

            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay base de datos en uso por lo que no se puede alterar la tabla.";
                return "Error";
            }
            
            //Obtener el nombre de la tabla que se desea
            String nombre = context.GetChild(2).GetText();
            //Si id_completo de columnas
            if (context.ChildCount == 7)
            {
                Tabla tabla = masterTabla.getTable(nombre);
                if (hayVerbose)
                {
                    mensajes += "Verificando que exista la tabla sobre la cual hacer la insercion..." + Environment.NewLine;
                }

                //Verificar que exista la tabla
                if (tabla == null)
                {
                    errores += "Error en línea " + context.start.Line +
                              ": La tabla '" + nombre + 
                              "' no existe en la base de datos '" + BDenUso + 
                              "'." + Environment.NewLine;
                    return "Error";
                }

                if (hayVerbose)
                {
                    mensajes += "Analizando si los datos ingresados cumplen con el formato de la tabla..." + Environment.NewLine;
                }

                //Obtener valores a insertar 
                List<String> valores= new List<String>(Regex.Split(Visit(context.GetChild(5)), ",(?=(?:[^']*'[^']*')*[^']*$)"));
                if (valores.Count > tabla.columnas.Count)
                {
                    errores += "Error en línea " + context.start.Line +
                              ": La cantidad de columnas en la tabla '" + tabla.nombre +
                              "' no concuerda con la cantidad de valores ingresados  (" + valores.Count + 
                              "," + tabla.columnas.Count +
                              ")." + Environment.NewLine;
                    return "Error";
                }
                if (hayVerbose && valores.Count != tabla.columnas.Count)
                {
                    mensajes += "Llenando los datos con valores default para lo que falta..." + Environment.NewLine;
                }

                //Llenar los valores que faltan con lo default
                while(valores.Count != tabla.columnas.Count)
                {
                    int i = valores.Count;
                    if (tabla.tipos_columnas[i].Equals("INT"))
                    {
                        valores.Add("INT  0");
                    }
                    else if (tabla.tipos_columnas[i].Equals("FLOAT"))
                    {
                        valores.Add("FLOAT0.0");
                    }
                    else if (tabla.tipos_columnas[i].Equals("DATE"))
                    {
                        DateTime myDateTime = DateTime.Now;
                        valores.Add("DATE " + "'" + myDateTime.ToString("yyyy-MM-dd") + "'");
                    }
                    else if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                    {
                        valores.Add("CHAR ''");
                    }
                }

                //Separar los tipos y valores en listas
                List<String> listaValores = new List<String>();
                List<String> listaTipos = new List<String>();

                foreach(String elemento in valores)
                {
                    String tipo = elemento.Substring(0, 5).Trim();
                    String valor = elemento.Substring(5);
                    listaTipos.Add(tipo);
                    listaValores.Add(valor);
                }
                if (hayVerbose)
                {
                    mensajes += "Verificando tipos y conversiones implicitas de los datos ingresados..." + Environment.NewLine;
                }

                //Verificar si los tipos concuerdan o la conversion implicita entre los tipos
                for (int i = 0; i < listaTipos.Count; i++ ) 
                {
                    if (!(listaTipos[i].Equals(tabla.tipos_columnas[i])
                      || (listaTipos[i].Equals("NULL"))
                      || (listaTipos[i].Equals("DATE") && tabla.tipos_columnas[i].StartsWith("CHAR"))
                      || (listaTipos[i].Equals("INT") && tabla.tipos_columnas[i].Equals("FLOAT"))
                      || (listaTipos[i].Equals("FLOAT") && tabla.tipos_columnas[i].Equals("INT"))
                      || (listaTipos[i].StartsWith("CHAR") && tabla.tipos_columnas[i].StartsWith("CHAR"))))
                    {
                        errores += "Error en línea " + context.start.Line +
                                   ": El tipo del valor '" + listaValores[i] +
                                   "' no concuerda con el tipo de la columna '" + tabla.tipos_columnas[i] + 
                                   "' (" + listaTipos[i] +
                                   "," + tabla.tipos_columnas[i] +
                                   ")." + Environment.NewLine;
                        return "Error";

                    }
                    if (listaTipos[i].Equals("DATE") && tabla.tipos_columnas[i].StartsWith("CHAR"))
                    {
                        listaTipos[i] = "CHAR";
                    }
                }
                //Generar lo que se va a agregar
                List<Object> row = new List<Object>();
                for (int i = 0; i < listaValores.Count; i++)
                {
                    if (listaTipos[i].Equals("NULL"))
                    {
                        row.Add(null);
                    }
                    else if (listaTipos[i].Equals("INT"))
                    {
                        row.Add(Convert.ToInt32(listaValores[i]));
                    }
                    else if (listaTipos[i].Equals("FLOAT"))
                    {
                        row.Add(Convert.ToSingle(listaValores[i]));
                    }
                    else if (listaTipos[i].Equals("DATE"))
                    {
                        row.Add(Convert.ToString(listaValores[i].Substring(1,listaValores[i].Length - 2)));
                    }
                    else if (listaTipos[i].StartsWith("CHAR"))
                    {
                        String tipo = tabla.tipos_columnas[i];
                        tipo = tipo.Substring(5);
                        tipo = tipo.Substring(0, tipo.Length - 1);
                        int largo = Convert.ToInt32(tipo);
                        String elemento = listaValores[i].Substring(1, listaValores[i].Length - 2);
                        //Revisar si no se pasa del tamaño establecido por la columna
                        if (elemento.Length > largo)
                        {
                            elemento = elemento.Substring(0, largo);
                            row.Add(elemento);
                        }
                        else
                        {
                            row.Add(elemento);
                        }
                    }

                }
 
                //Cargar la tabla
                FilaTabla datos = getFilaTabla(tabla);
                if (hayVerbose)
                {
                    mensajes += "Analizando si los datos ingresados cumplen con las restricciones de la tabla..." + Environment.NewLine;
                }

                //Verificar las restricciones
                bool aceptado = verificarRestricciones(datos, row, context.start.Line, -1);
                if (!aceptado)
                {
                    return "Error";
                }

                //Agregar los elementos
                datos.agregarFila(row);                
                
                //Actualizar cantidad de registros
                masterBD.getBD(BDenUso).registros++;
                cantInserts++;
                mensajes += "Se han insertado los datos en las columnas (" + row.Count + ") en la tabla '" + tabla.nombre + "' exitosamente."+ Environment.NewLine;
                return "void";
            }
            //Con id_completo de columnas
            else
            {
                Tabla tabla = masterTabla.getTable(nombre);

                if (hayVerbose)
                {
                    mensajes += "Verificando que exista la tabla sobre la cual hacer la insercion..." + Environment.NewLine;
                }

                //Verificar que exista la tabla
                if (tabla == null)
                {
                    errores += "Error en línea " + context.start.Line +
                               ": La tabla '" + nombre +
                               "' no existe en la base de datos '" + BDenUso +
                               "'." + Environment.NewLine;
                    return "Error";
                }
                //Obtener las columnas a las cuales insertar
                ListaTablas = new List<Tabla>();
                ListaTablas.Add(tabla);
                String columnasSelectas = Visit(context.GetChild(4));
                if (columnasSelectas.Equals(""))
                {
                    return "Error";
                }
                String[] listaColumnas = columnasSelectas.Replace(tabla.nombre+".","").Split(',');
                if (hayVerbose)
                {
                    mensajes += "Analizando si los datos ingresados cumplen con el formato de la tabla..." + Environment.NewLine;
                }
                
                var hashset = new HashSet<string>();
                foreach (var elemento in listaColumnas)
                {
                    if (!hashset.Add(elemento))
                    {
                        errores += "Error en línea " + context.start.Line +
                                   ": se repitio la columna '" + elemento +
                                   "' mas de una vez en las columnas selectas." + Environment.NewLine;
                        return "Error";
                    }
                }
                //Obtener valores a insertar 
                String[] valores = Regex.Split(Visit(context.GetChild(8)), ",(?=(?:[^']*'[^']*')*[^']*$)"); ;
                if (valores.Length != listaColumnas.Length)
                {
                    errores += "Error en línea " + context.start.Line +
                               ": La cantidad de columnas selectas en la tabla '" + tabla.nombre +
                               "' no concuerda con la cantidad de valores ingresados  (" + valores.Length +
                               "," + listaColumnas.Length +
                               ")." + Environment.NewLine;
                    return "Error";
                }
                //Separar los tipos y valores en listas
                List<String> listaValores = new List<String>();
                List<String> listaTipos = new List<String>();

                foreach (String elemento in valores)
                {
                    String tipo = elemento.Substring(0, 5).Trim();
                    String valor = elemento.Substring(5);
                    listaTipos.Add(tipo);
                    listaValores.Add(valor);
                }

                if (hayVerbose)
                {
                    mensajes += "Verificando tipos y conversiones implicitas de los datos ingresados..." + Environment.NewLine;
                }
                
                //Verificar si los tipos concuerdan o la conversion implicita entre los tipos
                for (int i = 0; i < listaTipos.Count; i++)
                {
                    int indice = tabla.columnas.IndexOf(listaColumnas[i]);
                    if (!(listaTipos[i].Equals(tabla.tipos_columnas[indice])
                      || (listaTipos[i].Equals("NULL"))
                      || (listaTipos[i].Equals("DATE") && tabla.tipos_columnas[indice].StartsWith("CHAR"))
                      || (listaTipos[i].Equals("INT") && tabla.tipos_columnas[indice].Equals("FLOAT"))
                      || (listaTipos[i].Equals("FLOAT") && tabla.tipos_columnas[indice].Equals("INT"))
                      || (listaTipos[i].StartsWith("CHAR") && tabla.tipos_columnas[indice].StartsWith("CHAR"))))
                    {
                        errores += "Error en línea " + context.start.Line +
                                   ": El tipo del valor '" + listaValores[i] +
                                   "' no concuerda con el tipo de la columna '" + tabla.tipos_columnas[indice] +
                                   "' (" + listaTipos[i] +
                                   "," + tabla.tipos_columnas[indice] +
                                   ")." + Environment.NewLine;
                        return "Error";
                    }
                    if (listaTipos[i].Equals("DATE") && tabla.tipos_columnas[indice].StartsWith("CHAR"))
                    {
                        listaTipos[i] = "CHAR";
                    }
                }

                //Generar lo que se va a agregar
                List<String> listaCol = new List<String>(listaColumnas);
                List<Object> row = new List<Object>();
                if (hayVerbose && listaCol.Count != tabla.columnas.Count)
                {
                    mensajes += "Llenando los datos con valores default para lo que falta..." + Environment.NewLine;
                }

                for (int i = 0; i < tabla.columnas.Count; i++)
                {
                    if (listaCol.Contains(tabla.columnas[i]))
                    {
                        //Saber que valor se va a agregar aqui
                        int indice = listaCol.IndexOf(tabla.columnas[i]);

                        //Si el valor ingresado es NULL
                        if (listaValores.Equals("NULL"))
                        {
                            row.Add(null);
                        }
                        //Se agrega el valor segun el tipo
                        else if (tabla.tipos_columnas[i].Equals("INT"))
                        {
                            row.Add(Convert.ToInt32(listaValores[indice]));
                        }
                        else if (tabla.tipos_columnas[i].Equals("FLOAT"))
                        {
                            row.Add(Convert.ToSingle(listaValores[indice]));
                        }
                        else if (tabla.tipos_columnas[i].Equals("DATE"))
                        {
                            row.Add(Convert.ToString(listaValores[indice].Substring(1, listaValores[indice].Length - 2)));
                        }
                        else if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                        {
                            String tipo = tabla.tipos_columnas[i];
                            tipo = tipo.Substring(5);
                            tipo = tipo.Substring(0, tipo.Length - 1);
                            int largo = Convert.ToInt32(tipo);

                            String elemento = listaValores[indice].Substring(1, listaValores[indice].Length - 2);
                            //Revisar si no se pasa del tamaño establecido por la columna
                            if (elemento.Length > largo)
                            {
                                elemento = elemento.Substring(0, largo);
                                row.Add(elemento);
                            }
                            else
                            {
                                row.Add(elemento);
                            }
                        }

                    }
                    else
                    {
                        //Se agrega el valor default
                        if (tabla.tipos_columnas[i].Equals("INT"))
                        {
                            row.Add(0);
                        }
                        else if (tabla.tipos_columnas[i].Equals("FLOAT"))
                        {
                            row.Add(Convert.ToSingle(0.0));
                        }
                        else if (tabla.tipos_columnas[i].Equals("DATE"))
                        {
                            DateTime myDateTime = DateTime.Now;
                            row.Add(myDateTime.ToString("yyyy-MM-dd"));
                        }
                        else if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                        {
                            row.Add("");
                        }
                    }


                }

                //Cargar la tabla
                FilaTabla datos = getFilaTabla(tabla);

                if (hayVerbose)
                {
                    mensajes += "Analizando si los datos ingresados cumplen con las restricciones de la tabla..." + Environment.NewLine;
                }

                //Verificar las restricciones
                bool aceptado = verificarRestricciones(datos, row, context.start.Line, -1);
                if (!aceptado)
                {
                    return "Error";
                }
                //Agregar los elementos
                datos.agregarFila(row);
                //datos.guardar();


                //Actualizar cantidad de registros
                masterBD.getBD(BDenUso).registros++;


                mensajes += "Se han insertado los datos(" + row.Count + ") en la tabla '" + tabla.nombre + "' exitosamente." + Environment.NewLine;
                return "void";
            }
            throw new NotImplementedException();
        }

        //Verifica la restricción de primary key para el caso de un update
        public bool verificarPrimaryKeyUpdate(List<Object> rowUpdate, List<String> columnas, FilaTabla fila, int nLinea) {
            foreach (Restriccion r in fila.tabla.restricciones) {
                if (r.tipo.Equals("PK")) {
                    foreach (String cP in r.columnasPropias) {
                        int i = columnas.IndexOf(cP);
                        if (i != -1) {
                            String nColumna = columnas.ElementAt(i);
                            int indexC = fila.tabla.columnas.IndexOf(nColumna);
                            for (int j = 0; j < fila.datos.elementos.Count; j++) {
                                if (rowUpdate.ElementAt(i) != null)
                                {
                                    if (fila.datos.elementos[j].ElementAt(indexC).Equals(rowUpdate.ElementAt(i)))
                                    {
                                        errores += "Error en la línea " + nLinea + ": Llave duplicada viola restricción de unicidad '" + r.nombre + "'.\r\n";
                                        return false;
                                    }
                                }
                                else
                                {
                                    errores += "Error en la línea " + nLinea +
                                           ": La llave primaria '" +
                                           r.nombre + "' no puede contener valores 'NULL'.\r\n";
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        //Verifica la restricción check para el caso de un update
        public bool verificarCheckUpdate(List<Object> rowUpdate, List<Object> row, List<String> columnas, FilaTabla datos, int nLinea)
        {
            foreach (Restriccion restriccion in datos.tabla.restricciones)
                if (restriccion.tipo.Equals("CH"))
                {
                    Stack<String> stack = new Stack<String>();
                    List<String> expresiones = new List<String>(Regex.Split(restriccion.restriccionCheck, " (?=(?:[^']*'[^']*')*[^']*$)"));
                    foreach (String e in expresiones)
                    {
                        if (e.Equals("OR"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();

                            if (uno.Equals("FALSE ") && dos.Equals("FALSE "))
                            {
                                stack.Push("FALSE ");
                            }
                            else
                            {
                                stack.Push("TRUE ");
                            }
                        }
                        else if (e.Equals("AND"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();

                            if (uno.Equals("TRUE ") && dos.Equals("TRUE "))
                            {
                                stack.Push("TRUE ");
                            }
                            else
                            {
                                stack.Push("FALSE ");
                            }
                        }
                        else if (e.Equals("<>"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                int indexUpdateUno = columnas.IndexOf(uno);
                                if (indexUpdateUno == -1)
                                    if (((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                                    {
                                        datoUno = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("INT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                                    }
                                    else
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                                    }
                                else
                                    datoUno = rowUpdate[indexUpdateUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                int indexUpdateDos = columnas.IndexOf(dos);
                                if (indexUpdateDos == -1)
                                    if (((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                                    {
                                        datoDos = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("INT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                                    }
                                    else
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                                    }
                                else
                                    datoDos = rowUpdate[indexUpdateDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                             && datoDos is Int32)
                            {
                                if (((Int32)datoUno).Equals(((Int32)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos)))==0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) == 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) == 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).Equals(((String)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                        }
                        else if (e.Equals("="))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                int indexUpdateUno = columnas.IndexOf(uno);
                                if (indexUpdateUno == -1)
                                    if (((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                                    {
                                        datoUno = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("INT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                                    }
                                    else
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                                    }
                                else
                                    datoUno = rowUpdate[indexUpdateUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                int indexUpdateDos = columnas.IndexOf(dos);
                                if (indexUpdateDos == -1)
                                    if (((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                                    {
                                        datoDos = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("INT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                                    }
                                    else
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                                    }
                                else
                                    datoDos = rowUpdate[indexUpdateDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                } 
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                             && datoDos is Int32)
                            {
                                if (!((Int32)datoUno).Equals(((Int32)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno), (((Single)datoDos))) != 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) != 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) != 0)
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                            else
                            {
                                if (!((String)datoUno).Equals(((String)datoDos)))
                                {
                                    stack.Push("FALSE ");
                                }
                                else
                                {
                                    stack.Push("TRUE ");
                                }
                            }
                        }
                        else if (e.Equals(">="))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                int indexUpdateUno = columnas.IndexOf(uno);
                                if (indexUpdateUno == -1)
                                    if (((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                                    {
                                        datoUno = null;
                                    } 
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("INT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                                    }
                                    else
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                                    }
                                else
                                    datoUno = rowUpdate[indexUpdateUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                int indexUpdateDos = columnas.IndexOf(dos);
                                if (indexUpdateDos == -1)
                                    if (((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                                    {
                                        datoDos = null;
                                    } 
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("INT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                                    }
                                    else
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                                    }
                                else
                                    datoDos = rowUpdate[indexUpdateDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                } 
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                             && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) >= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals("<="))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                int indexUpdateUno = columnas.IndexOf(uno);
                                if (indexUpdateUno == -1)
                                    if (((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                                    {
                                        datoUno = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("INT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                                    }
                                    else
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                                    }
                                else
                                    datoUno = rowUpdate[indexUpdateUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                int indexUpdateDos = columnas.IndexOf(dos);
                                if (indexUpdateDos == -1)
                                    if (((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                                    {
                                        datoDos = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("INT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                                    }
                                    else
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                                    }
                                else
                                    datoDos = rowUpdate[indexUpdateDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                             && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) <= 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals(">"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                int indexUpdateUno = columnas.IndexOf(uno);
                                if (indexUpdateUno == -1)
                                    if (((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                                    {
                                        datoUno = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("INT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                                    }
                                    else
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                                    }
                                else
                                    datoUno = rowUpdate[indexUpdateUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                int indexUpdateDos = columnas.IndexOf(dos);
                                if (indexUpdateDos == -1)
                                    if (((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                                    {
                                        datoDos = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("INT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                                    }
                                    else
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                                    }
                                else
                                    datoDos = rowUpdate[indexUpdateDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                             && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) > 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals("<"))
                        {
                            String dos = stack.Pop();
                            String uno = stack.Pop();
                            int indexUno = datos.tabla.columnas.IndexOf(uno);
                            int indexDos = datos.tabla.columnas.IndexOf(dos);
                            Object datoUno, datoDos;
                            if (indexUno >= 0)
                            {
                                int indexUpdateUno = columnas.IndexOf(uno);
                                if (indexUpdateUno == -1)
                                    if (((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                                    {
                                        datoUno = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("INT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                                    }
                                    else
                                    {
                                        datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                                    }
                                else
                                    datoUno = rowUpdate[indexUpdateUno];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (uno.Equals("NULL "))
                                {
                                    datoUno = null;
                                }
                                else if (uno.StartsWith("'"))
                                {
                                    datoUno = uno.Substring(1, uno.Length - 2);
                                }
                                else if (int.TryParse(uno, out num))
                                {
                                    datoUno = num;
                                }
                                else if (float.TryParse(uno, out numF))
                                {
                                    datoUno = numF;
                                }
                                else
                                {
                                    datoUno = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }
                            if (indexDos >= 0)
                            {
                                int indexUpdateDos = columnas.IndexOf(dos);
                                if (indexUpdateDos == -1)
                                    if (((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                                    {
                                        datoDos = null;
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("INT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                                    }
                                    else if (datos.tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                                    }
                                    else
                                    {
                                        datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                                    }
                                else
                                    datoDos = rowUpdate[indexUpdateDos];
                            }
                            else
                            {
                                int num;
                                float numF;
                                if (dos.Equals("NULL "))
                                {
                                    datoDos = null;
                                }
                                else if (dos.StartsWith("'"))
                                {
                                    datoDos = dos.Substring(1, dos.Length - 2);
                                }
                                else if (int.TryParse(dos, out num))
                                {
                                    datoDos = num;
                                }
                                else if (float.TryParse(dos, out numF))
                                {
                                    datoDos = numF;
                                }
                                else
                                {
                                    datoDos = "Para que deje de alegar abajo";
                                    throw new NotImplementedException();
                                }
                            }

                            if (datoUno == null || datoDos == null)
                            {
                                stack.Push("TRUE ");
                            }
                            else if (datoUno is Int32
                             && datoDos is Int32)
                            {
                                if (((Int32)datoUno).CompareTo(((Int32)datoDos)) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Single)
                            {
                                if (compareSingle(((Single)datoUno),(((Single)datoDos))) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Single
                             && datoDos is Int32)
                            {
                                if (compareSingle(((Single)datoUno), (((Int32)datoDos))) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (datoUno is Int32
                                  && datoDos is Single)
                            {
                                if (compareSingle(((Int32)datoUno), (((Single)datoDos))) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else if (isDate((String)datoUno) && isDate((String)datoDos))
                            {
                                DateTime primera = Convert.ToDateTime(((String)datoUno));
                                DateTime segunda = Convert.ToDateTime(((String)datoDos));

                                if (primera.CompareTo(segunda) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                            else
                            {
                                if (((String)datoUno).CompareTo(((String)datoDos)) < 0)
                                {
                                    stack.Push("TRUE ");
                                }
                                else
                                {
                                    stack.Push("FALSE ");
                                }
                            }
                        }
                        else if (e.Equals("NOT"))
                        {
                            String uno = stack.Pop();
                            if (uno.Equals("TRUE "))
                            {
                                stack.Push("FALSE ");
                            }
                            else
                            {
                                stack.Push("TRUE ");
                            }
                        }
                        //Es el nombre de la columna o un dato
                        else
                        {
                            if (e.Equals("NULL"))
                                stack.Push("NULL ");
                            else
                                stack.Push(e);
                        }
                    }
                    if (stack.Pop().Equals("FALSE "))
                    {
                        errores += "Error en línea " + nLinea + ": Inserción en la tabla '" + datos.tabla.nombre + "' viola la revisión '" + restriccion.nombre + "'." + Environment.NewLine;
                        return false;
                    }
                }
            return true;
        }

        //Verifica la restricción de foreign key para el caso de un update
        private bool verificarForeignKeyUpdate(FilaTabla datos, List<Object> datosUpdate, List<String> columnasUpdate, int nLinea)
        {
            foreach (Restriccion restriccion in datos.tabla.restricciones)
            {
                if (restriccion.tipo.Equals("FK"))
                {
                    String nTablaF = restriccion.tabla;
                    Tabla tForanea = masterTabla.getTable(nTablaF);
                    if (tForanea == null)
                    {
                        errores += "Error en línea " + nLinea + ": La tabla '" + restriccion.tabla + "' no existe en la base de datos '" + BDenUso + "'.\r\n";
                        return false;
                    }
                    FilaTabla fTabla = getFilaTabla(tForanea);

                    for (int cindex = 0; cindex < restriccion.columnasPropias.Count; cindex++)
                    {
                        bool banderaExiste = false;
                        String cP = restriccion.columnasPropias.ElementAt(cindex);
                        String cF = restriccion.columnasForaneas.ElementAt(cindex);
                        Object columnaUpdate;
                        int iColUp = columnasUpdate.IndexOf(cP);
                        if (iColUp != -1)
                        {
                            columnaUpdate = datosUpdate.ElementAt(iColUp);
                            int index1 = fTabla.tabla.columnas.IndexOf(cF);
                            for (int j = 0; j < fTabla.getTamanio(); j++)
                            {
                                if (fTabla.getRowElement(j, index1).Equals(columnaUpdate))
                                {
                                    banderaExiste = true;
                                    break;
                                }
                            }
                            if (columnaUpdate == null) 
                                banderaExiste = true;
                            if (!banderaExiste)
                            {
                                errores += "Error en línea " + nLinea + ": Update en la tabla '" + restriccion.tabla + "' viola la llave foránea '" + restriccion.nombre + "'.\r\n";
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        //Visita update definido dentro de la gramática. Realiza la funcionalidad del update según lo ingresado.
        override
        public string VisitUpdate(SqlParser.UpdateContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso en la cual hacer update..." + Environment.NewLine;
            }
            String nTabla = context.GetChild(1).GetText();
            if (BDenUso.Equals(""))
            { //No hay ninguna base de datos en uso
                errores += "Error en línea " + context.start.Line + ": No se encuentra ninguna base de datos en uso." + Environment.NewLine;
                return "Error";
            }
            if (hayVerbose)
            {
                mensajes += "Verificando que exista la tabla en la cual hacer update..." + Environment.NewLine;
            }
            Tabla tActual = masterTabla.getTable(nTabla);
            if (tActual == null)
            { //No existe la tabla
                errores += "Error en línea " + context.start.Line + ": La tabla '" + nTabla + "' no existe en la base de datos '" + BDenUso + "'." + Environment.NewLine;
                return "Error";
            }
            ListaTablas = new List<Tabla>();
            ListaTablas.Add(tActual);
            datosUpdate = new List<Object>();
            columnasUpdate = new List<String>();
            if (hayVerbose)
            {
                mensajes += "Verificando las columnas que se desea actualizar y obteniendo los datos..." + Environment.NewLine;
            }
            if (Visit(context.GetChild(3)).Equals("Error")) //Verifica que exista la columna y no se repita, y obtiene los datos
            {
                //Mensaje de error
                return "Error";
            }
            FilaTabla datos = getFilaTabla(tActual);

            if (context.ChildCount == 4) //No tiene WHERE
            {
                if (hayVerbose)
                {
                    mensajes += "Verificando las llaves primarias en los datos a actualizar..." + Environment.NewLine;
                }
                //Verificar restriccion de primary key
                bool banderaR = verificarPrimaryKeyUpdate(datosUpdate, columnasUpdate, datos, context.start.Line);

                if (!banderaR)
                    return "Error";

                if (hayVerbose)
                {
                    mensajes += "Verificando las llaves foraneas que puedan hacer referencia a la tabla..." + Environment.NewLine;
                }
                //Verificar restriccion de foreign key de es referenciada por otra tabla
                foreach (List<Object> fila in datos.datos.elementos)
                {
                    string pk = "";
                    if (esReferenciado(fila, tActual, masterTabla, out pk))
                    {
                        foreach (Restriccion r in datos.tabla.restricciones)
                            if (r.tipo.Equals("PK"))
                                foreach (String c in columnasUpdate)
                                    if (r.columnasPropias.Contains(c))
                                    {
                                        errores += "Error en línea " + context.start.Line +
                                                    ": Al menos una de las filas a actualizar es actualmente referenciada por la llave foranea " + pk + "." + Environment.NewLine;
                                        return "Error";
                                    }
                    }
                }
                if (hayVerbose)
                {
                    mensajes += "Verificando referencias en los datos nuevos de la actualizacion..." + Environment.NewLine;
                }
                //Verificar restriccion de foreign key de hace referencia a un dato ya existente o null
                if (!verificarForeignKeyUpdate(datos, datosUpdate, columnasUpdate, context.start.Line))
                    return "Error";
                if (hayVerbose)
                {
                    mensajes += "Verificando condiciones del check en los datos nuevos de la actualizacion..." + Environment.NewLine;
                }
                //Verificar restriccion de check
                List<List<Object>> datosPosiblesACambiar = new List<List<Object>>();
                foreach (List<Object> e in datos.datos.elementos)
                {
                    bool banderaCheck = verificarCheckUpdate(datosUpdate, e, columnasUpdate, datos, context.start.Line);
                    if (banderaCheck)
                        datosPosiblesACambiar.Add(e);
                    else
                        return "Error";
                }

                //Hacer update
                List<int> indicesC = new List<int>();
                foreach (String nombreC in columnasUpdate)
                    indicesC.Add(datos.tabla.columnas.IndexOf(nombreC));

                foreach (List<Object> e in datosPosiblesACambiar)
                {
                    for (int i = 0; i < datosUpdate.Count; i++)
                        e[indicesC[i]] = datosUpdate[i];
                }
                mensajes += "Se han actualizado " + datos.datos.elementos.Count + " registros con éxito." + Environment.NewLine;
            }
            else
            { //Si tiene WHERE

                if (hayVerbose)
                {
                    mensajes += "Obteniendo la expresion en el where..." + Environment.NewLine;
                }
                //Obtener expresion postfix
                ListaTablas = new List<Tabla>();
                ListaTablas.Add(tActual);
                String postfix = Visit(context.GetChild(5));
                postfix = postfix.Replace(tActual.nombre + ".", "");
                if (postfix.StartsWith("BOOL "))
                {
                    postfix = postfix.Substring(5);
                }
                else
                {
                    errores += "Error en línea " + context.start.Line +
                              ": La condicion del where no regresa un valor booleano." + Environment.NewLine;
                    return "Error";
                }

                //Lista de elementos a actualizar
                List<List<Object>> paraUpdate = new List<List<Object>>();
                if (hayVerbose)
                {
                    mensajes += "Filtrando los datos a actualizar..." + Environment.NewLine;
                }
                //Verificar si existe alguna referencia hacia la tabla
                foreach (List<Object> fila in datos.datos.elementos)
                {
                    //Llena la lista de los elementos que seran borrados
                    if (cumpleCondicion(fila, tActual, postfix))
                        paraUpdate.Add(fila);
                }

                FilaTabla nuevosDatos = new FilaTabla(tActual, BDenUso);
                nuevosDatos.datos.elementos = paraUpdate;
                if (hayVerbose)
                {
                    mensajes += "Verificando las llaves primarias en los datos a actualizar..." + Environment.NewLine;
                }

                //Verificar restriccion de primary key
                bool banderaR = verificarPrimaryKeyUpdate(datosUpdate, columnasUpdate, nuevosDatos, context.start.Line);

                if (!banderaR)
                    return "Error";

                if (hayVerbose)
                {
                    mensajes += "Verificando las llaves foraneas que puedan hacer referencia a la tabla..." + Environment.NewLine;
                }
                //Verificar restriccion de foreign key
                foreach (List<Object> fila in nuevosDatos.datos.elementos)
                {
                    string pk = "";
                    if (esReferenciado(fila, tActual, masterTabla, out pk))
                    {
                        foreach (Restriccion r in datos.tabla.restricciones)
                            if (r.tipo.Equals("PK"))
                                foreach (String c in columnasUpdate)
                                    if (r.columnasPropias.Contains(c))
                                    {
                                        errores += "Error en línea " + context.start.Line +
                                                    ": Al menos una de las filas a actualizar es actualmente referenciada por la llave foranea " + pk + "." + Environment.NewLine;
                                        return "Error";
                                    }
                    }
                }
                if (hayVerbose)
                {
                    mensajes += "Verificando referencias en los datos nuevos de la actualizacion..." + Environment.NewLine;
                }
                //Verificar restriccion de foreign key de hace referencia a un dato ya existente o null
                if (!verificarForeignKeyUpdate(datos, datosUpdate, columnasUpdate, context.start.Line))
                    return "Error";

                if (hayVerbose)
                {
                    mensajes += "Verificando condiciones del check en los datos nuevos de la actualizacion..." + Environment.NewLine;
                }
                //Verificar restriccion de check
                List<List<Object>> datosPosiblesACambiar = new List<List<Object>>();
                foreach (List<Object> e in nuevosDatos.datos.elementos)
                {
                    bool banderaCheck = verificarCheckUpdate(datosUpdate, e, columnasUpdate, nuevosDatos, context.start.Line);
                    if (banderaCheck)
                        datosPosiblesACambiar.Add(e);
                    else
                        return "Error";
                }

                //Hacer update
                List<int> indicesC = new List<int>();
                foreach (String nombreC in columnasUpdate)
                    indicesC.Add(nuevosDatos.tabla.columnas.IndexOf(nombreC));

                foreach (List<Object> e in datosPosiblesACambiar)
                {
                    for (int i = 0; i < datosUpdate.Count; i++)
                        e[indicesC[i]] = datosUpdate[i];
                }
                //datos.guardar();
                mensajes += "Se han actualizado " + nuevosDatos.datos.elementos.Count + " registros con éxito." + Environment.NewLine;
            }
            return "void";
        }

        //Visita alter_table definido dentro de la gramática. Altera una tabla segun lo ingresado.
        override
        public string VisitAlter_table(SqlParser.Alter_tableContext context)
        {
            String nTabla = context.GetChild(2).GetText();
            if (hayVerbose)
            {
                mensajes += "Verificando si hay una base de datos en uso..." + Environment.NewLine;
            }
            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay base de datos en uso por lo que no se puede alterar la tabla.";
                return "Error";
            }

            //Verificar si la tabla existe
            if (hayVerbose)
            {
                mensajes += "Verificando si la tabla existe..." + Environment.NewLine;
            }
            if (!masterTabla.containsTable(nTabla))
            {
                errores += "Error en linea " + context.start.Line + ": La base de datos " + BDenUso + " no contiene una tabla " + nTabla + "." + Environment.NewLine;
                return "Error";
            }

            //Mandar datos
            ListaTablas = new List<Tabla>();
            ListaTablas.Add(masterTabla.getTable(nTabla));

            return Visit(context.GetChild(3));
        }

        //Visita accion_DropColumn definido dentro de la gramática. Elimina una columna de una tabla.
        override
        public string VisitAccion_DropColumn(SqlParser.Accion_DropColumnContext context)
        {
            Tabla tActual = ListaTablas[0];
            String cBorrar = context.GetChild(2).GetText();
            if (hayVerbose)
            {
                mensajes += "Verificando que la columna exista..." + Environment.NewLine;
            }
            if (!tActual.columnas.Contains(cBorrar))
            {
                errores += "Error en la línea " + context.start.Line + ": La tabla '" + tActual.nombre + "' no contiene la columna '" + cBorrar + "'.\r\n";
                return "Error";
            }

            if (hayVerbose)
            {
                mensajes += "Verificando que la columna a borrar no tenga restricciones..." + Environment.NewLine;
            }
            foreach (Restriccion r in tActual.restricciones)
            {
                if (r.columnasPropias.Contains(cBorrar))
                {
                    errores += "Error en la línea " + context.start.Line + ": La columna '" + cBorrar + "' tiene una restricción por lo que no se puede borrar.\r\n";
                    return "Error";
                }
            }

            foreach (Tabla t in masterTabla.tablas)
            {
                if (!t.nombre.Equals(tActual.nombre))
                {
                    foreach (Restriccion r in t.restricciones)
                    {
                        if (r.columnasForaneas.Contains(cBorrar))
                        {
                            int i = r.columnasForaneas.IndexOf(cBorrar);
                            errores += "Error en la línea " + context.start.Line + ": La columna '" + cBorrar + "' es referenciada como llave foránea de la columna '" + r.columnasPropias.ElementAt(i) + "' de la tabla '" + t.nombre + "'.\r\n";
                            return "Error";
                        }
                    }
                }
            }

            int ind = tActual.columnas.IndexOf(cBorrar);
            tActual.columnas.Remove(cBorrar);
            tActual.tipos_columnas.RemoveAt(ind);

            mensajes += "Se ha removido la columna '" + cBorrar + "' de la tabla '" + tActual.nombre + "' con éxito.\r\n";
            return "void";
        }

        //Verifica si lo ingresado cumple cierta condición según el WHERE.
        private bool cumpleCondicion(List<Object> row, Tabla tabla, String postfix)
        {
            Stack<String> stack = new Stack<String>();
            List<String> expresiones = new List<String>(Regex.Split(postfix, " (?=(?:[^']*'[^']*')*[^']*$)"));
            foreach (String e in expresiones)
            {
                if (e.Equals("OR"))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();

                    if (uno.Equals("FALSE ") && dos.Equals("FALSE "))
                    {
                        stack.Push("FALSE ");
                    }
                    else
                    {
                        stack.Push("TRUE ");
                    }
                }
                else if (e.Equals("AND"))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();

                    if (uno.Equals("TRUE ") && dos.Equals("TRUE "))
                    {
                        stack.Push("TRUE ");
                    }
                    else
                    {
                        stack.Push("FALSE ");
                    }
                }
                else if (e.Equals("<>"))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();
                    int indexUno = tabla.columnas.IndexOf(uno);
                    int indexDos = tabla.columnas.IndexOf(dos);
                    Object datoUno, datoDos;
                    if (indexUno >= 0)
                    {
                        if (row[indexUno] == null || ((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                        {
                            datoUno = null;
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("INT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                        }
                        else
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (uno.Equals("NULL "))
                        {
                            datoUno = null;
                        }
                        else if (uno.StartsWith("'"))
                        {
                            datoUno = uno.Substring(1, uno.Length - 2);
                        }
                        else if (int.TryParse(uno, out num))
                        {
                            datoUno = num;
                        }
                        else if (float.TryParse(uno, out numF))
                        {
                            datoUno = numF;
                        }
                        else
                        {
                            datoUno = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (indexDos >= 0)
                    {
                        if (row[indexDos] == null || ((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                        {
                            datoDos = null;
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("INT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                        }
                        else
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (dos.Equals("NULL "))
                        {
                            datoDos = null;
                        }
                        else if (dos.StartsWith("'"))
                        {
                            datoDos = dos.Substring(1, dos.Length - 2);
                        }
                        else if (int.TryParse(dos, out num))
                        {
                            datoDos = num;
                        }
                        else if (float.TryParse(dos, out numF))
                        {
                            datoDos = numF;
                        }
                        else
                        {
                            datoDos = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (datoUno == null || datoDos == null)
                    {
                        stack.Push("FALSE ");
                    }
                    else if (datoUno is Int32
                          && datoDos is Int32)
                    {
                        if (((Int32)datoUno).Equals(((Int32)datoDos)))
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Single)
                    {
                        if (compareSingle(((Single)datoUno), (((Single)datoDos))) == 0)
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Int32)
                    {
                        if (compareSingle(((Single)datoUno), (((Int32)datoDos))) == 0)
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else if (datoUno is Int32
                          && datoDos is Single)
                    {
                        if (compareSingle(((Int32)datoUno), (((Single)datoDos))) == 0)
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else
                    {
                        if (((String)datoUno).Equals(((String)datoDos)))
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                }
                else if (e.Equals("="))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();
                    int indexUno = tabla.columnas.IndexOf(uno);
                    int indexDos = tabla.columnas.IndexOf(dos);
                    Object datoUno, datoDos;
                    if (indexUno >= 0)
                    {
                        if (row[indexUno] == null || ((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                        {
                            datoUno = null;
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("INT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                        }
                        else
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (uno.Equals("NULL "))
                        {
                            datoUno = null;
                        }
                        else if (uno.StartsWith("'"))
                        {
                            datoUno = uno.Substring(1, uno.Length - 2);
                        }
                        else if (int.TryParse(uno, out num))
                        {
                            datoUno = num;
                        }
                        else if (float.TryParse(uno, out numF))
                        {
                            datoUno = numF;
                        }
                        else
                        {
                            datoUno = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (indexDos >= 0)
                    {
                        if (row[indexDos] == null || ((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                        {
                            datoDos = null;
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("INT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                        }
                        else
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (dos.Equals("NULL "))
                        {
                            datoDos = null;
                        }
                        else if (dos.StartsWith("'"))
                        {
                            datoDos = dos.Substring(1, dos.Length - 2);
                        }
                        else if (int.TryParse(dos, out num))
                        {
                            datoDos = num;
                        }
                        else if (float.TryParse(dos, out numF))
                        {
                            datoDos = numF;
                        }
                        else
                        {
                            datoDos = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }

                    if (datoUno == null || datoDos == null)
                    {
                        stack.Push("FALSE ");
                    }
                    else if (datoUno is Int32
                          && datoDos is Int32)
                    {
                        if (!((Int32)datoUno).Equals(((Int32)datoDos)))
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Single)
                    {
                        if (compareSingle(((Single)datoUno), (((Single)datoDos))) != 0)
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Int32)
                    {
                        if (compareSingle(((Single)datoUno), (((Int32)datoDos))) != 0)
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else if (datoUno is Int32
                          && datoDos is Single)
                    {
                        if (compareSingle(((Int32)datoUno), (((Single)datoDos))) != 0)
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                    else
                    {
                        if (!((String)datoUno).Equals(((String)datoDos)))
                        {
                            stack.Push("FALSE ");
                        }
                        else
                        {
                            stack.Push("TRUE ");
                        }
                    }
                }
                else if (e.Equals(">="))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();
                    int indexUno = tabla.columnas.IndexOf(uno);
                    int indexDos = tabla.columnas.IndexOf(dos);
                    Object datoUno, datoDos;
                    if (indexUno >= 0)
                    {
                        if (row[indexUno] == null || ((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                        {
                            datoUno = null;
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("INT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                        }
                        else
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (uno.Equals("NULL "))
                        {
                            datoUno = null;
                        }
                        else if (uno.StartsWith("'"))
                        {
                            datoUno = uno.Substring(1, uno.Length - 2);
                        }
                        else if (int.TryParse(uno, out num))
                        {
                            datoUno = num;
                        }
                        else if (float.TryParse(uno, out numF))
                        {
                            datoUno = numF;
                        }
                        else
                        {
                            datoUno = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (indexDos >= 0)
                    {
                        if (row[indexDos] == null || ((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                        {
                            datoDos = null;
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("INT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                        }
                        else
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (dos.Equals("NULL "))
                        {
                            datoDos = null;
                        }
                        else if (dos.StartsWith("'"))
                        {
                            datoDos = dos.Substring(1, dos.Length - 2);
                        }
                        else if (int.TryParse(dos, out num))
                        {
                            datoDos = num;
                        }
                        else if (float.TryParse(dos, out numF))
                        {
                            datoDos = numF;
                        }
                        else
                        {
                            datoDos = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }

                    if (datoUno == null || datoDos == null)
                    {
                        stack.Push("FALSE ");
                    }
                    else if (datoUno is Int32
                          && datoDos is Int32)
                    {
                        if (((Int32)datoUno).CompareTo(((Int32)datoDos)) >= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Single)
                    {
                        if (compareSingle(((Single)datoUno),(((Single)datoDos))) >= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Int32)
                    {
                        if (compareSingle(((Single)datoUno), (((Int32)datoDos))) >= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Int32
                          && datoDos is Single)
                    {
                        if (compareSingle(((Int32)datoUno), (((Single)datoDos))) >= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (isDate((String)datoUno) && isDate((String)datoDos))
                    {
                        DateTime primera = Convert.ToDateTime(((String)datoUno));
                        DateTime segunda = Convert.ToDateTime(((String)datoDos));

                        if (primera.CompareTo(segunda) >= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else
                    {
                        if (((String)datoUno).CompareTo(((String)datoDos)) >= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                }
                else if (e.Equals("<="))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();
                    int indexUno = tabla.columnas.IndexOf(uno);
                    int indexDos = tabla.columnas.IndexOf(dos);
                    Object datoUno, datoDos;
                    if (indexUno >= 0)
                    {
                        if (row[indexUno] == null || ((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                        {
                            datoUno = null;
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("INT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                        }
                        else
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (uno.Equals("NULL "))
                        {
                            datoUno = null;
                        }
                        else if (uno.StartsWith("'"))
                        {
                            datoUno = uno.Substring(1, uno.Length - 2);
                        }
                        else if (int.TryParse(uno, out num))
                        {
                            datoUno = num;
                        }
                        else if (float.TryParse(uno, out numF))
                        {
                            datoUno = numF;
                        }
                        else
                        {
                            datoUno = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (indexDos >= 0)
                    {
                        if (row[indexDos] == null || ((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                        {
                            datoDos = null;
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("INT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                        }
                        else
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (dos.Equals("NULL "))
                        {
                            datoDos = null;
                        }
                        else if (dos.StartsWith("'"))
                        {
                            datoDos = dos.Substring(1, dos.Length - 2);
                        }
                        else if (int.TryParse(dos, out num))
                        {
                            datoDos = num;
                        }
                        else if (float.TryParse(dos, out numF))
                        {
                            datoDos = numF;
                        }
                        else
                        {
                            datoDos = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }

                    if (datoUno == null || datoDos == null)
                    {
                        stack.Push("FALSE ");
                    }
                    else if (datoUno is Int32
                          && datoDos is Int32)
                    {
                        if (((Int32)datoUno).CompareTo(((Int32)datoDos)) <= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Single)
                    {
                        if (compareSingle(((Single)datoUno), (((Single)datoDos))) <= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Int32)
                    {
                        if (compareSingle(((Single)datoUno), (((Int32)datoDos))) <= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Int32
                          && datoDos is Single)
                    {
                        if (compareSingle(((Int32)datoUno), (((Single)datoDos))) <= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (isDate((String)datoUno) && isDate((String)datoDos))
                    {
                        DateTime primera = Convert.ToDateTime(((String)datoUno));
                        DateTime segunda = Convert.ToDateTime(((String)datoDos));

                        if (primera.CompareTo(segunda) <= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else
                    {
                        if (((String)datoUno).CompareTo(((String)datoDos)) <= 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                }
                else if (e.Equals(">"))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();
                    int indexUno = tabla.columnas.IndexOf(uno);
                    int indexDos = tabla.columnas.IndexOf(dos);
                    Object datoUno, datoDos;
                    if (indexUno >= 0)
                    {
                        if (row[indexUno] == null || ((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                        {
                            datoUno = null;
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("INT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                        }
                        else
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (uno.Equals("NULL "))
                        {
                            datoUno = null;
                        }
                        else if (uno.StartsWith("'"))
                        {
                            datoUno = uno.Substring(1, uno.Length - 2);
                        }
                        else if (int.TryParse(uno, out num))
                        {
                            datoUno = num;
                        }
                        else if (float.TryParse(uno, out numF))
                        {
                            datoUno = numF;
                        }
                        else
                        {
                            datoUno = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (indexDos >= 0)
                    {
                        if (row[indexDos] == null || ((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                        {
                            datoDos = null;
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("INT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                        }
                        else
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (dos.Equals("NULL "))
                        {
                            datoDos = null;
                        }
                        else if (dos.StartsWith("'"))
                        {
                            datoDos = dos.Substring(1, dos.Length - 2);
                        }
                        else if (int.TryParse(dos, out num))
                        {
                            datoDos = num;
                        }
                        else if (float.TryParse(dos, out numF))
                        {
                            datoDos = numF;
                        }
                        else
                        {
                            datoDos = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }

                    if (datoUno == null || datoDos == null)
                    {
                        stack.Push("FALSE ");
                    }
                    else if (datoUno is Int32
                          && datoDos is Int32)
                    {
                        if (((Int32)datoUno).CompareTo(((Int32)datoDos)) > 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Single)
                    {
                        if (compareSingle(((Single)datoUno), (((Single)datoDos))) > 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Int32)
                    {
                        if (compareSingle(((Single)datoUno), (((Int32)datoDos))) > 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Int32
                          && datoDos is Single)
                    {
                        if (compareSingle(((Int32)datoUno), (((Single)datoDos))) > 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (isDate((String)datoUno) && isDate((String)datoDos))
                    {
                        DateTime primera = Convert.ToDateTime(((String)datoUno));
                        DateTime segunda = Convert.ToDateTime(((String)datoDos));

                        if (primera.CompareTo(segunda) > 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else
                    {
                        if (((String)datoUno).CompareTo(((String)datoDos)) > 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                }
                else if (e.Equals("<"))
                {
                    String dos = stack.Pop();
                    String uno = stack.Pop();
                    int indexUno = tabla.columnas.IndexOf(uno);
                    int indexDos = tabla.columnas.IndexOf(dos);
                    Object datoUno, datoDos;
                    if (indexUno >= 0)
                    {
                        if (row[indexUno] == null || ((MsgPack.MessagePackObject)row[indexUno]).IsNil)
                        {
                            datoUno = null;
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("INT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexUno].Equals("FLOAT"))
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsSingle();
                        }
                        else
                        {
                            datoUno = ((MsgPack.MessagePackObject)row[indexUno]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (uno.Equals("NULL "))
                        {
                            datoUno = null;
                        }
                        else if (uno.StartsWith("'"))
                        {
                            datoUno = uno.Substring(1, uno.Length - 2);
                        }
                        else if (int.TryParse(uno, out num))
                        {
                            datoUno = num;
                        }
                        else if (float.TryParse(uno, out numF))
                        {
                            datoUno = numF;
                        }
                        else
                        {
                            datoUno = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }
                    if (indexDos >= 0)
                    {
                        if (row[indexDos] == null || ((MsgPack.MessagePackObject)row[indexDos]).IsNil)
                        {
                            datoDos = null;
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("INT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[indexDos].Equals("FLOAT"))
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsSingle();
                        }
                        else
                        {
                            datoDos = ((MsgPack.MessagePackObject)row[indexDos]).AsString();
                        }
                    }
                    else
                    {
                        int num;
                        float numF;
                        if (dos.Equals("NULL "))
                        {
                            datoDos = null;
                        }
                        else if (dos.StartsWith("'"))
                        {
                            datoDos = dos.Substring(1, dos.Length - 2);
                        }
                        else if (int.TryParse(dos, out num))
                        {
                            datoDos = num;
                        }
                        else if (float.TryParse(dos, out numF))
                        {
                            datoDos = numF;
                        }
                        else
                        {
                            datoDos = "Para que deje de alegar abajo";
                            throw new NotImplementedException();
                        }
                    }

                    if (datoUno == null || datoDos == null)
                    {
                        stack.Push("FALSE ");
                    }
                    else if (datoUno is Int32
                          && datoDos is Int32)
                    {
                        if (((Int32)datoUno).CompareTo(((Int32)datoDos)) < 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Single)
                    {
                        if (compareSingle(((Single)datoUno), (((Single)datoDos))) < 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Single
                     && datoDos is Int32)
                    {
                        if (compareSingle(((Single)datoUno), (((Int32)datoDos))) < 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (datoUno is Int32
                          && datoDos is Single)
                    {
                        if (compareSingle(((Int32)datoUno), (((Single)datoDos))) < 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else if (isDate((String)datoUno) && isDate((String)datoDos))
                    {
                        DateTime primera = Convert.ToDateTime(((String)datoUno));
                        DateTime segunda = Convert.ToDateTime(((String)datoDos));

                        if (primera.CompareTo(segunda) < 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                    else
                    {
                        if (((String)datoUno).CompareTo(((String)datoDos)) < 0)
                        {
                            stack.Push("TRUE ");
                        }
                        else
                        {
                            stack.Push("FALSE ");
                        }
                    }
                }
                else if (e.Equals("NOT"))
                {
                    String uno = stack.Pop();
                    if (uno.Equals("TRUE "))
                    {
                        stack.Push("FALSE ");
                    }
                    else
                    {
                        stack.Push("TRUE ");
                    }
                }
                //Es el nombre de la columna o un dato
                else
                {
                    if(e.Equals("NULL"))
                    {
                        stack.Push("NULL ");
                    }
                    else
                    {
                        stack.Push(e);
                    }

                }
            }
            return stack.Pop().Equals("TRUE ");
        }

        //Verifica si una columna es referenciada por otra para permitir ciertas funcionalidades
        private bool esReferenciado(List<Object> row, Tabla tabla, MasterTabla mTablas, out String pk)
        {
            //Para cada tabla
            foreach (Tabla otra in mTablas.tablas)
            {
                //Para cada restriccion
                foreach (Restriccion restriccion in otra.restricciones)
                {
                    //Verificar que no sea la misma tabla
                    if (tabla == otra)
                    {
                        break;
                    }

                    //La restriccion es llave foranea y hacia la tabla en cuestion
                    if (restriccion.tipo.Equals("FK") && restriccion.tabla.Equals(tabla.nombre))
                    {
                        //Se cargan datos de la otra tabla
                        FilaTabla datos = getFilaTabla(otra);

                        //Para cada fila en la otra tabla
                        foreach (List<Object> rowOtra in datos.datos.elementos)
                        {
                            //Se verifica que toda la llave sea igual en ambas tablas
                            bool esReferenciada = true;
                            for (int i = 0; i < restriccion.columnasForaneas.Count; i++)
                            {
                                //Indices de columnas en cada tabla
                                int indicePropio = tabla.columnas.IndexOf(restriccion.columnasForaneas[i]);
                                int indiceOtro = otra.columnas.IndexOf(restriccion.columnasPropias[i]);

                                //Si uno de los dos valores es falso
                                if (((MsgPack.MessagePackObject)row[indicePropio]).IsNil || ((MsgPack.MessagePackObject)rowOtra[indiceOtro]).IsNil)
                                {
                                    esReferenciada = false;
                                    break;
                                }
                                //Cada caso segun el tipo que es
                                else if (tabla.tipos_columnas[indicePropio].Equals("INT"))
                                {
                                    //Obtener valores
                                    Int32 valorPropio = ((MsgPack.MessagePackObject)row[indicePropio]).AsInt32();
                                    Int32 valorOtro = ((MsgPack.MessagePackObject)rowOtra[indiceOtro]).AsInt32();

                                    //Si es distinto, no es referenciada
                                    if (!valorPropio.Equals(valorOtro))
                                    {
                                        esReferenciada = false;
                                        break;
                                    }
                                }
                                else if (tabla.tipos_columnas[indicePropio].Equals("FLOAT"))
                                {
                                    //Obtener valores
                                    Single valorPropio = ((MsgPack.MessagePackObject)row[indicePropio]).AsSingle();
                                    Single valorOtro = ((MsgPack.MessagePackObject)rowOtra[indiceOtro]).AsSingle();

                                    //Si es distinto, no es referenciada
                                    if (compareSingle(((Single)valorPropio), (((Single)valorOtro))) != 0)
                                    {
                                        esReferenciada = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    //Obtener valores
                                    String valorPropio = ((MsgPack.MessagePackObject)row[indicePropio]).AsString();
                                    String valorOtro = ((MsgPack.MessagePackObject)rowOtra[indiceOtro]).AsString();

                                    //Si es distinto, no es referenciada
                                    if (!valorPropio.Equals(valorOtro))
                                    {
                                        esReferenciada = false;
                                        break;
                                    }
                                }
                            }
                            if (esReferenciada)
                            {
                                pk = "'" + restriccion.nombre + "'" + "de la tabla '" + otra.nombre + "'";
                                return true;
                            }
                        }
                    }
                }
            }
            pk = "";
            return false;
        }

        //Visita delete definido dentro de la gramática. Realiza la funcionalidad del delete según lo ingresado.
        override
        public string VisitDelete(SqlParser.DeleteContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso en la cual borrar..." + Environment.NewLine;
            }

            //Revisar que haya base de datos en uso
            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay base de datos en uso por lo que no se puede alterar la tabla.";
                return "Error";
            }
            
            //Obtener el nombre de la tabla que se desea
            String nombre = context.GetChild(2).GetText();
            Tabla tabla = masterTabla.getTable(nombre);
            if (hayVerbose)
            {
                mensajes += "Verificando que exista la tabla sobre la cual borrar..." + Environment.NewLine;
            }
            //Verificar que exista la tabla
            if (tabla == null)
            {
                errores += "Error en línea " + context.start.Line +
                            ": La tabla '" + nombre +
                            "' no existe en la base de datos '" + BDenUso +
                            "'." + Environment.NewLine;
                return "Error";
            }

            if (context.ChildCount == 3)
            {
                //Cargar los datos en si
                FilaTabla datos = getFilaTabla(tabla);
                if (hayVerbose)
                {
                    mensajes += "Verificando si hay referencias hacia los datos a borrar de la tabla..." + Environment.NewLine;
                }
                //Verificar si existe alguna referencia hacia la tabla
                foreach (List<Object> fila in datos.datos.elementos)
                {
                    string pk = "";
                    if (esReferenciado(fila, tabla, masterTabla, out pk))
                    {
                        errores += "Error en línea " + context.start.Line +
                                    ": Al menos una de las filas a borrar es actualmente referenciada por la llave foranea " + pk + "." + Environment.NewLine;
                        return "Error";
                    }
                }

                //Contar los datos a borrar
                int cantidad = datos.datos.elementos.Count;
                
                //Borrar los datos y guardar los cambios
                datos.datos.elementos.Clear();
                //datos.guardar();

                //Actualizar cantidad de registros
                tabla.cantidad_registros = tabla.cantidad_registros - cantidad;
                masterBD.getBD(BDenUso).registros -= cantidad;
                
                mensajes += "Se ha vaciado la tabla '" + tabla.nombre+ "'." + Environment.NewLine; 
                return "void";
            }
            else
            {
                if (hayVerbose)
                {
                    mensajes += "Analizando la expresion del where..." + Environment.NewLine;
                }

                //Obtener expresion postfix
                ListaTablas = new List<Tabla>();
                ListaTablas.Add(tabla);
                String postfix = Visit(context.GetChild(4));
                postfix = postfix.Replace(tabla.nombre+".","");
                if (postfix.StartsWith("BOOL "))
                {
                    postfix = postfix.Substring(5);
                }
                else
                {
                    errores += "Error en línea " + context.start.Line +
                              ": La condicion del where no regresa un valor booleano." + Environment.NewLine;
                    return "Error";
                }

                //Lista de elementos a borrar
                List<List<Object>> paraBorrar = new List<List<Object>>();

                //Cargar los datos en si
                FilaTabla datos = getFilaTabla(tabla);

                if (hayVerbose)
                {
                    mensajes += "Filtrando los datos a borrar segun la condicion de la expresion en el where..." + Environment.NewLine;
                    mensajes += "Verificando si hay referencias hacia los datos a borrar de la tabla..." + Environment.NewLine;
                }

                //Verificar si existe alguna referencia hacia la tabla
                foreach(List<Object> fila in datos.datos.elementos)
                {
                    //Llena la lista de los elementos que seran borrados
                    if (cumpleCondicion(fila, tabla, postfix))
                    {
                        paraBorrar.Add(fila);
                        string pk = "";
                        if (esReferenciado(fila, tabla, masterTabla, out pk))
                        {
                            errores += "Error en línea " + context.start.Line +
                                      ": Al menos una de las filas a borrar es actualmente referenciada por la llave foranea " + pk + "." + Environment.NewLine;
                            return "Error";
                        }
                    }
                }
                //Cantidad de datos a borrar
                int cantidad = paraBorrar.Count;

                //Borrar los datos
                foreach (List<Object> fila in paraBorrar)
                {
                    datos.datos.elementos.Remove(fila);
                }
                //Guardar cambios
                //datos.guardar();

                //Actualizar cantidad de registros
                tabla.cantidad_registros = tabla.cantidad_registros - cantidad;
                masterBD.getBD(BDenUso).registros -= cantidad;

                mensajes += "Se han removido " + cantidad + " registros de la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                return "void";

            }

        }

        //Visita renombrar_BD definido dentro de la gramática. Realiza la funcionalidad renombrar una base de datos.
        override
        public string VisitRenombrar_BD(SqlParser.Renombrar_BDContext context)
        {
            String nombre = context.GetChild(2).GetText();
            String nuevoNombre = context.GetChild(5).GetText();

            if (hayVerbose)
            {
                mensajes += "Verificando si la base de datos en uso es la que se le quiere cambiar nombre..." + Environment.NewLine;
            }

            if (nombre.Equals(BDenUso))
            {
                BDenUso = nuevoNombre;
            }

            if (hayVerbose)
            {
                mensajes += "Verificando que la base de datos a la que se le quiere cambiar nombre exista..." + Environment.NewLine;
            }
            if (masterBD.containsBD(nombre))
            {
                foreach (BaseDatos bd in masterBD.basesDeDatos)
                {
                    if (bd.nombre.Equals(nombre))
                    {
                        bd.nombre = nuevoNombre;
                        if (hayVerbose)
                        {
                            mensajes += "Renombrando la base de datos..." + Environment.NewLine;
                        }
                        String pathViejo = "Databases\\" + nombre + "\\" + nombre + ".xml";
                        String pathNuevo = "Databases\\" + nombre + "\\" + nuevoNombre + ".xml";
                        System.IO.File.Move(pathViejo, pathNuevo);

                        pathViejo = "Databases\\" + nombre;
                        pathNuevo = "Databases\\" + nuevoNombre;
                        Directory.Move(pathViejo, pathNuevo);

                        break;
                    }
                }
                mensajes += "Se ha cambiado el nombre de la base de datos de '" + nombre + "' a '" + nuevoNombre + "' con éxito.\r\n";
                return "void";
            }
            else
            {
                errores += "Error en linea" + context.start.Line + "No existe la base de datos '" + nombre + "' por lo que no se le puede cambiar el nombre.\r\n";
                return "Error";
            }
        }

        //Visita botar_table definido dentro de la gramática. Realiza la funcionalidad eliminar una tabla de una base de datos
        override
        public string VisitBotar_table(SqlParser.Botar_tableContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso en la cual botar tablas..." + Environment.NewLine;
            }

            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay base de datos en uso por lo que no se puede alterar la tabla.";
                return "Error";
            }
            if (hayVerbose)
            {
                mensajes += "Verificando que exista la tabla a botar en la base de datos..." + Environment.NewLine;
            }

            Tabla tabla = masterTabla.getTable(context.GetChild(2).GetText());
            if (tabla == null)
            {
                errores += "Error en línea " + context.start.Line +
                           ": No existe la tabla '" + context.GetChild(2).GetText() + 
                           "' en la base de datos '" + BDenUso + "'."+Environment.NewLine;
                return "Error";
            }
            bool esReferenciada = false;
            String referencia = "";
            if (hayVerbose)
            {
                mensajes += "Verificando si hay referencias hacia la tabla a borrar..." + Environment.NewLine;
            }
            foreach (Tabla t in masterTabla.tablas)
            {
                foreach (Restriccion restriccion in t.restricciones)
                {
                    if (restriccion.tipo.Equals("FK") && restriccion.tabla.Equals(tabla.nombre))
                    {
                        esReferenciada = true;
                        referencia = t.nombre;
                        break;
                        
                    }
                }
                if (esReferenciada)
                {
                    break;
                }
            }
            if (esReferenciada)
            {
                errores += "Error en línea " + context.start.Line +
                           ": La tabla '" + tabla.nombre +
                           "' es referenciada por '" + referencia +
                           "', bote la restricción antes de botar la tabla." + Environment.NewLine;
                return "Error";
            }
            else
            {
                masterTabla.tablas.Remove(tabla);

                String path = "Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat";
                System.IO.File.Delete(path);

                BaseDatos bd = masterBD.getBD(BDenUso);
                bd.cantidad_tablas--;
                bd.registros = bd.registros - tabla.cantidad_registros;

                mensajes += "Se ha eliminado la tabla '" + tabla.nombre + "' de la base de datos '" + BDenUso + "' con éxito.\r\n";

                return "void";
            }
        }

        //Visita show_columns definido dentro de la gramática. Muestra las columnas de una tabla.
        override
        public string VisitShow_columns(SqlParser.Show_columnsContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso para mostrar columnas..." + Environment.NewLine;
            }

            String nTabla = context.GetChild(3).GetText();
            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay ninguna base de datos en uso.\r\n";
                return "Error";
            }
            if (hayVerbose)
            {
                mensajes += "Verificando que la tabla exista en la base de datos..." + Environment.NewLine;
            }
            if (masterTabla.containsTable(nTabla))
            {
                if (hayVerbose)
                {
                    mensajes += "Colocando los datos de las columnas en una talba..." + Environment.NewLine;
                }

                Tabla t = masterTabla.getTable(nTabla);
                resultados.ColumnCount = 3;
                if (t.columnas.Count == 0)
                    resultados.RowCount = 1;
                else
                {
                    resultados.RowCount = t.columnas.Count;
                    for (int i = 0; i < resultados.RowCount; i++)
                    {
                        resultados.Rows[i].Cells[0].Value = t.columnas.ElementAt(i);
                        resultados.Rows[i].Cells[1].Value = t.tipos_columnas.ElementAt(i);
                        for (int j = 0; j < t.restricciones.Count; j++)
                        {
                            if (t.restricciones.ElementAt(j).columnasPropias.Contains(t.columnas.ElementAt(i)))
                            {
                                if (resultados.Rows[i].Cells[2].Value != null)
                                {
                                    resultados.Rows[i].Cells[2].Value += ", ";
                                }
                                resultados.Rows[i].Cells[2].Value += t.restricciones.ElementAt(j).ToString();

                                if (t.restricciones.ElementAt(j).columnasForaneas != null)
                                    if (t.restricciones.ElementAt(j).columnasForaneas.Count != 0)
                                    {
                                        resultados.Rows[i].Cells[2].Value += "(";
                                        for (int k = 0; k < t.restricciones.ElementAt(j).columnasForaneas.Count; k++)
                                            if (k == 0)
                                                resultados.Rows[i].Cells[2].Value += t.restricciones.ElementAt(j).columnasForaneas.ElementAt(k);
                                            else
                                                resultados.Rows[i].Cells[2].Value += ", " + t.restricciones.ElementAt(j).columnasForaneas.ElementAt(k);
                                        resultados.Rows[i].Cells[2].Value += ")\"";
                                    }
                            }
                        }
                    }
                }
                resultados.Columns[0].HeaderText = "Columna";
                resultados.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                resultados.Columns[1].HeaderText = "Tipo";
                resultados.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                resultados.Columns[2].HeaderText = "Restricciones";
                resultados.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": No existe una tabla '" + nTabla + "' en la base de datos '" + BDenUso + "'.\r\n";
                return "Error";
            }    
        }

        //Visita and_expression definido dentro de la gramática.
        override
        public string VisitAnd_expression(SqlParser.And_expressionContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                //Obtener el primer hijo
                String and = Visit(context.GetChild(0));
                String tipo_and = and.Substring(0, 5).Trim();
                and = and.Substring(5);

                //Obtener el segundo hijo
                String difEq = Visit(context.GetChild(2));
                String tipo_difEq = difEq.Substring(0, 5).Trim();
                difEq = difEq.Substring(5);

                if (tipo_difEq.Equals("BOOL") && tipo_and.Equals("BOOL"))
                {
                    return "BOOL " + and + " " + difEq + " " + "AND";
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Los tipos de los elementos no concuerdan en un 'AND'." + Environment.NewLine;
                    return "ERROR" + and + " " + difEq + " " + "AND";
                }
            }
        }

        //Visita multi_id definido dentro de la gramática.
        override
        public string VisitMulti_id(SqlParser.Multi_idContext context)
        {
            Tabla tabla = ListaTablas[0];
            if (context.ChildCount == 1)
            {
                int num = tabla.columnas.IndexOf(context.GetText());
                if (num == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": No se encontro la columna '" + context.GetText() + "' en la tabla '"+tabla.nombre+"'." + Environment.NewLine;
                    return "";
                }

                return context.GetText();
            }
            else
            {
                int num = tabla.columnas.IndexOf(context.GetChild(0).GetText());
                if (num == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": No se encontro la columna '" + context.GetChild(0).GetText() + "' en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                    return "" + "," + Visit(context.GetChild(2));
                }

                return context.GetChild(0).GetText() + "," + Visit(context.GetChild(2));
            }
        }

        //Visita mayMin_expression definido dentro de la gramática.
        override
        public string VisitMayMin_expression(SqlParser.MayMin_expressionContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                //Obtener el primer hijo
                String mayMin = Visit(context.GetChild(0));
                String tipo_mayMin = mayMin.Substring(0, 5).Trim();
                mayMin = mayMin.Substring(5);

                //Obtener el segundo hijo
                String neg = Visit(context.GetChild(2));
                String tipo_neg = neg.Substring(0, 5).Trim();
                neg = neg.Substring(5);

                if (tipo_mayMin.Equals(tipo_neg)
                || (tipo_mayMin.Equals("FLOAT") && tipo_neg.Equals("INT")) 
                || (tipo_mayMin.Equals("INT") && tipo_neg.Equals("FLOAT"))
                || (tipo_mayMin.StartsWith("CHAR") && tipo_neg.Equals("DATE")) 
                || (tipo_mayMin.Equals("DATE") && tipo_neg.StartsWith("CHAR"))
                || (tipo_mayMin.Equals("NULL") && !tipo_neg.Equals("ERROR"))
                || (!tipo_mayMin.Equals("ERROR") && tipo_neg.Equals("NULL")))
                {
                    return "BOOL " + mayMin + " " + neg + " " + context.GetChild(1).GetText();
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Los tipos de los elementos no concuerdan en un '" + context.GetChild(1).GetText() + "'." + Environment.NewLine;
                    return "ERROR" + mayMin + " " + neg + " " + context.GetChild(1).GetText();
                }
            }
        }

        //Visita accion_addColumn definido dentro de la gramática. Agrega una columna a una tabla.
        override
        public string VisitAccion_addColumn(SqlParser.Accion_addColumnContext context)
        {
            //No constraints
            if(context.ChildCount == 4)
            {
                Tabla tabla = ListaTablas[0];
                String columna = context.GetChild(2).GetText();
                String tipo = Visit(context.GetChild(3));

                if (hayVerbose)
                {
                    mensajes += "Verificando que los tipos de las columnas sean validos..." + Environment.NewLine;
                }
                if (tipo.Equals("Error"))
                {
                    errores += "Error en línea " + context.start.Line +
                               ": El tipo CHAR debe contener al menos 1 caracter." + Environment.NewLine;
                    return "Error";

                }

                //Revisar si ya existe la columna
                if (hayVerbose)
                {
                    mensajes += "Verificando que si la tabla no contiene una columna que se llame igual..." + Environment.NewLine;
                }
                if (tabla.columnas.Contains(columna))
                {
                    errores += "Error en línea " + context.start.Line +
                               ": La tabla '" + tabla.nombre +
                               "' ya contiene una columna con el nombre '" +
                               columna + "'." + Environment.NewLine;
                    return "Error";
                }

                //Agregar la columna
                tabla.columnas.Add(columna);
                tabla.tipos_columnas.Add(tipo);
                
                //Agregar la columna a los datos
                FilaTabla contenido = getFilaTabla(tabla);

                for (int i = 0; i < contenido.getTamanio(); i++ )
                {
                    List<Object> fila = contenido.getRow(i);
                    if (tipo.Equals("INT"))
                    {
                        fila.Add(0);
                    }
                    else if (tipo.Equals("FLOAT"))
                    {
                        fila.Add(0.0);
                    }
                    else if (tipo.Equals("DATE"))
                    {
                        DateTime myDateTime = DateTime.Now;
                        fila.Add(myDateTime.ToString("yyyy-MM-dd"));

                    }
                    else if (tipo.StartsWith("CHAR"))
                    {
                        fila.Add("");
                    }
                    
                }

                mensajes += "Se ha agregado la columna '" + columna + "' en la tabla '" + tabla.nombre + "' con éxito." + Environment.NewLine;
                return "void";
            }
            //Con constraints
            else
            {
                Tabla tabla = ListaTablas[0];
                String columna = context.GetChild(2).GetText();
                String tipo = context.GetChild(3).GetText();

                if (hayVerbose)
                {
                    mensajes += "Verificando si la columna ya existe..." + Environment.NewLine;
                }
                //Revisar si ya existe la columna
                if (tabla.columnas.Contains(columna))
                {
                    errores += "Error en línea " + context.start.Line + 
                               ": La tabla '" + tabla.nombre + 
                               "' ya contiene una columna con el nombre '" + 
                               columna + "'." + Environment.NewLine;
                    return "Error";
                }

                //Agregar la columna
                tabla.columnas.Add(columna);
                tabla.tipos_columnas.Add(tipo);

                //Verificar las constraints agregadas
                if (hayVerbose)
                {
                    mensajes += "Verificando que las restricciones sean válidas..." + Environment.NewLine;
                }
                String resultadoConstraints = Visit(context.GetChild(4));
                if (resultadoConstraints.Equals("Error"))
                {
                    tabla.columnas.Remove(columna);
                    errores += "Error en línea " + context.start.Line +
                               ": La columna '" + columna +
                               "' no fue agregada en la tabla '" +
                               tabla.nombre + "' por errores en las Constraints." + Environment.NewLine;
                    return "Error";
                }

                //Agregar la columna a los datos
                FilaTabla contenido = getFilaTabla(tabla);

                for (int i = 0; i < contenido.getTamanio(); i++)
                {
                    List<Object> fila = contenido.getRow(i);
                    if (tipo.Equals("INT"))
                    {
                        fila.Add(0);
                    }
                    else if (tipo.Equals("FLOAT"))
                    {
                        fila.Add(0.0);
                    }
                    else if (tipo.Equals("DATE"))
                    {
                        DateTime myDateTime = DateTime.Now;
                        fila.Add(myDateTime.ToString("yyyy-MM-dd"));

                    }
                    else if (tipo.StartsWith("CHAR"))
                    {
                        fila.Add("");
                    }

                }


                mensajes += "Se ha agregado la columna '" + columna + "' en la tabla '" + tabla.nombre + "' con éxito." + Environment.NewLine;
                
                return "void";
            }
        }

        //Verifica de que tablas puede venir una columna
        private List<String> getTablasOrigen(String columna)
        {
            List<String> resultado = new List<String>();

            foreach (Tabla t in ListaTablas)
            {
                if (t.columnas.Contains(columna))
                {
                    resultado.Add(t.nombre);
                }
            }
            return resultado;
        }

        //Visita exp_Ident definido dentro de la gramática.
        override
        public string VisitExp_Ident(SqlParser.Exp_IdentContext context)
        {
            String nombreT = "", columna;
            int indiceTabla = -1;
            int indiceColumna = -1;

            Tabla tabla = null;

            if (context.GetText().Contains('.'))
            {
                String[] split = context.GetText().Split('.');
                nombreT = split[0];
                columna = split[1];

                tabla = null;

                foreach (Tabla t in ListaTablas)
                {
                    if (t.nombre.Equals(nombreT))
                    {
                        tabla = t;
                        indiceTabla = ListaTablas.IndexOf(t);
                    }
                }
                if (tabla == null)
                {
                    errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                    return "ERRORerr";
                }
            }
            else
            {
                columna = context.GetText();
                List<String> nombresPosibles = getTablasOrigen(columna);
                if(nombresPosibles.Count == 1 )
                {
                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombresPosibles[0]))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                            nombreT = nombresPosibles[0];
                        }
                    }
                }
                else
                {
                    if (nombresPosibles.Count == 0)
                    {
                        errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                    }
                    else
                    {
                        errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                    }

                    return "ERRORerr";
                }
            }

            indiceColumna = tabla.columnas.IndexOf(columna);
            if (indiceColumna == -1)
            {
                errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + nombreT + "'." + Environment.NewLine;
                return "ERRORerr";
            }

            if(tabla.tipos_columnas[indiceColumna].Equals("INT"))
            {
                return "INT  " + nombreT + "." + columna;
            }
            else if (tabla.tipos_columnas[indiceColumna].Equals("FLOAT"))
            {
                return "FLOAT" + nombreT + "." + columna;
            }
            else if (tabla.tipos_columnas[indiceColumna].Equals("DATE"))
            {
                return "DATE " + nombreT + "." + columna;
            }
            else if (tabla.tipos_columnas[indiceColumna].StartsWith("CHAR"))
            {
                return "CHAR " + nombreT + "." + columna;
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": Error desconocido." + Environment.NewLine;
                return "ERRORerr";
            }
        }

        //Visita exp_Int definido dentro de la gramática.
        override
        public string VisitExp_Int(SqlParser.Exp_IntContext context)
        {
            return "INT  " + context.GetText();
        }

        //Visita exp_Float definido dentro de la gramática.
        override
        public string VisitExp_Float(SqlParser.Exp_FloatContext context)
        {
            return "FLOAT" + context.GetText();
        }

        //Verifica si un string puede ser una fecha
        private bool isDate(String texto)
        {
            if (texto.StartsWith("'"))
                texto = texto.Substring(1, texto.Length - 2);
            String[] lista = texto.Split('-');

            if (lista.Length != 3)
            {
                return false;
            }
            if (lista[0].Length != 4)
            {
                return false;
            }
            if(!(lista[1].Length == 2 && lista[2].Length == 2))
            {
                return false;
            }

            int mes;
            int dia;
            int ano;

            if (!Int32.TryParse(lista[0], out ano))
            {
                return false;
            }
            if (Int32.TryParse(lista[1], out mes))
            {
                if (Int32.TryParse(lista[2], out dia))
                {
                    if(mes >= 1 && mes <= 12)
                    {
                        if (mes == 1
                         || mes == 3
                         || mes == 5
                         || mes == 7
                         || mes == 8
                         || mes == 10
                         || mes == 12)
                        {
                            if (dia >= 1 && dia <= 31)
                            {
                                return true;
                            }
                        }
                        if (mes == 4
                         || mes == 6
                         || mes == 9
                         || mes == 11)
                        {
                            if (dia >= 1 && dia <= 30)
                            {
                                return true;
                            }
                        }
                        if (mes == 2)
                        {
                            if (dia >= 1 && dia <= 28)
                            {
                                return true;
                            }
                            else if (dia == 29 && ano % 4 == 0 && (ano % 100 != 0 || (ano % 100 == 0 && ano % 400 == 0)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        //Visita exp_String definido dentro de la gramática.
        override
        public string VisitExp_String(SqlParser.Exp_StringContext context)
        {
            if (isDate(context.GetText()))
            {
                return "DATE " + context.GetText();
            }
            else
            {
                return "CHAR " + context.GetText();
            }
        }

        //Visita exp_Null definido dentro de la gramática.
        override
        public string VisitExp_Null(SqlParser.Exp_NullContext context)
        {
            return "NULL NULL";
        }

        //Visita mostrar_BD definido dentro de la gramática. Muestra las bases de datos existentes.
        override
        public string VisitMostrar_BD(SqlParser.Mostrar_BDContext context)
        {
            resultados.ColumnCount = 2;
            if (masterBD.basesDeDatos.Count == 0)
                resultados.RowCount = 1;
            else
            {
                resultados.RowCount = masterBD.basesDeDatos.Count;
                if (hayVerbose)
                {
                    mensajes += "Recopilando las bases de datos existentes..." + Environment.NewLine;
                }
                for (int i = 0; i < resultados.RowCount; i++)
                {
                    resultados.Rows[i].Cells[0].Value = masterBD.basesDeDatos.ElementAt(i).nombre;
                    resultados.Rows[i].Cells[1].Value = masterBD.basesDeDatos.ElementAt(i).cantidad_tablas + "";
                }
            }
            resultados.Columns[0].HeaderText = "Nombre";
            resultados.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            resultados.Columns[1].HeaderText = "Cantidad de tablas";
            resultados.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            
            return "void";
        }

        //Visita Columnas definido dentro de la gramática.
        override
        public string VisitColumnas(SqlParser.ColumnasContext context)
        {
            return context.GetChild(0).GetText() + " " + Visit(context.GetChild(1));
        }

        //Visita difEq_expression definido dentro de la gramática.
        override
        public string VisitDifEq_expression(SqlParser.DifEq_expressionContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                //Obtener el primer hijo
                String difEq = Visit(context.GetChild(0));
                String tipo_difEq = difEq.Substring(0, 5).Trim();
                difEq = difEq.Substring(5);

                //Obtener el segundo hijo
                String mayMin = Visit(context.GetChild(2));
                String tipo_mayMin = mayMin.Substring(0, 5).Trim();
                mayMin = mayMin.Substring(5);

                if (tipo_difEq.Equals(tipo_mayMin) 
                || (tipo_difEq.Equals("FLOAT") && tipo_mayMin.Equals("INT")) 
                || (tipo_difEq.Equals("INT") && tipo_mayMin.Equals("FLOAT"))
                || (tipo_difEq.StartsWith("CHAR") && tipo_mayMin.Equals("DATE")) 
                || (tipo_difEq.Equals("DATE") && tipo_mayMin.StartsWith("CHAR"))
                || (tipo_difEq.Equals("NULL") && !tipo_mayMin.Equals("ERROR"))
                || (!tipo_difEq.Equals("ERROR") && tipo_mayMin.Equals("NULL")))
                {
                    return "BOOL " + difEq + " " + mayMin + " " + context.GetChild(1).GetText();
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Los tipos de los elementos no concuerdan en un '" + context.GetChild(1).GetText() + "'." + Environment.NewLine;
                    return "ERROR" + difEq + " " + mayMin + " " + context.GetChild(1).GetText();
                }
            }
        }

        //Visita query_vacio definido dentro de la gramática. Permite los comentarios de línea y el uso ilimitado de ';'
        override
        public string VisitQuery_vacio(SqlParser.Query_vacioContext context)
        {
            return "void";
        }

        //Visita query definido dentro de la gramática.
        override
        public string VisitQuery(SqlParser.QueryContext context)
        {
            return Visit(context.GetChild(0));
        }

        //Visita id_completo_orderVarios definido dentro de la gramática.
        override
        public string VisitId_completo_orderVarios(SqlParser.Id_completo_orderVariosContext context)
        {
            String retornoOtro = Visit(context.GetChild(0));
            if (retornoOtro.StartsWith("ERROR"))
            {
                return "Error";
            }
            String retornoPropio;
            if (context.ChildCount == 3)
            {
                String nombreT, columna;
                Tabla tabla = null;
                int indiceTabla, indiceColumna;
                if (context.GetChild(2).GetText().Contains('.'))
                {
                    String[] split = context.GetChild(2).GetText().Split('.');
                    nombreT = split[0];
                    columna = split[1];

                    tabla = null;

                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombreT))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                        }
                    }
                    if (tabla == null)
                    {
                        errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                        return "ERRORerr";
                    }
                }
                else
                {
                    columna = context.GetChild(2).GetText();
                    List<String> nombresPosibles = getTablasOrigen(columna);
                    if (nombresPosibles.Count == 1)
                    {
                        foreach (Tabla t in ListaTablas)
                        {
                            if (t.nombre.Equals(nombresPosibles[0]))
                            {
                                tabla = t;
                                indiceTabla = ListaTablas.IndexOf(t);
                                nombreT = nombresPosibles[0];
                            }
                        }
                    }
                    else
                    {
                        if (nombresPosibles.Count == 0)
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                        }
                        else
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                        }

                        return "ERRORerr";
                    }
                }

                indiceColumna = tabla.columnas.IndexOf(columna);
                if (indiceColumna == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                    return "ERRORerr";
                }

                retornoPropio = "ASC" + tabla.nombre + "." + tabla.columnas[indiceColumna];
            }
            else
            {
                String nombreT, columna;
                Tabla tabla = null;
                int indiceTabla, indiceColumna;
                if (context.GetChild(2).GetText().Contains('.'))
                {
                    String[] split = context.GetChild(2).GetText().Split('.');
                    nombreT = split[0];
                    columna = split[1];

                    tabla = null;

                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombreT))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                        }
                    }
                    if (tabla == null)
                    {
                        errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                        return "ERRORerr";
                    }
                }
                else
                {
                    columna = context.GetChild(2).GetText();
                    List<String> nombresPosibles = getTablasOrigen(columna);
                    if (nombresPosibles.Count == 1)
                    {
                        foreach (Tabla t in ListaTablas)
                        {
                            if (t.nombre.Equals(nombresPosibles[0]))
                            {
                                tabla = t;
                                indiceTabla = ListaTablas.IndexOf(t);
                                nombreT = nombresPosibles[0];
                            }
                        }
                    }
                    else
                    {
                        if (nombresPosibles.Count == 0)
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                        }
                        else
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                        }

                        return "ERRORerr";
                    }
                }

                indiceColumna = tabla.columnas.IndexOf(columna);
                if (indiceColumna == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                    return "ERRORerr";
                }
                String orden;
                if (context.GetChild(3).GetText().Equals("ASC"))
                {
                    orden = "ASC";
                }
                else
                {
                    orden = "DES";
                }
                retornoPropio = orden + tabla.nombre + "." + tabla.columnas[indiceColumna];
            }
            return retornoOtro + "," + retornoPropio;
        }

        //Visita id_completo_orderSolo definido dentro de la gramática.
        override
        public string VisitId_completo_orderSolo(SqlParser.Id_completo_orderSoloContext context)
        {
            if (context.ChildCount == 1)
            {
                String nombreT, columna;
                Tabla tabla = null;
                int indiceTabla, indiceColumna;
                if (context.GetText().Contains('.'))
                {
                    String[] split = context.GetText().Split('.');
                    nombreT = split[0];
                    columna = split[1];

                    tabla = null;

                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombreT))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                        }
                    }
                    if (tabla == null)
                    {
                        errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                        return "ERRORerr";
                    }
                }
                else
                {
                    columna = context.GetText();
                    List<String> nombresPosibles = getTablasOrigen(columna);
                    if (nombresPosibles.Count == 1)
                    {
                        foreach (Tabla t in ListaTablas)
                        {
                            if (t.nombre.Equals(nombresPosibles[0]))
                            {
                                tabla = t;
                                indiceTabla = ListaTablas.IndexOf(t);
                                nombreT = nombresPosibles[0];
                            }
                        }
                    }
                    else
                    {
                        if (nombresPosibles.Count == 0)
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                        }
                        else
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                        }

                        return "ERRORerr";
                    }
                }

                indiceColumna = tabla.columnas.IndexOf(columna);
                if (indiceColumna == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                    return "ERRORerr";
                }

                return "ASC"+ tabla.nombre + "." + tabla.columnas[indiceColumna];
            }
            else
            {
                String nombreT, columna;
                Tabla tabla = null;
                int indiceTabla, indiceColumna;
                if (context.GetChild(0).GetText().Contains('.'))
                {
                    String[] split = context.GetChild(0).GetText().Split('.');
                    nombreT = split[0];
                    columna = split[1];

                    tabla = null;

                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombreT))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                        }
                    }
                    if (tabla == null)
                    {
                        errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                        return "ERRORerr";
                    }
                }
                else
                {
                    columna = context.GetChild(0).GetText();
                    List<String> nombresPosibles = getTablasOrigen(columna);
                    if (nombresPosibles.Count == 1)
                    {
                        foreach (Tabla t in ListaTablas)
                        {
                            if (t.nombre.Equals(nombresPosibles[0]))
                            {
                                tabla = t;
                                indiceTabla = ListaTablas.IndexOf(t);
                                nombreT = nombresPosibles[0];
                            }
                        }
                    }
                    else
                    {
                        if (nombresPosibles.Count == 0)
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                        }
                        else
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                        }

                        return "ERRORerr";
                    }
                }

                indiceColumna = tabla.columnas.IndexOf(columna);
                if (indiceColumna == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + tabla.nombre + "'." + Environment.NewLine;
                    return "ERRORerr";
                }
                String orden;
                if (context.GetChild(1).GetText().Equals("ASC"))
                {
                    orden = "ASC";
                }
                else
                {
                    orden = "DES";
                }
                return orden + tabla.nombre + "." + tabla.columnas[indiceColumna];
            }
        }

        //Visita constrain_check definido dentro de la gramática. Agrega una restricción check.
        override
        public string VisitConstrain_check(SqlParser.Constrain_checkContext context)
        {
            Restriccion restriccion = new Restriccion("CH");
            Tabla propia = ListaTablas[0];
            String postfixTipo = Visit(context.GetChild(3));
            String tipo = postfixTipo.Substring(0, 4);
            String postfix = "";
            if (!tipo.Equals("BOOL")) {
                errores += "Error en línea " + context.start.Line + ": La expresión '" + context.GetChild(3).GetText() + "' no es de tipo boolean.\r\n";
                return "Error";
            }
            postfix = postfixTipo.Substring(5);
            postfix = postfix.Replace(propia.nombre+".", "");
            //Nombrar y agregar la restriccion

            String nombreCH = context.GetChild(0).GetText();
            foreach (Restriccion r in propia.restricciones)
                if (r.nombre.Equals(nombreCH))
                {
                    errores += "Error en línea " + context.start.Line + ": El nombre '" + nombreCH + "' ya es utilizado en otra restriccion dentro de la tabla '" + propia.nombre + "'.\r\n";
                    return "Error";
                }

            restriccion.nombre = nombreCH;
            restriccion.restriccionCheck = postfix;
            propia.restricciones.Add(restriccion);

            FilaTabla datos = getFilaTabla(propia);
            if (datos != null)
            {
                for (int i = 0; i < datos.datos.elementos.Count; i++)
                {
                    List<Object> fila = new List<object>(datos.obtenerFila(i));
                    //Verificar si existe una fila que no cumpla con la restriccion
                    if (!verificarRestricciones(datos, fila, context.start.Line, i))
                    {
                        //Remover la restriccion
                        propia.restricciones.Remove(restriccion);
                        //Mensaje de error
                        errores = errores.Substring(0, errores.LastIndexOf("Error en línea "));
                        errores += "Error en línea " + context.start.Line +
                                   ": Los datos en la tabla '" + propia.nombre +
                                   "' no cumplen con la restriccion '" + restriccion.nombre +
                                   "'." + Environment.NewLine;
                        return "Error";
                    }
                }
            }
            mensajes += "Se ha agregado la Constraint '" + nombreCH + "' en la tabla '" + propia.nombre + "' con éxito." + Environment.NewLine;
            return "void";
        }

        //Visita full_query definido dentro de la gramática.
        override
        public string VisitFull_query(SqlParser.Full_queryContext context)
        {
            String retorno = "void";
            foreach (SqlParser.QueryContext query in context._queries)
            {
                resultados.DataSource = null;
                resultados.DataBindings.Clear();
                resultados.ColumnCount = 0;
                if (!Visit(query).Equals("void")) 
                { 
                    retorno = "Error";
                    break;
                }
            }

            if (cantInserts > 0)
                mensajes += "Se han insertando " + cantInserts + " registros con éxito durante la ejecución." + Environment.NewLine;
            //Serealizar masterBD
            
            serializarMasterBD();
            if (!BDenUso.Equals(""))
            {
                //Serealizar masterTabla
                serializarMasterTabla();
                //Guardar los datos de la tabla
                guardarDatosTablas();
            }
            
            return retorno;
        }

        //Visita usar_BD definido dentro de la gramática. Permite usar una base de datos.
        override
        public string VisitUsar_BD(SqlParser.Usar_BDContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando si hay una base de datos en uso..." + Environment.NewLine;
            }
            if (!BDenUso.Equals("")) {
                if (hayVerbose)
                {
                    mensajes += "Serializando los datos de la base de datos anterior..." + Environment.NewLine;
                }
                serializarMasterTabla(); 
                serializarMasterBD();
                guardarDatosTablas();
            }
            
            String nombre;
            nombre = context.GetChild(2).GetText();
            MasterBD bdatos;
            bdatos = deserializarMasterBD();

            if (hayVerbose)
            {
                mensajes += "Verificando que la base de datos a usar exista..." + Environment.NewLine;
            }
            if (bdatos.containsBD(nombre))
            {
                BDenUso = nombre;
                masterTabla = deserializarMasterTabla();
                cargarDatosTablas();
                mensajes += "La base de datos que usará a partir de este momento será '" + nombre + "'.\r\n";
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' no existe.\r\n";
                return "Error";
            }
        }

        //Visita multi_columnas definido dentro de la gramática.
        override
        public string VisitMulti_columnas(SqlParser.Multi_columnasContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                return Visit(context.GetChild(0)) + "," + Visit(context.GetChild(2));
            }
        }

        //Visita accion_DropConstraint definido dentro de la gramática. Elimina una restricción de una tabla.
        override
        public string VisitAccion_DropConstraint(SqlParser.Accion_DropConstraintContext context)
        {
            Tabla tablaA = ListaTablas.ElementAt(0);
            String nRestriccion = context.GetChild(2).GetText();
            if (hayVerbose)
            {
                mensajes += "Verificando que la restricción exista..." + Environment.NewLine;
            }
            for (int i = 0; i < tablaA.restricciones.Count;i++){
                Restriccion rAux = tablaA.restricciones.ElementAt(i);
                if (rAux.nombre.Equals(nRestriccion)) {
                    tablaA.restricciones.RemoveAt(i);
                    mensajes += "Se ha eliminado la restricción '" + nRestriccion + "' de la tabla '" + tablaA.nombre + "' con éxito.\r\n";
                    return "void";
                }
            }
            errores += "Error en la línea "+context.start.Line+": No existe la restriccion '"+nRestriccion+"' en la tabla '"+tablaA.nombre+"'.\r\n";
            return "Error";
        }

        //Visita constraint_completo definido dentro de la gramática.
        override
        public string VisitConstraint_completo(SqlParser.Constraint_completoContext context)
        {
            return Visit(context.GetChild(1));
        }

        //Visita constrain_fk definido dentro de la gramática. Permite crear una restricción de foreign key.
        override
        public string VisitConstrain_fk(SqlParser.Constrain_fkContext context)
        {
            Restriccion restriccion = new Restriccion("FK");

            Tabla propia = ListaTablas[0];
            Tabla foranea = masterTabla.getTable(context.GetChild(7).GetText());
            //Revisar si la tabla foranea existe
            if (foranea == null)
            {
                errores += "Error en línea " + context.start.Line + ": La tabla '" + context.GetChild(7).GetText()  + "' no existe."+ Environment.NewLine;
                return "Error";
            }
            //Obtener la lista de columnas de la tabla propia
            String listaPropiaS = Visit(context.GetChild(4));
            String[] listaPropia = listaPropiaS.Split(',');

            ListaTablas.Remove(propia);

            //Obtener la lista de columnas de la tabla foranea
            ListaTablas.Add(foranea);
            String listaForaneaS = Visit(context.GetChild(9));
            String[] listaForanea = listaForaneaS.Split(',');

            ListaTablas.Remove(foranea);
            ListaTablas.Add(propia);

            //Verificar si las listas son del mismo tamaño
            if (listaPropia.Length != listaForanea.Length)
            {
                errores += "Error en línea " + context.start.Line + ": La cantidad de columnas referenciadas no concuerdan con las propias." + Environment.NewLine;
                return "Error";
            }

            //Añadir los indices de la tabla propia a la restriccion
            foreach (String item in listaPropia)
            {
                if (!item.Equals(""))
                {
                    restriccion.columnasPropias.Add(item);
                }
                else
                {
                    return "Error";
                }
            }
            //Añadir los indices de la tabla foranea a la restriccion
            foreach (String item in listaForanea)
            {
                if (!item.Equals(""))
                {
                    restriccion.columnasForaneas.Add(item);
                }
                else
                {
                    return "Error";
                }
            }

            //Verificar si los tipos de ambas tablas concuerdan
            for (int i=0; i < restriccion.columnasPropias.Count; i++)
            {
                int inPro = propia.columnas.IndexOf(restriccion.columnasPropias[i]);
                int inFor = foranea.columnas.IndexOf(restriccion.columnasForaneas[i]);

                if (!propia.tipos_columnas[inPro].Equals(foranea.tipos_columnas[inFor]))
                {
                    if (!(propia.tipos_columnas[inPro].StartsWith("CHAR") && foranea.tipos_columnas[inPro].StartsWith("CHAR")))
                    { 
                        errores += "Error en línea " + context.start.Line + ": Los tipos de '" + propia.columnas[inPro] + "' de la tabla '" + propia.nombre + "' y '" + foranea.columnas[inFor] + "' de la tabla '" + foranea.nombre + "' ('" + propia.tipos_columnas[inPro] + "', '" + foranea.tipos_columnas[inFor] + "') no concuerdan." + Environment.NewLine;
                        return "Error";
                    }
                }
            }
            //Nombrar y agregar la restriccion
            
            String nombreFK = context.GetChild(0).GetText();
            foreach (Restriccion r in propia.restricciones)
                if (r.nombre.Equals(nombreFK))
                {
                    errores += "Error en línea " + context.start.Line + ": El nombre '" + nombreFK + "' ya es utilizado en otra restriccion dentro de la tabla '" + propia.nombre + "'.\r\n";
                    return "Error";
                }

            restriccion.nombre = nombreFK;

            propia.restricciones.Add(restriccion);
            restriccion.tabla = foranea.nombre;

            bool fueVerificado = false;
            foreach (Restriccion res in foranea.restricciones)
            {
                if (res.tipo.Equals("PK"))
                {
                    fueVerificado = true;
                    if(res.columnasPropias.Count == restriccion.columnasForaneas.Count){
                        //El tamaño concuerda
                        foreach(String nombreRestriccion in res.columnasPropias){
                            if(!restriccion.columnasForaneas.Contains(nombreRestriccion)){
                                //Error, la llave foranea no apunta a una llave primaria
                                errores += "Error en línea " + context.start.Line +
                                           ": La restriccion de llave foranea '" + restriccion.nombre +
                                           "' no apunta a una llave primaria de '" + foranea.nombre +
                                           "'." + Environment.NewLine;
                                return "Error";
                            }
                        }
                    }
                    else
                    {
                        //Error, la llave foranea no apunta a una llave primaria
                        errores += "Error en línea " + context.start.Line +
                                   ": La restriccion de llave foranea '" + restriccion.nombre +
                                   "' no apunta a una llave primaria de '" + foranea.nombre +
                                   "'." + Environment.NewLine;
                        return "Error";

                    }
                }
            }
            if (!fueVerificado)
            {
                //Error, la llave foranea no apunta a una llave primaria
                errores += "Error en línea " + context.start.Line +
                           ": La restriccion de llave foranea '" + restriccion.nombre +
                           "' no apunta a una llave primaria de '" + foranea.nombre +
                           "'." + Environment.NewLine;
                return "Error";
            }


            //Verificar que los datos de la tabla cumplan con la restriccion
            FilaTabla datos = getFilaTabla(propia);
            if (datos != null) 
            { 
                for (int i = 0; i < datos.datos.elementos.Count; i++)
                {
                    List<Object> fila = new List<object>(datos.obtenerFila(i));
                    //Verificar si existe una fila que no cumpla con la restriccion
                    if (!verificarRestricciones(datos, fila, context.start.Line, i))
                    {
                        //Remover la restriccion
                        propia.restricciones.Remove(restriccion);
                        //Mensaje de error
                        errores = errores.Substring(0, errores.LastIndexOf("Error en línea "));
                        errores += "Error en línea " + context.start.Line +
                                   ": Los datos en la tabla '" + propia.nombre +
                                   "' no cumplen con la restriccion '" + restriccion.nombre +
                                   "'." + Environment.NewLine;
                        return "Error";
                    }
                }
            }
            mensajes += "Se ha agregado la Constraint '" + nombreFK + "' en la tabla '" + propia.nombre + "' con éxito." + Environment.NewLine;
            return "void";
        }

        //Visita paren_expression definido dentro de la gramática.
        override
        public string VisitParen_expression(SqlParser.Paren_expressionContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                return Visit(context.GetChild(1));
            }
        }

        //Visita multi_constraint_completo definido dentro de la gramática.
        override
        public string VisitMulti_constraint_completo(SqlParser.Multi_constraint_completoContext context)
        {
            if (context.ChildCount == 1)
            {
               return Visit(context.GetChild(0));
            }
            else
            {
                String multi = Visit(context.GetChild(0));
                String solo = Visit(context.GetChild(2));
                if(multi.Equals("void") && solo.Equals("void")){
                    return "void";
                }
                else
                {
                    return "Error";
                }
            }
        }

        //Visita neg_expression definido dentro de la gramática.
        override
        public string VisitNeg_expression(SqlParser.Neg_expressionContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else
            {
                //Obtener el hijo
                String neg = Visit(context.GetChild(0));
                String tipo_neg = neg.Substring(0, 5).Trim();
                neg = neg.Substring(5);

                if (tipo_neg.Equals("BOOl") || tipo_neg.Equals("NULL"))
                {
                    return "BOOL " + neg + " " + "NOT";
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Los tipos de los elementos no concuerdan, para negarlo debe ser booleano." + Environment.NewLine;
                    return "ERROR" + neg + " " + "NOT";
                }
            }
        }

        //Visita show_tables definido dentro de la gramática. Muestra las tablas de una base de datos.
        override
        public string VisitShow_tables(SqlParser.Show_tablesContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya base de datos en uso para mostrar tablas..." + Environment.NewLine;
            }
            if (BDenUso.Equals("")) {
                errores += "Error en línea "+context.start.Line+": No se encuentra en uso ninguna base de datos.\r\n";
                return "Error";
            }
            if (hayVerbose)
            {
                mensajes += "Colocando los datos a mostrar en una tabla..." + Environment.NewLine;
            }
            resultados.ColumnCount = 2;
            if (masterTabla.tablas.Count == 0)
                resultados.RowCount = 1;
            else
            {
                resultados.RowCount = masterTabla.tablas.Count;
                for (int i = 0; i < resultados.RowCount; i++)
                {
                    resultados.Rows[i].Cells[0].Value = masterTabla.tablas.ElementAt(i).nombre;
                    resultados.Rows[i].Cells[1].Value = masterTabla.tablas.ElementAt(i).cantidad_registros;
                }
            }
            resultados.Columns[0].HeaderText = "Nombre";
            resultados.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            resultados.Columns[1].HeaderText = "Cant. de registros";
            resultados.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            
            mensajes += "Se han mostrado todas las tablas (" + masterTabla.tablas.Count + ") que contiene '" + BDenUso + "' con éxito.\r\n";
            return "void";
        }

        //Visita identificador_completo definido dentro de la gramática.
        override
        public string VisitIdentificador_completo(SqlParser.Identificador_completoContext context)
        {
            String nombreT = "", columna;
            int indiceTabla = -1;
            int indiceColumna = -1;

            Tabla tabla = null;

            if (context.ChildCount == 1)
            {
                if (context.GetText().Contains('.'))
                {
                    String[] split = context.GetText().Split('.');
                    nombreT = split[0];
                    columna = split[1];

                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombreT))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                        }
                    }
                    if (tabla == null)
                    {
                        errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                        return "Error";
                    }
                }
                else
                {
                    columna = context.GetText();
                    List<String> nombresPosibles = getTablasOrigen(columna);
                    if (nombresPosibles.Count == 1)
                    {
                        foreach (Tabla t in ListaTablas)
                        {
                            if (t.nombre.Equals(nombresPosibles[0]))
                            {
                                tabla = t;
                                indiceTabla = ListaTablas.IndexOf(t);
                                nombreT = nombresPosibles[0];
                            }
                        }
                    }
                    else
                    {
                        if (nombresPosibles.Count == 0)
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                        }
                        else
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                        }

                        return "Error";
                    }
                }

                indiceColumna = tabla.columnas.IndexOf(columna);
                if (indiceColumna == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + nombreT + "'." + Environment.NewLine;
                    return "Error";
                }

                if (tabla.tipos_columnas[indiceColumna].Equals("INT"))
                {
                    return nombreT + "." + columna;
                }
                else if (tabla.tipos_columnas[indiceColumna].Equals("FLOAT"))
                {
                    return nombreT + "." + columna;
                }
                else if (tabla.tipos_columnas[indiceColumna].Equals("DATE"))
                {
                    return nombreT + "." + columna;
                }
                else if (tabla.tipos_columnas[indiceColumna].StartsWith("CHAR"))
                {
                    return nombreT + "." + columna;
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Error desconocido." + Environment.NewLine;
                    return "Error";
                }
            }
            else
            {
                String res = Visit(context.GetChild(0))+",";
                if (context.GetChild(2).GetText().Contains('.'))
                {
                    String[] split = context.GetChild(2).GetText().Split('.');
                    nombreT = split[0];
                    columna = split[1];

                    foreach (Tabla t in ListaTablas)
                    {
                        if (t.nombre.Equals(nombreT))
                        {
                            tabla = t;
                            indiceTabla = ListaTablas.IndexOf(t);
                        }
                    }
                    if (tabla == null)
                    {
                        errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe existe en este contexto." + Environment.NewLine;
                        return "Error";
                    }
                }
                else
                {
                    columna = context.GetChild(2).GetText();
                    List<String> nombresPosibles = getTablasOrigen(columna);
                    if (nombresPosibles.Count == 1)
                    {
                        foreach (Tabla t in ListaTablas)
                        {
                            if (t.nombre.Equals(nombresPosibles[0]))
                            {
                                tabla = t;
                                indiceTabla = ListaTablas.IndexOf(t);
                                nombreT = nombresPosibles[0];
                            }
                        }
                    }
                    else
                    {
                        if (nombresPosibles.Count == 0)
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no pertenece a ninguna tabla." + Environment.NewLine;
                        }
                        else
                        {
                            errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
                        }

                        return "Error";
                    }
                }

                indiceColumna = tabla.columnas.IndexOf(columna);
                if (indiceColumna == -1)
                {
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' no existe en la tabla '" + nombreT + "'." + Environment.NewLine;
                    return "Error";
                }

                if (tabla.tipos_columnas[indiceColumna].Equals("INT"))
                {
                    return res+nombreT + "." + columna;
                }
                else if (tabla.tipos_columnas[indiceColumna].Equals("FLOAT"))
                {
                    return res+nombreT + "." + columna;
                }
                else if (tabla.tipos_columnas[indiceColumna].Equals("DATE"))
                {
                    return res+nombreT + "." + columna;
                }
                else if (tabla.tipos_columnas[indiceColumna].StartsWith("CHAR"))
                {
                    return res+nombreT + "." + columna;
                }
                else
                {
                    errores += "Error en línea " + context.start.Line + ": Error desconocido." + Environment.NewLine;
                    return "Error";
                }
            }
        }

        //Visita multi_accion definido dentro de la gramática.
        override
        public string VisitMulti_accion(SqlParser.Multi_accionContext context)
        {
            if (context.ChildCount == 1)
            {
                return Visit(context.GetChild(0));
            }
            else {
                if (context.GetText().Contains("RENAMETO")) {
                    errores += "";
                    return "Error";
                }
                else{
                    if (Visit(context.GetChild(0)).Equals("Error"))
                        return "Error";
                    else
                        if (Visit(context.GetChild(2)).Equals("Error"))
                            return "Error";
                        else
                            return "void";
                }
            }
        }

        //Visita crear_tabla definido dentro de la gramática. Permite crear una tabla en una base de datos.
        override
        public string VisitCrear_tabla(SqlParser.Crear_tablaContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que haya una base de datos en uso..." + Environment.NewLine;
            }
            if (BDenUso.Equals(""))
            {
                errores += "Error en linea " + context.start.Line + ": No hay base de datos en uso para agregar la tabla " + context.GetChild(2).GetText() + "." + BDenUso+ Environment.NewLine;
                return "Error";
            }

            //Generar la nueva tabla
            Tabla nueva = new Tabla();
            nueva.nombre = context.GetChild(2).GetText();

            //Verificar si la tabla ya existe
            if (hayVerbose)
            {
                mensajes += "Verificando que la tabla no exista..." + Environment.NewLine;
            }
            if (masterTabla.containsTable(nueva.nombre))
            {
                errores += "Error en linea " + context.start.Line + ": La base de datos " + BDenUso + " ya contiene una tabla " + nueva.nombre + "." + Environment.NewLine;
                return "Error";
            }
            else
            {
                masterTabla.agregarTabla(nueva);
            }

            if (context.ChildCount == 5)
            {
                //Crear el archivo vacio de la tabla
                if (hayVerbose)
                {
                    mensajes += "Creando los archivos..." + Environment.NewLine;
                }
                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), BDenUso);
                string fileName = nueva.nombre + ".dat";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();


                mensajes += "Se ha creado la tabla '" + nueva.nombre + "' en '" + BDenUso + "' con éxito.\r\n";
                return "void";
            }

            //Generar las columnas
            List<String> columnas = Visit(context.GetChild(4)).Split(',').ToList();
            
            //Verificar si los tipos estan correctos
            if (hayVerbose)
            {
                mensajes += "Verificando que los tipos son correctos..." + Environment.NewLine;
            }
            foreach (String columna in columnas)
            {
                String[] tupla = columna.Split(' ');
                if (tupla[1].Equals("Error"))
                {
                    masterTabla.borrarTabla(nueva.nombre);
                    errores += "Error en línea " + context.start.Line +
                            ": El tipo CHAR debe contener al menos 1 caracter." + Environment.NewLine;
                    return "Error";

                }
            }

            //Generar las columnas
            if (hayVerbose)
            {
                mensajes += "Generando las columnas de la tabla..." + Environment.NewLine;
            }
            if (!nueva.generarColumnas(columnas))
            {
                masterTabla.borrarTabla(nueva.nombre);
                errores += "Error en línea " + context.start.Line +
                           ": Se declararon dos columnas con el mismo nombre '" + nueva.columnas[0] + "'." + Environment.NewLine;
                return "Error";
            }

            //En caso que no haya constraints
            if (context.ChildCount == 6)
            {
                //Crear el archivo vacio de la tabla
                if (hayVerbose)
                {
                    mensajes += "Creando los archivos..." + Environment.NewLine;
                } 
                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), BDenUso);
                string fileName = nueva.nombre + ".dat";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();


                mensajes += "Se ha creado la tabla '" + nueva.nombre + "' en '" + BDenUso + "' con éxito.\r\n";
                return "void";
            }
            //Caso en que si hay constraints
            else
            {
                //Manejar las constraint
                if (hayVerbose)
                {
                    mensajes += "Verificando que las restricciones sean válidas..." + Environment.NewLine;
                }
                ListaTablas = new List<Tabla>();
                ListaTablas.Add(nueva);
                if(Visit(context.GetChild(6)).Equals("Error")){
                    masterTabla.borrarTabla(nueva.nombre);
                    return "Error";
                }

                //Crear el archivo vacio de la tabla
                if (hayVerbose)
                {
                    mensajes += "Creando los archivos..." + Environment.NewLine;
                } 
                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), BDenUso);
                string fileName = nueva.nombre + ".dat";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();

                mensajes += "Se ha creado la tabla '" + nueva.nombre + "' en '" + BDenUso + "' con éxito.\r\n";
                return "void";
            }
        }

        //Visita crear_BD definido dentro de la gramática. Permite crear una base de datos
        override
        public string VisitCrear_BD(SqlParser.Crear_BDContext context)
        {
            String nombre;
            nombre = context.GetChild(2).GetText();
            if (hayVerbose) {
                mensajes += "Verificando que la base de datos a crear no exista..."+Environment.NewLine;
            }
            if (!masterBD.containsBD(nombre))
            {
                BaseDatos nBaseDatos = new BaseDatos(nombre);
                masterBD.agregarBD(nBaseDatos);

                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), nombre);
                System.IO.Directory.CreateDirectory(path);
                string fileName = nombre + ".xml";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();
                serializarMasterBD();

                mensajes += "La base de datos '" + nombre + "' ha sido creada exitosamente.\r\n";
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' ya existe en el DBMS.\r\n";
                return "Error";
            }
        }

        //Visita constrain_pk definido dentro de la gramática. Crea una restricción de primary key.
        override
        public string VisitConstrain_pk(SqlParser.Constrain_pkContext context)
        {
            Restriccion restriccion = new Restriccion("PK");
            String listaS = Visit(context.GetChild(4));
            String[] lista = listaS.Split(',');

            foreach (String item in lista){
                if (!item.Equals(""))
                {
                    restriccion.columnasPropias.Add(item);
                }
                else
                {
                    return "Error";
                }
            }
            

            String nombrePK = context.GetChild(0).GetText();
            Tabla tActual = ListaTablas[0];
            foreach (Restriccion r in tActual.restricciones)
            {
                if (r.nombre.Equals(nombrePK))
                {
                    errores += "Error en línea " + context.start.Line + ": El nombre '" + nombrePK + "' ya es utilizado en otra restriccion dentro de la tabla '" + tActual.nombre + "'.\r\n";
                    return "Error";
                }
                if (r.tipo.Equals("PK"))
                {
                    errores += "Error en línea " + context.start.Line + ": No se pueden admitir dos llaves primarias en la tabla '" + tActual.nombre + "'.\r\n";
                    return "Error";
                }
            }
            restriccion.nombre = nombrePK;

            ListaTablas[0].restricciones.Add(restriccion);

            FilaTabla datos = getFilaTabla(tActual);
            if (datos !=null)
                if (!verificarRestriccionPK(datos, context.start.Line))
                {
                    //Remover la restriccion
                    tActual.restricciones.Remove(restriccion);
                    //Mensaje de error
                    //errores = errores.Substring(0, errores.LastIndexOf("Error en línea "));
                    errores += "Error en línea " + context.start.Line +
                               ": Los datos en la tabla '" + tActual.nombre +
                               "' no cumplen con la restriccion '" + restriccion.nombre +
                               "'." + Environment.NewLine;
                    return "Error";
                }
            
            mensajes += "Se ha agregado la Constraint '" + nombrePK + "' en la tabla '" + tActual.nombre + "' con éxito." + Environment.NewLine;
            return "void";
        }

        //Visita accion_addConstraint definido dentro de la gramática. Permite agregar una restricción.
        override
        public string VisitAccion_addConstraint(SqlParser.Accion_addConstraintContext context)
        {
            if (hayVerbose)
            {
                mensajes += "Verificando que las restricciones sean válidas..." + Environment.NewLine;
            }
            if (Visit(context.GetChild(1)).Equals("void"))
            {
                return "void";
            }
            else
            {
                return "Error";
            }
        }

        //Deserializa los datos de la metadata de las bases de datos
        private MasterBD deserializarMasterBD() {
            MasterBD bdatos;
            StreamReader reader = null;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            try
            {
                reader = new StreamReader("Databases\\masterBDs.xml");
                bdatos = (MasterBD)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (DirectoryNotFoundException e)
            {
                bdatos = new MasterBD();
                System.IO.Directory.CreateDirectory("Databases");
                System.IO.FileStream fs = System.IO.File.Create("Databases\\masterBDs.xml");
                fs.Close();
            }
            catch (InvalidOperationException e) {
                bdatos = new MasterBD();
                reader.Close();
            }
            return bdatos;
        }

        //Deserializa los datos de la metadata de las tablas de una base de datos
        private MasterTabla deserializarMasterTabla() {
            //Deserealizar el archivo maestro de tablas
            MasterTabla mTabla;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterTabla));
            StreamReader reader = null;
            try
            {
                reader = new StreamReader("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mTabla = (MasterTabla)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (DirectoryNotFoundException e)
            {
                mTabla = new MasterTabla();
                System.IO.Directory.CreateDirectory("Databases\\" + BDenUso);
                System.IO.FileStream fs = System.IO.File.Create("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                fs.Close();
            }
            catch (InvalidOperationException e) {
                mTabla = new MasterTabla();
                reader.Close();
            }
            return mTabla;
        }

        //Serializa los datos de la metadata de las bases de datos
        private void serializarMasterBD() {
            XmlSerializer mySerializer = new XmlSerializer(typeof(MasterBD));
            StreamWriter myWriter = new StreamWriter("Databases\\masterBDs.xml");
            mySerializer.Serialize(myWriter, masterBD);
            myWriter.Close();
        }

        //Serializa los datos de la metadata de las tablas de una base de datos
        private void serializarMasterTabla() {
            XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
            StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
            mySerializer.Serialize(myWriter, masterTabla);
            myWriter.Close();
        }

        //Carga los datos de las tablas
        private void cargarDatosTablas()
        {
            datosTablas = new List<FilaTabla>();
            foreach (Tabla tabla in masterTabla.tablas)
            {
                FilaTabla datos = new FilaTabla(tabla, BDenUso);
                datos.cargar();
                datosTablas.Add(datos);
            }
        }

        //Guarda los datos de las tablas
        private void guardarDatosTablas()
        {
            foreach (FilaTabla filaTabla in datosTablas)
            {
                filaTabla.guardar();
            }
        }

        //Obtiene la tabla que busca y los datos de ella
        private FilaTabla getFilaTabla(Tabla tabla)
        {
            foreach (FilaTabla filaTabla in datosTablas)
            {
                if (tabla.nombre.Equals(filaTabla.tabla.nombre))
                {
                    return filaTabla;
                }
            }
            return null;
        }

        //Compara dos listas de elementos
        private int comparar(object[] uno, object[] dos)
        {
            //Devuelve 1 si uno es mayor a dos
            //Devuelve 0 si son iguales
            //Devuelve -1 si dos es mayor a uno
        
            foreach (String[] ordenActual in ordenDeColumnas)
            {
                //Obtener ASC o DESC
                int cambio;
                if (ordenActual[0].Equals("ASC"))
                {
                    cambio = -1;
                }
                else
                {
                    cambio = 1;
                }
                //Obtener el indice de la lista de objetos a comparar
                int indice = Convert.ToInt32(ordenActual[2]);
                int comparacion = 0;
                //Verificar Null
                if (uno[indice] == null && dos[indice] == null)
                {
                    comparacion = 0;
                }
                else if (uno[indice] == null)
                { 
                    comparacion = 1;
                }
                else if (dos[indice] == null)
                {
                    comparacion = -1;
                }
                //Comparar segun el tipo
                else if (ordenActual[3].Equals("INT"))
                {
                    comparacion = ((Int32) uno[indice]).CompareTo((Int32) dos[indice]);
                }
                else if (ordenActual[3].Equals("FLOAT"))
                {
                    comparacion = compareSingle(((Single)uno[indice]), (((Single)dos[indice])));
                }
                else if (ordenActual[3].StartsWith("CHAR"))
                {
                    comparacion = ((String)uno[indice]).CompareTo((String)dos[indice]);
                }
                else if (ordenActual[3].Equals("DATE"))
                {
                    DateTime fechaUno = DateTime.Parse((String)uno[indice]);
                    DateTime fechaDos = DateTime.Parse((String)dos[indice]);

                    comparacion = fechaUno.CompareTo(fechaDos);
                }
                else
                {
                    throw new NotImplementedException();
                }
                //Decidir que hacer segun la comparacion
                if (comparacion < 0)
                {
                    return 1 * cambio;
                }
                else if (comparacion > 0)
                {
                    return -1 * cambio;
                }
            }
            //Son iguales
            return 0;
        }

        //Parte la lista en dos sublistas y elige el pivote
        private int Partition(List<object[]> particion, int left, int right)
        {
            object[] pivot = particion[left];
            while (true)
            {
                while (comparar(particion[left], pivot) == -1)
                    left++;

                while (comparar(particion[right], pivot) == 1)
                    right--;

                if (left < right)
                {
                    object[] temp = particion[right];
                    particion[right] = particion[left];
                    particion[left] = temp;
                    if (comparar(particion[right], particion[left]) == 0)
                    {
                        left++;
                        right--;
                    }

                }
                else
                {
                    return right;
                }
            }
        }

        //Realiza el ordenamiento con el algoritmo quicksort
        private void QuickSort_Recursive(List<object[]> arr, int left, int right)
        {
            // For Recusrion
            if (left < right)
            {
                int pivot = Partition(arr, left, right);

                if (pivot > 1)
                    QuickSort_Recursive(arr, left, pivot - 1);

                if (pivot + 1 < right)
                    QuickSort_Recursive(arr, pivot + 1, right);
            }
        }

        //Método para comparar dos valores float 
        private int compareSingle(Single uno, Single dos)
        {
            if (Math.Abs(uno - dos) <= Single.Epsilon)
            {
                return 0;
            }
            else
            {
                return uno.CompareTo(dos);
            }
        }

        //Metodo para comparar un float y un int
        private int compareSingle(Single uno, Int32 dos)
        {
            return compareSingle(uno, Convert.ToSingle(dos));
        }

        //Metodo para comparar un int y un float
        private int compareSingle(Int32 uno, Single dos)
        {
            return compareSingle(Convert.ToSingle(uno), dos);
        }
    }
}