
public class RequestPasteFromClipboard
{
    public FileInfoNodeDto FileInfoNode { get; set; }
    public string FileName { get; set; }
}

public class FileInfoNodeDto
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string FullPath { get; set; }
    public string RelativePath { get; set; }
    public string Type { get; set; }
    public int Level { get; set; }
    public bool Expandable { get; set; } = true;
    // DataText è opzionale per questa operazione
    public string DataText { get; set; } = "";
}