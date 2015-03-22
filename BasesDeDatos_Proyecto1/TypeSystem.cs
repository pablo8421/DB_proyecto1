using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BasesDeDatos_Proyecto1
{
    class TypeSystem : SqlBaseVisitor<String>
    {
        private bool correcto;
        private String errores;

        public TypeSystem() {
            correcto = true;
            errores = "";
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
        public string VisitVotar_BD(SqlParser.Votar_BDContext context)
        {
            throw new NotImplementedException();
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
        public string VisitVotar_table(SqlParser.Votar_tableContext context)
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
            BaseDatos[] basesdatos;
            XmlSerializer serializer = new XmlSerializer(typeof(BaseDatos[]));
            StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
            reader.ReadToEnd();
            basesdatos = (BaseDatos[])serializer.Deserialize(reader);
            reader.Close();
            foreach (BaseDatos bd in basesdatos)
                Console.WriteLine(bd);  //Pasarlo al textarea del form
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

                //MasterBD maestro = new MasterBD();
                //maestro.agregarBD(new BaseDatos("Nombre"));
                //XmlSerializer serializador = new XmlSerializer(typeof(MasterBD));
                //StreamWriter miEscritor = new StreamWriter("Databases\\masterBDs.xml");
                //serializador.Serialize(miEscritor, maestro);


                XElement master = XElement.Load("Databases\\masterBDs.xml");
                IEnumerable<XElement> basesdatos =
                    from el in master.Elements(nombre)
                    select el;

                if (basesdatos.ToList<XElement>().Count == 0)
                {
                    MasterBD bdatos;
                    XmlSerializer serializer = new XmlSerializer(typeof(MasterBD));
                    StreamReader reader = new StreamReader("Databases\\masterBDs.xml");
                    bdatos = (MasterBD)serializer.Deserialize(reader);
                    reader.Close();
                    
                    BaseDatos nBaseDatos = new BaseDatos(nombre);
                    bdatos.agregarBD(nBaseDatos);
                    XmlSerializer mySerializer = new XmlSerializer(typeof(MasterBD));                   
                    StreamWriter myWriter = new StreamWriter("Databases\\masterBDs.xml");
                    mySerializer.Serialize(myWriter, nBaseDatos);
                    myWriter.Close();
                    string path = System.IO.Path.Combine(Path.GetFullPath("Databases"), nombre);
                    System.IO.Directory.CreateDirectory(path);
                    string fileName = nombre+".xml";
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
