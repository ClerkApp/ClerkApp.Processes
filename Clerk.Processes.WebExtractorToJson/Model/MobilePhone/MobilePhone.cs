using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Clerk.Processes.WebExtractorToJson.Model.MobilePhone
{
    public partial class MobilePhone
    {
        [JsonProperty("store")]
        public Store Store { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("colors")]
        public List<string> Colors { get; set; }

        [JsonProperty("network")]
        public Network Network { get; set; }

        [JsonProperty("launch")]
        public Launch Launch { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonProperty("display")]
        public Display Display { get; set; }

        [JsonProperty("platform")]
        public Platform Platform { get; set; }

        [JsonProperty("memory")]
        public Memory Memory { get; set; }

        [JsonProperty("camera")]
        public Camera Camera { get; set; }

        [JsonProperty("sound")]
        public Sound Sound { get; set; }

        [JsonProperty("comms")]
        public Comms Comms { get; set; }

        [JsonProperty("features")]
        public Features Features { get; set; }

        [JsonProperty("battery")]
        public Battery Battery { get; set; }
    }

    public partial class Battery
    {
        [JsonProperty("info")]
        public string Info { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("charging")]
        public List<string> Charging { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("dimensions")]
        public string Dimensions { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("build")]
        public List<string> Build { get; set; }

        [JsonProperty("sim")]
        public string Sim { get; set; }
    }

    public partial class Camera
    {
        [JsonProperty("selfie")]
        public Main Selfie { get; set; }

        [JsonProperty("main")]
        public Main Main { get; set; }
    }

    public partial class Main
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("spec")]
        public List<string> Spec { get; set; }

        [JsonProperty("feature")]
        public string Feature { get; set; }

        [JsonProperty("video")]
        public List<string> Video { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        [JsonProperty("grandsId")]
        public List<string> GrandsId { get; set; }
    }

    public partial class Comms
    {
        [JsonProperty("wlan")]
        public string Wlan { get; set; }

        [JsonProperty("bluetooth")]
        public string Bluetooth { get; set; }

        [JsonProperty("usb")]
        public string Usb { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }

        [JsonProperty("gps")]
        public bool Gps { get; set; }

        [JsonProperty("infrared")]
        public bool Infrared { get; set; }

        [JsonProperty("radio")]
        public bool Radio { get; set; }

        [JsonProperty("nfc")]
        public bool Nfc { get; set; }

        [JsonProperty("notch")]
        public bool Notch { get; set; }

        [JsonProperty("under")]
        public bool Under { get; set; }

        [JsonProperty("cardSlot")]
        public bool CardSlot { get; set; }

        [JsonProperty("loudspeaker")]
        public bool Loudspeaker { get; set; }

        [JsonProperty("jack")]
        public bool Jack { get; set; }

        [JsonProperty("typeC")]
        public bool TypeC { get; set; }

        [JsonProperty("gprs")]
        public bool Gprs { get; set; }

        [JsonProperty("fingerprint")]
        public bool Fingerprint { get; set; }

        [JsonProperty("water")]
        public bool Water { get; set; }
    }

    public partial class Display
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("size")]
        public Size Size { get; set; }

        [JsonProperty("resolution")]
        public Resolution Resolution { get; set; }
    }

    public partial class Resolution
    {
        [JsonProperty("ratio")]
        public string Ratio { get; set; }

        [JsonProperty("ppi")]
        public int Ppi { get; set; }

        [JsonProperty("pixels")]
        public string Pixels { get; set; }
    }

    public partial class Size
    {
        [JsonProperty("inches")]
        public string Inches { get; set; }

        [JsonProperty("cm")]
        public string Cm { get; set; }

        [JsonProperty("screenBodyRatio")]
        public string ScreenBodyRatio { get; set; }
    }

    public partial class Features
    {
        [JsonProperty("sensors")]
        public List<string> Sensors { get; set; }
    }

    public partial class Images
    {
        [JsonProperty("front")]
        public string Front { get; set; }

        [JsonProperty("back")]
        public string Back { get; set; }

        [JsonProperty("others")]
        public List<string> Others { get; set; }
    }

    public partial class Launch
    {
        [JsonProperty("announced")]
        public string Announced { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class Memory
    {
        [JsonProperty("internal")]
        public List<Internal> Internal { get; set; }
    }

    public partial class Internal
    {
        [JsonProperty("memory")]
        public string Memory { get; set; }

        [JsonProperty("ram")]
        public string Ram { get; set; }
    }

    public partial class Name
    {
        [JsonProperty("main")]
        public string Main { get; set; }

        [JsonProperty("others")]
        public List<string> Others { get; set; }
    }

    public partial class Network
    {
        [JsonProperty("bands")]
        public Bands Bands { get; set; }

        [JsonProperty("speed")]
        public string Speed { get; set; }
    }

    public partial class Bands
    {
        [JsonProperty("2g")]
        public string B2G { get; set; }

        [JsonProperty("3g")]
        public string B3G { get; set; }

        [JsonProperty("4g")]
        public string B4G { get; set; }
    }

    public partial class Platform
    {
        [JsonProperty("os")]
        public Os Os { get; set; }

        [JsonProperty("chipset")]
        public Chipset Chipset { get; set; }

        [JsonProperty("cpu")]
        public Cpu Cpu { get; set; }

        [JsonProperty("gpu")]
        public List<string> Gpu { get; set; }
    }

    public partial class Chipset
    {
        [JsonProperty("series")]
        public List<string> Series { get; set; }

        [JsonProperty("codeName")]
        public List<string> CodeName { get; set; }

        [JsonProperty("version")]
        public List<string> Version { get; set; }

        [JsonProperty("technology")]
        public List<string> Technology { get; set; }
    }

    public partial class Cpu
    {
        [JsonProperty("numberOfCore")]
        public List<int> NumberOfCore { get; set; }

        [JsonProperty("name")]
        public List<string> Name { get; set; }
    }

    public partial class Os
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("release")]
        public Release Release { get; set; }

        [JsonProperty("upgradable")]
        public string Upgradable { get; set; }
    }

    public partial class Release
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Sound
    {
        [JsonProperty("others")]
        public string Others { get; set; }
    }

    public partial class Store
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("stock")]
        public int Stock { get; set; }

        [JsonProperty("price")]
        public Price Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("combinations")]
        public Combinations Combinations { get; set; }

        [JsonProperty("reviews")]
        public List<Review> Reviews { get; set; }
    }

    public partial class Combinations
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("products")]
        public List<string> Products { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("special")]
        public Status Special { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Visibility { get; set; }
    }

    public partial class Review
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("billId")]
        public string BillId { get; set; }

        [JsonProperty("postDate")]
        public DateTime PostDate { get; set; }

        [JsonProperty("editDate")]
        public DateTime EditDate { get; set; }

        [JsonProperty("stars")]
        public int Stars { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
