using MasterRest.Entities;
using MasterRest.Helpers;
using MasterRest.Models;
using System.Linq;
using System.Web.Mvc;

namespace MasterRest.Controllers
{
    public class SesionController : Controller
    {
        private MasterRestEntities db = new MasterRestEntities();
        
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection datos)
        {
            if (datos["Username"] != null && datos["Password"] != null)
            {
                var usuario = datos["Username"];
                var cUsuario = db.cUsuario.FirstOrDefault(_ => _.usuario == usuario && _.estatus == "ACTIVO");
                if (cUsuario != null)
                {
                    if (cUsuario.clave == Utilities.Encriptar(datos["Password"]))
                    {
                        if (cUsuario.mEmpleado != null && cUsuario.mEmpleado.mPlaza != null && cUsuario.mEmpleado.mPlaza.Count() > 0)
                        {
                            Session.Timeout = 60 * 10;
                            Session["MR"] = new Session()
                            {
                                idmEmpleado = cUsuario.mEmpleado.idmEmpleado,
                                nombreCompleto = cUsuario.mEmpleado.nombre + " " + cUsuario.mEmpleado.paterno + " " + cUsuario.mEmpleado.materno,
                                fotoPerfil = cUsuario.mEmpleado.fotoPerfil != null && cUsuario.mEmpleado.fotoPerfil != "" ? cUsuario.mEmpleado.fotoPerfil : "assets/img/avatars/1.png",
                                genero = cUsuario.mEmpleado.sexo,
                                idcUsuario = cUsuario.idcUsuario,
                                usuario = cUsuario.usuario,
                                idmPlaza = cUsuario.mEmpleado.mPlaza.FirstOrDefault().idmPlaza,
                                puesto = cUsuario.mEmpleado.mPlaza.FirstOrDefault().cPuesto.puesto,
                                area = cUsuario.mEmpleado.mPlaza.FirstOrDefault().cArea.area,
                                idcEmpresa = cUsuario.mEmpleado.mPlaza.FirstOrDefault().cEmpresa.idcEmpresa,
                                empresa = cUsuario.mEmpleado.mPlaza.FirstOrDefault().cEmpresa.nombreComercial,
                                rol = cUsuario.mEmpleado.mPlaza.FirstOrDefault().cRol.rol
                            };

                            return RedirectToAction(actionName: "Dashboard");
                        }
                    }
                }
            }
            ViewBag.Error = "el usuario y contraseña son incorrectos";
            return RedirectToAction("Index", "Sesion");
        }

        public ActionResult Logout()
        {
            Session.Remove("MR");

            return RedirectToAction("Index");
        }

        [SessionExpire]
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}