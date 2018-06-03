using System;
using System.Collections.Generic;
using System.Data;
using Silverwave.Core;
using Silverwave.HabboHotel.Items;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Web;
using System.IO;

namespace Silverwave.HabboHotel.Items
{
    class ItemManager
    {
        private Dictionary<UInt32, Item> Items;
        private WebClient furniDownloader;
        bool downloadFurni = false;
        private int currentFurniRev = 0;
        
        internal ItemManager()
        {
            Items = new Dictionary<uint, Item>();
        }

        internal void LoadItems(IQueryAdapter dbClient)
        {

            Items = new Dictionary<uint, Item>();

            dbClient.setQuery("SELECT * FROM furniture");
            DataTable ItemData = dbClient.getTable();

            if (ItemData != null)
            {
                uint id;
                int spriteID;
                string publicName;
                string itemName;
                string type;
                int width;
                int length;
                double height;
                bool allowStack;
                bool allowWalk;
                bool allowSit;
                bool allowRecycle;
                bool allowTrade;
                bool allowMarketplace;
                bool allowGift;
                bool allowInventoryStack;
                InteractionType interactionType;
                int cycleCount;
                string vendingIDS;
                string[] ToggleHeight = null;
                bool StackMultiplier;
                bool sub;
                int effect;
                int flatId;

                foreach (DataRow dRow in ItemData.Rows)
                {
                    try
                    {
                        id = Convert.ToUInt32(dRow[0]);
                        spriteID = (int)dRow[10];
                        publicName = (string)dRow[1];
                        itemName = (string)dRow[2];
                        type = (string)dRow[3].ToString();
                        width = (int)dRow[4];
                        length = (int)dRow[5];
                        if (dRow[6].ToString().Contains(";"))
                        {
                            ToggleHeight = dRow[6].ToString().Split(';');
                            height = Convert.ToDouble(ToggleHeight[0]);
                        }
                        else
                            // Replaced the "dRow[6]" with a 7 instead.
                            height = Convert.ToDouble(dRow[7]);

                        allowStack = Convert.ToInt32(dRow[7]) == 1;
                        allowWalk = Convert.ToInt32(dRow[9]) == 1;
                        allowSit = Convert.ToInt32(dRow[8]) == 1;
                        allowRecycle = Convert.ToInt32(dRow[11]) == 1;
                        allowTrade = Convert.ToInt32(dRow[12]) == 1;
                        allowMarketplace = Convert.ToInt32(dRow[13]) == 1;
                        allowGift = Convert.ToInt32(dRow[14]) == 1;
                        allowInventoryStack = Convert.ToInt32(dRow[15]) == 1;
                        interactionType = InterractionTypes.GetTypeFromString((string)dRow[16]);
                        cycleCount = (int)dRow[17];
                        vendingIDS = (string)dRow[18];
                        sub = SilverwaveEnvironment.EnumToBool(dRow[22].ToString());
                        effect = (int)dRow[23];
                        StackMultiplier = SilverwaveEnvironment.EnumToBool(dRow[21].ToString());
                        flatId = (int)dRow["flat_id"];
                        Item item = new Item(id, spriteID, publicName, itemName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, cycleCount, vendingIDS, sub, effect,StackMultiplier, ToggleHeight,flatId);
                        
                        Items.Add(id, item);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.ReadKey();
                        Logging.WriteLine("Could not load item #" + Convert.ToUInt32(dRow[0]) + ", please verify the data is okay.");
                    }
                }
                if (downloadFurni)
                    DownloadFurnis();
            }
        }


