namespace GuedesPlace.DoorLabel.Models;

public class RoomLabel{
    public string Name { get; set; }
    public List<RoomLabelElement> Elements{ get; set; }
    public DynamicsDisplayConfiguration Configuration { get; set; }
    public byte[]? picture {set;get;}
}