using System;
using System.Collections.Generic;
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
        public bool correcto;
        public String errores;
        public DataGridView resultados;

        public TypeSystem() {
            correcto = true;
            errores = "";
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

                return "void";
            }
            else
            {
                correcto = false;
                errores = "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' no existe en el DBMS.";
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
            throw new NotImplementedException();
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
            return "void";
        }

        override
        public string VisitColumnas(SqlParser.ColumnasContext context)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        override
        public string VisitMulti_columnas(SqlParser.Multi_columnasContext context)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        override
        public string VisitCrear_BD(SqlParser.Crear_BDContext context)
        {
            if (correcto)
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
                    if (!System.IO.File.Exists(path))
                    {
                        System.IO.FileStream fs = System.IO.File.Create(path);
                        fs.Close();
                    }
                }
                else
                {
                    correcto = false;
                    errores = "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' ya existe en el DBMS.";
                    return "Error";
                }
                return "void";
            }
            return "Error";
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
