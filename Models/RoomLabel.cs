namespace GuedesPlace.DoorLabel.Models;

public class RoomLabel{
    public required string Name { get; set; }
    public required List<RoomLabelElement> Elements{ get; set; }
    public required DynamicsDisplayConfiguration Configuration { get; set; }
    public required string SpecialSortOrder {get;set;} 
    public byte[]? Picture {set;get;}
}