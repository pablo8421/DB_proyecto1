using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleMsgPack;
namespace BasesDeDatos_Proyecto1
{
    class FilaTabla
    {
        MsgPack msgPack;
        public Tabla tabla;
        String BDenUso;

        public FilaTabla(Tabla tabla, String BDenUso)
        {
            this.tabla= tabla;
            this.BDenUso = BDenUso;
            msgPack = new MsgPack();
        }

        public bool guardar()
        {
            msgPack.SetAsBytes(msgPack.Encode2Bytes());
            return msgPack.SaveBytesToFile(BDenUso + "\\" + tabla.nombre + ".dat");
        }

        public bool cargar()
        {
            bool retorno = msgPack.LoadFileAsBytes(BDenUso + "\\" + tabla.nombre + ".dat");
            msgPack.DecodeFromBytes(msgPack.GetAsBytes());
            return retorno;
        }

        public int getTamanio()
        {
            return tabla.cantidad_registros;
        }

        private MsgPack getRowMsqPack(int num)
        {
            return msgPack.ForcePathObject("row_"+num);
        }

        public List<Object> getRow(int num)
        {
            List<Object> lista = new List<Object>();
            int i = 0;
            foreach (MsgPack item in getRowMsqPack(num))
            {
                if(tabla.tipos_columnas[i].Equals("INT")){
                    lista.Add(item.AsInteger);
                }
                if (tabla.tipos_columnas[i].Equals("FLOAT"))
                {
                    lista.Add(item.AsFloat);
                }
                if (tabla.tipos_columnas[i].Equals("DATE"))
                {
                    lista.Add(item.AsString);
                }
                if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                {
                    lista.Add(item.AsString);
                }
                i++;
            }
            return lista;
        }

        public Object getRowElement(int row, int columna)
        {
            if (tabla.tipos_columnas[columna].Equals("INT"))
            {
                return msgPack.ForcePathObject("row_" + row).AsArray[columna].AsInteger;
            }
            else if (tabla.tipos_columnas[columna].Equals("FLOAT"))
            {
                return msgPack.ForcePathObject("row_" + row).AsArray[columna].AsFloat;
            }
            else if (tabla.tipos_columnas[columna].Equals("DATE"))
            {
                return msgPack.ForcePathObject("row_" + row).AsArray[columna].AsString;
            }
            else if (tabla.tipos_columnas[columna].StartsWith("CHAR"))
            {
                return msgPack.ForcePathObject("row_" + row).AsArray[columna].AsString;
            }
            else
            {
                return null;
            }
        }

        public void agregarFila(List<String> fila)
        {
            if (fila.Count == tabla.columnas.Count)
            {
                int i = 0;
                foreach (String elemento in fila)
                {
                    if (tabla.tipos_columnas[i].Equals("INT"))
                    {
                        msgPack.ForcePathObject("row_" + tabla.cantidad_registros).AsArray.Add(Convert.ToInt32(elemento));
                    }
                    if (tabla.tipos_columnas[i].Equals("FLOAT"))
                    {
                        msgPack.ForcePathObject("row_" + tabla.cantidad_registros).AsArray.Add(Convert.ToSingle(elemento));
                    }
                    if (tabla.tipos_columnas[i].Equals("DATE"))
                    {
                        msgPack.ForcePathObject("row_" + tabla.cantidad_registros).AsArray.Add(elemento);
                    }
                    if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                    {
                        msgPack.ForcePathObject("row_" + tabla.cantidad_registros).AsArray.Add(elemento);
                    }
                    i++;
                }
                tabla.cantidad_registros = tabla.cantidad_registros + 1;
            }
        }

        public void mostrarTablaEnConsola()
        {
            for (int i = 0; i < tabla.cantidad_registros; i++)
            {
                int j = 0;
                foreach (MsgPack item in getRowMsqPack(i))
                {
                    if (tabla.tipos_columnas[j].Equals("INT"))
                    {
                        Console.Write(item.AsInteger+"  ");
                    }
                    if (tabla.tipos_columnas[j].Equals("FLOAT"))
                    {
                        Console.Write(item.AsFloat + "  ");
                    }
                    if (tabla.tipos_columnas[j].Equals("DATE"))
                    {
                        Console.Write(item.AsString + "  ");
                    }
                    if (tabla.tipos_columnas[j].StartsWith("CHAR"))
                    {
                        Console.Write(item.AsString + "  ");
                    }
                    Console.WriteLine();
                    i++;
                }
            }
        }

    }
}
