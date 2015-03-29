using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BasesDeDatos_Proyecto1
{
    class TypeSystem : SqlBaseVisitor<String>
    {
        public String errores;
        public String mensajes;
        public DataGridView resultados;
        public String BDenUso;
        private MasterTabla masterTabla;
        private List<Tabla> ListaTablas;
        private Stack<String> expStack;

        public TypeSystem() {
            errores = "";
            mensajes = "";
            BDenUso = "";
            resultados = new DataGridView();
            masterTabla = new MasterTabla();
            ListaTablas = new List<Tabla>();
            expStack = new Stack<String>();
        }

        override
        public string VisitId_completo(SqlParser.Id_completoContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitAsignacion(SqlParser.AsignacionContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitTipo(SqlParser.TipoContext context)
        {
            throw new NotImplementedException();
        }

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

        override
        public string VisitSelect(SqlParser.SelectContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitValor_completo(SqlParser.Valor_completoContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitAccion_rename(SqlParser.Accion_renameContext context)
        {
            String nombre = ListaTablas.ElementAt(0).nombre;
            String nuevoNombre = context.GetChild(2).GetText();

            if (masterTabla.containsTable(nuevoNombre)) {
                errores += "Error en linea " + context.start.Line + ": Ya existe una tabla con el nombre '" + nuevoNombre + "' por lo que no se le puede cambiar de nombre a la tabla '" + nombre + "'.\r\n";
                return "Error";
            }

            foreach (Tabla t in masterTabla.tablas)
            {
                if (t.nombre.Equals(nombre))
                {
                    t.nombre = nuevoNombre;

                    String pathViejo = "Databases\\" + BDenUso + "\\" + nombre + ".dat";
                    String pathNuevo = "Databases\\" + BDenUso + "\\" + nuevoNombre + ".dat";
                    System.IO.File.Move(pathViejo, pathNuevo);

                    XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                    StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                    mySerializer.Serialize(myWriter, masterTabla);
                    myWriter.Close();

                    mensajes += "Se ha renombrado la tabla '" + nombre + "' a '" + nuevoNombre + "' con éxito.\r\n";

                    break;
                }
            }
            
            return "void";
        }

        override
        public string VisitBotar_BD(SqlParser.Botar_BDContext context)
        {
            String nombre = context.GetChild(2).GetText();

            MasterBD bdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            bdatos = (MasterBD)serializer.Deserialize(reader);
            reader.Close();

            if (bdatos.containsBD(nombre)) 
            {
                DialogResult resultado = MessageBox.Show("Seguro que desea botar "+nombre+ " con "+bdatos.getRegistros(nombre)+ " registros","Confirmación", MessageBoxButtons.YesNo);

                if (resultado == DialogResult.Yes)
                {
                    bdatos.borrarBD(nombre);

                    if (nombre.Equals(BDenUso))
                        BDenUso = "";

                    XmlSerializer mySerializer = new XmlSerializer(typeof(MasterBD));
                    StreamWriter myWriter = new StreamWriter("Databases\\masterBDs.xml");
                    mySerializer.Serialize(myWriter, bdatos);
                    myWriter.Close();

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
                errores = "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' no existe en el DBMS.\r\n";
                return "Error";
            }
        }

        override
        public string VisitInsert(SqlParser.InsertContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitUpdate(SqlParser.UpdateContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitAlter_table(SqlParser.Alter_tableContext context)
        {
            String nTabla = context.GetChild(2).GetText();
            if (BDenUso.Equals("")) { 
                errores += "Error en línea "+context.start.Line+": No hay base de datos en uso por lo que no se puede alterar la tabla.";
                return "Error";
            }

            //Deserealizar el archivo maestro de tablas
            MasterTabla mTabla;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterTabla));
            StreamReader reader = new StreamReader("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
            try
            {
                mTabla = (MasterTabla)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                mTabla = new MasterTabla();
            }
            reader.Close();

            //Verificar si la tabla existe
            if (!mTabla.containsTable(nTabla))
            {
                errores += "Error en linea " + context.start.Line + ": La base de datos " + BDenUso + " no contiene una tabla " + nTabla + "." + Environment.NewLine;
                return "Error";
            }

            //Mandar datos
            masterTabla = mTabla;
            ListaTablas = new List<Tabla>();
            ListaTablas.Add(masterTabla.getTable(nTabla));

            return Visit(context.GetChild(3));
        }

        override
        public string VisitAccion_DropColumn(SqlParser.Accion_DropColumnContext context)
        {
            Tabla tActual = ListaTablas[0];
            String cBorrar = context.GetChild(2).GetText();
            if (!tActual.columnas.Contains(cBorrar)) { 
                errores += "Error en la línea "+context.start.Line+": La tabla '"+tActual.nombre+"' no contiene la columna '"+cBorrar+"'.\r\n";
                return "Error";
            }
            
            foreach (Restriccion r in tActual.restricciones){
                if (r.columnasPropias.Contains(cBorrar)) {
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
                            errores += "Error en la línea " + context.start.Line + ": La columna '" + cBorrar + "' es referenciada como llave foránea de la columna '"+r.columnasPropias.ElementAt(i)+"' de la tabla '"+t.nombre+"'.\r\n";
                            return "Error";
                        }
                    }    
                }
            }

            int ind = tActual.columnas.IndexOf(cBorrar);
            tActual.columnas.Remove(cBorrar);
            tActual.tipos_columnas.RemoveAt(ind);
            XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
            StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
            mySerializer.Serialize(myWriter, masterTabla);
            myWriter.Close();
            mensajes += "Se ha removido la columna '" + cBorrar + "' de la tabla '" + tActual.nombre + "' con éxito.\r\n";
            return "void";
        }

        override
        public string VisitDelete(SqlParser.DeleteContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitRenombrar_BD(SqlParser.Renombrar_BDContext context)
        {
            String nombre = context.GetChild(2).GetText();
            String nuevoNombre = context.GetChild(5).GetText();

            if (nombre.Equals(BDenUso))
            {
                BDenUso = nuevoNombre;
            }

            MasterBD bdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            bdatos = (MasterBD)serializer.Deserialize(reader);
            reader.Close();

            if (bdatos.containsBD(nombre))
            {
                foreach (BaseDatos bd in bdatos.basesDeDatos)
                {
                    if (bd.nombre.Equals(nombre))
                    {
                        bd.nombre = nuevoNombre;

                        String pathViejo = "Databases\\" + nombre + "\\" + nombre + ".xml";
                        String pathNuevo = "Databases\\" + nombre + "\\" + nuevoNombre + ".xml";
                        System.IO.File.Move(pathViejo, pathNuevo);

                        pathViejo = "Databases\\" + nombre;
                        pathNuevo = "Databases\\" + nuevoNombre;
                        Directory.Move(pathViejo, pathNuevo);

                        XmlSerializer mySerializer = new XmlSerializer(typeof(MasterBD));
                        StreamWriter myWriter = new StreamWriter("Databases\\masterBDs.xml");
                        mySerializer.Serialize(myWriter, bdatos);
                        myWriter.Close();

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

        override
        public string VisitBotar_table(SqlParser.Botar_tableContext context)
        {
            Tabla tabla = masterTabla.getTable(context.GetChild(2).GetText());
            if (tabla == null)
            {
                errores += "Error en linea" + context.start.Line +
                           "No existe la tabla'" + context.GetChild(2).GetText() + 
                           "' en la base de datos '" + BDenUso + "'."+Environment.NewLine;
                return "Error";
            }
            bool esReferenciada = false;
            String referencia = "";
            foreach (Tabla t in masterTabla.tablas)
            {
                foreach (Restriccion restriccion in t.restricciones)
                {
                    if (restriccion.tipo.Equals("FK") && restriccion.tabla.Equals(tabla.nombre))
                    {
                        esReferenciada = true;
                        referencia = t.nombre;
                        goto End;
                        
                    }
                }
            }
            End:
            if (esReferenciada)
            {
                errores += "Error en linea" + context.start.Line +
                           "La tabla '" + tabla.nombre +
                           "' es referenciada por '" + referencia +
                           "', bote la restriccion antes de botar la tabla." + Environment.NewLine;
                return "Error";
            }
            else
            {
                masterTabla.tablas.Remove(tabla);

                String path = "Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat";
                System.IO.File.Delete(path);


                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mySerializer.Serialize(myWriter, masterTabla);
                myWriter.Close();

                return "void";
            }
        }

        override
        public string VisitShow_columns(SqlParser.Show_columnsContext context)
        {
            String nTabla = context.GetChild(3).GetText();
            if (BDenUso.Equals(""))
            {
                errores += "Error en línea " + context.start.Line + ": No hay ninguna base de datos en uso.\r\n";
                return "Error";
            }
            MasterTabla mTabla;
            mTabla = deserializarMasterTabla();
            if (mTabla.containsTable(nTabla))
            {
                Tabla t = mTabla.getTable(nTabla);
                resultados.RowCount = t.columnas.Count + 1;
                resultados.ColumnCount = 3;
                resultados.Rows[0].Cells[0].Value = "Columna";
                resultados.Rows[0].Cells[1].Value = "Tipo";
                resultados.Rows[0].Cells[2].Value = "Restricciones";
                for (int i = 1; i < resultados.RowCount; i++)
                {
                    resultados.Rows[i].Cells[0].Value = t.columnas.ElementAt(i - 1);
                    resultados.Rows[i].Cells[1].Value = t.tipos_columnas.ElementAt(i - 1);
                    for (int j = 0; j < t.restricciones.Count; j++)
                    {
                        if (t.restricciones.ElementAt(j).columnasPropias.Contains(t.columnas.ElementAt(i - 1)))
                        {
                            if (resultados.Rows[i].Cells[2].Value != null)
                            {
                                resultados.Rows[i].Cells[2].Value += ", ";
                            }                       
                            resultados.Rows[i].Cells[2].Value += t.restricciones.ElementAt(j).ToString();

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
                resultados.Rows[0].DefaultCellStyle.BackColor = Color.LightGray;
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": No existe una tabla '" + nTabla + "' en la base de datos '" + BDenUso + "'.\r\n";
                return "Error";
            }    
        }

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
                || (tipo_mayMin.Equals("INT") && tipo_neg.Equals("FLOAT")))
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

        override
        public string VisitAccion_addColumn(SqlParser.Accion_addColumnContext context)
        {
            //No constraints
            if(context.ChildCount == 4)
            {
                Tabla tabla = ListaTablas[0];
                String columna = context.GetChild(2).GetText();
                String tipo = context.GetChild(3).GetText();

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
                
                //Agregar la columna a los datos
                FilaTabla contenido = new FilaTabla(tabla, BDenUso);
                contenido.cargar();

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

                contenido.guardar();

                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mySerializer.Serialize(myWriter, masterTabla);
                myWriter.Close();

                mensajes += "Se ha agregado la columna '" + columna + "' en la tabla '" + tabla.nombre + "' con éxito." + Environment.NewLine;
                return "void";
            }
            //Con constraints
            else
            {
                Tabla tabla = ListaTablas[0];
                String columna = context.GetChild(2).GetText();
                String tipo = context.GetChild(3).GetText();

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
                String resultadoConstraints = Visit(context.GetChild(4));
                if (resultadoConstraints.Equals("Error"))
                {
                    tabla.columnas.Remove(columna);
                    errores += "Error en línea " + context.start.Line +
                               ": La columna '" + columna +
                               "' no fue agregada en la tabla'" +
                               tabla.nombre + "' por errores en las Constraints." + Environment.NewLine;
                    return "Error";
                }

                //Agregar la columna a los datos
                FilaTabla contenido = new FilaTabla(tabla, BDenUso);
                contenido.cargar();

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

                contenido.guardar();

                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mySerializer.Serialize(myWriter, masterTabla);
                myWriter.Close();

                return "void";
            }
        }

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
                    errores += "Error en línea " + context.start.Line + ": La tabla '" + nombreT + "' no existe." + Environment.NewLine;
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
                    errores += "Error en línea " + context.start.Line + ": La columna '" + columna + "' puede pertenecer a varias tablas." + Environment.NewLine;
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

        override
        public string VisitExp_Int(SqlParser.Exp_IntContext context)
        {
            return "INT  " + context.GetText();
        }

        override
        public string VisitExp_Float(SqlParser.Exp_FloatContext context)
        {
            return "FLOAT" + context.GetText();
        }

        private bool isDate(String texto)
        {
            String[] lista = texto.Split();

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

            if (Int32.TryParse(lista[1], out mes))
            {
                if (Int32.TryParse(lista[2], out dia))
                {
                    if(mes >= 1 && mes <= 12
                    && dia >= 1 && dia <= 31)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

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

        override
        public string VisitMostrar_BD(SqlParser.Mostrar_BDContext context)
        {
            MasterBD basesdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            try
            {
                basesdatos = (MasterBD)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                basesdatos = new MasterBD();
            }
            reader.Close();
            resultados.ColumnCount = 2;
            resultados.RowCount = basesdatos.basesDeDatos.Count+1;

            resultados.Rows[0].Cells[0].Value = "Nombre";
            resultados.Rows[0].Cells[1].Value = "Cantidad de tablas";
            for (int i = 1; i < resultados.RowCount; i++)
            {
                resultados.Rows[i].Cells[0].Value = basesdatos.basesDeDatos.ElementAt(i-1).nombre;
                resultados.Rows[i].Cells[1].Value = basesdatos.basesDeDatos.ElementAt(i-1).cantidad_tablas+"";
            }
            resultados.Rows[0].DefaultCellStyle.BackColor = Color.LightGray;
            return "void";
        }

        override
        public string VisitColumnas(SqlParser.ColumnasContext context)
        {
            return context.GetChild(0).GetText() + " " + context.GetChild(1).GetText();
        }

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
                || (tipo_difEq.Equals("INT") && tipo_mayMin.Equals("FLOAT")))
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

        override
        public string VisitQuery(SqlParser.QueryContext context)
        {
            return Visit(context.GetChild(0));
        }

        override
        public string VisitId_completo_order(SqlParser.Id_completo_orderContext context)
        {
            throw new NotImplementedException();
        }

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

            mensajes += "Se ha agregado la Constraint '" + nombreCH + "' en la tabla '" + propia.nombre + "' con éxito." + Environment.NewLine;
            return "void";
        }

        override
        public string VisitFull_query(SqlParser.Full_queryContext context)
        {
            foreach(SqlParser.QueryContext query in context._queries){
                if(Visit(query).Equals("void")){
                    //Seguir iterando?
                }
                else
                {
                    return "Error";
                }
            }
            return "void";
        }

        override
        public string VisitUsar_BD(SqlParser.Usar_BDContext context)
        {
            String nombre;
            nombre = context.GetChild(2).GetText();
            MasterBD bdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            try
            {
                bdatos = (MasterBD)serializer.Deserialize(reader);
            }
            catch (Exception e) {
                bdatos = new MasterBD();
            }
            reader.Close();

            if (bdatos.containsBD(nombre))
            {
                BDenUso = nombre;
                mensajes += "La base de datos que usará a partir de este momento será '" + nombre + "'.\r\n";
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' no existe.\r\n";
                return "Error";
            }
        }

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

        override
        public string VisitAccion_DropConstraint(SqlParser.Accion_DropConstraintContext context)
        {
            Tabla tablaA = ListaTablas.ElementAt(0);
            String nRestriccion = context.GetChild(2).GetText();
            for (int i = 0; i < tablaA.restricciones.Count;i++){
                Restriccion rAux = tablaA.restricciones.ElementAt(i);
                if (rAux.nombre.Equals(nRestriccion)) {
                    tablaA.restricciones.RemoveAt(i);
                    XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                    StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                    mySerializer.Serialize(myWriter, masterTabla);
                    myWriter.Close();
                    mensajes += "Se ha eliminado la restricción '" + nRestriccion + "' de la tabla '" + tablaA.nombre + "' con éxito.\r\n";
                    return "void";
                }
            }
            errores += "Error en la línea "+context.start.Line+": No existe la restriccion '"+nRestriccion+"' en la tabla '"+tablaA.nombre+"'.\r\n";
            return "Error";
        }

        override
        public string VisitConstraint_completo(SqlParser.Constraint_completoContext context)
        {
            return Visit(context.GetChild(1));
        }

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
                    errores += "Error en línea " + context.start.Line + ": Los tipos de '" + propia.columnas[inPro] + "' de la tabla '" + propia.nombre + "' y '" + foranea.columnas[inFor] + "' de la tabla '" + foranea.nombre + "' ('" + propia.tipos_columnas[inPro] + "', '" + foranea.tipos_columnas[inFor] + "') no concuerdan." + Environment.NewLine;
                    return "Error";
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

            mensajes += "Se ha agregado la Constraint '" + nombreFK + "' en la tabla '" + propia.nombre + "' con éxito." + Environment.NewLine;
            return "void";
        }

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

                if (tipo_neg.Equals("BOOl"))
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

        override
        public string VisitShow_tables(SqlParser.Show_tablesContext context)
        {
            MasterTabla mTabla;
            if (BDenUso.Equals("")) {
                errores += "Error en línea "+context.start.Line+": No se encuentra en uso ninguna base de datos.\r\n";
                return "Error";
            }
            mTabla = deserializarMasterTabla();
            resultados.RowCount = mTabla.tablas.Count + 1;
            resultados.ColumnCount = 2;
            resultados.Rows[0].Cells[0].Value = "Nombre";
            resultados.Rows[0].Cells[1].Value = "Cant. de registros";

            for (int i = 1; i < resultados.RowCount; i++ )
            {
                resultados.Rows[i].Cells[0].Value = mTabla.tablas.ElementAt(i - 1).nombre;
                resultados.Rows[i].Cells[1].Value = mTabla.tablas.ElementAt(i - 1).cantidad_registros;
            }
            resultados.Rows[0].DefaultCellStyle.BackColor = Color.LightGray;
            mensajes += "Se han mostrado todas las tablas ("+mTabla.tablas.Count+") que contiene '" + BDenUso + "' con éxito.\r\n";
            return "void";
        }

        override
        public string VisitIdentificador_completo(SqlParser.Identificador_completoContext context)
        {
            throw new NotImplementedException();
        }

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

        override
        public string VisitCrear_tabla(SqlParser.Crear_tablaContext context)
        {
            if (BDenUso.Equals(""))
            {
                errores += "Error en linea " + context.start.Line + ": No hay base de datos en uso para agregar la tabla " + context.GetChild(2).GetText() + "." + BDenUso+ Environment.NewLine;
                return "Error";
            }

            //Generar la nueva tabla
            List<String> columnas = Visit(context.GetChild(4)).Split(',').ToList();
            Tabla nueva = new Tabla();
            nueva.nombre = context.GetChild(2).GetText();
            nueva.generarColumnas(columnas);

            //Deserealizar el archivo maestro de tablas
            MasterTabla mTabla;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterTabla));
            StreamReader reader = new StreamReader("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
            try
            {
                mTabla = (MasterTabla)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                mTabla = new MasterTabla();
            }
            reader.Close();

            masterTabla = mTabla;

            //Verificar si la tabla ya exite
            if (mTabla.containsTable(nueva.nombre))
            {
                errores += "Error en linea " + context.start.Line + ": La base de datos "+BDenUso+" ya contiene una tabla " + nueva.nombre + "." + Environment.NewLine;
                return "Error";
            }
            else
            {
                mTabla.agregarTabla(nueva);
            }

            //En caso que no haya constraints
            if (context.ChildCount == 6)
            {
                //Crear el archivo vacio de la tabla
                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), BDenUso);
                //No se si deberia de ser un xml
                string fileName = nueva.nombre + ".dat";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();

                //Serializar el objeto de la base de datos
                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mySerializer.Serialize(myWriter, mTabla);
                myWriter.Close();

                //Actualizar masterBDs
                MasterBD masterBD;
                serializer = new XmlSerializer(typeof(MasterBD));
                reader = new StreamReader("Databases\\masterBDs.xml");
                try
                {
                    //Deserealizar y actualizar datos
                    masterBD = (MasterBD)serializer.Deserialize(reader);
                    reader.Close();

                    masterBD.actualizarCantidadEnBD(BDenUso, mTabla.tablas.Count);
                    

                    //Serealizar el archivo maestros
                    mySerializer = new XmlSerializer(typeof(MasterBD));
                    myWriter = new StreamWriter("Databases\\masterBDs.xml");
                    mySerializer.Serialize(myWriter, masterBD);
                    myWriter.Close();
                    mensajes += "Se ha creado la tabla '"+nueva.nombre+"' en '"+BDenUso+"' con éxito.\r\n";
                }
                catch (Exception e)
                {
                    //Nada? No deberia pasar
                    reader.Close();
                }
                

                return "void";
            }
            //Caso en que si hay constraints
            else
            {
                //Manejar las constraint
                ListaTablas.Add(nueva);
                if(Visit(context.GetChild(5)).Equals("Error")){
                    return "Error";
                }

                //Crear el archivo vacio de la tabla
                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), BDenUso);

                string fileName = nueva.nombre + ".dat";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();
                
                //Serializar el objeto de la base de datos
                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mySerializer.Serialize(myWriter, mTabla);
                myWriter.Close();

                //Actualizar masterBDs
                MasterBD masterBD;
                serializer = new XmlSerializer(typeof(MasterBD));
                reader = new StreamReader("Databases\\masterBDs.xml");
                try
                {
                    //Deserealizar y actualizar datos
                    masterBD = (MasterBD)serializer.Deserialize(reader);
                    reader.Close();

                    masterBD.actualizarCantidadEnBD(BDenUso, mTabla.tablas.Count);


                    //Serealizar el archivo maestros
                    mySerializer = new XmlSerializer(typeof(MasterBD));
                    myWriter = new StreamWriter("Databases\\masterBDs.xml");
                    mySerializer.Serialize(myWriter, masterBD);
                    myWriter.Close();
                    mensajes += "Se ha creado la tabla '" + nueva.nombre + "' en '" + BDenUso + "' con éxito.\r\n";
                }
                catch (Exception e)
                {
                    //Nada? No deberia pasar
                    reader.Close();
                }


                return "void";
            }
        }

        override
        public string VisitCrear_BD(SqlParser.Crear_BDContext context)
        {
            String nombre;
            nombre = context.GetChild(2).GetText();
                
            MasterBD bdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            try
            {
                bdatos = (MasterBD)serializer.Deserialize(reader);
            }
            catch (Exception e) {
                bdatos = new MasterBD();
            }
            reader.Close();

            if (!bdatos.containsBD(nombre))
            {
                BaseDatos nBaseDatos = new BaseDatos(nombre);
                bdatos.agregarBD(nBaseDatos);
                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterBD));
                StreamWriter myWriter = new StreamWriter("Databases\\masterBDs.xml");
                mySerializer.Serialize(myWriter, bdatos);
                myWriter.Close();
                string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), nombre);
                System.IO.Directory.CreateDirectory(path);
                string fileName = nombre + ".xml";
                path = System.IO.Path.Combine(path, fileName);
                System.IO.FileStream fs = System.IO.File.Create(path);
                fs.Close();

                mensajes += "La base de datos '" + nombre + "' ha sido creada exitosamente.\r\n";
                return "void";
            }
            else
            {
                errores += "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' ya existe en el DBMS.\r\n";
                return "Error";
            }
        }

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
                if (r.nombre.Equals(nombrePK))
                {
                    errores += "Error en línea "+context.start.Line+": El nombre '"+nombrePK+"' ya es utilizado en otra restriccion dentro de la tabla '"+tActual.nombre+"'.\r\n";
                    return "Error";
                }
            restriccion.nombre = nombrePK;

            ListaTablas[0].restricciones.Add(restriccion);

            mensajes += "Se ha agregado la Constraint '" + nombrePK + "' en la tabla '" + tActual.nombre + "' con éxito." + Environment.NewLine;
            return "void";
        }

        override
        public string VisitAccion_addConstraint(SqlParser.Accion_addConstraintContext context)
        {
            if (Visit(context.GetChild(1)).Equals("void"))
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterTabla));
                StreamWriter myWriter = new StreamWriter("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
                mySerializer.Serialize(myWriter, masterTabla);
                myWriter.Close();
                return "void";
            }
            return "Error";
        }

        private MasterBD deserializarMasterBD() {
            MasterBD bdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            bdatos = (MasterBD)serializer.Deserialize(reader);
            reader.Close();
            return bdatos;
        }

        private MasterTabla deserializarMasterTabla() {
            //Deserealizar el archivo maestro de tablas
            MasterTabla mTabla;
            XmlSerializer serializer = new XmlSerializer(typeof(MasterTabla));
            StreamReader reader = new StreamReader("Databases\\" + BDenUso + "\\" + BDenUso + ".xml");
            try
            {
                mTabla = (MasterTabla)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                mTabla = new MasterTabla();
            }
            reader.Close();
            return mTabla;
        }

        /*
        public string Visit(Antlr4.Runtime.Tree.IParseTree tree)
        {
            throw new NotImplementedException();
        }

        public string VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
        {
            throw new NotImplementedException();
        }

        public string VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode node)
        {
            throw new NotImplementedException();
        }

        public string VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode node)
        {
            throw new NotImplementedException();
        }
        */
    }
}
