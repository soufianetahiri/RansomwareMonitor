
# RansomwareMonitor
A ransomware group monitoring bot written in C#.
I used a Windows Form for few reasons, the very first because why not! the second it was the only way I found to "see what was parsed while it was parsed" the third because of the waiter on LockBit blog.

Each time it parses a claim, it generates a hash then stored it on a local SQLite db, this is my quick and dirty way to verify that we do not notify two times about the same claim.

The bot sends notifications to aTelegram channel + Twitter, you need to modfy API keys accordinly (have a look at the code you w'ill find out where to edit)

      public Tweet()
            {
                APIKey = "x";
                APISecret = "x";
                AccessToken = "x-x";
                AccessSecret = "x";
                hashs = "{0}... #ransomware #leaks #infosec #databreach #cyberattack. More @:https://t.me/ransomwatcher"; //190
        }


    Consts.ChatId = "-x";
    Consts.TokenId = "x:x";

A loosy way to update the list of URLs / Database of hashes is included too, he sameway for the APIs you need to look the code


     private void UpdateSettings()
            {
                try
                {
                    using WebClient client = new WebClient();
                    statusLabel.Text = "Downloading ransome wiki...";
                    client.DownloadFile("https://xxxxxxxxx/appsettings.json",
                                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "appsettings.json");
                    _config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();
                    GangList = _config.GetSection("Gangs").GetChildren().ToDictionary(x => x.Key, x => x.Value);
                }
                catch (Exception ex)
                { log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message); }
            }
              private bool DownloadDb()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://xxxxxxxxx/history.db",
                                        Consts.HistoryTemp);
                    if (File.Exists(Consts.Historydb))
                    {
                        if (File.Exists(Consts.HistoryTemp))
                        {
                            if (new FileInfo(Consts.HistoryTemp).Length > new FileInfo(Consts.Historydb).Length)
                            {
                                File.Replace(Consts.HistoryTemp, Consts.Historydb, null, true);
                            }

                        }
                    }
                    else if (File.Exists(Consts.HistoryTemp))
                    {
                        File.Move(Consts.HistoryTemp, Consts.Historydb);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
        }

To push hashes to a remote db, you need to edit:

    string  connetionString  =  "server=127.0.0.1;port=5522;database=*;uid=*;*";

