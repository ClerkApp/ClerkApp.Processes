using Clerk.Processes.WebExtractorToJson.Model.GSMArena;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Clerk.Processes.WebExtractorToJson.Model.MobilePhone
{
    public class Mobile
    {
        public Guid Id { get; set; }
        public DataLink DataLinks { get; set; }
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

        public Mobile(GsmArenaModel gsmModel, string phoneUrl)
        {
            Id = gsmModel.Name.Main[0].BuildGuid();
            DataLinks = new DataLink()
            {
                Image = gsmModel.Photo,
                Link = new Uri(phoneUrl)
            };
            Status = new Status
            {
                Announced = gsmModel.Launch.Announced[0].ToLower(),
                Year = int.Parse(gsmModel.Launch.Announced[0].SplitAndTrim(",").First()),
                Launch = gsmModel.Launch.Status[0].ToLower()
            };
            Name = new Name
            {
                Brand = gsmModel.Name.Main[0].SplitAndTrim(" ")[0].ToLower(),
                Main = gsmModel.Name.Main[0].SplitAndTrim("(")[0].ToLower()
            };
            Network = new Network
            {
                Band = EnrichNetworkBands(gsmModel.Network),
                Speed = gsmModel.Network.Speed is null ? new List<string>() : gsmModel.Network.Speed[0].Split(",").ToListTrim(),
                Technology = EnrichTechnologyBands(gsmModel)
            };
            Body = EnrichBody(gsmModel);
            Platform = EnrichPlatform(gsmModel);
            Memory = EnrichMemory(gsmModel.Memory);
            Display = EnrichDisplay(gsmModel.Display);
            Camera = EnrichCamera(gsmModel);
            Sound = EnrichSound(gsmModel);
            Comms = EnrichComms(gsmModel);
            Battery = EnrichBattery(gsmModel.Battery);
            Price = EnrichPrice(gsmModel);
            Colors = gsmModel.Misc.Colors[0].SplitAndTrim(",").ToListTrim();
            Features = EnrichFeatures(gsmModel);
        }

        private TechSpec EnrichTechnologyBands(GsmArenaModel gsmModel)
        {
            var technology = new TechSpec()
            {
                Techs = new List<string>(),
                TotalTechs = 0
            };


            if (gsmModel.Network.Technology != null)
            {
                foreach (var value in gsmModel.Network.Technology[0].Split("/").ToListTrim())
                {
                    technology.Techs.Add(value);
                    technology.TotalTechs += 1;
                }
            }

            return technology;
        }

        private Sound EnrichSound(GsmArenaModel gsmModel)
        {
            bool hasJack = false;
            bool hasSpeaker = false;
            bool isStereo = false;

            if (gsmModel.Sound.The35MmJack != null)
            {
                hasJack = gsmModel.Sound.The35MmJack[0].ToLower().Contains("yes");
            }

            if (gsmModel.Sound.Loudspeaker != null)
            {
                hasSpeaker = gsmModel.Sound.Loudspeaker[0].ToLower().Contains("yes");
                isStereo = gsmModel.Sound.Loudspeaker[0].ToLower().Contains("stereo");
            }

            return new Sound
            {
                Jack = hasJack,
                Loudspeaker = hasSpeaker,
                Stereo = isStereo
            };
        }

        private static ImageData EnrichImages(Uri gsmModelPhoto)
        {
            var img = new WebClient().DownloadData(gsmModelPhoto);
            var images = new List<byte[]> { img };

            return new ImageData
            {
                Images = images
            };
        }

        private Comms EnrichComms(GsmArenaModel gsmModel)
        {
            double.TryParse(gsmModel.Comms.Usb[0].SplitAndTrim(",")[0], out var usbResult);
            double.TryParse(Regex.Match(gsmModel.Comms.Bluetooth[0].SplitAndTrim(",")[0], @"\d+(\.?)\d*").Value, out var blueResult);

            return new Comms
            {
                Bluetooth = double.Parse(blueResult.ToString(CultureInfo.InvariantCulture)),
                Gps = gsmModel.Comms.Gps is null
                    ? new List<string>()
                    : gsmModel.Comms.Gps[0].SplitAndTrim(",").Skip(1).ToList().Select(x => x.SplitAndTrim(" ")[^1]).ToList(),
                Usb = new KeyValuePair<string, double>(gsmModel.Comms.Usb[0].SplitAndTrim(",")[^1].Trim(), double.Parse(usbResult.ToString(CultureInfo.InvariantCulture))),
                Wifi = gsmModel.Comms.Wlan is null
                    ? new List<string>()
                    : gsmModel.Comms.Wlan[0].SplitAndTrim(",")[0].SplitAndTrim("/").ToListTrim()
            };
        }

        private static Features EnrichFeatures(GsmArenaModel gsmModel)
        {
            var finger = new Spec<bool>
            {
                Value = false,
                Name = new List<string>()
            };
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
                foreach (var sensor in gsmModel.Features.Sensors[0].SplitAndTrim("),"))
                {
                    var hasFinger = sensor.Contains("fingerprint", StringComparison.CurrentCultureIgnoreCase);
                    if (hasFinger)
                    {
                        finger.Value = true;
                        finger.Name.Add($"{sensor})");
                    }
                }
            }

            if (gsmModel.Comms?.Gps != null)
            {
                gps = !gsmModel.Comms.Gps[0].ToLower().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Comms?.Nfc != null)
            {
                nfc = !gsmModel.Comms.Nfc[0].ToLower().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Comms?.Radio != null)
            {
                nfc = !gsmModel.Comms.Radio[0].ToLower().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Sound?.The35MmJack != null)
            {
                jack = !gsmModel.Sound.The35MmJack[0].ToLower().Contains("no", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.MainCamera?.Video != null)
            {
                gyro = gsmModel.MainCamera.Video[0].ToLower().Contains("gyro", StringComparison.CurrentCultureIgnoreCase);
            }
            if (gsmModel.Battery?.Charging != null)
            {
                foreach (var charging in gsmModel.Battery.Charging.ToListTrim())
                {
                    if (charging.Contains("fast", StringComparison.CurrentCultureIgnoreCase))
                    {
                        fastCharging = true;
                    }
                }
            }
            if (gsmModel.Body.Orphans != null)
            {
                foreach (var orphans in gsmModel.Body.Orphans.ToListTrim())
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

        private static Dictionary<string, double> EnrichPrice(GsmArenaModel gsmModel)
        {
            var prices = new Dictionary<string, double>();

            var pricesList = gsmModel.Misc.Price?[0].SplitAndTrim("/").ToListTrim();

            if (pricesList != null)
            {
                foreach (var price in pricesList)
                {
                    if (price.Contains("about", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (price.Contains("usd", StringComparison.InvariantCultureIgnoreCase))
                        {
                            prices.Add(
                                "USD", double.Parse(Regex.Match(price.RemoveWhitespace(), @"(\d+[.,]?\d*)(?=(usd))").Value)
                            );
                        }
                        if (price.Contains("eur", StringComparison.InvariantCultureIgnoreCase))
                        {
                            prices.Add(
                                "EUR", double.Parse(Regex.Match(price.RemoveWhitespace(), @"(\d+[.,]?\d*)(?=(eur))").Value)
                            );
                        }
                    }
                    else
                    {
                        var valueResult = Regex.Match(price.RemoveWhitespace(), @"[\W](?=[1-9])").Value;
                        var name = ((ConvertPrice)valueResult.ToUpper().ToCharArray()[0]).ToString();

                        prices.Add(
                            name.ToUpper(), double.Parse(Regex.Match(price, @"(\d+[.,]?\d*)").Value)
                        );
                    }
                }
            }

            return prices;
        }

        private enum ConvertPrice
        {
            USD = '$',
            EUR = '€',
            GBP = '£',
            RUP = '₹'
        }

        private static Battery EnrichBattery(BatteryJson gsmModel)
        {
            if (gsmModel.Charging != null)
            {
                var batteryCharger = gsmModel.Charging.ToListTrim().Where(x => x.Contains("fast charging", StringComparison.CurrentCultureIgnoreCase)).ToArray();
                var wirelessCharger = gsmModel.Charging.ToListTrim().Where(x => x.Contains("wireless charging", StringComparison.CurrentCultureIgnoreCase));

                int.TryParse(Regex.Match(gsmModel.Orphans.First().RemoveWhitespace(), @"(?<!(mah))\d+").Value,
                    out var capacityResult1);

                var powerResult = 0;
                if (!batteryCharger.Length.Equals(0))
                {
                    int.TryParse(Regex.Match(batteryCharger[0].RemoveWhitespace(), @"(?<!(W))\d+").Value,
                        out powerResult);
                }

                return new Battery
                {
                    Capacity = int.Parse(capacityResult1.ToString()),
                    Fast = batteryCharger.Any(),
                    Power = int.Parse(powerResult.ToString()),
                    Wireless = wirelessCharger.Any()
                };
            }

            int.TryParse(Regex.Match(gsmModel.Orphans[0].RemoveWhitespace(), @"(?<!(mah))\d+").Value, out var capacityResult2);
            return new Battery
            {
                Capacity = int.Parse(capacityResult2.ToString()),
                Fast = false,
                Power = 0,
                Wireless = false
            };
        }

        private static Platform EnrichPlatform(GsmArenaModel gsmModel)
        {
            var platform = new Platform
            {
                Chipset = new Dictionary<string, List<Chipset>>(),
                Cpu = new Dictionary<string, List<Cpu>>(),
                Gpu = new Dictionary<string, List<Gpu>>(),
                Os = new Os(),
                Tests = new Dictionary<string, double>()
            };

            if (gsmModel.Platform is null)
            {
                return new Platform();
            }

            foreach (var chipset in gsmModel.Platform.Chipset.ToListTrim())
            {
                double result = 0;
                if (chipset.Contains("nm"))
                {
                    double.TryParse(chipset.SplitAndTrim("(")[1].Split("nm")[0], out result);
                }

                int generationResult = 0;
                if (chipset.Contains("dragon"))
                {
                    int.TryParse(chipset.SplitAndTrim("(")[0].SplitAndTrim(" ")[^1], out generationResult);
                }
                else if (chipset.Contains("mediatek"))
                {
                    int.TryParse(Regex.Match(chipset, @"(?<=(MT))[0-9]*").Value, out generationResult);
                }
                else
                {
                    int.TryParse(chipset.SplitAndTrim(" ")[1], out generationResult);
                }

                var type = chipset.SplitAndTrim(")").Last().Contains(" - ") ? chipset.SplitAndTrim("-")[^1] : "global";

                if (platform.Chipset.ContainsKey(type))
                {
                    platform.Chipset[type].Add(new Chipset
                    {
                        Generation = generationResult,
                        Name = chipset.SplitAndTrim("(")[0],
                        Size = result
                    });
                }
                else
                {
                    platform.Chipset.Add(type, new List<Chipset>()
                    {
                        new Chipset
                        {
                            Generation = generationResult,
                            Name = chipset.SplitAndTrim("(")[0],
                            Size = result
                        }
                    });
                }
            }

            foreach (var gpu in gsmModel.Platform.Gpu.ToListTrim())
            {
                int.TryParse(gpu.SplitAndTrim("-")[0].SplitAndTrim(" ")[^1], out var generationResult);
                var type = gpu.SplitAndTrim(")").Last().Contains(" - ") ? gpu.SplitAndTrim("-")[^1] : "global";

                if (platform.Gpu.ContainsKey(type))
                {
                    platform.Gpu[type].Add(new Gpu
                    {
                        Generation = generationResult,
                        Name = gpu.SplitAndTrim("-")[0],
                    });
                }
                else
                {
                    platform.Gpu.Add(type, new List<Gpu>()
                    {
                        new Gpu
                        {
                            Generation = generationResult,
                            Name = gpu.SplitAndTrim("-")[0],
                        }
                    });
                }
            }

            foreach (var cpu in gsmModel.Platform.Cpu.ToListTrim())
            {
                Enum.TryParse(string.Join("", cpu.ToLower().SplitAndTrim(" ")[0].SplitAndTrim("-")), out Cores result);

                var cpuTypes = new List<CpuType>();

                if (cpu.Contains("("))
                {
                    foreach (var cpuType in Regex.Match(cpu, @"(?<=\()(.*)(?=\))").Value.SplitAndTrim("&"))
                    {
                        int.TryParse(cpuType.SplitAndTrim("x")[0], out var countResult);

                        double ghzResult;
                        if (cpu.SplitAndTrim(" ").Length > 1)
                        {
                            double.TryParse(cpuType.SplitAndTrim("x")[1].SplitAndTrim(" ")[0], out ghzResult);
                        }
                        else
                        {
                            double.TryParse(cpuType.SplitAndTrim("x")[0].SplitAndTrim(" ")[0], out ghzResult);
                        }

                        cpuTypes.Add(new CpuType
                        {
                            Count = int.Parse(countResult.ToString()),
                            Ghz = double.Parse(ghzResult.ToString(CultureInfo.InvariantCulture)),
                            Name = cpuType.Split("hz")[^1].Trim()
                        });
                    }
                }
                else
                {
                    double ghzResult;
                    if (cpu.SplitAndTrim(" ").Length > 1)
                    {
                        double.TryParse(cpu.SplitAndTrim(" ")[1], out ghzResult);
                    }
                    else
                    {
                        double.TryParse(cpu.SplitAndTrim(" ")[0], out ghzResult);
                    }

                    cpuTypes.Add(new CpuType
                    {
                        Count = 1,
                        Ghz = double.Parse(ghzResult.ToString(CultureInfo.InvariantCulture)),
                        Name = "unknown"
                    });
                }

                var type = cpu.SplitAndTrim(")").Last().Contains(" - ") ? cpu.SplitAndTrim("-")[^1] : "global";

                if (platform.Cpu.ContainsKey(type))
                {
                    platform.Cpu[type].Add(new Cpu
                    {
                        Cores = (int)result,
                        CpuList = cpuTypes,
                    });
                }
                else
                {
                    platform.Cpu.Add(type, new List<Cpu>()
                    {
                        new Cpu
                        {
                            Cores = (int)result,
                            CpuList = cpuTypes,
                        }
                    });
                }
            }

            if (gsmModel.Platform.Os != null)
            {
                int.TryParse(Regex.Match(gsmModel.Platform.Os.First().SplitAndTrim(",").First(), @"\d+").Value, out var osResult);
                if (gsmModel.Platform.Os.First().Contains(","))
                {
                    platform.Os.Interface = gsmModel.Platform.Os.First().Split(",")[1].ToLower();
                }
                else
                { 
                    platform.Os.Interface = gsmModel.Platform.Os.First().Trim().ToLower();
                }
                platform.Os.Version = osResult;
                platform.Os.System = gsmModel.Platform.Os.First().SplitAndTrim(" ").First();
            }

            if (gsmModel.Tests != null)
            {
                foreach (var perf in gsmModel.Tests.Performance.ToListTrim())
                {
                    double.TryParse(perf.Split(":").ToListTrim()[1].Split(" ")[0].Replace("fps", ""),
                        out var scoreResult);

                    var type = perf.SplitAndTrim(":")[0].Replace("\n", "").ToLower();
                    platform.Tests.Add(type, double.Parse(scoreResult.ToString(CultureInfo.InvariantCulture)));
                }
            }

            return platform;
        }

        private static BandSpec EnrichNetworkBands(NetworkJson gsmNetwork)
        {
            var networks = new BandSpec()
            {
                Bands = new List<BandType>(),
                TotalBands = 0
            };

            foreach (var (key, value) in gsmNetwork.Bands)
            {
                var listOfValues = value[0].SplitAndTrim("/")[0].Split(",").ToListTrim();

                networks.TotalBands += listOfValues.Count;
                networks.Bands.Add(new BandType
                {
                    Type = key,
                    Count = listOfValues.Count,
                    Values = listOfValues
                });
            }

            return networks;
        }

        private static Body EnrichBody(GsmArenaModel gsmModel)
        {
            var water = gsmModel.Body.Orphans?.FirstOrDefault(x => x.Contains("water") && x.Contains("resistant"));
            
            KeyValuePair<int, string> waterResistent;
            if (!string.IsNullOrWhiteSpace(water))
            {
                int.TryParse(water.SplitAndTrim(" ")[0].Split("ip")[^1], out var waterResult);
                waterResistent = new KeyValuePair<int, string>(int.Parse(waterResult.ToString()), water.ToLower());
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

            var dim = gsmModel.Body.Dimensions[0].SplitAndTrim(" ").ToListTrim();
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

            double.TryParse(gsmModel.Body.Weight[0].SplitAndTrim("g")[0].SplitAndTrim("k")[0], out var weightResult);

            var bodyWeight = gsmModel.Body.Weight.Contains("-") ? 0 : double.Parse(weightResult.ToString(CultureInfo.InvariantCulture));

            return new Body
            {
                Build = gsmModel.Body.Build is null ? new List<string>() : gsmModel.Body.Build[0].SplitAndTrim(",").ToListTrim(),
                Sim = gsmModel.Body.Sim[0].ToLower(),
                WaterResistant = waterResistent,
                Weight = bodyWeight,
                Dimensions = dimensions
            };
        }

        private Display EnrichDisplay(DisplayJson gsmModelDisplay)
        {
            int.TryParse(gsmModelDisplay.Protection?[0]?.SplitAndTrim(" ")[^1], out var protectionResult);
            var protection = new Spec<int>()
            {
                Name = (gsmModelDisplay.Protection ?? Array.Empty<string>()).ToListTrim(),
                Value = protectionResult
            };

            List<string> refreshList = null;
            var refreshRates = new List<int>();


            if (gsmModelDisplay.Orphans != null)
            {
                refreshList = gsmModelDisplay.Orphans.ToList().FirstOrDefault(x => x.Contains("refresh rate"))?.Split("/").ToListTrim();
            }

            if (refreshList != null)
            {
                refreshRates.AddRange(refreshList.Select(refresh => int.Parse(refresh.Split("hz")[0])).ToList());
            }

            int.TryParse(gsmModelDisplay.Resolution[0].SplitAndTrim("~")[^1].Split("ppi")[0],
                out var densityResult);
            int.TryParse(gsmModelDisplay.Resolution[0]?.SplitAndTrim("x")[0],
                out var heightResult);
            int.TryParse(Array.ConvertAll(gsmModelDisplay.Resolution[0]?.SplitAndTrim("x") ?? Array.Empty<string>(), p => p.Trim())[1].SplitAndTrim(" ")[0],
                out var weightResult);
            var resolution = new Resolution
            {
                Density = int.Parse(densityResult.ToString()),
                Height = int.Parse(heightResult.ToString()),
                Weight = int.Parse(weightResult.ToString()),
                Ratio = Regex.Match(gsmModelDisplay.Resolution[0] ?? string.Empty, @"\d+\:\d+").Value
            };

            var type = new Spec<string>()
            {
                Name = gsmModelDisplay.Type[0]?.SplitAndTrim(",").ToListTrim(),
                Value = gsmModelDisplay.Type[0]?.SplitAndTrim(",")[^1].SplitAndTrim(" ")[0]
            };

            var bodyRatio = gsmModelDisplay.Size.FirstOrDefault(x => x.Contains("ratio"));
            double ratioDensity = 0;
            if (bodyRatio != null)
            {
                double.TryParse(Regex.Match(bodyRatio, @"\d+\.?\d*").Value, out ratioDensity);
            }

            var sizes = gsmModelDisplay.Size.ToListTrim()[0]?.SplitAndTrim(",").ToListTrim();
            double.TryParse(Regex.Match(sizes.FirstOrDefault(x => x.Contains("in")) ?? string.Empty, @"\d+(\.?)\d*").Value,
                out var inSize);
            double.TryParse(Regex.Match(sizes.FirstOrDefault(x => x.Contains("cm")) ?? string.Empty, @"\d+(\.?)\d*").Value,
                out var cmSize);

            return new Display
            {
                Protection = protection,
                RefreshRate = refreshRates,
                Resolution = resolution,
                Type = type,
                Size = new Size
                {
                    BodyRatio = new KeyValuePair<string, double>(bodyRatio != null ? bodyRatio.Trim() : "", ratioDensity),
                    Cm = cmSize,
                    In = inSize
                }
            };
        }

        private Memory EnrichMemory(MemoryJson gsmModelMemory)
        {
            var internalLists = new List<InternalMemory>();

            if (gsmModelMemory.Internal is null)
            {
                return new Memory();
            }

            foreach (var intern in gsmModelMemory.Internal[0].Split(",").ToListTrim())
            {
                int.TryParse(intern.Split("gb ram")[0].SplitAndTrim(" ")[^1], out var ramResult);
                int.TryParse(intern.Split("gb")[0], out var sizeResult);
                internalLists.Add(new InternalMemory
                {
                    Ram = int.Parse(ramResult.ToString()),
                    Size = int.Parse(sizeResult.ToString())
                });
            }

            return new Memory
            {
                CardSlot = true,
                Internals = internalLists
            };
        }

        private Camera EnrichCamera(GsmArenaModel gsmModel)
        {
            if (gsmModel.MainCamera is null)
            {
                return new Camera();
            }

            var mainVideos = new List<Spec<int>>();
            var mainVideoLists = gsmModel.MainCamera?.Video?[0]?.SplitAndTrim(",").Where(x => x.Contains("@")).Select(x => x.RemoveWhitespace());

            if (mainVideoLists != null)
            {
                foreach (var video in mainVideoLists)
                {
                    int.TryParse(video.SplitAndTrim("p")[0].SplitAndTrim("k")[0], out var keyResult);
                    var value = int.Parse(keyResult.ToString());
                    if (mainVideos.Any(s => s.Value.Equals(value)))
                    {
                        mainVideos.First(s => s.Value.Equals(value)).Name.Add(video);
                    }
                    else
                    {
                        mainVideos.Add(new Spec<int>
                        {
                            Name = new List<string> { video },
                            Value = value
                        });
                    }
                }
            }

            var mainCameraLists = gsmModel.MainCamera.Cameras.ToListTrim();
            var mainLens = new List<LensCamera>();
            foreach (var mainCamera in mainCameraLists)
            {
                var zoom = new Dictionary<double,string>();
                var zoomOptions = mainCamera.SplitAndTrim(",").Where(x => x.Contains("zoom")).Select(x => x.TrimStart());
                foreach (var zoomOption in zoomOptions)
                {
                    double.TryParse(zoomOption.SplitAndTrim("x")[0], out var zoomResult);
                    zoom.Add(double.Parse(zoomResult.ToString(CultureInfo.InvariantCulture)), zoomOption);
                }

                var micro = Regex.Match(mainCamera.SplitAndTrim(",").FirstOrDefault(x => x.Contains("µm")) ?? "0.0µm",
                    @"(\d+\.\d*)(?=(µm))").Value;
                var size = Regex.Match(mainCamera.RemoveWhitespace(), @"(\d+(\.?)\d*)(?=(mm))").Value;
                var mp = Regex.Match(mainCamera.RemoveWhitespace(), @"\d+(\.?)\d*(?=(mp))").Value;
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

            var selfieVideos = new List<Spec<int>>();
            var selfieVideoLists = gsmModel.SelfieCamera?.Video?[0]?.SplitAndTrim(",").Where(x => x.Contains("@")).Select(x => x.RemoveWhitespace());

            if (selfieVideoLists != null)
            {
                foreach (var video in selfieVideoLists)
                {
                    int.TryParse(video.SplitAndTrim("p")[0].SplitAndTrim("k")[0], out var keyResult);
                    var value = int.Parse(keyResult.ToString());
                    if (selfieVideos.Any(s => s.Value.Equals(value)))
                    {
                        selfieVideos.First(s => s.Value.Equals(value)).Name.Add(video);
                    }
                    else
                    {
                        selfieVideos.Add(new Spec<int>
                        {
                            Name = new List<string> { video },
                            Value = value
                        });
                    }
                }
            }

            var selfieCameraLists = gsmModel.SelfieCamera.Cameras.ToListTrim();
            var selfieLens = new List<LensCamera>();
            foreach (var selfieCamera in selfieCameraLists)
            {
                var zoom = new Dictionary<double, string>();
                var zoomOptions = selfieCamera.SplitAndTrim(",").Where(x => x.Contains("zoom")).Select(x => x.TrimStart());
                foreach (var zoomOption in zoomOptions)
                {
                    double.TryParse(zoomOption.SplitAndTrim("x")[0], out var zoomResult);
                    zoom.Add(double.Parse(zoomResult.ToString(CultureInfo.InvariantCulture)), zoomOption);
                }
                var micro = Regex.Match(selfieCamera.SplitAndTrim(",").FirstOrDefault(x => x.Contains("µm")) ?? "0.0µm",
                    @"(\d+\.\d*)(?=(µm))").Value;
                var size = Regex.Match(selfieCamera.RemoveWhitespace(), @"(\d+(\.?)\d*)(?=(mm))").Value;
                var mp = Regex.Match(selfieCamera.RemoveWhitespace(), @"\d+(\.?)\d*(?=(mp))").Value;
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
                mainFeatures.AddRange(gsmModel.MainCamera.Features[0].SplitAndTrim(",").ToListTrim());
            }

            var selfieFeatures = new List<string>();
            if (gsmModel.SelfieCamera?.Features != null)
            {
                selfieFeatures.AddRange(gsmModel.SelfieCamera.Features[0].SplitAndTrim(",").ToListTrim());
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

    public class DataLink
    {
        public Uri Image { get; set; }
        public Uri Link { get; set; }
    }

    public class ImageData
    {
        public List<byte[]> Images { get; set; }
    }

    public enum Cores
    {
        singlecore = 1,
        dualcore = 2,
        quadcore = 4,
        hexacore = 6,
        octacore = 8
    }

    public class Status
    {
        public string Announced { get; set; }
        public int Year { get; set; }
        public string Launch { get; set; }
    }

    public class Name
    {
        public string Brand { get; set; }
        public string Main { get; set; }
    }

    public class Network
    {
        public TechSpec Technology { get; set; }
        public List<string> Speed { get; set; }
        public BandSpec Band { get; set; }
    }

    public class TechSpec
    {
        public int TotalTechs { get; set; }

        public List<string> Techs { get; set; }
    }

    public class BandSpec
    {
        public int TotalBands { get; set; }

        public List<BandType> Bands { get; set; }

    }

    public class BandType
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public List<string> Values { get; set; }
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
        public Os Os { get; set; }
        public Dictionary<string, List<Chipset>> Chipset { get; set; }
        public Dictionary<string, List<Cpu>> Cpu { get; set; }
        public Dictionary<string, List<Gpu>> Gpu { get; set; }
        public Dictionary<string, double> Tests { get; set; }
    }

    public class Os
    {
        public string System { get; set; }
        public int Version { get; set; }
        public string Interface { get; set; }
    }

    public class Chipset
    {
        public string Name { get; set; }
        public int Generation { get; set; }
        public double Size { get; set; }
    }

    public class Cpu
    {
        public int Cores { get; set; }
        public List<CpuType> CpuList { get; set; }
    }

    public class CpuType
    {
        public int Count { get; set; }
        public double Ghz { get; set; }
        public string Name { get; set; }
    }

    public class Gpu
    {
        public string Name { get; set; }
        public int Generation { get; set; }
    }

    public class Memory
    {
        public List<InternalMemory> Internals { get; set; }
        public bool CardSlot { get; set; }
    }

    public class InternalMemory
    {
        public int Size { get; set; }
        public int Ram { get; set; }
    }

    public class Display
    {
        public List<int> RefreshRate { get; set; }
        public Size Size { get; set; }
        public Resolution Resolution { get; set; }
        public Spec<int> Protection { get; set; }
        public Spec<string> Type { get; set; }
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
        public List<Spec<int>> Videos { get; set; }
    }

    public class SelfieCamera
    {
        public int Number { get; set; }
        public List<LensCamera> Lens { get; set; }
        public List<string> Features { get; set; }
        public List<Spec<int>> Videos { get; set; }
    }

    public class Spec<T>
    {
        public T Value { get; set; }
        public List<string> Name { get; set; }
    }

    public class PriceSpec<T>
    {
        public T Value { get; set; }
        public string Type { get; set; }
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
    }

    public class Comms
    {
        public List<string> Gps { get; set; }
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
        public Spec<bool> Fingerprint { get; set; }
    }

    public class Battery
    {
        public int Capacity { get; set; }
        public bool Fast { get; set; }
        public int Power { get; set; }
        public bool Wireless { get; set; }
    }
}
