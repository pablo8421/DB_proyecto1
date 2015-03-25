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
        Tabla tabla;
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
            return msgPack.SaveBytesToFile(BDenUso + "\\" + tabla.nombre + ".data");
        }

        public bool cargar()
        {
            bool retorno = msgPack.LoadFileAsBytes(BDenUso + "\\" + tabla.nombre + ".data");
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

    }
}
