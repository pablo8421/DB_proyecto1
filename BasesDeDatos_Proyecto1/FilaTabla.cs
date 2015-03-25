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
            msgPack.DecodeFromBytes(unpack.GetAsBytes());
            return retorno;
        }
    }
}
