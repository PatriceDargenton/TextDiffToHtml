
namespace TextDiffToHtml
{
    internal class HtmlRenderer
    {
        public delegate void PartialRenderDelegate(string htmlFragment);

        public required PartialRenderDelegate OnPartialRender;
        
        public bool cancel = false;
        public int line = 0;
        public int lines = 0;
        public float progress = 0;

        public void Init()
        {
            this.cancel = false;
            this.line = 0;
            this.lines = 0;
            this.progress = 0;
        }
    }
}
