using MasterRest.Entities;
using MasterRest.Helpers;
using MasterRest.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace MasterRest.Controllers
{
    [SessionExpire]
    public class EmpleadoController : Controller
    {
        private MasterRestEntities db = new MasterRestEntities();

        private Session session => Session["MR"] as Session;

        public ActionResult Index()
        {
            var mEmpleado = db.mEmpleado.Where(_ => _.idcEmpresa == session.idcEmpresa).Include(_ => _.dEmpleadoDomicilio).Include(_ => _.mPlaza).ToList();

            return View(mEmpleado);
        }

        public ActionResult Details(int? id, string seccion)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mEmpleado mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.edad = mEmpleado.fechaNacimiento != null ? DateTime.Today.AddTicks(-((DateTime)mEmpleado.fechaNacimiento).Ticks).Year - 1 : 0;
            ViewBag.antiguedad = Utilities.Antiguedad(mEmpleado.fechaContratacion??DateTime.Now, DateTime.Now);
            var jefeInmediatoPuesto = "";
            var idmPlazaJefeInmediato = "";
            if (mEmpleado.mPlaza != null && mEmpleado.mPlaza.Count > 0 && mEmpleado.mPlaza.FirstOrDefault().idmPlazaJefeInmediato != null)
            {
                var ppr = db.mPlaza.Find(mEmpleado.mPlaza.FirstOrDefault().idmPlazaJefeInmediato);
                if (ppr != null)
                {
                    jefeInmediatoPuesto = ppr.cPuesto.puesto;
                    idmPlazaJefeInmediato = ppr.mEmpleado != null ? ppr.mEmpleado.nombre + " " + ppr.mEmpleado.paterno + " " + ppr.mEmpleado.materno : "";
                }
            }
            ViewBag.jefeInmediatoPuesto = jefeInmediatoPuesto;
            ViewBag.idmPlazaJefeInmediato = idmPlazaJefeInmediato;

            ViewBag.seccion = seccion;

            return View(mEmpleado);
        }

        public ActionResult Create()
        {
            ViewBag.idmPlaza = new SelectList(db.mPlaza.Where(_ => _.estatus == "VACANTE").OrderBy(_ => _.cPuesto.puesto).Select(_ => new { _.idmPlaza, puesto = _.idmPlaza + " | " + _.cPuesto.puesto }), "idmPlaza ", "puesto");
            ViewBag.cBanco = new SelectList(db.cBanco.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.banco), "idcBanco", "banco");
            ViewBag.cEstadoCivil = new SelectList(db.cEstadoCivil.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.estadoCivil), "estadoCivil", "estadoCivil");
            ViewBag.cEscolaridad = new SelectList(db.cEscolaridad.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.escolaridad), "escolaridad", "escolaridad");
            ViewBag.cProfesion = new SelectList(db.cProfesion.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.profesion), "profesion", "profesion");
            ViewBag.tipoNomina = new SelectList(db.cTipoNomina.Where(_ => _.estatus == "ACTIVO"), "tipoNomina", "tipoNomina");
            ViewBag.idcEntidadFederativa = new SelectList(db.cEntidadFederativa, dataValueField: "idcEntidadFederativa", dataTextField: "entidadFederativa");
            ViewBag.idmPlazaJefeInmediato = new SelectList(db.mPlaza.Where(_ => _.estatus == "VACANTE" || _.estatus == "AUTORIZADA" || _.estatus == "PRE-ASIGNADO").OrderBy(_ => _.cPuesto.puesto).Select(_ => new { _.idmPlaza, puesto = _.cPuesto.puesto + (_.mEmpleado != null ? " - (" + _.mEmpleado.nombre + " " + _.mEmpleado.paterno + " " + _.mEmpleado.materno + ")" : "") }), "idmPlaza", "puesto");
            ViewBag.idcTipoContrato = new SelectList(db.cTipoContrato.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcTipoContrato", dataTextField: "contrato");
            ViewBag.idcDocumento = new SelectList(db.cDocumento.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcDocumento", dataTextField: "documento");
            ViewBag.nacionalidad = new List<SelectListItem>() {
                new SelectListItem { Text = "MEXICANA", Value = "MEXICANA" },
                new SelectListItem { Text = "EXTRANGERA", Value = "EXTRANGERA" } };
            ViewBag.beneficio = db.cBeneficio.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.beneficio).ToList();
            ViewBag.documentoProbatorio = new SelectList(db.cDocumentoProbatorio.Where(_ => _.estatus == "ACTIVO"), "documentoProbatorio", "documentoProbatorio");
            ViewBag.tipoSangre = new List<SelectListItem>() {
                new SelectListItem { Text = "O+", Value = "O+" },
                new SelectListItem { Text = "O-", Value = "O-" },
                new SelectListItem { Text = "A+", Value = "A+" },
                new SelectListItem { Text = "A-", Value = "A-" },
                new SelectListItem { Text = "B+", Value = "B+" },
                new SelectListItem { Text = "B-", Value = "B-" },
                new SelectListItem { Text = "AB+", Value = "AB+" },
                new SelectListItem { Text = "AB-", Value = "AB-" } };
            ViewBag.tipoInstitucion = new List<SelectListItem>() {
                new SelectListItem { Text = "PUBLICO", Value = "PUBLICO" },
                new SelectListItem { Text = "PRIVADO", Value = "PRIVADO" } };
            ViewBag.estatusInstitucion = new List<SelectListItem>() {
                new SelectListItem { Text = "CONCLUIDO", Value = "CONCLUIDO" },
                new SelectListItem { Text = "TRUNCO", Value = "TRUNCO" } };
            ViewBag.sexo = new List<SelectListItem>() {
                new SelectListItem { Text = "MUJER", Value = "MUJER" },
                new SelectListItem { Text = "HOMBRE", Value = "HOMBRE" } };
            ViewBag.esquemaPago = new List<SelectListItem>() {
                new SelectListItem { Text = "INHOUSE", Value = "INHOUSE" },
                new SelectListItem { Text = "ESPECIALIZADO", Value = "ESPECIALIZADO" } };

            ViewBag.documentos = db.cDocumento.Where(_ => _.estatus == "ACTIVO" && (_.responsable == "ALTA" || _.responsable == "CANDIDATO")).OrderBy(_ => _.documento).ToList();

            var ln = new List<SelectListItem>();
            ln.Add(new SelectListItem() { Value = "NACIDO EN EL EXTRANJERO", Text = "NACIDO EN EL EXTRANJERO" });
            var ef = new SelectList(db.cEntidadFederativa, dataValueField: "entidadFederativa", dataTextField: "entidadFederativa");
            ln.AddRange(ef);
            ViewBag.lugarNacimiento = new SelectList(ln, "Value", "Text");

            return View(new mEmpleado());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idmEmpleado,idcEmpresa,iddEmpleadoDomicilio,nombre,paterno,materno,fechaContratacion,fechaIngresoImss,fechaNacimiento,lugarNacimiento,curp,rfc,nss,estadoCivil,hijos,nacionalidad,sexo,tipoSangre,correoCoorporativo,correoPersonal,telefono,telefonoContacto,nombreContacto,parentescoContacto,escolaridad,profesion,institucion,estatusInstitucion,esquemaPago,tipoNomina,patronal,noCreditoInfonavit,descuentoInfonavit,noCreditoFonacot,descuentoFonacot,codigoBanco,codigoSucursal,noCuenta,clabeInterbancaria,salarioDiarioExcedente,salarioDiarioCotizacion,salarioDiario,salarioIntegrado,salarioMensual,fotoPerfil,observaciones,estatus,tipoInstitucion,telefonoCelularEmpresa,telefonoFijoEmpresa,telefonoExtension,hijosEdadXML,documentoProbatorio,factorInfonavit,factorFonacot,idcHorario")] mEmpleado mEmpleado, FormCollection datos, HttpPostedFileBase fileFoto, HttpPostedFileBase fileContrato, HttpPostedFileBase fileDocumento)
        {
            if (db.mEmpleado.Where(_ => _.curp == mEmpleado.curp).Count() == 0)
            {
                if (ModelState.IsValid)
                {
                    var dEmpleadoDomicilio = new dEmpleadoDomicilio()
                    {
                        idcMunicipio = short.Parse(datos["idcMunicipio"]),
                        colonia = datos["colonia"],
                        calle = datos["calle"],
                        noExterior = datos["noExterior"],
                        noInterior = datos["noInterior"],
                        codigoPostal = datos["codigoPostal"],
                        referencia = datos["referencia"],
                        estatus = "ACTIVO"
                    };
                    db.dEmpleadoDomicilio.Add(dEmpleadoDomicilio);
                    db.SaveChanges();

                    mEmpleado.iddEmpleadoDomicilio = dEmpleadoDomicilio.iddEmpleadoDomicilio;
                    mEmpleado.idcEmpresa = session.idcEmpresa;
                    db.mEmpleado.Add(mEmpleado);
                    db.SaveChanges();

                    var mPlaza = db.mPlaza.Find(int.Parse(datos["idmPlaza"]));
                    mPlaza.idmEmpleado = mEmpleado.idmEmpleado;
                    mPlaza.estatus = "AUTORIZADA";
                    if (datos["idmPlazaJefeInmediato"] != null && datos["idmPlazaJefeInmediato"] != "" && mPlaza.idmPlazaJefeInmediato != int.Parse(datos["idmPlazaJefeInmediato"]))
                    {
                        mPlaza.idmPlazaJefeInmediato = datos["idmPlazaJefeInmediato"] != "" ? int.Parse(datos["idmPlazaJefeInmediato"]) : (int?)null;
                    }
                    db.Entry(mPlaza).State = EntityState.Modified;
                    db.SaveChanges();

                    var cUsuario = new cUsuario()
                    {
                        idmEmpleado = mEmpleado.idmEmpleado,
                        usuario = CreateUserName(mEmpleado.nombre, mEmpleado.paterno, mEmpleado.materno),
                        clave = CreatePassword(mEmpleado.nombre, mEmpleado.paterno),
                        estatus = "ACTIVO"
                    };
                    db.cUsuario.Add(cUsuario);
                    db.SaveChanges();

                    var fechaInicial = DateTime.Parse(datos["fechaInicialContrato"]);
                    var idcTipoContrato = short.Parse(datos["idcTipoContrato"]);

                    var tiempoContrato = datos["diasContrato"] != null && datos["diasContrato"] != "" ? short.Parse(datos["diasContrato"]) : (short)0;
                    var fechaFinal = (DateTime?)null;
                    var proyecto = "";
                    var cTipoContrato = db.cTipoContrato.Find(idcTipoContrato);
                    if (!cTipoContrato.contrato.Equals("INDETERMINADO") && datos["fechaVencimientoContrato"] != null && datos["fechaVencimientoContrato"] != "")
                    {
                        var fechaVencimientoContrato = DateTime.Parse(datos["fechaVencimientoContrato"]);
                        fechaFinal = fechaVencimientoContrato;
                    }
                    if (!cTipoContrato.contrato.Equals("OBRA O TIEMPO DETERMINADO"))
                    {
                        proyecto = datos["proyecto"].ToString();
                    }
                    var eEmpleadoContrato = new eEmpleadoContrato()
                    {
                        idmEmpleado = mEmpleado.idmEmpleado,
                        noContrato = "1",
                        fechaInicial = fechaInicial,
                        idcTipoContrato = idcTipoContrato,
                        tiempoContrato = tiempoContrato,
                        fechaFinal = fechaFinal,
                        descripcion = datos["descripcion"] != null && datos["descripcion"] != "" ? datos["descripcion"].ToString() : "",
                        proyecto = proyecto
                    };
                    db.eEmpleadoContrato.Add(eEmpleadoContrato);
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

                    try
                    {
                        string ruta = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).FullName + "Documentacion\\" + mEmpleado.curp;
                        if (!Directory.Exists(ruta))
                        {
                            Directory.CreateDirectory(ruta);
                        }

                        if (fileFoto != null)
                        {
                            var fileName = fileFoto.FileName.Split('.');
                            string ext = fileName[fileName.Length - 1];
                            var name = mEmpleado.curp + "_FOTO_PERFIL";
                            new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(name)) System.IO.File.Delete(f); });
                            var img = System.Drawing.Image.FromStream(fileFoto.InputStream, true, true);
                            var porcentaje = (img.Height > 500) ? (500f / (float)img.Height) : (img.Height > 500) ? (((float)img.Height / 500f) + 1) : 1f;
                            var bmp = new Bitmap((Bitmap)img, new Size((int)Math.Round(porcentaje * img.Width), (int)Math.Round(porcentaje * img.Height)));
                            var info = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
                            using (var ep = new EncoderParameters(1))
                            {
                                ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)40);
                                bmp.Save(ruta + "\\" + name + "." + ext, info, ep);
                            }
                            mEmpleado.fotoPerfil = "Documentacion/" + mEmpleado.curp + "/" + name + "." + ext;
                            db.Entry(mEmpleado).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        if (fileContrato != null)
                        {
                            string nombre = mEmpleado.curp + "_CONTRATO_" + eEmpleadoContrato.noContrato;
                            string ext = fileContrato.FileName.Split('.')[1];
                            new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre)) System.IO.File.Delete(f); });
                            fileContrato.SaveAs(ruta + "\\" + nombre + "." + ext);
                            eEmpleadoContrato.ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + ext;
                            db.Entry(eEmpleadoContrato).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        if (fileDocumento != null && datos["idcDocumento"] != null && datos["idcDocumento"] != "")
                        {
                            byte idcDocumento = byte.Parse(datos["idcDocumento"]);
                            var cDocumento = db.cDocumento.Find(idcDocumento);
                            string nombre = mEmpleado.curp + "_" + cDocumento.documento.Replace(" ", "_");
                            string ext = fileDocumento.FileName.Split('.')[1];
                            new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre.ToUpper())) System.IO.File.Delete(f); });
                            fileDocumento.SaveAs(ruta + "\\" + nombre + "." + ext);
                            var eEmpleadoDocumentacion = db.eEmpleadoDocumentacion.Where(x => x.idmEmpleado == mEmpleado.idmEmpleado && x.idcDocumento == idcDocumento).FirstOrDefault();
                            if (eEmpleadoDocumentacion != null)
                            {
                                eEmpleadoDocumentacion.ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + ext;
                                db.Entry(eEmpleadoDocumentacion).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                db.eEmpleadoDocumentacion.Add(new eEmpleadoDocumentacion
                                {
                                    idmEmpleado = mEmpleado.idmEmpleado,
                                    idcDocumento = idcDocumento,
                                    ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + ext
                                });
                                db.SaveChanges();
                            }
                        }
                    }
                    catch { }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id, string seccion)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mEmpleado mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            ViewBag.cBanco = new SelectList(db.cBanco.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.banco), "idcBanco", "banco", mEmpleado.cBanco?.banco);
            ViewBag.cEstadoCivil = new SelectList(db.cEstadoCivil.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.estadoCivil), "estadoCivil", "estadoCivil", mEmpleado.cEstadoCivil?.estadoCivil);
            ViewBag.cEscolaridad = new SelectList(db.cEscolaridad.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.escolaridad), "escolaridad", "escolaridad", mEmpleado.cEscolaridad?.escolaridad);
            ViewBag.cProfesion = new SelectList(db.cProfesion.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.profesion), "profesion", "profesion", mEmpleado.cProfesion?.profesion);
            ViewBag.tipoNomina = new SelectList(db.cTipoNomina.Where(_ => _.estatus == "ACTIVO"), "idcTipoNomina", "tipoNomina", mEmpleado.tipoNomina);

            var idcEntidadFederativa = 0;
            var idcMunicipio = 0;
            var ef = db.cEntidadFederativa.FirstOrDefault(_ => _.entidadFederativa == mEmpleado.dEmpleadoDomicilio.cMunicipio.cEntidadFederativa.entidadFederativa);
            if (ef != null)
            {
                idcEntidadFederativa = ef.idcEntidadFederativa;
                var m = db.cMunicipio.FirstOrDefault(_ => _.idcEntidadFederativa == ef.idcEntidadFederativa && _.municipio == mEmpleado.dEmpleadoDomicilio.cMunicipio.municipio);
                if (m != null)
                {
                    idcMunicipio = m.idcMunicipio;
                }
            }

            ViewBag.idcEntidadFederativa = new SelectList(db.cEntidadFederativa, dataValueField: "idcEntidadFederativa", dataTextField: "entidadFederativa", idcEntidadFederativa);
            ViewBag.idcMunicipio = new SelectList(db.cMunicipio.Where(_ => _.idcEntidadFederativa == idcEntidadFederativa), dataValueField: "municipio", dataTextField: "municipio", idcMunicipio);

            var ji = db.mPlaza
                .Where(_ => _.estatus == "VACANTE" || _.estatus == "AUTORIZADA" || _.estatus == "PRE-ASIGNADO")
                .OrderBy(_ => _.cPuesto.puesto)
                .Select(_ => new { _.idmPlaza, puesto = _.cPuesto.puesto + (_.mEmpleado != null ? " - (" + _.mEmpleado.nombre + " " + _.mEmpleado.paterno + " " + _.mEmpleado.materno + ")" : "") });

            ViewBag.idmPlazaJefeInmediato = new SelectList(ji, "idmPlaza", "puesto", mEmpleado.mPlaza.Count > 0 ? mEmpleado.mPlaza.FirstOrDefault().idmPlazaJefeInmediato : 0);
            ViewBag.idcTipoContrato = new SelectList(db.cTipoContrato.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcTipoContrato", dataTextField: "contrato");
            ViewBag.idcDocumento = new SelectList(db.cDocumento.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcDocumento", dataTextField: "documento");
            ViewBag.nacionalidad = new List<SelectListItem>() {
                new SelectListItem { Text = "MEXICANA", Value = "MEXICANA", Selected = mEmpleado.nacionalidad == "MEXICANA" },
                new SelectListItem { Text = "EXTRANGERA", Value = "EXTRANGERA", Selected = mEmpleado.nacionalidad == "EXTRANGERA"  } };
            ViewBag.beneficio = db.cBeneficio.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.beneficio).ToList();
            ViewBag.documentoProbatorio = new SelectList(db.cDocumentoProbatorio.Where(_ => _.estatus == "ACTIVO"), "documentoProbatorio", "documentoProbatorio", mEmpleado.documentoProbatorio);
            ViewBag.tipoSangre = new List<SelectListItem>() {
                new SelectListItem { Text = "O+", Value = "O+", Selected = mEmpleado.tipoSangre == "O+" ? true : false } ,
                new SelectListItem { Text = "O-", Value = "O-", Selected = mEmpleado.tipoSangre == "O-" ? true : false } ,
                new SelectListItem { Text = "A+", Value = "A+", Selected = mEmpleado.tipoSangre == "A+" ? true : false },
                new SelectListItem { Text = "A-", Value = "A-", Selected = mEmpleado.tipoSangre == "A-" ? true : false },
                new SelectListItem { Text = "B+", Value = "B+", Selected = mEmpleado.tipoSangre == "B+" ? true : false } ,
                new SelectListItem { Text = "B-", Value = "B-", Selected = mEmpleado.tipoSangre == "B-" ? true : false } ,
                new SelectListItem { Text = "AB+", Value = "AB+", Selected = mEmpleado.tipoSangre == "AB+" ? true : false } ,
                new SelectListItem { Text = "AB-", Value = "AB-", Selected = mEmpleado.tipoSangre == "AB-" ? true : false } };
            ViewBag.tipoInstitucion = new List<SelectListItem>() {
                new SelectListItem { Text = "PUBLICO", Value = "PUBLICO", Selected = mEmpleado.tipoInstitucion == "PUBLICO" ? true : false },
                new SelectListItem { Text = "PRIVADO", Value = "PRIVADO", Selected = mEmpleado.tipoInstitucion == "PRIVADO" ? true : false } };
            ViewBag.estatusInstitucion = new List<SelectListItem>() {
                new SelectListItem { Text = "CONCLUIDO", Value = "CONCLUIDO", Selected = mEmpleado.estatusInstitucion == "CONCLUIDO" ? true : false },
                new SelectListItem { Text = "TRUNCO", Value = "TRUNCO", Selected = mEmpleado.estatusInstitucion == "TRUNCO" ? true : false } };
            ViewBag.sexo = new List<SelectListItem>() {
                new SelectListItem { Text = "MUJER", Value = "MUJER", Selected = mEmpleado.sexo == "MUJER" ? true : false },
                new SelectListItem { Text = "HOMBRE", Value = "HOMBRE", Selected = mEmpleado.sexo == "HOMBRE" ? true : false } };
            ViewBag.esquemaPago = new List<SelectListItem>() {
                new SelectListItem { Text = "INHOUSE", Value = "INHOUSE", Selected = mEmpleado.esquemaPago == "INHOUSE" ? true : false },
                new SelectListItem { Text = "ESPECIALIZADO", Value = "ESPECIALIZADO", Selected = mEmpleado.esquemaPago == "ESPECIALIZADO" ? true : false } };

            ViewBag.documentos = db.cDocumento.Where(_ => _.estatus == "ACTIVO" && _.responsable != "").OrderBy(_ => _.documento).ToList();

            ViewBag.seccion = seccion;

            var ln = new List<SelectListItem>();
            ln.Add(new SelectListItem() { Value = "NACIDO EN EL EXTRANJERO", Text = "NACIDO EN EL EXTRANJERO" });
            var ef2 = new SelectList(db.cEntidadFederativa, dataValueField: "entidadFederativa", dataTextField: "entidadFederativa");
            ln.AddRange(ef2);
            ViewBag.lugarNacimiento = new SelectList(ln, "Value", "Text", mEmpleado.lugarNacimiento);

            return View(mEmpleado);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idmEmpleado,idcEmpresa,iddEmpleadoDomicilio,nombre,paterno,materno,fechaContratacion,fechaIngresoImss,fechaNacimiento,lugarNacimiento,curp,rfc,nss,estadoCivil,hijos,nacionalidad,sexo,tipoSangre,correoCoorporativo,correoPersonal,telefono,telefonoContacto,nombreContacto,parentescoContacto,escolaridad,profesion,institucion,estatusInstitucion,esquemaPago,tipoNomina,patronal,noCreditoInfonavit,descuentoInfonavit,noCreditoFonacot,descuentoFonacot,codigoBanco,codigoSucursal,noCuenta,clabeInterbancaria,salarioDiarioExcedente,salarioDiarioCotizacion,salarioDiario,salarioIntegrado,salarioMensual,fotoPerfil,observaciones,estatus,tipoInstitucion,telefonoCelularEmpresa,telefonoFijoEmpresa,telefonoExtension,hijosEdadXML,documentoProbatorio,factorInfonavit,factorFonacot,idcHorario")] mEmpleado mEmpleado, FormCollection datos)
        {
            if (ModelState.IsValid)
            {
                var dh_m_Empleado_anterior = new mEmpleado();

                using (MasterRestEntities db2 = new MasterRestEntities())
                {
                    dh_m_Empleado_anterior = db2.mEmpleado.Include(_ => _.mPlaza).FirstOrDefault(_ => _.idmEmpleado == mEmpleado.idmEmpleado);
                }

                if (Request.Files.Count > 0)
                {
                    try
                    {
                        string ruta = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).FullName + "Documentacion\\" + mEmpleado.curp;
                        if (!Directory.Exists(ruta))
                        {
                            Directory.CreateDirectory(ruta);
                        }

                        foreach (var d in db.cDocumento)
                        {
                            HttpPostedFileBase fileDocumento = Request.Files["fileDocumento_" + d.idcDocumento];
                            if (fileDocumento != null && fileDocumento.ContentLength > 0)
                            {
                                string nombre = mEmpleado.curp + "_" + d.claveDocumento;
                                string extension = fileDocumento.FileName.Split('.')[1];
                                new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre.ToUpper())) System.IO.File.Delete(f); });
                                fileDocumento.SaveAs(ruta + "\\" + nombre + "." + extension);
                                var eEmpleadoDocumentacion = db.eEmpleadoDocumentacion.FirstOrDefault(x => x.idmEmpleado == mEmpleado.idmEmpleado && x.idcDocumento == d.idcDocumento);
                                if (eEmpleadoDocumentacion != null)
                                {
                                    eEmpleadoDocumentacion.ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + extension;
                                    db.Entry(eEmpleadoDocumentacion).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.eEmpleadoDocumentacion.Add(new eEmpleadoDocumentacion
                                    {
                                        idmEmpleado = mEmpleado.idmEmpleado,
                                        idcDocumento = d.idcDocumento,
                                        ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + extension
                                    });
                                }
                            }
                        }
                        db.SaveChanges();

                        HttpPostedFileBase fileFoto = Request.Files["fileFoto"];
                        if (fileFoto != null && fileFoto.ContentLength > 0)
                        {
                            var fileName = fileFoto.FileName.Split('.');
                            string ext = fileName[fileName.Length - 1];
                            var nombre = mEmpleado.curp + "_FOTO_PERFIL";
                            new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre)) System.IO.File.Delete(f); });
                            var img = System.Drawing.Image.FromStream(fileFoto.InputStream, true, true);
                            var porcentaje = (img.Height > 500) ? (500f / (float)img.Height) : (img.Height > 500) ? (((float)img.Height / 500f) + 1) : 1f;
                            var bmp = new Bitmap((Bitmap)img, new Size((int)Math.Round(porcentaje * img.Width), (int)Math.Round(porcentaje * img.Height)));
                            var info = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
                            using (var ep = new EncoderParameters(1))
                            {
                                ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)40);
                                bmp.Save(ruta + "\\" + nombre + "." + ext, info, ep);
                            }
                            mEmpleado.fotoPerfil = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + ext;
                        }

                    }
                    catch { }
                }

                var dEmpleadoDomicilio = db.dEmpleadoDomicilio.Find(mEmpleado.iddEmpleadoDomicilio);
                dEmpleadoDomicilio.idcMunicipio = datos["idcMunicipio"] != null ? short.Parse(datos["idcMunicipio"]) : dEmpleadoDomicilio.idcMunicipio;
                dEmpleadoDomicilio.colonia = datos["Colonia"] != null ? datos["Colonia"] : dEmpleadoDomicilio.colonia;
                dEmpleadoDomicilio.calle = datos["Calle"] != null ? datos["Calle"] : dEmpleadoDomicilio.calle;
                dEmpleadoDomicilio.noExterior = datos["noExterior"] != null ? datos["noExterior"] : dEmpleadoDomicilio.noExterior;
                dEmpleadoDomicilio.noInterior = datos["noInterior"] != null ? datos["noInterior"] : dEmpleadoDomicilio.noInterior;
                dEmpleadoDomicilio.referencia = datos["referencia"] != null ? datos["referencia"] : dEmpleadoDomicilio.referencia;
                dEmpleadoDomicilio.codigoPostal = datos["CodigoPostal"] != null ? datos["CodigoPostal"] : dEmpleadoDomicilio.codigoPostal;
                db.Entry(dEmpleadoDomicilio).State = EntityState.Modified;
                db.SaveChanges();

                mEmpleado.salarioMensual = datos["salarioMensual2"] != null && datos["salarioMensual2"] != "" ? Utilities.Encriptar(datos["salarioMensual2"]) : mEmpleado.salarioMensual != null && mEmpleado.salarioMensual != "" ? Utilities.Encriptar(mEmpleado.salarioMensual) : null;
                mEmpleado.salarioDiario = datos["salarioDiario2"] != null && datos["salarioDiario2"] != "" ? Utilities.Encriptar(datos["salarioDiario2"]) : mEmpleado.salarioDiario != null && mEmpleado.salarioDiario != "" ? Utilities.Encriptar(mEmpleado.salarioDiario) : null;
                mEmpleado.salarioIntegrado = datos["salarioIntegrado2"] != null && datos["salarioIntegrado2"] != "" ? Utilities.Encriptar(datos["salarioIntegrado2"]) : mEmpleado.salarioIntegrado != null && mEmpleado.salarioIntegrado != "" ? Utilities.Encriptar(mEmpleado.salarioIntegrado) : null;
                mEmpleado.salarioDiarioCotizacion = datos["salarioDiarioCotizacion2"] != null && datos["salarioDiarioCotizacion2"] != "" ? Utilities.Encriptar(datos["salarioDiarioCotizacion2"]) : mEmpleado.salarioDiarioCotizacion != null && mEmpleado.salarioDiarioCotizacion != "" ? Utilities.Encriptar(mEmpleado.salarioDiarioCotizacion) : null;
                mEmpleado.salarioDiarioExcedente = datos["salarioDiarioExcedente2"] != null && datos["salarioDiarioExcedente2"] != "" ? Utilities.Encriptar(datos["salarioDiarioExcedente2"]) : mEmpleado.salarioDiarioExcedente != null && mEmpleado.salarioDiarioExcedente != "" ? Utilities.Encriptar(mEmpleado.salarioDiarioExcedente) : null;

                db.Entry(mEmpleado).State = EntityState.Modified;
                db.SaveChanges();

                var mPlaza = db.mPlaza.FirstOrDefault(_ => _.idmEmpleado == mEmpleado.idmEmpleado);

                if (mPlaza != null)
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
                }

                mEmpleado.dEmpleadoDomicilio = db.dEmpleadoDomicilio.Find(mEmpleado.iddEmpleadoDomicilio);
                mEmpleado.mPlaza = db.mPlaza.Where(_ => _.idmEmpleado == mEmpleado.idmEmpleado).ToList();

                //auditrail("Edit", "EDITAR Empleado", "MODIFICAR", mEmpleado.idmEmpleado.ToString(), makeJson(mEmpleado, mEmpleado.mPlaza.Count() > 0 ? mEmpleado.mPlaza.FirstOrDefault().ePlazaBeneficio.ToList() : new List<ePlazaBeneficio>()), "");
            }

            return RedirectToAction("Index", new { buscar = mEmpleado.paterno + " " + mEmpleado.materno + " " + mEmpleado.nombre });
        }

        [SessionExpire]
        public ActionResult Salary(int? id, string seccion)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mEmpleado mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            
            return View(mEmpleado);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Salary([Bind(Include = "idmEmpleado,idcEmpresa,iddEmpleadoDomicilio,nombre,paterno,materno,fechaContratacion,fechaIngresoImss,fechaNacimiento,lugarNacimiento,curp,rfc,nss,estadoCivil,hijos,nacionalidad,sexo,tipoSangre,correoCoorporativo,correoPersonal,telefono,telefonoContacto,nombreContacto,parentescoContacto,escolaridad,profesion,institucion,estatusInstitucion,esquemaPago,tipoNomina,patronal,noCreditoInfonavit,descuentoInfonavit,noCreditoFonacot,descuentoFonacot,codigoBanco,codigoSucursal,noCuenta,clabeInterbancaria,salarioDiarioExcedente,salarioDiarioCotizacion,salarioDiario,salarioIntegrado,salarioMensual,fotoPerfil,observaciones,estatus,tipoInstitucion,telefonoCelularEmpresa,telefonoFijoEmpresa,telefonoExtension,hijosEdadXML,documentoProbatorio,factorInfonavit,factorFonacot,idcHorario")] mEmpleado mEmpleado, FormCollection datos)
        {
            var dh_m_Empleado2 = db.mEmpleado.Find(mEmpleado.idmEmpleado);

            dh_m_Empleado2.salarioMensual = datos["salarioMensual2"] != null && datos["salarioMensual2"] != "" ? Utilities.Encriptar(datos["salarioMensual2"]) : mEmpleado.salarioMensual != null && mEmpleado.salarioMensual != "" ? Utilities.Encriptar(mEmpleado.salarioMensual) : null;
            dh_m_Empleado2.salarioDiario = datos["salarioDiario2"] != null && datos["salarioDiario2"] != "" ? Utilities.Encriptar(datos["salarioDiario2"]) : mEmpleado.salarioDiario != null && mEmpleado.salarioDiario != "" ? Utilities.Encriptar(mEmpleado.salarioDiario) : null;
            dh_m_Empleado2.salarioIntegrado = datos["salarioIntegrado2"] != null && datos["salarioIntegrado2"] != "" ? Utilities.Encriptar(datos["salarioIntegrado2"]) : mEmpleado.salarioIntegrado != null && mEmpleado.salarioIntegrado != "" ? Utilities.Encriptar(mEmpleado.salarioIntegrado) : null;
            dh_m_Empleado2.salarioDiarioCotizacion = datos["salarioDiarioCotizacion2"] != null && datos["salarioDiarioCotizacion2"] != "" ? Utilities.Encriptar(datos["salarioDiarioCotizacion2"]) : mEmpleado.salarioDiarioCotizacion != null && mEmpleado.salarioDiarioCotizacion != "" ? Utilities.Encriptar(mEmpleado.salarioDiarioCotizacion) : null;
            dh_m_Empleado2.salarioDiarioExcedente = datos["salarioDiarioExcedente2"] != null && datos["salarioDiarioExcedente2"] != "" ? Utilities.Encriptar(datos["salarioDiarioExcedente2"]) : mEmpleado.salarioDiarioExcedente != null && mEmpleado.salarioDiarioExcedente != "" ? Utilities.Encriptar(mEmpleado.salarioDiarioExcedente) : null;

            db.Entry(dh_m_Empleado2).State = EntityState.Modified;
            db.SaveChanges();

            //auditrail("Salary", "EDITAR SALARIO Empleado", "MODIFICAR SALARIO", dh_m_Empleado2.idmEmpleado.ToString(), makeJson(dh_m_Empleado2, dh_m_Empleado2.mPlaza.Count() > 0 ? dh_m_Empleado2.mPlaza.FirstOrDefault().ePlazaBeneficio.ToList() : new List<ePlazaBeneficio>()), "");

            return RedirectToAction("Index", new { buscar = dh_m_Empleado2.paterno + " " + dh_m_Empleado2.materno + " " + dh_m_Empleado2.nombre });
        }


        [SessionExpire]
        public ActionResult Discharge(int? id) 
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mEmpleado mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            ViewBag.nombre = mEmpleado.nombre + " " + mEmpleado.paterno + " " + mEmpleado.materno;
            ViewBag.puesto = mEmpleado.mPlaza.FirstOrDefault().cPuesto.puesto;
            ViewBag.tipoBaja = new SelectList(db.cTipoBaja.Where(_ => _.estatus == "ACTIVO"), "tipoBaja", "tipoBaja");
            ViewBag.motivoBaja = new SelectList(db.cMotivoBaja.Where(_ => _.estatus == "ACTIVO"), "motivo", "motivo");
            return View(new eEmpleadoBaja()
            {
                idmEmpleado = mEmpleado.idmEmpleado,
                idmPlaza = mEmpleado.mPlaza.FirstOrDefault().idmPlaza,
                fechaAlta = mEmpleado.fechaContratacion,
                recontratable = true
            });
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Discharge([Bind(Include = "ideEmpleadoBaja,idmEmpleado,idmPlaza,empresa,sucursal,rol,idmPlazaJefeInmediato,puesto,area,motivoBaja,tipoBaja,fechaContratacion,fechaBaja,observacionesBaja,recontratable,json")] eEmpleadoBaja eEmpleadoBaja, FormCollection datos)
        {
            try
            {
                var mPlaza = db.mPlaza.FirstOrDefault(_ => _.idmPlaza == eEmpleadoBaja.idmPlaza);
                var mEmpleado = db.mEmpleado.Find(eEmpleadoBaja.idmPlaza);
                var cUsuario = db.cUsuario.Find(eEmpleadoBaja.idmPlaza);
                //var json = makeJson(mEmpleado, mPlaza != null ? mPlaza.ePlazaBeneficio.ToList() : new List<ePlazaBeneficio>());
                //auditrail("Discharge", "BAJA Empleado", "DAR DE BAJA", mEmpleado.idmEmpleado.ToString(), json, "");

                mPlaza.idmEmpleado = null;
                mPlaza.estatus = "VACANTE";
                db.Entry(mPlaza).State = EntityState.Modified;
                db.SaveChanges();

                mEmpleado.estatus = "BAJA";
                db.Entry(mEmpleado).State = EntityState.Modified;
                db.SaveChanges();

                if (cUsuario != null)
                {
                    cUsuario.estatus = "INACTIVO";
                    db.Entry(cUsuario).State = EntityState.Modified;
                    db.SaveChanges();
                }

                eEmpleadoBaja.empresa = mPlaza.cEmpresa.nombreComercial;
                eEmpleadoBaja.sucursal = mPlaza.cSucursal.sucursal;
                eEmpleadoBaja.rol = mPlaza.cRol.rol;
                var idmPlazaJefeInmediato = db.mPlaza.Find(mPlaza.idmPlazaJefeInmediato);
                eEmpleadoBaja.idmPlazaJefeInmediato = idmPlazaJefeInmediato != null && idmPlazaJefeInmediato.mEmpleado != null ? idmPlazaJefeInmediato.mEmpleado.nombre + " " + idmPlazaJefeInmediato.mEmpleado.paterno + " " + idmPlazaJefeInmediato.mEmpleado.materno : "";
                eEmpleadoBaja.puesto = mPlaza.cPuesto.puesto;
                eEmpleadoBaja.area = mPlaza.cArea.area;
                eEmpleadoBaja.recontratable = datos["recontratable"] != null ? true : false;
                //eEmpleadoBaja.json = json;
                db.eEmpleadoBaja.Add(eEmpleadoBaja);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                var err = "" + e.Message;
            }

            return RedirectToAction("Index");
        }

        [SessionExpire]
        public ActionResult Promote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vacantes = db.mPlaza.Where(m => m.estatus == "VACANTE").ToList();
            if (vacantes.Count() == 0)
            {
                ViewBag.sinVacantes = "No hay vacantes para promover";
                return View(new eEmpleadoPromocion());
            }
            var mEmpleado = db.mEmpleado.FirstOrDefault(x => x.idmEmpleado == id);
            if (mEmpleado != null)
            {
                ViewBag.nombre = mEmpleado.nombre + " " + mEmpleado.paterno + " " + mEmpleado.materno;
            }
            ViewBag.idmPlaza = id;
            ViewBag.tipoNomina = mEmpleado.tipoNomina;
            ViewBag.idcMotivoMovimiento = new SelectList(db.cMotivoMovimiento.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcMotivoMovimiento", dataTextField: "motivo");

            var plazasVacantes = vacantes.Select(m => new { puesto = (m.idmPlaza + " | " + m.cPuesto.puesto), m.idmPlaza }).OrderBy(x => x.puesto).ToList();
            ViewBag.idmPlaza = new SelectList(plazasVacantes, dataValueField: "idmPlaza", dataTextField: "puesto");
            var mPlaza = db.mPlaza.FirstOrDefault(_ => _.idmPlaza == id);
            var eEmpleadoPromocion = new eEmpleadoPromocion();
            eEmpleadoPromocion.puestoActual = mPlaza != null ? mPlaza.cPuesto.puesto : "";
            eEmpleadoPromocion.sueldoActual = mEmpleado.salarioMensual != null && mEmpleado.salarioMensual != "" ? mEmpleado.salarioMensual : "";
            ViewBag.empresa = new SelectList(db.cEmpresa.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.nombreComercial), "empresa", "empresa");

            ViewBag.salarioMensual = mEmpleado.salarioMensual != null && mEmpleado.salarioMensual != "" ? mEmpleado.salarioMensual : "";

            return View(eEmpleadoPromocion);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Promote([Bind(Include = "ideEmpleadoPromocion,idmEmpleado,idcMotivoMovimiento,fechaMovimiento,sueldoActual,SueldoNuevo,puestoActual,puestoNuevo,areaActual,areaNuevo,sucursalActual,sucursalNuevo,jefeInmediatoActual,jefeInmediatoNuevo,observaciones")] eEmpleadoPromocion eEmpleadoPromocion, FormCollection datos)
        {
            var mEmpleado = db.mEmpleado.Where(x => x.idmEmpleado == (int)eEmpleadoPromocion.idmEmpleado).FirstOrDefault();
            var sueldoActual = mEmpleado.salarioMensual;

            var plantillaAnterior = db.mPlaza.Where(m => m.idmPlaza == eEmpleadoPromocion.idmEmpleado).FirstOrDefault();
            if (plantillaAnterior != null)
            {

                var mm = eEmpleadoPromocion.idcMotivoMovimiento;

                if (mm == 2 || mm == 3 || mm == 4 || mm == 5)
                {
                    plantillaAnterior.idmEmpleado = null;
                    plantillaAnterior.estatus = "VACANTE";
                    db.Entry(plantillaAnterior).State = EntityState.Modified;

                    var plantillaNueva = db.mPlaza.Find(int.Parse(datos["idmPlaza"]));
                    plantillaNueva.idmPlaza = (int)eEmpleadoPromocion.idmEmpleado;
                    plantillaNueva.estatus = "AUTORIZADA";
                    db.Entry(plantillaNueva).State = EntityState.Modified;

                    eEmpleadoPromocion.puestoNuevo = plantillaNueva.cPuesto.puesto;
                }

                if (mm == 1 || mm == 2 || mm == 4 || mm == 5)
                {
                    mEmpleado.salarioMensual = eEmpleadoPromocion.sueldoNuevo != "" ? eEmpleadoPromocion.sueldoNuevo : "";
                    mEmpleado.salarioDiario = datos["salarioDiario"] != null && !datos["salarioDiario"].Equals("") ? datos["salarioDiario"] : "";
                    mEmpleado.salarioIntegrado = datos["salarioIntegrado"] != null && !datos["salarioIntegrado"].Equals("") ? datos["salarioIntegrado"] : "";
                    db.Entry(mEmpleado).State = EntityState.Modified;

                    eEmpleadoPromocion.sueldoNuevo = !eEmpleadoPromocion.sueldoNuevo.Equals("") ? eEmpleadoPromocion.sueldoNuevo : "";
                }

                if (mm == 6)
                {
                    eEmpleadoPromocion.sucursalActual = plantillaAnterior.cSucursal.sucursal;
                    eEmpleadoPromocion.sucursalNuevo = datos["empresa"].ToString();

                    plantillaAnterior.cSucursal.sucursal = datos["empresa"].ToString();
                    db.Entry(plantillaAnterior).State = EntityState.Modified;
                }


                if (mm == 9 && datos["idmPlazaJefeInmediato"] != null && datos["idmPlazaJefeInmediato"] != "")
                {
                    var jin_ = int.Parse(datos["idmPlazaJefeInmediato"]);
                    var jia = db.mPlaza.Find(plantillaAnterior.idmPlazaJefeInmediato);
                    var jin = db.mPlaza.Find(jin_);
                    eEmpleadoPromocion.jefeInmediatoActual = jia.cPuesto.puesto + (jia.mEmpleado != null ? " - (" + jia.mEmpleado.nombre + " " + jia.mEmpleado.paterno + " " + jia.mEmpleado.materno + ")" : "");
                    eEmpleadoPromocion.jefeInmediatoNuevo = jin.cPuesto.puesto + (jin.mEmpleado != null ? " - (" + jin.mEmpleado.nombre + " " + jin.mEmpleado.paterno + " " + jin.mEmpleado.materno + ")" : "");
                    plantillaAnterior.idmPlazaJefeInmediato = jin_;
                    db.Entry(plantillaAnterior).State = EntityState.Modified;
                }

                eEmpleadoPromocion.sueldoActual = sueldoActual;
                eEmpleadoPromocion.observaciones = eEmpleadoPromocion.observaciones != null ? eEmpleadoPromocion.observaciones : "";
                db.eEmpleadoPromocion.Add(eEmpleadoPromocion);
                db.SaveChanges();
            }

            var mPlaza = db.mPlaza.FirstOrDefault(_ => _.idmPlaza == mEmpleado.idmEmpleado);

            //auditrail("Promote", "PROMOVER Empleado", "PROMOVER", mEmpleado.idmEmpleado.ToString(), makeJson(mEmpleado, mPlaza != null ? mPlaza.ePlazaBeneficio.ToList() : new List<ePlazaBeneficio>()), "");

            return RedirectToAction("Index");
        }

        [SessionExpire]
        public ActionResult Reenter(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mEmpleado mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            ViewBag.cBanco = new SelectList(db.cBanco.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.banco), "idcBanco", "banco", mEmpleado.cBanco?.banco);
            ViewBag.cEstadoCivil = new SelectList(db.cEstadoCivil.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.estadoCivil), "estadoCivil", "estadoCivil", mEmpleado.cEstadoCivil?.estadoCivil);
            ViewBag.cEscolaridad = new SelectList(db.cEscolaridad.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.escolaridad), "escolaridad", "escolaridad", mEmpleado.cEscolaridad?.escolaridad);
            ViewBag.cProfesion = new SelectList(db.cProfesion.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.profesion), "profesion", "profesion", mEmpleado.cProfesion?.profesion);
            ViewBag.tipoNomina = new SelectList(db.cTipoNomina.Where(_ => _.estatus == "ACTIVO"), "tipoNomina", "tipoNomina", mEmpleado.tipoNomina);
            ViewBag.idcEntidadFederativa = new SelectList(db.cEntidadFederativa, dataValueField: "idcEntidadFederativa", dataTextField: "entidadFederativa", mEmpleado.dEmpleadoDomicilio?.cMunicipio?.idcEntidadFederativa);
            ViewBag.idcMunicipio = new SelectList(db.cMunicipio, dataValueField: "idcMunicipio", dataTextField: "municipio", mEmpleado.dEmpleadoDomicilio.idcMunicipio);
            ViewBag.idmPlaza = new SelectList(db.mPlaza.Where(_ => _.estatus == "VACANTE").OrderBy(_ => _.cPuesto.puesto).Select(_ => new { _.idmPlaza, puesto = _.idmPlaza + " | " + _.cPuesto.puesto }), "idmPlaza ", "puesto");

            var ji = db.mPlaza
                .Where(_ => _.estatus == "VACANTE" || _.estatus == "AUTORIZADA" || _.estatus == "PRE-ASIGNADO")
                .OrderBy(_ => _.cPuesto.puesto)
                .Select(_ => new { _.idmPlaza, puesto = _.cPuesto.puesto + (_.mEmpleado != null ? " - (" + _.mEmpleado.nombre + " " + _.mEmpleado.paterno + " " + _.mEmpleado.materno + ")" : "") });

            ViewBag.idmPlazaJefeInmediato = new SelectList(ji, "idmPlaza", "puesto", mEmpleado.mPlaza.Count > 0 ? mEmpleado.mPlaza.FirstOrDefault().idmPlazaJefeInmediato : 0);
            ViewBag.idcTipoContrato = new SelectList(db.cTipoContrato.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcTipoContrato", dataTextField: "contrato");
            ViewBag.idcDocumento = new SelectList(db.cDocumento.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcDocumento", dataTextField: "documento");
            ViewBag.nacionalidad = new List<SelectListItem>() {
                new SelectListItem { Text = "MEXICANA", Value = "MEXICANA", Selected = mEmpleado.nacionalidad == "MEXICANA" },
                new SelectListItem { Text = "EXTRANGERA", Value = "EXTRANGERA", Selected = mEmpleado.nacionalidad == "EXTRANGERA"  } };
            ViewBag.beneficio = db.cBeneficio.Where(_ => _.estatus == "ACTIVO").OrderBy(_ => _.beneficio).ToList();
            ViewBag.documentoProbatorio = new SelectList(db.cDocumentoProbatorio.Where(_ => _.estatus == "ACTIVO"), "documentoProbatorio", "documentoProbatorio", mEmpleado.documentoProbatorio);
            ViewBag.tipoSangre = new List<SelectListItem>() {
                new SelectListItem { Text = "O+", Value = "O+", Selected = mEmpleado.tipoSangre == "O+" ? true : false } ,
                new SelectListItem { Text = "O-", Value = "O-", Selected = mEmpleado.tipoSangre == "O-" ? true : false } ,
                new SelectListItem { Text = "A+", Value = "A+", Selected = mEmpleado.tipoSangre == "A+" ? true : false },
                new SelectListItem { Text = "A-", Value = "A-", Selected = mEmpleado.tipoSangre == "A-" ? true : false },
                new SelectListItem { Text = "B+", Value = "B+", Selected = mEmpleado.tipoSangre == "B+" ? true : false } ,
                new SelectListItem { Text = "B-", Value = "B-", Selected = mEmpleado.tipoSangre == "B-" ? true : false } ,
                new SelectListItem { Text = "AB+", Value = "AB+", Selected = mEmpleado.tipoSangre == "AB+" ? true : false } ,
                new SelectListItem { Text = "AB-", Value = "AB-", Selected = mEmpleado.tipoSangre == "AB-" ? true : false } };
            ViewBag.tipoInstitucion = new List<SelectListItem>() {
                new SelectListItem { Text = "PUBLICO", Value = "PUBLICO", Selected = mEmpleado.tipoInstitucion == "PUBLICO" ? true : false },
                new SelectListItem { Text = "PRIVADO", Value = "PRIVADO", Selected = mEmpleado.tipoInstitucion == "PRIVADO" ? true : false } };
            ViewBag.estatusInstitucion = new List<SelectListItem>() {
                new SelectListItem { Text = "CONCLUIDO", Value = "CONCLUIDO", Selected = mEmpleado.estatusInstitucion == "CONCLUIDO" ? true : false },
                new SelectListItem { Text = "TRUNCO", Value = "TRUNCO", Selected = mEmpleado.estatusInstitucion == "TRUNCO" ? true : false } };
            ViewBag.sexo = new List<SelectListItem>() {
                new SelectListItem { Text = "MUJER", Value = "MUJER", Selected = mEmpleado.sexo == "MUJER" ? true : false },
                new SelectListItem { Text = "HOMBRE", Value = "HOMBRE", Selected = mEmpleado.sexo == "HOMBRE" ? true : false } };
            ViewBag.esquemaPago = new List<SelectListItem>() {
                new SelectListItem { Text = "INHOUSE", Value = "INHOUSE", Selected = mEmpleado.esquemaPago == "INHOUSE" ? true : false },
                new SelectListItem { Text = "ESPECIALIZADO", Value = "ESPECIALIZADO", Selected = mEmpleado.esquemaPago == "ESPECIALIZADO" ? true : false } };
            ViewBag.documentos = db.cDocumento.Where(_ => _.estatus == "ACTIVO" && _.responsable != "").ToList();

            var ln = new List<SelectListItem>();
            ln.Add(new SelectListItem() { Value = "NACIDO EN EL EXTRANJERO", Text = "NACIDO EN EL EXTRANJERO" });
            var ef = new SelectList(db.cEntidadFederativa, dataValueField: "entidadFederativa", dataTextField: "entidadFederativa");
            ln.AddRange(ef);
            ViewBag.lugarNacimiento = new SelectList(ln, "Value", "Text", mEmpleado.lugarNacimiento);

            return View(mEmpleado);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reenter([Bind(Include = "idmPlaza,iddEmpleadoDomicilio,numeroEmpleado,nombre,paterno,materno,fechaContratacion,fechaIngresoImss,fechaNacimiento,lugarNacimiento,curp,rfc,nss,estadoCivil,hijos,nacionalidad,sexo,tipoSangre,correoCoorporativo,correoPersonal,telefono,telefonoContacto,nombreContacto,parentescoContacto,escolaridad,profesion,institucion,estatusInstitucion,esquemaPago,tipoNomina,patronal,noCreditoInfonavit,descuentoInfonavit,noCreditoFonacot,descuentoFonacot,codigoBanco,codigoSucursal,noCuenta,clabeInterbancaria,salarioDiarioExcedente,salarioDiarioCotizacion,salarioDiario,salarioIntegrado,salarioMensual,fotoPerfil,observaciones,estatus,tipoInstitucion,telefonoCelularEmpresa,telefonoFijoEmpresa,telefonoExtension,hijosEdadXML,documentoProbatorio,factorInfonavit,factorFonacot,idcHorario")] mEmpleado mEmpleado, FormCollection datos)
        {
            if (ModelState.IsValid)
            {
                if (Request.Files.Count > 0)
                {
                    try
                    {
                        string ruta = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).FullName + "Documentacion\\" + mEmpleado.curp;
                        if (!Directory.Exists(ruta))
                        {
                            Directory.CreateDirectory(ruta);
                        }

                        foreach (var d in db.cDocumento)
                        {
                            HttpPostedFileBase fileDocumento = Request.Files["fileDocumento_" + d.idcDocumento];
                            if (fileDocumento != null && fileDocumento.ContentLength > 0)
                            {
                                string nombre = mEmpleado.curp + "_" + d.claveDocumento;
                                string extension = fileDocumento.FileName.Split('.')[1];
                                new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre.ToUpper())) System.IO.File.Delete(f); });
                                fileDocumento.SaveAs(ruta + "\\" + nombre + "." + extension);

                                var eEmpleadoDocumentacion = db.eEmpleadoDocumentacion.FirstOrDefault(x => x.idmEmpleado == mEmpleado.idmEmpleado && x.idcDocumento == d.idcDocumento);
                                if (eEmpleadoDocumentacion != null)
                                {
                                    eEmpleadoDocumentacion.ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + extension;
                                    db.Entry(eEmpleadoDocumentacion).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.eEmpleadoDocumentacion.Add(new eEmpleadoDocumentacion
                                    {
                                        idmEmpleado = mEmpleado.idmEmpleado,
                                        idcDocumento = d.idcDocumento,
                                        ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + extension
                                    });
                                }
                            }
                        }
                        db.SaveChanges();

                        HttpPostedFileBase fileFoto = Request.Files["fileFoto"];
                        if (fileFoto != null && fileFoto.ContentLength > 0)
                        {
                            var fileName = fileFoto.FileName.Split('.');
                            string ext = fileName[fileName.Length - 1];
                            string nombre = mEmpleado.curp + "_FOTO_PERFIL";
                            new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre)) System.IO.File.Delete(f); });
                            var img = System.Drawing.Image.FromStream(fileFoto.InputStream, true, true);
                            var porcentaje = (img.Height > 500) ? (500f / (float)img.Height) : (img.Height > 500) ? (((float)img.Height / 500f) + 1) : 1f;
                            var bmp = new Bitmap((Bitmap)img, new Size((int)Math.Round(porcentaje * img.Width), (int)Math.Round(porcentaje * img.Height)));
                            var info = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
                            using (var ep = new EncoderParameters(1))
                            {
                                ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)40);
                                bmp.Save(ruta + "\\" + nombre + "." + ext, info, ep);
                            }
                            mEmpleado.fotoPerfil = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + ext;
                        }

                    }
                    catch { }
                }

                mEmpleado.estatus = "ACTIVO";

                mEmpleado.salarioMensual = mEmpleado.salarioMensual != null && mEmpleado.salarioMensual != "" ? Utilities.Encriptar(mEmpleado.salarioMensual) : null;
                mEmpleado.salarioDiario = mEmpleado.salarioDiario != null && mEmpleado.salarioDiario != "" ? Utilities.Encriptar(mEmpleado.salarioDiario) : null;
                mEmpleado.salarioIntegrado = mEmpleado.salarioIntegrado != null && mEmpleado.salarioIntegrado != "" ? Utilities.Encriptar(mEmpleado.salarioIntegrado) : null;
                mEmpleado.salarioDiarioCotizacion = mEmpleado.salarioDiarioCotizacion != null && mEmpleado.salarioDiarioCotizacion != "" ? Utilities.Encriptar(mEmpleado.salarioDiarioCotizacion) : null;
                mEmpleado.salarioDiarioExcedente = mEmpleado.salarioDiarioExcedente != null && mEmpleado.salarioDiarioExcedente != "" ? Utilities.Encriptar(mEmpleado.salarioDiarioExcedente) : null;

                db.Entry(mEmpleado).State = EntityState.Modified;
                db.SaveChanges();

                var mPlaza = db.mPlaza.Find(int.Parse(datos["idmPlaza"]));
                mPlaza.idmEmpleado = mEmpleado.idmEmpleado;
                mPlaza.estatus = "AUTORIZADA";
                if (datos["idmPlazaJefeInmediato"] != null && datos["idmPlazaJefeInmediato"] != "" && mPlaza.idmPlazaJefeInmediato != int.Parse(datos["idmPlazaJefeInmediato"]))
                {
                    mPlaza.idmPlazaJefeInmediato = datos["idmPlazaJefeInmediato"] != "" ? int.Parse(datos["idmPlazaJefeInmediato"]) : (int?)null;
                }
                db.Entry(mPlaza).State = EntityState.Modified;
                db.SaveChanges();

                var cUsuario = db.cUsuario.FirstOrDefault(_ => _.idmEmpleado == mEmpleado.idmEmpleado);
                if (cUsuario != null)
                {
                    cUsuario.estatus = "ACTIVO";
                    db.Entry(cUsuario).State = EntityState.Modified;
                }

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
                                    db.Entry(beneficio).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                db.ePlazaBeneficio.Add(new ePlazaBeneficio() { idmPlaza = mPlaza.idmPlaza, idcBeneficio = b.idcBeneficio, valor = monto != null ? monto : "" });
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

                mEmpleado.dEmpleadoDomicilio = db.dEmpleadoDomicilio.Find(mEmpleado.iddEmpleadoDomicilio);

                mEmpleado.mPlaza = db.mPlaza.Where(_ => _.idmEmpleado == mEmpleado.idmEmpleado).ToList();

                //auditrail("Reenter", "REINGRESAR Empleado", "REINGRESAR", mEmpleado.idmEmpleado.ToString(), makeJson(mEmpleado, mPlaza != null ? mPlaza.ePlazaBeneficio.ToList() : new List<ePlazaBeneficio>()), "");
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mEmpleado mEmpleado = db.mEmpleado.Find(id);
            if (mEmpleado == null)
            {
                return HttpNotFound();
            }
            return View(mEmpleado);
        }

        [SessionExpire]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var eEmpleadoContrato = db.eEmpleadoContrato.Where(_ => _.idmEmpleado == id);
            var eEmpleadoDocumentacion = db.eEmpleadoDocumentacion.Where(_ => _.idmEmpleado == id);
            var eEmpleadoPromocion = db.eEmpleadoPromocion.Where(_ => _.idmEmpleado == id);
            var eEmpleadoBaja = db.eEmpleadoBaja.Where(_ => _.idmEmpleado == id);
            var mEmpleado = db.mEmpleado.Find(id);
            var dEmpleadoDomicilio = db.dEmpleadoDomicilio.Where(_ => _.iddEmpleadoDomicilio == mEmpleado.iddEmpleadoDomicilio);
            var mPlaza = db.mPlaza.FirstOrDefault(_ => _.idmPlaza == id);

            //auditrail("DeleteConfirmed", "ELIMINAR Empleado", "ELIMINAR", mEmpleado.idmEmpleado.ToString(), makeJson(mEmpleado, mPlaza != null ? mPlaza.ePlazaBeneficio.ToList() : new List<ePlazaBeneficio>()), "");

            db.eEmpleadoContrato.RemoveRange(eEmpleadoContrato);
            db.eEmpleadoDocumentacion.RemoveRange(eEmpleadoDocumentacion);
            db.eEmpleadoPromocion.RemoveRange(eEmpleadoPromocion);
            db.eEmpleadoBaja.RemoveRange(eEmpleadoBaja);
            db.dEmpleadoDomicilio.RemoveRange(dEmpleadoDomicilio);
            if (mPlaza != null)
            {
                mPlaza.estatus = "VACANTE";
                mPlaza.idmEmpleado = null;
                db.Entry(mPlaza).State = EntityState.Modified;
            }
            db.mEmpleado.Remove(mEmpleado);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [SessionExpire]
        public ActionResult UploadFile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eEmpleadoDocumentacion = new eEmpleadoDocumentacion();
            eEmpleadoDocumentacion.idmEmpleado = (int)id;
            ViewBag.idcDocumento = new SelectList(db.cDocumento.Where(_ => _.estatus == "ACTIVO"), dataValueField: "idcDocumento", dataTextField: "documento");
            return View(eEmpleadoDocumentacion);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile([Bind(Include = "ideDocumentacion,idmPlaza,idcDocumento,ruta")] eEmpleadoDocumentacion eEmpleadoDocumentacion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eEmpleadoDocumentacion).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [SessionExpire]
        public ActionResult UploadContract(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(new eEmpleadoContrato());
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadContract([Bind(Include = "ideContrato,idmPlaza,noContrato,descripcion,fechaInicial,fechaFinal,idcTipoContrato,tiempoContrato,proyecto,ruta")] eEmpleadoContrato eEmpleadoContrato)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eEmpleadoContrato).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [SessionExpire]
        [HttpPost]
        public JsonResult GuardarContrato(FormCollection datos)
        {
            var idmEmpleado = int.Parse(datos["idmEmpleado"]);
            var idcTipoContrato = short.Parse(datos["idcTipoContrato"]);
            var fechaInicial = DateTime.Parse(datos["fechaInicial"]);
            var tiempoContrato = (short?)null;
            var fechaFinal = (DateTime?)null;
            if (!db.cTipoContrato.Find(idcTipoContrato).contrato.Equals("INDETERMINADO"))
            {
                fechaFinal = DateTime.Parse(datos["fechaFinal"]);
                tiempoContrato = (short)Math.Abs((fechaInicial.Month - ((DateTime)fechaFinal).Month) + 12 * (fechaInicial.Year - ((DateTime)fechaFinal).Year));
            }

            var eEmpleadoContrato = new eEmpleadoContrato()
            {
                idmEmpleado = idmEmpleado,
                noContrato = (db.eEmpleadoContrato.Where(_ => _.idmEmpleado == idmEmpleado).Count() + 1).ToString(),
                descripcion = datos["descripcion"] != null ? datos["descripcion"] : "",
                fechaInicial = fechaInicial,
                fechaFinal = fechaFinal,
                idcTipoContrato = short.Parse(datos["idcTipoContrato"]),
                tiempoContrato = datos["diasContrato"] != null && datos["diasContrato"] != "" ? short.Parse(datos["diasContrato"]) : (short)0,
                ruta = ""
            };

            if (ModelState.IsValid)
            {
                db.eEmpleadoContrato.Add(eEmpleadoContrato);
                db.SaveChanges();

                if (Request.Files.Count > 0)
                {
                    try
                    {
                        var mEmpleado = db.mEmpleado.Find(idmEmpleado);

                        string ruta = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).FullName + "Documentacion\\" + mEmpleado.curp;
                        if (!Directory.Exists(ruta))
                        {
                            Directory.CreateDirectory(ruta);
                        }

                        HttpPostedFileBase fileContrato = Request.Files["fileContrato"];
                        if (fileContrato.ContentLength > 0)
                        {
                            string nombre = mEmpleado.curp + "_CONTRATO_" + eEmpleadoContrato.ideEmpleadoContrato;
                            var argName = fileContrato.FileName.Split('.');
                            string extension = argName[argName.Length - 1];
                            new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre)) System.IO.File.Delete(f); });
                            fileContrato.SaveAs(ruta + "\\" + nombre + "." + extension);

                            eEmpleadoContrato.ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + extension;
                            db.Entry(eEmpleadoContrato).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    catch { }
                }
            }

            return Json(db.eEmpleadoContrato.Where(_ => _.idmEmpleado == idmEmpleado).ToList().Select(_ => new
            {
                no = _.noContrato,
                contrato = _.cTipoContrato.contrato,
                fechaInicial = _.fechaInicial.ToString("yyyy-MM-dd"),
                fechaInicial_format = _.fechaInicial.ToString("dd/MM/yyyy"),
                fechaFinal = _.fechaFinal != null ? ((DateTime)_.fechaFinal).ToString("yyyy-MM-dd") : "",
                fechaFinal_format = _.fechaFinal != null ? ((DateTime)_.fechaFinal).ToString("dd/MM/yyyy") : "",
                ruta = _.ruta,
                idcTipoContrato = _.idcTipoContrato,
                ideEmpleadoContrato = _.ideEmpleadoContrato,
                descripcion = _.descripcion,
                proyecto = _.proyecto,
                tiempoContrato = _.tiempoContrato
            }));
        }

        [SessionExpire]
        [HttpPost]
        public JsonResult EditarContrato(FormCollection datos)
        {
            var idmEmpleado = int.Parse(datos["idmEmpleado"]);
            var ideContrato = short.Parse(datos["ideContrato"]);
            var idcTipoContrato = short.Parse(datos["idcTipoContrato"]);
            var fechaInicial = DateTime.Parse(datos["fechaInicial"]);

            var tiempoContrato = (short?)null;
            var fechaFinal = (DateTime?)null;
            if (!db.cTipoContrato.Find(idcTipoContrato).contrato.Equals("INDETERMINADO"))
            {
                fechaFinal = DateTime.Parse(datos["fechaFinal"]);
                tiempoContrato = (short)Math.Abs((fechaInicial.Month - ((DateTime)fechaFinal).Month) + 12 * (fechaInicial.Year - ((DateTime)fechaFinal).Year));
            }

            var eEmpleadoContrato = db.eEmpleadoContrato.Find(ideContrato);
            eEmpleadoContrato.idmEmpleado = idmEmpleado;
            eEmpleadoContrato.descripcion = datos["descripcion"] != null ? datos["descripcion"] : "";
            eEmpleadoContrato.fechaInicial = fechaInicial;
            eEmpleadoContrato.fechaFinal = fechaFinal;
            eEmpleadoContrato.idcTipoContrato = short.Parse(datos["idcTipoContrato"]);
            eEmpleadoContrato.tiempoContrato = datos["diasContrato"] != null && datos["diasContrato"] != "" ? short.Parse(datos["diasContrato"]) : (short)0;
            eEmpleadoContrato.proyecto = datos["proyecto"];

            if (Request.Files.Count > 0)
            {
                try
                {
                    var mEmpleado = db.mEmpleado.Find(idmEmpleado);
                    string ruta = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).FullName + "Documentacion\\" + mEmpleado.curp;
                    if (!Directory.Exists(ruta))
                    {
                        Directory.CreateDirectory(ruta);
                    }
                    HttpPostedFileBase fileContrato = Request.Files["fileContrato"];
                    if (fileContrato.ContentLength > 0)
                    {
                        string nombre = mEmpleado.curp + "_CONTRATO_" + eEmpleadoContrato.ideEmpleadoContrato;
                        var argName = fileContrato.FileName.Split('.');
                        string extension = argName[argName.Length - 1];
                        new List<string>(Directory.GetFiles(ruta)).ForEach(f => { if (f.ToUpper().Contains(nombre)) System.IO.File.Delete(f); });
                        fileContrato.SaveAs(ruta + "\\" + nombre + "." + extension);
                        eEmpleadoContrato.ruta = "Documentacion/" + mEmpleado.curp + "/" + nombre + "." + extension;
                    }
                }
                catch { }
            }

            db.Entry(eEmpleadoContrato).State = EntityState.Modified;
            db.SaveChanges();

            return Json(db.eEmpleadoContrato.Where(_ => _.idmEmpleado == idmEmpleado).ToList().Select(_ => new
            {
                no = _.noContrato,
                contrato = _.cTipoContrato.contrato,
                fechaInicial = _.fechaInicial.ToString("yyyy-MM-dd"),
                fechaInicial_format = _.fechaInicial.ToString("dd/MM/yyyy"),
                fechaFinal = _.fechaFinal != null ? ((DateTime)_.fechaFinal).ToString("yyyy-MM-dd") : "",
                fechaFinal_format = _.fechaFinal != null ? ((DateTime)_.fechaFinal).ToString("dd/MM/yyyy") : "",
                ruta = _.ruta,
                idcTipoContrato = _.idcTipoContrato,
                ideEmpleadoContrato = _.ideEmpleadoContrato,
                descripcion = _.descripcion,
                proyecto = _.proyecto,
                tiempoContrato = _.tiempoContrato
            }));
        }

        [SessionExpire]
        public ActionResult CreateContract(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.idcTipoContrato = new SelectList(db.cTipoContrato, dataValueField: "idcTipoContrato", dataTextField: "contrato");
            var fechaInicial = DateTime.Now;
            var noContrato = 1;
            var etc = db.eEmpleadoContrato.Where(_ => _.idmEmpleado == id).ToList();
            if (etc.Count > 0)
            {
                noContrato = etc.Count + 1;
                if (etc.LastOrDefault().fechaFinal != null) { fechaInicial = (DateTime)etc.LastOrDefault().fechaFinal; }
            }
            var t = db.mEmpleado.Find(id);
            ViewBag.nombre = t != null ? t.paterno + " " + t.materno + " " + t.nombre : "";
            var etx = new eEmpleadoContrato()
            {
                idmEmpleado = id,
                noContrato = noContrato.ToString(),
                fechaInicial = fechaInicial
            };
            return View(etx);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateContract([Bind(Include = "ideContrato,idmPlaza,noContrato,descripcion,fechaInicial,fechaFinal,idcTipoContrato,tiempoContrato,proyecto,ruta")] eEmpleadoContrato eEmpleadoContrato, FormCollection datos, HttpPostedFileBase file)
        {
            if (eEmpleadoContrato.fechaInicial != null)
            {
                if (!db.cTipoContrato.Find(eEmpleadoContrato.idcTipoContrato).contrato.Equals("INDETERMINADO"))
                {
                    eEmpleadoContrato.tiempoContrato = (short)Math.Abs((eEmpleadoContrato.fechaInicial.Month - ((DateTime)eEmpleadoContrato.fechaFinal).Month) + 12 * (eEmpleadoContrato.fechaInicial.Year - ((DateTime)eEmpleadoContrato.fechaFinal).Year));
                }
                else
                {
                    eEmpleadoContrato.tiempoContrato = null;
                    eEmpleadoContrato.fechaFinal = null;
                }
                db.eEmpleadoContrato.Add(eEmpleadoContrato);
                db.SaveChanges();
            }

            var mEmpleado = db.mEmpleado.Find(eEmpleadoContrato.idmEmpleado);
            if (file != null && mEmpleado != null)
            {
                string ruta = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).FullName + "Documentacion\\" + mEmpleado.curp;
                if (!System.IO.Directory.Exists(ruta))
                {
                    System.IO.Directory.CreateDirectory(ruta);
                }
                string nombre = mEmpleado.curp + "_CONTRATO_" + eEmpleadoContrato.ideEmpleadoContrato;
                var argName = file.FileName.Split('.');
                string extension = argName[argName.Length - 1];
                new List<string>(Directory.GetFiles(ruta)).ForEach(_ => { if (_.ToUpper().Contains(nombre)) System.IO.File.Delete(_); });
                file.SaveAs(ruta + "\\" + nombre + "." + extension);
            }

            //auditrail("CreateContract", "CREAR CONTRATO Empleado", "CREAR CONTRATO", "", "", "");

            return RedirectToAction("Contract", "Report");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult BuscarPlantilla(string palabra)
        {
            try
            {
                palabra = palabra.ToUpper();

                var data = db.mPlaza.Where(m => m.estatus == "VACANTE")
                           .Select(m => new { nombrePuesto = (m.cPuesto.puesto + " | AREA: " + m.cArea.area), m.idmPlaza }).OrderBy(x => x.nombrePuesto).ToList();
                if (palabra != "")
                {
                    data = db.mPlaza.Where(m => m.estatus == "VACANTE" && m.cPuesto.puesto.Contains(palabra))
                           .Select(m => new { nombrePuesto = (m.cPuesto.puesto + " | AREA: " + m.cArea.area), m.idmPlaza }).OrderBy(x => x.nombrePuesto).ToList();
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return View("Index");
            }
        }

        private string CreateUserName(string nombre, string paterno, string materno)
        {
            try
            {
                if (nombre != "" && paterno != null)
                {
                    var nombres = nombre.Split(' ');
                    var usuario = nombres[0].ToLower() + "." + paterno.Split(' ')[0].ToLower();
                    if (db.cUsuario.Where(_ => _.usuario == usuario).ToList().Count() == 0)
                    {
                        return usuario;
                    }
                    if (nombres.Length > 1)
                    {
                        usuario = nombres[1].ToLower() + "." + paterno.Split(' ')[0].ToLower();
                        if (db.cUsuario.Where(_ => _.usuario == usuario).ToList().Count() == 0)
                        {
                            return usuario;
                        }
                    }
                    usuario = nombres[0].ToLower() + "." + materno.Split(' ')[0].ToLower();
                    if (db.cUsuario.Where(_ => _.usuario == usuario).ToList().Count() == 0)
                    {
                        return usuario;
                    }
                    return usuario;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        private string CreatePassword(string nombre, string paterno)
        {
            try
            {
                if (nombre != "" && paterno != null)
                {
                    nombre = (nombre.Length > 3 ? nombre.Substring(0, 3) : nombre).ToLower();
                    nombre = char.ToUpper(nombre[0]) + nombre.Substring(1);
                    paterno = (paterno.Length > 3 ? paterno.Substring(0, 3) : paterno).ToLower();
                    paterno = char.ToUpper(paterno[0]) + paterno.Substring(1);
                    var numero = new Random().Next(1234, 9876);
                    return Utilities.Encriptar(paterno + "." + nombre + numero);
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        public JsonResult municipios(int id)
        {
            var municipios = new List<SelectListItem>() { new SelectListItem { Text = "", Value = "" } };
            foreach (var m in db.cMunicipio.Where(_ => _.idcEntidadFederativa == id))
            {
                municipios.Add(new SelectListItem { Text = m.municipio, Value = m.idcMunicipio.ToString() });
            }
            return Json(new SelectList(municipios, dataValueField: "Value", dataTextField: "Text"));
        }

        public JsonResult entidadFederativa(string claveCurp)
        {
            var entidadFederativa = "";
            var ef = db.cEntidadFederativa.FirstOrDefault(_ => _.claveCurp == claveCurp);
            if (ef != null)
            {
                entidadFederativa = ef.entidadFederativa;
            }
            return Json(entidadFederativa, JsonRequestBehavior.AllowGet);
        }

        public JsonResult beneficiosPorPlantilla(int idmPlaza)
        {
            var mPlaza = db.mPlaza.Find(idmPlaza);

            var idmPlazaJefeInmediato = db.mPlaza.Where(_ => (_.estatus == "VACANTE" || _.estatus == "AUTORIZADA")).OrderBy(_ => _.cPuesto.puesto)
                    .Select(_ => new { _.idmPlaza, puesto = _.cPuesto.puesto + (_.mEmpleado != null ? " - (" + _.mEmpleado.nombre + " " + _.mEmpleado.paterno + " " + _.mEmpleado.materno + ")" : "") }).ToList();

            var cbeneficio = db.cBeneficio.Select(_ => new { _.idcBeneficio, _.tipo, }).ToList();

            var ebeneficio = mPlaza != null ? db.ePlazaBeneficio.Where(_ => _.idmPlaza == mPlaza.idmPlaza).Select(_ => new eBen { idcBeneficio = _.idcBeneficio, idePlazaBeneficio = _.idePlazaBeneficio, idmPlaza = _.idmPlaza, tipo = _.tipo, valor = _.valor }).ToList() : new List<eBen>();

            return Json(new {idmPlazaJefeInmediato, cbeneficio, ebeneficio }, JsonRequestBehavior.AllowGet);
        }

        private class eBen
        {
            public int idcBeneficio { get; set; }
            public int idePlazaBeneficio { get; set; }
            public int idmPlaza { get; set; }
            public string tipo { get; set; }
            public string valor { get; set; }
        }
        
        public JsonResult validarCURP(string curp)
        {
            return Json((db.mEmpleado.Where(_ => _.curp == curp).Count() > 0) ? 1 : 0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarRFC(string rfc)
        {
            return Json((db.mEmpleado.Where(_ => _.rfc == rfc).Count() > 0) ? 1 : 0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarNSS(string nss)
        {
            return Json((db.mEmpleado.Where(_ => _.nss == nss).Count() > 0) ? 1 : 0, JsonRequestBehavior.AllowGet);
        }

    }
}