using Clerk.Processes.WebExtractorToJson.Model.GSMArena;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Clerk.Processes.WebExtractorToJson.Model.MobilePhone
{
    public class PhoneElastic
    {
        public PhoneElastic(GsmArenaModel gsmModel)
        {
            Image = new WebClient().DownloadData(gsmModel.Photo);
            Status = new Status
            {
                Announced = gsmModel.Launch.Announced.FirstOrDefault(),
                Launch = gsmModel.Launch.Status.FirstOrDefault()
            };
            Name = new Name
            {
                Brand = gsmModel.Name.Main.First().Split(' ').FirstOrDefault(),
                Main = gsmModel.Name.Main.First().Split('(').FirstOrDefault()
            };
            Network = new Network
            {
                Bands = EnrichNetworkBands(gsmModel.Network),
                Speed = gsmModel.Network.Speed is null ? new List<string>() : gsmModel.Network.Speed.First().Split(",").ToListTrim(),
                Technology = gsmModel.Network.Technology is null ? new List<string>() : gsmModel.Network.Technology.First().Split("/").ToListTrim(),
            };
            Body = EnrichBody(gsmModel);
            Platform = EnrichPlatform(gsmModel);
            Memory = EnrichMemory(gsmModel.Memory);
            Display = EnrichDisplay(gsmModel.Display);
            Camera = EnrichCamera(gsmModel);
            Sound = new Sound
            {
                Jack = "Yes".Equals(gsmModel.Sound.The35MmJack.FirstOrDefault()),
                Loudspeaker = gsmModel.Sound.Loudspeaker.Contains("Yes"),
                Stereo = gsmModel.Sound.Loudspeaker.Contains("stereo")
            };
            Comms = EnrichComms(gsmModel);
            Battery = EnrichBattery(gsmModel.Battery);
            Price = EnrichPrice(gsmModel);
            Colors = gsmModel.Misc.Colors.First().Split(',').ToListTrim();
            Features = EnrichFeatures(gsmModel);
        }

        private Comms EnrichComms(GsmArenaModel gsmModel)
        {
            double.TryParse(gsmModel.Comms.Usb.First().Split(',').First(), out var usbResult);
            double.TryParse(Regex.Match(gsmModel.Comms.Bluetooth.First().Split(',').First(), @"\d+(\.?)\d*").Value, out var blueResult);

            return new Comms
            {
                Bluetooth = double.Parse(blueResult.ToString(CultureInfo.InvariantCulture)),
                GPS = gsmModel.Comms.Gps is null
                    ? new List<string>()
                    : gsmModel.Comms.Gps.First().Split(',').Skip(1).ToList().Select(x => x.Split(' ').Last()).ToList(),
                Usb = new KeyValuePair<string, double>(gsmModel.Comms.Usb.First().Split(',').Last(), double.Parse(usbResult.ToString(CultureInfo.InvariantCulture))),
                Wifi = gsmModel.Comms.Wlan is null
                    ? new List<string>()
                    : gsmModel.Comms.Wlan.First().Split(',').First().Split('/').ToListTrim()
            };
        }

        private static Features EnrichFeatures(GsmArenaModel gsmModel)
        {
            KeyValuePair<bool, string> finger = default;
            var gps = false;
            var nfc = false;
            var radio = false;
            var gyro = false;
            var jack = false;
            var waterResistant = false;
            var dustResistant = false;
            var fastCharging = false;


            if (gsmModel.Features?.Sensors != null)
            {
                foreach (var sensor in gsmModel.Features.Sensors.First().Split(','))
                {
                    var hasFinger = sensor.Contains("Fingerprint", StringComparison.CurrentCultureIgnoreCase);
                    finger = new KeyValuePair<bool, string>(hasFinger, hasFinger ? sensor : "");
                }
            }

            if (gsmModel.Comms?.Gps != null)
            {
                gps = !gsmModel.Comms.Gps.First().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Comms?.Nfc != null)
            {
                nfc = !gsmModel.Comms.Nfc.First().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Comms?.Radio != null)
            {
                nfc = !gsmModel.Comms.Radio.First().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Sound?.The35MmJack != null)
            {
                jack = !gsmModel.Sound.The35MmJack.First().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.MainCamera?.Video != null)
            {
                gyro = gsmModel.MainCamera.Video.First().Contains("gyro", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Battery?.Charging != null)
            {
                foreach (var charging in gsmModel.Battery.Charging.ToList())
                {
                    if (charging.Contains("Fast", StringComparison.CurrentCultureIgnoreCase))
                    {
                        fastCharging = true;
                    }
                }
            }
            if (gsmModel.Body.Orphans != null)
            {
                foreach (var orphans in gsmModel.Body.Orphans)
                {
                    if (orphans.Contains("water"))
                    {
                        waterResistant = true;
                    }
                    if (orphans.Contains("dust"))
                    {
                        dustResistant = true;
                    }
                }
            }

            return new Features
            {
                Fingerprint = finger,
                Gps = gps,
                FastCharging = fastCharging,
                Jack = jack,
                Nfc = nfc,
                Radio = radio,
                Gyro = gyro,
                WaterResistant = waterResistant,
                DustResistant = dustResistant,
            };
        }

        public byte[] Image { get; set; }
        public Status Status { get; set; }
        public Name Name { get; set; }
        public Network Network { get; set; }
        public Body Body { get; set; }
        public Platform Platform { get; set; }
        public Memory Memory { get; set; }
        public Display Display { get; set; }
        public Camera Camera { get; set; }
        public Sound Sound { get; set; }
        public Comms Comms { get; set; }
        public Battery Battery { get; set; }
        public Features Features { get; set; }
        public Dictionary<string, double> Price { get; set; }
        public List<string> Colors { get; set; }

        private static Dictionary<string, double> EnrichPrice(GsmArenaModel gsmModel)
        {
            var prices = new Dictionary<string, double>();

            if (gsmModel.Misc.Price != null)
            {
                var pricesList = gsmModel.Misc.Price.First().Split('/').ToListTrim();

                if (pricesList != null)
                {
                    foreach (var price in pricesList)
                    {
                        if (price.Contains("About"))
                        {
                            prices.Add("EUR", double.Parse(Regex.Match(price.RemoveWhitespace(), @"(\d+[.,]?\d*)(?=(EUR))").Value));
                        }
                        else
                        {
                            prices.Add(Regex.Match(price.RemoveWhitespace(), @"[\W](?=[1-9])").Value, double.Parse(Regex.Match(price, @"(\d+[.,]?\d*)").Value));
                        }
                    }
                }
            }

            return prices;
        }

        private static Battery EnrichBattery(BatteryJson gsmModel)
        {
            if (gsmModel.Charging != null)
            {
                var batteryCharger = gsmModel.Charging.Where(x => x.Contains("Fast charging", StringComparison.CurrentCultureIgnoreCase)).ToList();
                var wirelessCharger = gsmModel.Charging.Where(x => x.Contains("wireless charging", StringComparison.CurrentCultureIgnoreCase));

                int.TryParse(Regex.Match(gsmModel.Orphans.FirstOrDefault().RemoveWhitespace(), @"(?<!(mAh))\d+").Value,
                    out var capacityResult);
                int.TryParse(Regex.Match(batteryCharger.FirstOrDefault().RemoveWhitespace(), @"(?<!(W))\d+").Value,
                    out var powerResult);

                return new Battery
                {
                    Capacity = int.Parse(capacityResult.ToString()),
                    Fast = batteryCharger.Any(),
                    Power = int.Parse(powerResult.ToString()),
                    Wireless = wirelessCharger.Any()
                };
            }

            return new Battery
            {
                Capacity = int.Parse(Regex.Match(gsmModel.Orphans.FirstOrDefault().RemoveWhitespace(), @"(?<!(mAh))\d+").Value),
                Fast = false,
                Power = 0,
                Wireless = false
            };
        }

        private static Platform EnrichPlatform(GsmArenaModel gsmModel)
        {
            var platform = new Platform
            {
                Chipset = new List<Chipset>(),
                CPU = new List<CPU>(),
                GPU = new List<GPU>(),
                Os = new Dictionary<string, int>(),
                Tests = new List<Performance>()
            };

            foreach (var chipset in gsmModel.Platform.Chipset.ToListTrim())
            {
                double result = 0;
                if (chipset.Contains("nm"))
                {
                    double.TryParse(chipset.Split('(')[1].Split("nm+").FirstOrDefault(), out result);
                }
                platform.Chipset.Add(new Chipset
                {
                    Generation = chipset.Split('(').First().Split(' ').Last(),
                    Name = chipset.Split('(').FirstOrDefault(),
                    Size = result,
                    Type = chipset.Split('-').Last()
                });
            }

            foreach (var gpu in gsmModel.Platform.Gpu.ToListTrim())
            {
                platform.GPU.Add(new GPU()
                {
                    Generation = gpu.Split('-').First().Split(' ').Last(),
                    Name = gpu.Split('-').FirstOrDefault(),
                    Type = gpu.Split('-').Last()
                });
            }

            foreach (var cpu in gsmModel.Platform.Cpu.ToListTrim())
            {
                Enum.TryParse(string.Join("", cpu.ToLower().Split(' ')[0].Split('-')), out Cores result);

                var cpuTypes = new List<CpuType>();

                if (cpu.Contains("("))
                {
                    foreach (var cpuType in Regex.Match(cpu, @"(?<=\()(.*)(?=\))").Value.Split('&'))
                    {
                        cpuTypes.Add(new CpuType
                        {
                            Count = int.Parse(cpuType.Split('x').First()),
                            Ghz = double.Parse(cpuType.Split('x')[1].Split(' ').First()),
                            Name = cpuType.Split("Hz").Last()
                        });
                    }
                }
                else
                {
                    cpuTypes.Add(new CpuType
                    {
                        Count = 1,
                        Ghz = double.Parse(cpu.Split(' ')[1]),
                        Name = "unknown"
                    });
                }

                platform.CPU.Add(new CPU()
                {
                    Cores = (int)result,
                    CpuList = cpuTypes,
                    Type = cpu.Split('-').Last()
                });
            }

            foreach (var os in gsmModel.Platform.Os.First().Split(','))
            {
                int.TryParse(Regex.Match(os, @"\d+").Value, out var osResult);
                platform.Os.Add(string.Join(' ', os.Split(' ').SkipLast(1)), int.Parse(osResult.ToString()));
            }


            if (gsmModel.Tests != null)
            {
                foreach (var perf in gsmModel.Tests.Performance.ToListTrim())
                {
                    double.TryParse(perf.Split(":").ToListTrim()[1].Split(" ").First().Replace("fps", ""),
                        out var scoreResult);
                    platform.Tests.Add(new Performance
                    {
                        Score = double.Parse(scoreResult.ToString(CultureInfo.InvariantCulture)),
                        Type = perf.Split(':').FirstOrDefault()?.Replace("\n", "")
                    });
                }
            }

            return platform;
        }

        public enum Cores
        {
            singlecore = 1,
            dualcore = 2,
            quadcore = 4,
            hexacore = 6,
            octacore = 8
        }

        private static Dictionary<string, List<string>> EnrichNetworkBands(NetworkJson gsmNetwork)
        {
            var networks = new Dictionary<string, List<string>>();

            foreach (var (key, value) in gsmNetwork.Bands)
            {
                networks.Add(key, value.FirstOrDefault().Split('/').FirstOrDefault().Split(",").ToListTrim());
            }

            return networks;
        }

        private static Body EnrichBody(GsmArenaModel gsmModel)
        {
            var water = gsmModel.Body.Orphans?.FirstOrDefault(x => x.Contains("water") && x.Contains("resistant"));
            
            KeyValuePair<int, string> waterResistent;
            if (!string.IsNullOrWhiteSpace(water))
            {
                int.TryParse(water.Split(" ").First().Split("IP").Last(), out var waterResult);
                waterResistent = new KeyValuePair<int, string>(int.Parse(waterResult.ToString()), water);
            }
            else
            {
                waterResistent = new KeyValuePair<int, string>();
            }

            var dimensions = new Dimensions
            {
                Cm = new Dictionary<string, double>(),
                In = new Dictionary<string, double>()
            };

            var dim = gsmModel.Body.Dimensions.FirstOrDefault().Split(' ').ToListTrim();
            var cm = 0;
            foreach (var x in dim)
            {
                double.TryParse(Regex.Match(x, @"\d+(\.?)\d*").Value, out var result);
                if (result > 0)
                {
                    switch (cm)
                    {
                        case 0:
                            dimensions.Cm.Add("height", result);
                            cm++;
                            break;
                        case 1:
                            dimensions.Cm.Add("weight", result);
                            cm++;
                            break;
                        case 2:
                            dimensions.Cm.Add("thickness", result);
                            cm++;
                            break;
                        case 3:
                            dimensions.In.Add("height", result);
                            cm++;
                            break;
                        case 4:
                            dimensions.In.Add("weight", result);
                            cm++;
                            break;
                        case 5:
                            dimensions.In.Add("thickness", result);
                            cm++;
                            break;
                    }
                }
            }

            var bodyWeight = gsmModel.Body.Weight.Contains("-") ? 0 :
                double.Parse(gsmModel.Body.Weight.First()?.Split('g').First().Split('k').First() ?? string.Empty);

            return new Body
            {
                Build = gsmModel.Body.Build is null ? new List<string>() : gsmModel.Body.Build.First().Split(',').ToListTrim(),
                Sim = gsmModel.Body.Sim.FirstOrDefault(),
                WaterResistant = waterResistent,
                Weight = bodyWeight,
                Dimensions = dimensions
            };
        }

        private Display EnrichDisplay(DisplayJson gsmModelDisplay)
        {
            int.TryParse(gsmModelDisplay.Protection?.FirstOrDefault()?.Split(' ').Last(), out var protectionResult);
            var protection = new KeyValuePair<string, int>(gsmModelDisplay.Protection?.FirstOrDefault(), protectionResult);

            List<string> refreshList = null;
            var refreshRates = new List<string>();


            if (gsmModelDisplay.Orphans != null)
            {
                refreshList = gsmModelDisplay.Orphans.ToList().FirstOrDefault(x => x.Contains("refresh rate"))?.Split("/").ToListTrim();
            }

            if (refreshList != null)
            {
                refreshRates.AddRange(refreshList.Select(refresh => refresh.Split("Hz").FirstOrDefault()).ToList());
            }

            int.TryParse(gsmModelDisplay.Resolution.First().Split('~').Last().Split("ppi").FirstOrDefault(),
                out var densityResult);
            int.TryParse(gsmModelDisplay.Resolution.FirstOrDefault()?.Split('x').FirstOrDefault(),
                out var heightResult);
            int.TryParse(Array.ConvertAll(gsmModelDisplay.Resolution.FirstOrDefault()?.Split('x') ?? Array.Empty<string>(), p => p.Trim())[1].Split(' ').FirstOrDefault(),
                out var weightResult);
            var resolution = new Resolution
            {
                Density = int.Parse(densityResult.ToString()),
                Height = int.Parse(heightResult.ToString()),
                Weight = int.Parse(weightResult.ToString()),
                Ratio = Regex.Match(gsmModelDisplay.Resolution.FirstOrDefault() ?? string.Empty, @"\d+\:\d+").Value
            };

            var type = new KeyValuePair<string, string>(gsmModelDisplay.Type.FirstOrDefault()?.Split(',').FirstOrDefault(),
                gsmModelDisplay.Type.FirstOrDefault()?.Split(',').Last().Split(' ').FirstOrDefault());

            var bodyRatio = gsmModelDisplay.Size.FirstOrDefault(x => x.Contains("ratio"));
            var ratioDensity = 0;
            if (bodyRatio != null)
            {
                int.TryParse(Regex.Match(bodyRatio, @"\d+\.?\d*").Value, out ratioDensity);
            }

            var sizes = gsmModelDisplay.Size.ToListTrim().FirstOrDefault()?.Split(',').ToListTrim();
            double.TryParse(Regex.Match(sizes.FirstOrDefault(x => x.Contains("in")) ?? string.Empty, @"\d+(\.?)\d*").Value,
                out var cmSize);
            double.TryParse(Regex.Match(sizes.FirstOrDefault(x => x.Contains("cm")) ?? string.Empty, @"\d+(\.?)\d*").Value,
                out var inSize);

            return new Display
            {
                Protection = protection,
                RefreshRate = refreshRates,
                Resolution = resolution,
                Type = type,
                Size = new Size
                {
                    BodyRatio = new KeyValuePair<string, double>(bodyRatio, ratioDensity),
                    Cm = cmSize,
                    In = inSize
                }
            };
        }

        private Memory EnrichMemory(MemoryJson gsmModelMemory)
        {
            var internalLists = new List<InternalMemory>();

            foreach (var intern in gsmModelMemory.Internal.First().Split(",").ToListTrim())
            {
                int.TryParse(intern.Split("GB RAM").First().Split(' ').Last(), out var ramResult);
                int.TryParse(intern.Split("GB").First(), out var sizeResult);
                internalLists.Add(new InternalMemory
                {
                    RAM = int.Parse(ramResult.ToString()),
                    Size = int.Parse(sizeResult.ToString())
                });
            }

            return new Memory
            {
                CardSlot = true,
                Internals = internalLists,
                Type = ""
            };
        }

        private Camera EnrichCamera(GsmArenaModel gsmModel)
        {
            var selfieVideos = new Dictionary<int, List<string>>();
            if (gsmModel.SelfieCamera != null)
            {
                var selfieVideoLists = gsmModel.SelfieCamera.Video?.FirstOrDefault()?.Split(',').Where(x => x.Contains("@")).Select(x => x.RemoveWhitespace());

                if (selfieVideoLists != null)
                {
                    foreach (var video in selfieVideoLists)
                    {
                        int.TryParse(video.Split('p').FirstOrDefault()?.Split('K').FirstOrDefault(), out var keyResult);
                        var key = int.Parse(keyResult.ToString());
                        if (selfieVideos.ContainsKey(key))
                        {
                            selfieVideos[key].Add(video);
                        }
                        else
                        {
                            selfieVideos.Add(key, new List<string> { video });
                        }
                    }
                }
            }


            var mainVideos = new Dictionary<int, List<string>>();
            if (gsmModel.MainCamera != null)
            {
                var mainVideoLists = gsmModel.MainCamera.Video?.FirstOrDefault()?.Split(',').Where(x => x.Contains("@")).Select(x => x.RemoveWhitespace());

                if (mainVideoLists != null)
                {
                    foreach (var video in mainVideoLists)
                    {
                        int.TryParse(video.Split('p').FirstOrDefault()?.Split('K').FirstOrDefault(), out var keyResult);
                        var key = int.Parse(keyResult.ToString());
                        if (mainVideos.ContainsKey(key))
                        {
                            mainVideos[key].Add(video);
                        }
                        else
                        {
                            mainVideos.Add(key, new List<string> { video });
                        }
                    }
                }
            }

            var mainCameraLists = gsmModel.MainCamera.Cameras.ToListTrim();
            var mainLens = new List<LensCamera>();
            foreach (var mainCamera in mainCameraLists)
            {
                var zoom = new Dictionary<double,string>();
                var zoomOptions = mainCamera.Split(',').Where(x => x.Contains("zoom")).Select(x => x.TrimStart());
                foreach (var zoomOption in zoomOptions)
                {
                    double.TryParse(zoomOption.Split('x').FirstOrDefault(), out var zoomResult);
                    zoom.Add(double.Parse(zoomResult.ToString(CultureInfo.InvariantCulture)), zoomOption);
                }

                var micro = Regex.Match(mainCamera.Split(',').FirstOrDefault(x => x.Contains("µm")) ?? "0.0µm",
                    @"(\d+\.\d*)(?=(µm))").Value;
                var size = Regex.Match(mainCamera.RemoveWhitespace(), @"(\d+(\.?)\d*)(?=(mm))").Value;
                var mp = Regex.Match(mainCamera.RemoveWhitespace(), @"\d+(\.?)\d*(?=(MP))").Value;
                var aparture = Regex.Match(mainCamera, @"(?<=(f\/))\d+(\.?)\d*").Value;

                double.TryParse(aparture, out var apartureResult);
                double.TryParse(mp, out var megapixelsResult);
                double.TryParse(size, out var sizeResult);
                mainLens.Add(new LensCamera
                {
                    Aperture = double.Parse(apartureResult.ToString(CultureInfo.InvariantCulture)),
                    Zoom = zoom,
                    Megapixels = double.Parse(megapixelsResult.ToString(CultureInfo.InvariantCulture)),
                    Micro = double.Parse(micro),
                    Size = double.Parse(sizeResult.ToString(CultureInfo.InvariantCulture)),
                    Type = Regex.Match(mainCamera, @"(?<=\()(.+)(?=\))").Value
                });
            }


            var selfieCameraLists = gsmModel.MainCamera.Cameras.ToListTrim();
            var selfieLens = new List<LensCamera>();
            foreach (var selfieCamera in selfieCameraLists)
            {
                var zoom = new Dictionary<double, string>();
                var zoomOptions = selfieCamera.Split(',').Where(x => x.Contains("zoom")).Select(x => x.TrimStart());
                foreach (var zoomOption in zoomOptions)
                {
                    double.TryParse(zoomOption.Split('x').FirstOrDefault(), out var zoomResult);
                    zoom.Add(double.Parse(zoomResult.ToString(CultureInfo.InvariantCulture)), zoomOption);
                }
                var micro = Regex.Match(selfieCamera.Split(',').FirstOrDefault(x => x.Contains("µm")) ?? "0.0µm",
                    @"(\d+\.\d*)(?=(µm))").Value;
                var size = Regex.Match(selfieCamera.RemoveWhitespace(), @"(\d+(\.?)\d*)(?=(mm))").Value;
                var mp = Regex.Match(selfieCamera.RemoveWhitespace(), @"\d+(\.?)\d*(?=(MP))").Value;
                var aparture = Regex.Match(selfieCamera, @"(?<=(f\/))\d+(\.?)\d*").Value;


                double.TryParse(aparture, out var apartureResult);
                double.TryParse(mp, out var megapixelsResult);
                double.TryParse(size, out var sizeResult);
                selfieLens.Add(new LensCamera
                {
                    Aperture = double.Parse(apartureResult.ToString(CultureInfo.InvariantCulture)),
                    Zoom = zoom,
                    Megapixels = double.Parse(megapixelsResult.ToString(CultureInfo.InvariantCulture)),
                    Micro = double.Parse(micro),
                    Size = double.Parse(sizeResult.ToString(CultureInfo.InvariantCulture)),
                    Type = Regex.Match(selfieCamera, @"(?<=\()(.+)(?=\))").Value
                });
            }

            var mainFeatures = new List<string>();
            if (gsmModel.MainCamera?.Features != null)
            {
                mainFeatures.AddRange(gsmModel.MainCamera.Features.First().Split(',').ToListTrim());
            }

            var selfieFeatures = new List<string>();
            if (gsmModel.SelfieCamera?.Features != null)
            {
                selfieFeatures.AddRange(gsmModel.SelfieCamera.Features.First().Split(',').ToListTrim());
            }

            var mainCamerasNo = gsmModel.MainCamera?.Cameras?.Length ?? 0;
            var selfieCamerasNo = gsmModel.SelfieCamera?.Cameras?.Length ?? 0;

            return new Camera
            {
                Main = new MainCamera()
                {
                    Features = mainFeatures,
                    Lens = mainLens,
                    Number = mainCamerasNo,
                    Videos = mainVideos
                },
                Selfie = new SelfieCamera()
                {
                    Features = selfieFeatures,
                    Lens = selfieLens,
                    Number = selfieCamerasNo,
                    Videos = selfieVideos
                }
            };
        }
    }

    public class Status
    {
        public string Announced { get; set; }
        public string Launch { get; set; }
    }

    public class Name
    {
        public string Brand { get; set; }
        public string Main { get; set; }
    }

    public class Network
    {
        public List<string> Technology { get; set; }
        public List<string> Speed { get; set; }
        public Dictionary<string, List<string>> Bands { get; set; }
    }

    public class Body
    {
        public List<string> Build { get; set; }
        public string Sim { get; set; }
        public double Weight { get; set; }
        public KeyValuePair<int, string> WaterResistant { get; set; }
        public Dimensions Dimensions { get; set; }
    }

    public class Dimensions
    {
        public Dictionary<string, double> Cm { get; set; }
        public Dictionary<string, double> In { get; set; }
    }

    public class Platform
    {
        public Dictionary<string, int> Os { get; set; }
        public List<Chipset> Chipset { get; set; }
        public List<CPU> CPU { get; set; }
        public List<GPU> GPU { get; set; }
        public List<Performance> Tests { get; set; }
    }

    public class Performance
    {
        public string Type { get; set; }
        public double Score { get; set; }
    }

    public class Chipset
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Generation { get; set; }
        public double Size { get; set; }
    }

    public class CPU
    {
        public string Type { get; set; }
        public int Cores { get; set; }
        public List<CpuType> CpuList { get; set; }
    }

    public class CpuType
    {
        public int Count { get; set; }
        public double Ghz { get; set; }
        public string Name { get; set; }
    }

    public class GPU
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Generation { get; set; }
    }

    public class Memory
    {
        public string Type { get; set; }

        public List<InternalMemory> Internals { get; set; }
        public bool CardSlot { get; set; }
    }

    public class InternalMemory
    {
        public int Size { get; set; }
        public int RAM { get; set; }
    }

    public class Display
    {
        public List<string> RefreshRate { get; set; }
        public Size Size { get; set; }
        public Resolution Resolution { get; set; }
        public KeyValuePair<string, int> Protection { get; set; }
        public KeyValuePair<string, string> Type { get; set; }
    }

    public class Size
    {
        public double Cm { get; set; }
        public double In { get; set; }
        public KeyValuePair<string, double> BodyRatio { get; set; }
    }

    public class Resolution
    {
        public int Height { get; set; }
        public int Weight { get; set; }
        public int Density { get; set; }
        public string Ratio { get; set; }
    }

    public class Camera
    {
        public MainCamera Main { get; set; }
        public SelfieCamera Selfie { get; set; }
    }

    public class MainCamera
    {
        public int Number { get; set; }
        public List<LensCamera> Lens { get; set; }
        public List<string> Features { get; set; }
        public Dictionary<int, List<string>> Videos { get; set; }
    }

    public class SelfieCamera
    {
        public int Number { get; set; }
        public List<LensCamera> Lens { get; set; }
        public List<string> Features { get; set; }
        public Dictionary<int, List<string>> Videos { get; set; }
    }

    public class LensCamera
    {
        public string Type { get; set; }
        public double Size { get; set; }
        public double Megapixels { get; set; }
        public double Aperture { get; set; }
        public double Micro { get; set; }
        public Dictionary<double, string> Zoom { get; set; }
    }

    public class Sound
    {
        public bool Stereo { get; set; }
        public bool Loudspeaker { get; set; }
        public bool Jack { get; set; }
        public List<string> Others { get; set; }
    }

    public class Comms
    {
        public List<string> GPS { get; set; }
        public double Bluetooth { get; set; }
        public List<string> Wifi { get; set; }
        public KeyValuePair<string, double> Usb { get; set; }
    }

    public class Features
    {
        public bool WaterResistant { get; set; }
        public bool DustResistant { get; set; }
        public bool Radio { get; set; }
        public bool FastCharging { get; set; }
        public bool Jack { get; set; }
        public bool Nfc { get; set; }
        public bool Gps { get; set; }
        public bool Gyro { get; set; }
        public KeyValuePair<bool, string> Fingerprint { get; set; }
    }

    public class Battery
    {
        public int Capacity { get; set; }
        public bool Fast { get; set; }
        public int Power { get; set; }
        public bool Wireless { get; set; }
    }
}
