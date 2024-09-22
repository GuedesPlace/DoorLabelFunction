namespace GuedesPlace.DoorLabel.Models;

public class PictureCreationResult
{
    public BinaryData Data { get; set; }
    public List<int> GreyScale { get; set; }
}

public class PictureGreyScaleStorage
{
    public List<int> GreyScale { get; set; }
    public string PictureHash { get; set; }
    public string GreyScaleBase64 { get; set; }
}
