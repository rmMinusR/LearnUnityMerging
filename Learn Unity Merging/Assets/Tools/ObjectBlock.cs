struct ObjectBlock
{
    public string header;
    public string id;
    public string[] content;

    public ObjectBlock(string header, string[] content)
    {
        this.header = header;
        id = header.Split("&")[1];
        this.content = content;
    }
}