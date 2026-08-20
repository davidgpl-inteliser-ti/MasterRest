using MasterRest.Entities;
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
    public class PlazaController : Controller
    {
        private MasterRestEntities db = new MasterRestEntities();

        private Session session => Session["MR"] as Session;

        public ActionResult Index()
        {
            var mPlaza = db.mPlaza.Include(d => d.mEmpleado).ToList();

            ViewBag.eEmpleadoBaja = db.eEmpleadoBaja.Include(_ => _.mEmpleado).GroupBy(_ => _.idmPlaza).Select(a => a.OrderByDescending(b => b.idmPlaza).FirstOrDefault()).ToList();

            return View(mPlaza.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mPlaza = db.mPlaza.Find(id);
            if (mPlaza == null)
            {
                return HttpNotFound();
            }
            var idmPlazaJefeInmediato = "";
            var jefeInmediatoPuesto = "";
            if (mPlaza.idmPlazaJefeInmediato != null)
            {
                var p = db.mPlaza.Find(mPlaza.idmPlazaJefeInmediato);
                idmPlazaJefeInmediato = p != null ? (p.mEmpleado != null ? p.mEmpleado.nombre + " " + p.mEmpleado.paterno + " " + p.mEmpleado.materno : "") : "";
                jefeInmediatoPuesto = p != null ? p.cPuesto.puesto : "";
            }
            ViewBag.idmPlazaJefeInmediato = idmPlazaJefeInmediato;
            ViewBag.jefeInmediatoPuesto = jefeInmediatoPuesto;

            ViewBag.beneficio = db.cBeneficio.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.beneficio).ToList();

            return View(mPlaza);
        }

        public ActionResult Create()
        {
            ViewBag.cSucursal = new SelectList(db.cSucursal.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.sucursal), "idcSucursal", "sucursal");
            ViewBag.cRol = new SelectList(db.cRol.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.rol), "idcRol", "rol");
            ViewBag.cPuesto = new SelectList(db.cPuesto.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.puesto), "idcPuesto", "puesto");
            ViewBag.cArea = new SelectList(db.cArea.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.area), "idcArea", "area");
            return View(new mPlaza());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idmPlaza,idcEmpresa,idcSucursal,idmEmpleado,idcRol,idmPlazaJefeInmediato,idcPuesto,idcArea,estatus")] mPlaza mPlaza, FormCollection datos, int numeroPosiciones)
        {
            for (int i = 0; i < numeroPosiciones; i++)
            {
                var p = new mPlaza()
                {
                    idmPlaza = 0,
                    idcEmpresa = session.idcEmpresa,
                    idcSucursal = mPlaza.idcSucursal,
                    idmEmpleado = null,
                    idcRol = mPlaza.idcRol,
                    idmPlazaJefeInmediato = mPlaza.idmPlazaJefeInmediato,
                    idcPuesto = mPlaza.idcPuesto,
                    idcArea = mPlaza.idcArea,
                    estatus = "VACANTE"
                };

                db.mPlaza.Add(p);
                db.SaveChanges();

                var dh_e_beneficios = new List<ePlazaBeneficio>();
                foreach (var b in db.cBeneficio.ToList())
                {
                    var check = datos["check-" + b.idcBeneficio];
                    if (datos["check-" + b.idcBeneficio] != null)
                    {
                        var monto = datos["input-" + b.idcBeneficio];
                        try
                        {
                            dh_e_beneficios.Add(new ePlazaBeneficio() { idmPlaza = p.idmPlaza, idcBeneficio = b.idcBeneficio, valor = monto, tipo = b.tipo });
                        }
                        catch { }
                    }
                }
                if (dh_e_beneficios.Count > 0)
                {
                    db.ePlazaBeneficio.AddRange(dh_e_beneficios);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mPlaza mPlaza = db.mPlaza.Find(id);
            if (mPlaza == null)
            {
                return HttpNotFound();
            }
            ViewBag.cSucursal = new SelectList(db.cSucursal.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.sucursal), "idcSucursal", "sucursal", mPlaza.cSucursal.sucursal);
            ViewBag.cRol = new SelectList(db.cRol.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.rol), "idcRol", "rol", mPlaza.cRol.rol);
            ViewBag.cPuesto = new SelectList(db.cPuesto.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.puesto), "idcPuesto", "puesto", mPlaza.cPuesto.puesto);
            ViewBag.cArea = new SelectList(db.cArea.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.area), "idcArea", "area", mPlaza.cArea.area);
            return View(mPlaza);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idmPlaza,idcEmpresa,idcSucursal,idmEmpleado,idcRol,idmPlazaJefeInmediato,idcPuesto,idcArea,estatus")] mPlaza mPlaza, FormCollection datos, string idPosicion2)
        {
            db.Entry(mPlaza).State = EntityState.Modified;
            db.SaveChanges();

            var idcBeneficio = new List<int>();
            var dh_e_beneficios = new List<ePlazaBeneficio>();
            foreach (var b in db.cBeneficio.ToList())
            {
                try
                {
                    var check = datos["check-" + b.idcBeneficio];
                    if (datos["check-" + b.idcBeneficio] != null)
                    {
                        idcBeneficio.Add(b.idcBeneficio);
                        var monto = datos["input-" + b.idcBeneficio];
                        var beneficio = db.ePlazaBeneficio.FirstOrDefault(_ => _.idmPlaza == mPlaza.idmPlaza && _.idcBeneficio == b.idcBeneficio);
                        if (beneficio != null)
                        {
                            if (beneficio.valor != null)
                            {
                                beneficio.valor = monto;
                                beneficio.tipo = b.tipo;
                                db.Entry(beneficio).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            db.ePlazaBeneficio.Add(new ePlazaBeneficio() { idmPlaza = mPlaza.idmPlaza, idcBeneficio = b.idcBeneficio, valor = monto, tipo = b.tipo });
                            db.SaveChanges();
                        }
                    }
                }
                catch { }
            }

            foreach (var b in db.ePlazaBeneficio.Where(_ => _.idmPlaza == mPlaza.idmPlaza).ToList())
            {
                if (idcBeneficio.Where(_ => _ == b.idcBeneficio).ToList().Count == 0)
                {
                    db.Entry(b).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Deactivate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mPlaza = db.mPlaza.Find(id);
            if (mPlaza == null)
            {
                return HttpNotFound();
            }
            return View(mPlaza);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate([Bind(Include = "idmPlaza,idcEmpresa,idcSucursal,idmEmpleado,idcRol,idmPlazaJefeInmediato,idcPuesto,idcArea,estatus")] mPlaza mPlaza)
        {
            db.Entry(mPlaza).State = EntityState.Modified;
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