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
        Filas datos;

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
            return datos.elementos[row][columna]; 
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
                foreach (Object item in datos.elementos[i])
                {
                    if (tabla.tipos_columnas[j].Equals("INT"))
                    {
                        Console.Write(item + "  ");
                    }
                    else if (tabla.tipos_columnas[j].Equals("FLOAT"))
                    {
                        Console.Write(item + "  ");
                    }
                    else if (tabla.tipos_columnas[j].Equals("DATE"))
                    {
                        Console.Write(item + "  ");
                    }
                    else if (tabla.tipos_columnas[j].StartsWith("CHAR"))
                    {
                        Console.Write(item + "  ");
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
