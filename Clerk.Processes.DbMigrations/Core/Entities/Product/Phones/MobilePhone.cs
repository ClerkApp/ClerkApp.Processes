using System.Collections.Generic;
using Clerk.Processes.DbMigrations.Core.Enums;

namespace Clerk.Processes.DbMigrations.Core.Entities.Product.Phones
{
    public class MobilePhone : Product
    {
        public ProductPicture Picture { get; set; }
        
        public Platform Platform { get; set; }

        public Memory Memory { get; set; }

        public Camera FrontCamera { get; set; }

        public Camera BackCamera { get; set; }

        public Sound Sound { get; set; }

        public Battery Battery { get; set; }

        public Comms Comms { get; set; }

        public Feature Features { get; set; }
    }

    public class Platform
    {
        public Os Os { get; set; }

        public Chipset Chipset { get; set; }

        public Cpu Cpu { get; set; }

        public string Gpu { get; set; }
    }

    public class Os
    {
        public OsType Type { get; set; }

        public List<string> NameList { get; set; }

        public decimal Version { get; set; }

        public string Upgradable { get; set; }
    }

    public class Chipset
    {
        public string Series { get; set; }

        public string CodeName { get; set; }

        public int Version { get; set; }

        public string Technology { get; set; }
    }

    public class Cpu
    {
        public int Number { get; set; }

        public string Name { get; set; }
    }

    public class Memory
    {
        public InternalMemory Internal { get; set; }
        
        public ExternalMemory External { get; set; }

        public int SimSlot { get; set; }
    }

    public class InternalMemory
    {
        public int Device { get; set; }

        public int Ram { get; set; }
    }

    public class ExternalMemory
    {
        public string Type { get; set; }

        public int Limit { get; set; }

        public bool Exist { get; set; }

        public bool DedicatedSlot { get; set; }
    }

    public class Camera
    {
        public int Number { get; set; }

        public CameraPosition Position { get; set; }

        public List<string> Specs { get; set; }

        public List<string> Video { get; set; }

        public List<string> Features { get; set; }

    }

    public class Sound
    {
        public bool Loudspeaker { get; set; }

        public bool Jack { get; set; }

        public List<string> Feature { get; set; }
    }

    public class Battery
    {
        public bool Removable { get; set; }

        public bool FastCharging { get; set; }

        public int Capacity { get; set; }

        public List<string> Feature { get; set; }
    }

    public class Comms
    {
        public bool Nfc { get; set; }

        public bool Gps { get; set; }

        public bool Radio { get; set; }

        public bool Waterproof { get; set; }

        public bool TypeC { get; set; }

        public bool Infrared { get; set; }

        public bool Bluetooth { get; set; }

        public List<string> Feature { get; set; }
    }

    public class Feature
    {
        public bool Fingerprint { get; set; }

        public bool EyeScanner { get; set; }

        public bool FaceRecognition { get; set; }

        public List<string> Sensors { get; set; }

        public List<string> Features { get; set; }
    }
}
