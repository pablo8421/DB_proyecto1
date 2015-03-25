using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;
using System.IO;

namespace BasesDeDatos_Proyecto1
{
    class FilaTabla
    {
        public Tabla tabla;
        String BDenUso;
        public Filas datos;

        public FilaTabla(Tabla tabla, String BDenUso)
        {
            this.tabla= tabla;
            this.BDenUso = BDenUso;
            datos = new Filas();
        }

        public void guardar()
        {
            // Creates serializer.
            var serializer = SerializationContext.Default.GetSerializer<Filas>();
            // Pack obj to stream.
            using (Stream stream = File.Open("Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat", FileMode.Open))
            {
                serializer.Pack(stream, datos);
            }
        }

        public void cargar()
        {
            var serializer = MessagePackSerializer.Create<Filas>();

            using (Stream stream = File.Open("Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat", FileMode.Open))
            {
                datos = serializer.Unpack(stream);
            }
        }

        public int getTamanio()
        {
            return tabla.cantidad_registros;
        }


        public List<Object> getRow(int num)
        {
            return datos.elementos[num];
        }

        public Object getRowElement(int row, int columna)
        {
            if (tabla.tipos_columnas[columna].Equals("INT"))
            {
                return (Int32) datos.elementos[row][columna];
            }
            else if (tabla.tipos_columnas[columna].Equals("FLOAT"))
            {
                return (Single) datos.elementos[row][columna];
            }
            else if (tabla.tipos_columnas[columna].Equals("DATE"))
            {
                return (String)datos.elementos[row][columna];
            }
            else if (tabla.tipos_columnas[columna].StartsWith("CHAR"))
            {
                return (String) datos.elementos[row][columna];
            }
            else
            {
                return null;
            } 
        }

        public void agregarFila(List<String> fila)
        {
            List<Object> row = new List<Object>();
            int i = 0;
            foreach (String elemento in fila)
            {
                if (tabla.tipos_columnas[i].Equals("INT"))
                {
                    row.Add(Convert.ToInt32(elemento));
                }
                if (tabla.tipos_columnas[i].Equals("FLOAT"))
                {
                    row.Add(Convert.ToSingle(elemento));
                }
                if (tabla.tipos_columnas[i].Equals("DATE"))
                {
                    row.Add(elemento);
                }
                if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                {
                    row.Add(elemento);
                }
                i++;
            }
            tabla.cantidad_registros++;

            datos.elementos.Add(row);
        }

        public void mostrarTablaEnConsola()
        {
            for (int i = 0; i < tabla.cantidad_registros; i++)
            {
                int j = 0;
                foreach (MsgPack.MessagePackObject item in datos.elementos[i])
                {
                    if (tabla.tipos_columnas[j].Equals("INT"))
                    {
                        Console.Write(item.AsInt32() + "  ");
                    }
                    else if (tabla.tipos_columnas[j].Equals("FLOAT"))
                    {
                        Console.Write(item.AsSingle() + "  ");
                    }
                    else if (tabla.tipos_columnas[j].Equals("DATE"))
                    {
                        Console.Write(item.AsString() + "  ");
                    }
                    else if (tabla.tipos_columnas[j].StartsWith("CHAR"))
                    {
                        Console.Write(item.AsString() + "  ");
                    }
                    else
                    {
                        Console.Write("NOPE");
                    }
                    j++;
                }
                Console.WriteLine();
            }
        }

    }

    public class Filas
    {
        public List<List<Object>> elementos { get; set; }

        public Filas()
        {
            elementos = new List<List<Object>>();
        }
    }
}
