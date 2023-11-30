namespace DtrAnonsHelper.Business;

public class IframeBuilder
{
    public string Execute(string sourceLink)
    {
        string oid = sourceLink.Split("=")[1].Split("_")[0].Substring(5);
        string id = sourceLink.Split("=")[1].Split("_")[1].Split("%")[0];

        string builtIframe = $"<iframe src=\"https://vk.com/video_ext.php?oid={oid}&id={id}&hd=2\" " +
                             "width=\"853\" " +
                             "height=\"480\" " +
                             "allow=\"autoplay; " +
                             "encrypted-media; " +
                             "fullscreen; " +
                             "picture-in-picture;\" " +
                             "frameborder=\"0\" " +
                             "allowfullscreen></iframe>";

        return builtIframe;
    }
}