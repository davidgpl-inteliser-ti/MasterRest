using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterRest.Entities
{
    public class Session
    {
        public int idmEmpleado { get; set; }
        public string nombreCompleto { get; set; }
        public string fotoPerfil { get; set; }
        public string genero { get; set; }
        public int idcUsuario { get; set; }
        public string usuario { get; set; }
        public int idmPlaza { get; set; }
        public string puesto { get; set; }
        public string area { get; set; }
        public string empresa { get; set; }
        public string rol { get; set; }
    }
}