Ransomware groups blog's URLs are stored in appsettings.json as follow:

    {
      "Gangs": {
        "Conti": "https://continews.click", //"http://continewsnv5otx5kaoje7krkto2qbu3gtqef22mnr7eaxw3y6ncz3ad.onion.ws/", //https://continews.click/
        "Hive Leaks": "https://hiveapi4nyabjdfz2hxdsr7otrcv6zq6m4rk5i2w7j64lrtny4b7vjad.onion.ws/",
        "Corporate Leaks": "http://hxt254aygrsziejn.onion.ws/", //todo off
        "Pysa": "http://pysa2bitc5ldeyfak4seeruqymqs4sj5wt5qkcq7aoyg4h2acqieywad.onion.ws/partners.html",
        "Everest": "https://ransomocmou6mnbquqz44ewosbkjk3o5qjsl3orawojexfook2j7esad.onion.ws/feed/",
        "Dopple Leaks": "http://hpoo4dosa3x4ognfxpqcrjwnsigvslm7kv6hvmhh2yqczaxy3j6qnwad.onion.ws/", //todo off
        "Cuba": "https://cuba4ikm4jakjgmkezytyawtdgr2xymvy6nvzgw5cglswg3si76icnqd.onion.ws/feed/",
        "Ragnar_Locker": "http://rgleaktxuey67yrgspmhvtnrqtgogur35lwdrup4d3igtbm3pupc4lyd.onion.ws/",
        "Grief": "http://griefcameifmv4hfr3auozmovz5yi6m3h3dwbuqw7baomfxoxz4qteid.onion.ws/api/?type=",
        "Prometheus": "http://promethw27cbrcot.onion.ws/blog/", //todo off
        "LV": "https://rbvuetuneohce3ouxjlbxtimyyxokb4btncxjbo44fbgxqy7tskinwad.onion.ws/api/posts/1",
        "Xing Locker": "http://xingnewj6m4qytljhfwemngm7r7rogrindbq7wrfeepejgxc3bwci7qd.onion.ws/",
        "Lorenz": "http://lorenzmlwpzgxq736jzseuterytjueszsvznuibanxomlpkyxk6ksoyd.onion.ws/",
        "synACK / El_Cometa": "http://xqkz2rmrqkeqf6sjbrb47jfwnqxcd4o2zvaxxzrpbh2piknms37rw2ad.onion.ws/", //todo off
        "Arvin Leaks": "https://3kp6j22pz3zkv76yutctosa6djpj4yib2icvdqxucdaxxedumhqicpad.onion.ws/?feed=rss2",
        "Marketo": "https://marketo.cloud/?page={0}", //  "http://jvdamsif53dqjycuozlaye2s47p7xij4x6hzwzwhzrqmv36gkyzohhqd.onion.ws/",
        "n3tw0rm": "http://n3twormruynhn3oetmxvasum2miix2jgg56xskdoyihra4wthvlgyeyd.onion.ws/", //todo off
        "Pay2Key": "http://pay2key2zkg7arp3kv3cuugdaqwuesifnbofun4j6yjdw5ry7zw2asid.onion.ws/", //todo
        "REvil": "http://dnpscnbaix6nkwvystl3yxglz7nteicqrou3t75tpcc5532cztc46qyd.onion.ws/", //todo off
        "CL0P": "http://santat7kpllt6iyvqbr7q4amdv6dzrh6paatvyrzl7ry3zm72zigf4ad.onion.ws/",
        "Noname": "http://vfokxcdzjbpehgit223vzdzwte47l3zcqtafj34qrr26htjo4uf3obid.onion.ws/", //todo off
        "Payload.bin": "http://vbmisqjshn4yblehk2vbnil53tlqklxsdaztgphcilto3vdj4geao5qd.onion.ws/",
        "Mount Locker": "http://mountnewsokhwilx.onion.ws/", //todo off
        "RansomEXX": "http://rnsm777cdsjrsdlbs4v5qoeppu3px6sb2igmh53jzrx7ipcrbjz5b2ad.onion.ws/",
        "Vice Society": "http://vsociethok6sbprvevl4dlwbqrzyhxcxaqpvcqt5belwvsuxaxsutyad.onion.ws/",
        "eCh0raix": "http://veqlxhq7ub5qze3qy56zx2cig2e6tzsgxdspkubwbayqije6oatma6id.onion.ws/", //todo off
        "Qlocker": "http://gvka2m4qt5fod2fltkjmdk4gxh5oxemhpgmnmtjptms6fkgfzdd62tad.onion.ws/", //todo needs key
        "AvosLocker": "http://avosqxh72b5ia23dl5fgwcpndkctuzqvh2iefk5imp3pi5gfhel5klad.onion.ws/", //Captcha
        "Quantum Blog": "http://quantum445bh3gzuyilxdzs5xdepf3b7lkcupswvkryf3n7hgzpxebid.onion.ws/",
        "HARON Ransomware2": "http://midasbkic5eyfox4dhnijkzc7v7e4hpmsb2qgux7diqbpna4up4rtdad.onion.ws/blog.php",
        "LockData Auction": "http://wm6mbuzipviusuc42kcggzkdpbhuv45sn7olyamy6mcqqked3waslbqd.onion.ws/?page={0}",
        "BABUK": "http://nq4zyac4ukl4tykmidbzgdlvaboqeqsemkp4t35bzvjeve6zm2lqcjid.onion.ws/", //todo empty now
        "Atomsilo": "http://l5cjga2ksw6rxumu5l4xxn3cmahhi2irkbwg3amx6ajroyfmfgpfllid.onion.ws/list.html",
        "Bonaci Group": "http://bonacifryrxr4siz6ptvokuihdzmjzpveruklxumflz5thmkgauty2qd.onion.ws/", //todo
        "BlackByte": "http://f5uzduboq4fa2xkjloprmctk7ve3dm46ff7aniis66cbekakvksxgeqd.onion.ws/",
        "Karakurt": "https://karakurt.group/", //todo
        "MedusaLocker": "http://qd7pcafncosqfqu3ha6fcx4h6sr7tzwagzpcdcnytiw3b6varaeqv5yd.onion.ws/", // captcha
        "Entropy hall of fall": "http://leaksv7sroztl377bbohzl42i3ddlfsxopcb6355zc7olzigedm5agad.onion.ws/posts", //todo captcha
        "Snatch": "http://hl66646wtlp2naoqnhattngigjp5palgqmbwixepcjyq5i534acgqyad.onion.ws/index.php?filter=new", //"https://snatch.press/index.php?filter=new", http://snatch.press/
        "RobinHood": "https://robinhoodleaks.tumblr.com/", //todo off
        "Rook": "http://gamol6n6p2p4c3ad7gxmx3ur7wwdwlywebo2azv3vv5qlmjmole2zbyd.onion.ws/atom.xml",
        "54bb47h": "http://54bb47h.blog/feed/", //off
        "CRYP70N1C0D3": "http://7k4yyskpz3rxq5nyokf6ztbpywzbjtdfanweup3skctcxopmt7tq7eid.onion.ws/databases.html",
        "HolyGhost": "http://matmq3z3hiovia3voe2tix2x54sghc3tszj74xgdy4tqtypoycszqzqd.onion.ws/", //todo off
        "LOCKBIT 2.0": "https://lockbitaptc2iq4atewz2ise62q63wfktyrl4qtwuk5qax262kgtzjqd.onion.ws/",
        "Pandora Data Leak": "https://vbfqeh5nugm6r2u2qvghsdxm3fotf5wbxb5ltv6vw77vus5frdpuaiid.onion.ws/atom.xml",
        "AlphVM": "https://alphvmmm27o3abo3r2mlmjrpdmzle3rykajqc5xsj7j7ejksbpsa36ad.onion.ws/api/blog/all/0/{0}",
        "Moses Staff": "https://moses-staff.se/activities/",
        "DarkLeak Market": "http://darklmmmfuonklpy6s3tmvk5mrcdi7iapaw6eka45esmoryiiuug6aid.onion.ws/index.php?page={0}",
        "BlackShadow": "https://blackshadow.cc/",
        "Suncrypt": "https://x2miyuiwpib2imjr5ykyjngdu7v6vprkkhjltrk4qafymtawey4qzwid.onion.ws/",
        "Bl@ckt0r": "https://bl4cktorpms2gybrcyt52aakcxt6yn37byb65uama5cimhifcscnqkid.onion.ws", //todo
        "Night Sky": "https://gg5ryfgogainisskdvh4y373ap3b2mxafcibeh2lvq5x7fx76ygcosad.onion.ws"
      }
    }
