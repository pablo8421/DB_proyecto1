using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public string VisitId_completo(SqlParser.Id_completoContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAsignacion(SqlParser.AsignacionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitTipo(SqlParser.TipoContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMulti_exp(SqlParser.Multi_expContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitSelect(SqlParser.SelectContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitValor_completo(SqlParser.Valor_completoContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAccion_rename(SqlParser.Accion_renameContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitVotar_BD(SqlParser.Votar_BDContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitInsert(SqlParser.InsertContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitUpdate(SqlParser.UpdateContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAlter_table(SqlParser.Alter_tableContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAccion_DropColumn(SqlParser.Accion_DropColumnContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitDelete(SqlParser.DeleteContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitRenombrar_BD(SqlParser.Renombrar_BDContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitVotar_table(SqlParser.Votar_tableContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitShow_columns(SqlParser.Show_columnsContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAnd_expression(SqlParser.And_expressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMulti_id(SqlParser.Multi_idContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMayMin_expression(SqlParser.MayMin_expressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAccion_addColumn(SqlParser.Accion_addColumnContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitExp(SqlParser.ExpContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMostrar_BD(SqlParser.Mostrar_BDContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitColumnas(SqlParser.ColumnasContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitDifEq_expression(SqlParser.DifEq_expressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitQuery(SqlParser.QueryContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitId_completo_order(SqlParser.Id_completo_orderContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstrain_check(SqlParser.Constrain_checkContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitFull_query(SqlParser.Full_queryContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitUsar_BD(SqlParser.Usar_BDContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMulti_columnas(SqlParser.Multi_columnasContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitAccion_DropConstraint(SqlParser.Accion_DropConstraintContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstraint_completo(SqlParser.Constraint_completoContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitConstrain_fk(SqlParser.Constrain_fkContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitParen_expression(SqlParser.Paren_expressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMulti_constraint_completo(SqlParser.Multi_constraint_completoContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitNeg_expression(SqlParser.Neg_expressionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitShow_tables(SqlParser.Show_tablesContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitIdentificador_completo(SqlParser.Identificador_completoContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitMulti_accion(SqlParser.Multi_accionContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitCrear_tabla(SqlParser.Crear_tablaContext context)
        {
            throw new NotImplementedException();
        }

        public string VisitCrear_BD(SqlParser.Crear_BDContext context)
        {
            if (correcto)
            {
                String nombre;
                nombre = context.GetChild(2).GetText();

                XElement master = XElement.Load("masterBDs.xml");
                IEnumerable<XElement> basesdatos =
                    from el in master.Elements(nombre)
                    select el;

                if (basesdatos.ToList<XElement>().Count != 0)
                {
                    BaseDatos nBaseDatos = new BaseDatos(nombre);
                    XmlSerializer mySerializer = new XmlSerializer(typeof(BaseDatos));
                    StreamWriter myWriter = new StreamWriter("masterBDs.xml");
                    mySerializer.Serialize(myWriter, nBaseDatos);
                    myWriter.Close();
                }
                else
                {
                    correcto = true;
                    errores = "Error en línea " + context.start.Line + ": La base de datos '" + nombre + "' ya existe en el DBMS.";
                }
                return "void";
            }
            return "";
        }

        public string VisitConstrain_pk(SqlParser.Constrain_pkContext context)
        {
            throw new NotImplementedException();
        }

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
