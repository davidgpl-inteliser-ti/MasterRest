using System;
using System.Security.Cryptography;
using System.Text;

namespace MasterRest.Helpers
{
    public class Utilities
    {
        private static string claveEncriptacion = "r1k3$@.";

        public static String Encriptar(String cadena)
        {
            if (cadena != null && cadena != "")
            {
                try
                {
                    byte[] arCadena = UTF8Encoding.UTF8.GetBytes(cadena);
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                    byte[] arClave = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(claveEncriptacion));
                    md5.Clear();
                    tdes.Key = arClave;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cadTran = tdes.CreateEncryptor();
                    byte[] resultado = cadTran.TransformFinalBlock(arCadena, 0, arCadena.Length);
                    tdes.Clear();
                    return Convert.ToBase64String(resultado, 0, resultado.Length);
                }
                catch { }
            }
            return "";
        }

        public static String Desencriptar(String cadena)
        {
            if (cadena != null && cadena != "")
            {
                try
                {
                    byte[] arCadena = Convert.FromBase64String(cadena);
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                    byte[] arClave = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(claveEncriptacion));
                    md5.Clear();
                    tdes.Key = arClave;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cadTran = tdes.CreateDecryptor();
                    byte[] resultado = cadTran.TransformFinalBlock(arCadena, 0, arCadena.Length);
                    tdes.Clear();
                    return UTF8Encoding.UTF8.GetString(resultado);
                }
                catch { }
            }
            return "";
        }

        public static string Antiguedad(DateTime fechaIncial, DateTime fechaActual)
        {
            int Años = 0;
            int Meses = 0;
            int Dias = 0;
            DateTime FechaCalculo = fechaIncial;
            if (fechaIncial > DateTime.Today)
            {
                return "Aún no entra a laborar";
            }
            if (fechaActual.Date == fechaIncial.Date)
            {
                return "Hoy";
            }
            while (FechaCalculo.AddYears(1) <= DateTime.Today)
            {
                Años = Años + 1;
                FechaCalculo = FechaCalculo.AddYears(1);
            }
            while (FechaCalculo.AddMonths(1) <= DateTime.Today)
            {
                Meses = Meses + 1;
                FechaCalculo = FechaCalculo.AddMonths(1);
            }
            while (FechaCalculo.AddDays(1) <= DateTime.Today)
            {
                Dias = Dias + 1;
                FechaCalculo = FechaCalculo.AddDays(1);
            }
            return $"{(Años > 0 ? $"{Años}" + (Años == 1 ? " año" : " años") : "")} " +
                   $"{(Meses > 0 ? $"{Meses}" + (Meses == 1 ? " mes" : " meses") : "")} " +
                   $"{(Dias > 0 ? $"{Dias}" + (Dias == 1 ? " día" : " días") : "")}";
        }
    }
}