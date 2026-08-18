using MasterRest.Helpers;
using MasterRest.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MasterRest.Controllers
{
    [SessionExpire]
    public class UsuarioController : Controller
    {
        private MasterRestEntities db = new MasterRestEntities();

        public class Usuarios
        {
            public int idcUsuario { get; set; }
            public int idmEmpleado { get; set; }
            public int? numeroEmpleado { get; set; }
            public string nombre { get; set; }
            public string usuario { get; set; }
            public string clave { get; set; }
            public int? idcRol { get; set; }
            public string rol { get; set; }
            public string estatus { get; set; }
        }

        public ActionResult Index()
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            foreach (var t in db.mEmpleado.Where(_ => _.estatus != "BAJA").ToList())
            {
                var u = db.cUsuario.FirstOrDefault(_ => _.idmEmpleado == t.idmEmpleado);

                var p = t.mPlaza.FirstOrDefault();

                usuarios.Add(new Usuarios()
                {
                    idcUsuario = u != null ? u.idcUsuario : 0,
                    idmEmpleado = t.idmEmpleado,
                    nombre = t.nombre + " " + t.paterno + " " + t.materno,
                    usuario = u != null ? u.usuario : "sin usuario",
                    clave = u != null && u.clave != null ? Utilities.Desencriptar(u.clave) : "",
                    idcRol = p != null ? p.idcRol : 0,
                    rol = p != null && p.cRol != null ? p.cRol.rol : "",
                    estatus = u != null ? u.estatus : ""
                });
            }
            return View(usuarios);
        }

        
        public ActionResult Create(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombre = mEmpleado.nombre + " " + mEmpleado.paterno + " " + mEmpleado.materno;
            var cUsuario = new cUsuario()
            {
                idcUsuario = 0,
                idmEmpleado = id,
                usuario = "",
                clave = "",
                estatus = "ACTIVO",
            };

            var idcRol = 0;
            var idmPlaza = 0;
            var mPlaza = mEmpleado.mPlaza.FirstOrDefault();
            if (mPlaza != null)
            {
                idcRol = mPlaza.idcRol != null ? (int)mPlaza.idcRol : 0;
                idmPlaza = mPlaza.idmPlaza;
            }

            ViewBag.idcRol = new SelectList(db.cRol.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcRol", dataTextField: "rol", selectedValue: idcRol);

            return View(cUsuario);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idcUsuario,idmEmpleado,usuario,clave,estatus")] cUsuario cUsuario, FormCollection datos)
        {
            cUsuario.clave = Utilities.Encriptar(cUsuario.clave);
            db.cUsuario.Add(cUsuario);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            cUsuario cUsuario = db.cUsuario.Find(id);
            if (cUsuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombre = cUsuario.mEmpleado.nombre + " " + cUsuario.mEmpleado.paterno + " " + cUsuario.mEmpleado.materno;
            ViewBag.idmEmpleado = new SelectList(db.mEmpleado, "idmEmpleado", "nombre", cUsuario.idmEmpleado);
            ViewBag.estatus = new List<SelectListItem>() {
                new SelectListItem { Text = "ACTIVO", Value = "ACTIVO", Selected = cUsuario.estatus == "ACTIVO" ? true : false },
                new SelectListItem { Text = "INACTIVO", Value = "INACTIVO", Selected = cUsuario.estatus == "INACTIVO" ? true : false }
            };
            cUsuario.clave = cUsuario.clave != null && cUsuario.clave != "" ? Utilities.Desencriptar(cUsuario.clave) : "";

            var idcRol = 0;
            var idmPlaza = 0;
            var mPlaza = cUsuario.mEmpleado.mPlaza.FirstOrDefault();
            if (mPlaza != null)
            {
                idcRol = mPlaza.idcRol != null ? (int)mPlaza.idcRol : 0;
                idmPlaza = mPlaza.idmPlaza;
            }

            ViewBag.idcRol = new SelectList(db.cRol.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcRol", dataTextField: "rol", selectedValue: idcRol);

            return View(cUsuario);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idcUsuario,idmEmpleado,usuario,clave,estatus")] cUsuario cUsuario, FormCollection datos)
        {
            cUsuario.clave = Utilities.Encriptar(cUsuario.clave);
            db.Entry(cUsuario).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}