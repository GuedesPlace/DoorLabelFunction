using Newtonsoft.Json;

namespace GuedesPlace.DoorLabel.Models;

public class DynamicsRoomDisplay
{
    [JsonProperty(PropertyName = "gp_name")]
    public string Name { set; get; }
     [JsonProperty(PropertyName = "gp_roomdisplay_SystemUser_SystemUser")]
    public List<DynamicsUsers> Users { set; get; }

}

public class DynamicsUsers {
     [JsonProperty(PropertyName = "fullname")]
    public string Name { get; set; }
     [JsonProperty(PropertyName = "title")]
    public string Title {get;set;}
}