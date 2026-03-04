using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Eplicta.Conserver.ArchiveReport
{
    public class Renderer
    {
        public static Stream GetStream(object mets)
        {
            throw new NotImplementedException();
        }

        public static byte[] GetBytes(MetsData mets)
        {
            var exampel = "haha claud hjälper inte";
            byte[] bytes = Encoding.UTF8.GetBytes(exampel);
            return bytes;

            throw new NotImplementedException();
        }
    }
}