        private void DownloadFurnis()
        {
            
            XDocument xDoc = XDocument.Load(@"C:\myxml.xml");
            string fixedUrl = @"http://habboo-a.akamaihd.net/dcr/hof_furni/";
            string downloadUrl = "";
            string dynamicUrl = "";
            string furniName = "";
            string revision = "";
            string furniPublicName = "";
            string description = "";
            int flatId = -1;
            int spriteId = 0;
            int x = 0;
            int y = 0;
            string type = "s";
            string canstandon = "0";
            string cansiton = "0";
            string canlayon = "0";
            int specialType = 1;

            furniDownloader = new WebClient();
            furniDownloader.Encoding = System.Text.Encoding.UTF8;
            furniDownloader.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            using (IQueryAdapter dbClient = SilverwaveEnvironment.GetDatabaseManager().getQueryreactor())
            {

                var downloadRoomList = xDoc.Descendants("roomitemtypes").Descendants("furnitype");
                var downloadWallList = xDoc.Descendants("wallitemtypes").Descendants("furnitype");
                foreach (var downloadRoomItem in downloadRoomList)
                {
                    try
                    {
                        furniName = downloadRoomItem
                            .Attribute("classname")
                            .Value;
                        spriteId = Convert.ToInt32(downloadRoomItem
                            .Attribute("id")
                            .Value);
                        revision = downloadRoomItem
                            .Element("revision")
                            .Value;
                        flatId = Convert.ToInt32(downloadRoomItem
                            .Element("offerid")
                            .Value);
                        furniPublicName = downloadRoomItem
                            .Element("name")
                            .Value;
                        description = downloadRoomItem
                            .Element("description")
                            .Value;
                        specialType = Convert.ToInt32(downloadRoomItem
                            .Element("specialtype")
                            .Value);

                        x = Convert.ToInt32(downloadRoomItem
                            .Element("xdim")
                            .Value);
                        y = Convert.ToInt32(downloadRoomItem
                            .Element("ydim")
                            .Value);
                        type = "s";
                        canlayon = downloadRoomItem
                            .Element("canlayon")
                            .Value;
                        cansiton = downloadRoomItem
                            .Element("cansiton")
                            .Value;
                        canstandon = downloadRoomItem
                            .Element("canstandon")
                            .Value;
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    string[] split = furniName.Split('*');
                    downloadUrl = split[0] + ".swf";
                    dynamicUrl = fixedUrl + revision + "/" + downloadUrl;
                    Console.WriteLine(dynamicUrl);
                    try
                    {
                        //dbClient.runFastQuery("UPDATE furniture ");
                        dbClient.setQuery("INSERT INTO furniture" +
                            "(public_name,item_name,type,width,length,canlayon,can_sit,is_walkable,sprite_id,flat_id,revision,description,specialtype) VALUES " +
                            "(@pub_n,@real_n,@type,@x,@y,@lay,@sit,@walk,@sprite,@flat,@rev,@desc,@special) ON DUPLICATE KEY UPDATE " +
                            "public_name = VALUES(public_name)," +
                            "item_name = VALUES(item_name)," +
                            "type = VALUES(type)," +
                            "width = VALUES(width)," +
                            "length = VALUES(length)," +
                            "canlayon = VALUES(canlayon)," +
                            "can_sit = VALUES(can_sit)," +
                            "is_walkable = VALUES(is_walkable)," +
                            "sprite_id = VALUES(sprite_id)," +
                            "flat_id = VALUES(flat_id)," +
                            "revision = VALUES(revision)," +
                            "description = VALUES(description)," +
                            "specialtype = VALUES(specialtype); " +
                            "UPDATE catalog_items SET flat_id = @flat WHERE catalog_name = @real_n");
                        dbClient.addParameter("pub_n", furniPublicName);
                        dbClient.addParameter("real_n", furniName);
                        dbClient.addParameter("type", type);
                        dbClient.addParameter("x", x);
                        dbClient.addParameter("y", y);
                        dbClient.addParameter("lay", canlayon);
                        dbClient.addParameter("sit", cansiton);
                        dbClient.addParameter("walk", canstandon);
                        dbClient.addParameter("sprite", spriteId);
                        dbClient.addParameter("flat", flatId);
                        dbClient.addParameter("rev", revision);
                        dbClient.addParameter("desc", description);
                        dbClient.addParameter("special", specialType);
                        dbClient.runQuery();

                        if (!Directory.Exists(@"C:\hof_furni\" + revision + @"\")) Directory.CreateDirectory(@"C:\hof_furni\" + revision + @"\");
                        furniDownloader.DownloadFileCompleted += (sender, e) => Console.WriteLine("Finished " + furniName);
                        furniDownloader.DownloadFile(new Uri(dynamicUrl), @"C:\hof_furni\" + revision + @"\" + downloadUrl);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                foreach (var downloadWallItem in downloadWallList)
                {
                    furniName = downloadWallItem
                    .Attribute("classname")
                    .Value;
                    spriteId = Convert.ToInt32(downloadWallItem
                        .Attribute("id")
                        .Value);
                    revision = downloadWallItem
                        .Element("revision")
                        .Value;
                    flatId = Convert.ToInt32(downloadWallItem
                        .Element("offerid")
                        .Value);
                    furniPublicName = downloadWallItem
                        .Element("name")
                        .Value;
                    description = downloadWallItem
                        .Element("description")
                        .Value;
                    specialType = Convert.ToInt32(downloadWallItem
                        .Element("specialtype")
                        .Value);

                    type = "i";
                    canlayon = "0";
                    cansiton = "0";
                    canstandon = "0";
                    x = 0;
                    y = 0;


                    string[] split = furniName.Split('*');
                    downloadUrl = split[0] + ".swf";
                    dynamicUrl = fixedUrl + revision + "/" + downloadUrl;
                    Console.WriteLine(dynamicUrl);
                    try
                    {
                        //dbClient.runFastQuery("UPDATE furniture ");
                        dbClient.setQuery("INSERT INTO furniture" +
                            "(public_name,item_name,type,width,length,canlayon,can_sit,is_walkable,sprite_id,flat_id,revision,description,specialtype) VALUES " +
                            "(@pub_n,@real_n,@type,@x,@y,@lay,@sit,@walk,@sprite,@flat,@rev,@desc,@special) ON DUPLICATE KEY UPDATE " +
                            "public_name = VALUES(public_name)," +
                            "item_name = VALUES(item_name)," +
                            "type = VALUES(type)," +
                            "width = VALUES(width)," +
                            "length = VALUES(length)," +
                            "canlayon = VALUES(canlayon)," +
                            "can_sit = VALUES(can_sit)," +
                            "is_walkable = VALUES(is_walkable)," +
                            "sprite_id = VALUES(sprite_id)," +
                            "flat_id = VALUES(flat_id)," +
                            "revision = VALUES(revision)," +
                            "description = VALUES(description)," +
                            "specialtype = VALUES(specialtype); " +
                            "UPDATE catalog_items SET flat_id = @flat WHERE catalog_name = @real_n");
                        dbClient.addParameter("pub_n", furniPublicName);
                        dbClient.addParameter("real_n", furniName);
                        dbClient.addParameter("type", type);
                        dbClient.addParameter("x", x);
                        dbClient.addParameter("y", y);
                        dbClient.addParameter("lay", canlayon);
                        dbClient.addParameter("sit", cansiton);
                        dbClient.addParameter("walk", canstandon);
                        dbClient.addParameter("sprite", spriteId);
                        dbClient.addParameter("flat", flatId);
                        dbClient.addParameter("rev", revision);
                        dbClient.addParameter("desc", description);
                        dbClient.addParameter("special", specialType);
                        dbClient.runQuery();

                        if (!Directory.Exists(@"C:\hof_furni\" + revision + @"\")) Directory.CreateDirectory(@"C:\hof_furni\" + revision + @"\");
                        furniDownloader.DownloadFileCompleted += (sender, e) => Console.WriteLine("Finished " + furniName);
                        furniDownloader.DownloadFile(new Uri(dynamicUrl), @"C:\hof_furni\" + revision + @"\" + downloadUrl);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
        private int CurrentRevision
        {
            get
            {
                return this.currentFurniRev;
            }
            set
            {
                this.currentFurniRev = value;
            }
        }
        internal Boolean ContainsItem(uint Id)
        {
            return Items.ContainsKey(Id);
        }

        internal Item GetItem(uint Id)
        {
            if (ContainsItem(Id))
                return Items[Id];

            return null;
        }
    }
}
