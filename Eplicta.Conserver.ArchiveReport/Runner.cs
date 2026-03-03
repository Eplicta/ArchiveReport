namespace Eplicta.Conserver.ArchiveReport
{
    public class Runner
    {
        public static Stream Run(object mets)
        {
            return Renderer.GetStream(mets);
        }

        public static byte[] GetStream(object mets)
        {
            return Renderer.GetBytes(mets);
        }
    }
}
