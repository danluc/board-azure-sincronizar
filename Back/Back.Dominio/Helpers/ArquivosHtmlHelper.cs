using System.IO;
using System.Reflection;

namespace Back.Dominio.Helpers
{
    public class ArquivosHtmlHelper
    {
        public static string EmailSincronizar => RecuperarConteudoHtml("EmailSincronizar.html", "Recursos");

        private static string RecuperarConteudoHtml(string arquivo, string pasta)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var diretorio = assembly.Location.Replace(assembly.ManifestModule.Name, "");
            var pathArquivo = Path.Combine(diretorio, pasta, arquivo);

            var fi = new FileInfo(pathArquivo);
            using (TextReader reader = new StreamReader(fi.OpenRead()))
                return reader.ReadToEnd();
        }
    }
}
