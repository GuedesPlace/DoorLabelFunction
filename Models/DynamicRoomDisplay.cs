using Newtonsoft.Json;

namespace GuedesPlace.DoorLabel.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public class DynamicsRoomDisplay
{
    [JsonProperty(PropertyName = "gp_name")]
    public string Name { set; get; }
    [JsonProperty(PropertyName = "gp_roomdisplay_SystemUser_SystemUser")]
    public List<DynamicsUsers> Users { set; get; }
    [JsonProperty(PropertyName ="gp_configuration")]
    public DynamicsDisplayConfiguration Configuration {set;get;}

}

public class DynamicsUsers
{
    [JsonProperty(PropertyName = "fullname")]
    public string Name { get; set; }
    [JsonProperty(PropertyName = "title")]
    public string Title { get; set; }
}
public class DynamicsDisplayConfiguration
{
    //BASIC
    public int gp_start_employee_block { set; get; }
    public int gp_start_header_block { set; get; }
    public int gp_margin_right { set; get; }
    public int gp_margin_left { set; get; }

    //HEADER
    public int gp_font_style_header { set; get; }
    public int gp_font_size_header { set; get; }
    public string gp_font_family_header { set; get; }
    public int gp_alignment_header { set; get; }
    public int gp_font_weight_header { set; get; }

    //NAME
    public int gp_font_size_name { set; get; }
    public string gp_font_family_name { set; get; }
    public int gp_font_weight_name { set; get; }
    public int gp_font_style_name { set; get; }
    public int gp_alignment_name { set; get; }

    //TITLE
    public int gp_font_size_title { set; get; }
    public int gp_alignment_title { set; get; }
    public string gp_font_family_title { set; get; }
    public int gp_font_weight_title { set; get; }
    public int gp_font_style_title { set; get; }

    //LEAVE
    public int gp_font_size_leave { set; get; }
    public int gp_font_weight_leave { set; get; }
    public int gp_alignment_leave { set; get; }
    public string gp_font_family_leave { set; get; }
    public int gp_font_style_leave { set; get; }

    //PICTURE
    public int? gp_picture_position_x { set; get; }
    public int? gp_picture_position_y { set; get; }
    public bool gp_has_picture { set; get; }
    public string? gp_picture_url { set; get; }
    public string gp_displayconfigurationid {set;get;}
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
