using System.Text;

namespace Eplicta.Conserver.ArchiveReport
{
    // Hanterar konvertering av MetsData till olika format.
    public class Renderer
    {
        // Konverterar MetsData till en byte-array.
        public static byte[] GetBytes(MetsData mets)
        {
            var exampel = "haha claud hjälper inte";

            // Konverterar text till UTF8-bytes
            byte[] bytes = Encoding.UTF8.GetBytes(exampel);

            return bytes;
        }
    }
}