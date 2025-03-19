namespace GuedesPlace.DoorLabel.Models;
public class RoomLabelElement
{
    public required string Name { get; set; }
    public required string Title { get; set; }
    public required string EMail { set; get; }
    public DateTime? OutOfOfficeUntil { get; set; }
}