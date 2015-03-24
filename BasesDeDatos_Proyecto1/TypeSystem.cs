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

        public TypeSystem() {
            errores = "";
            mensajes = "";
            BDenUso = "";
            resultados = new DataGridView();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
                bdatos.borrarBD(nombre);

                XmlSerializer mySerializer = new XmlSerializer(typeof(MasterBD));
                StreamWriter myWriter = new StreamWriter("Databases\\masterBDs.xml");
                mySerializer.Serialize(myWriter, bdatos);
                myWriter.Close();

                String path = "Databases\\"+nombre;
                System.IO.Directory.Delete(path, true);
                mensajes += "Se ha borrado la base de datos '" + nombre + "' con éxito.\r\n";
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
            throw new NotImplementedException();
        }

        override
        public string VisitAccion_DropColumn(SqlParser.Accion_DropColumnContext context)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        override
        public string VisitShow_columns(SqlParser.Show_columnsContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitAnd_expression(SqlParser.And_expressionContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitMulti_id(SqlParser.Multi_idContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitMayMin_expression(SqlParser.MayMin_expressionContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitAccion_addColumn(SqlParser.Accion_addColumnContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitExp(SqlParser.ExpContext context)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        override
        public string VisitConstraint_completo(SqlParser.Constraint_completoContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitConstrain_fk(SqlParser.Constrain_fkContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitParen_expression(SqlParser.Paren_expressionContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitMulti_constraint_completo(SqlParser.Multi_constraint_completoContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitNeg_expression(SqlParser.Neg_expressionContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitShow_tables(SqlParser.Show_tablesContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitIdentificador_completo(SqlParser.Identificador_completoContext context)
        {
            throw new NotImplementedException();
        }

        override
        public string VisitMulti_accion(SqlParser.Multi_accionContext context)
        {
            throw new NotImplementedException();
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
                string fileName = nueva.nombre + ".xml";
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
                //Aca va cuando hay constraints
                return "Error";
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
            throw new NotImplementedException();
        }

        override
        public string VisitAccion_addConstraint(SqlParser.Accion_addConstraintContext context)
        {
            throw new NotImplementedException();
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
