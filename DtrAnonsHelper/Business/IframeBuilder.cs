namespace DtrAnonsHelper.Business;

public class IframeBuilder
{
    public string Execute(string sourceLink)
    {
        var (oid, id) = sourceLink.Length > 50 ? LongLinkProcessor(sourceLink) : ShortLinkProcessor(sourceLink);

        string builtIframe = $"https://vk.com/video_ext.php?oid={oid}&id={id}&hd=2";

        return builtIframe;
    }

    private (string, string) LongLinkProcessor(string sourceLink)
    {
        string oid = sourceLink.Split("=")[1].Split("_")[0].Substring(5);
        string id = sourceLink.Split("=")[1].Split("_")[1].Split("%")[0];

        return (oid, id);
    }

    private (string, string) ShortLinkProcessor(string sourceLink)
    {
        var numbers = sourceLink.Split("-")[1].Split("_");
        var oid = "-" +numbers[0];
        var id = numbers[1];

        return (oid, id);
    }
}