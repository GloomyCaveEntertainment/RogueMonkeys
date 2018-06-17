/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using SunCubeStudio.Localization;

public class DataMgr : MonoBehaviour {


    #region Public Data
    public enum SHOP_ITEM_TYPE { EMPTY = 0, GOLD_ITEM, EQUIPMENT }
    public enum BOX_QUALITY { S = 0, M, L, XL }

    [System.Serializable]
    public class QualityDistribution
    {
        public QualityDistribution(BOX_QUALITY bQ, float r, int value, List<ItemSizePair> iL)
        {
            Quality = bQ;
            Ratio = r;
            Value = value;
            ItemList = iL;
        }
        public BOX_QUALITY Quality;
        public float Ratio; // c [0,1]
        public int Value;
        public List<ItemSizePair> ItemList;
    }

    [System.Serializable]
    public class ItemSizePair
    {
        public ItemSizePair(string id, float p)
        {
            ItemId = id;
            Prob = p;
        }
        public string ItemId;
        public float Prob;
    }

    [System.Serializable]
    public class ShopItemTableEntry
    {
        //Ctor
        public ShopItemTableEntry(int bStage, int bLvl, List<QualityDistribution> boxDistr/*, List<ItemSizePair> itemP*/)
        {
            BreakStage = bStage;
            BreakLvl = bLvl;
            BoxQualityDistr = boxDistr;
            //ItemSizePairList = itemP;
        }

        public int BreakStage;
        public int BreakLvl;        //determines max lvl restriction for this entry
        public List<QualityDistribution> BoxQualityDistr;
        //public List<ItemSizePair> ItemSizePairList;
    }


    public static DataMgr Instance;
    #endregion


    #region Behaviour Methods
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadData();
    }
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            
        }
    }*/

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveData();
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    public void LoadItemData()
    {
        string slotTypeS, modTypeS;
        string auxId, auxSpriteId;
        int auxValue = -1, auxCount = -1;
        GoldItem auxGoldItm = null;
        EquipmentItem auxEquipItm = null;
        List<EquipmentItem.MOD_TYPE> auxMtList = new List<EquipmentItem.MOD_TYPE>();
        List<float> auxModValList = new List<float>();
        EquipmentItem.SLOT_TYPE auxSt = EquipmentItem.SLOT_TYPE.COLLECTOR_A;

        XmlReader reader = XmlReader.Create(new StringReader(_itemDataTA.text));
        //XmlDocument doc = new XmlDocument();
        // doc.LoadXml(_textDoc.text);

        if (_gameItemList == null)
            _gameItemList = new List<EquipmentItem>();
        else
            _gameItemList.Clear();

        if (_goldItemList == null)
            _goldItemList = new List<GoldItem>();
        else
            _goldItemList.Clear();

        _readingGoldItem = false;
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name.CompareTo("gold_item_type") == 0)
                        _readingGoldItem = true;
                    else if (reader.Name.CompareTo("equipment_item_data") == 0)
                        _readingGoldItem = false;
                    else
                    {
                        //Debug.Log("Value: " + reader.Name);
                        if (reader.Name.CompareTo("item") == 0)
                        {
                            if (_readingGoldItem)
                            {
                                auxSpriteId = reader.GetAttribute("spriteId");
                                //auxGoldItm = new GoldItem(reader.GetAttribute("id"), auxSpriteId, XmlConvert.ToInt32(reader.GetAttribute("value")));
                                auxGoldItm = new GoldItem(reader.GetAttribute("id"), auxSpriteId, XmlConvert.ToInt32(reader.GetAttribute("value")));
                                auxGoldItm._Sprite = Resources.Load(auxSpriteId, typeof(Sprite)) as Sprite;
                                _goldItemList.Add(auxGoldItm/*auxGoldItm*/);
                            }
                            else
                            {
                                auxId = reader.GetAttribute("id");
                                auxSpriteId = reader.GetAttribute("spriteId");
                                auxValue = XmlConvert.ToInt32(reader.GetAttribute("value"));
                                Debug.Log("Adding item.... "+ auxId);
                                //auxCount = XmlConvert.ToInt32(reader.GetAttribute("count"));

                                slotTypeS = reader.GetAttribute("slotType");
                                if (slotTypeS.CompareTo("COLLECTOR_A") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.COLLECTOR_A;
                                else if (slotTypeS.CompareTo("COLLECTOR_B") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.COLLECTOR_B;
                                else if (slotTypeS.CompareTo("STRIKER_A") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.STRIKER_A;
                                else if (slotTypeS.CompareTo("STRIKER_B") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.STRIKER_B;
                                else if (slotTypeS.CompareTo("SHAKER_A") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.SHAKER_A;
                                else if (slotTypeS.CompareTo("SHAKER_B") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.SHAKER_B;
                                else
                                    Debug.LogError("Error parsing slotType " + auxSt);

                                //Read mod list
                                auxMtList = new List<EquipmentItem.MOD_TYPE>();
                                auxModValList = new List<float>();
                                while (reader.Read())
                                {
                                    if (reader.IsStartElement() && reader.Name.CompareTo("mod") == 0)
                                    {
                                        modTypeS = reader.GetAttribute("modType");
                                        //Debug.Log("R E A D I N G: " + modTypeS);
                                        if (modTypeS.CompareTo("RELOAD_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.RELOAD_SPEED);
                                        else if (modTypeS.CompareTo("SACK_SIZE") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.SACK_SIZE);
                                        else if (modTypeS.CompareTo("COLLECTOR_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.COLLECTOR_SPEED);
                                        else if (modTypeS.CompareTo("STRIKER_HIT_SIZE") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE);
                                        else if (modTypeS.CompareTo("STRIKER_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.STRIKER_SPEED);
                                        else if (modTypeS.CompareTo("ACCURACY") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.ACURRACY);
                                        else if (modTypeS.CompareTo("FALL_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.FALL_SPEED);
                                        else if (modTypeS.CompareTo("GOLD_FIND_PROB") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.GOLD_FIND_PROB);
                                        else if (modTypeS.CompareTo("ITEM_FIND_PROB") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.ITEM_FIND_PROB);
                                        else
                                            Debug.LogError("Error parsing slotType " + modTypeS);

                                        auxModValList.Add(float.Parse(reader.GetAttribute("modVal")));

                                    }
                                    else if (!reader.IsStartElement() && reader.Name.CompareTo("item") == 0)
                                    {
                                        //Debug.Log("Item read ended, break reading mod List");
                                        break;
                                    }
                                }


                                auxEquipItm = new EquipmentItem(auxId, auxSpriteId, auxValue, auxSt, auxMtList, auxModValList/*,auxCount*/);
                                //auxSt, auxMt, float.Parse(reader.GetAttribute("modVal")), 0);
                                //Load Sprite reference
                                auxEquipItm._Sprite = Resources.Load(auxSpriteId, typeof(Sprite)) as Sprite;
                                _gameItemList.Add(auxEquipItm);
                            }
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    Debug.Log("End element Value: " + reader.Name);
                    break;
            }
        }
        Debug.Log("Current itemlist count:" + _gameItemList.Count);
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadShopData()
    {
        XmlReader reader = XmlReader.Create(new StringReader(_shopDataTA.text));
        int tempLvl = -1;
        int tempStage = -1;
        string qualityS;
        float tempBoxProb = 1f;
        int tempBoxVal = -1;
        BOX_QUALITY bQuality = BOX_QUALITY.S;

        //list init
        if (_tempDistr == null)
            _tempDistr = new List<QualityDistribution>();
        else
            _tempDistr.Clear();

        if (_tempSizePait == null)
            _tempSizePait = new List<ItemSizePair>();
        else
            _tempSizePait.Clear();

        if (_shopItemList == null)
            _shopItemList = new List<ShopItemTableEntry>();
        else
            _shopItemList.Clear();


        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name.CompareTo("stage_data") == 0)
                    {
                        tempStage = XmlConvert.ToInt32(reader.GetAttribute("val"));
                    }
                    else if (reader.Name.CompareTo("data_entry") == 0)
                    {
                        tempLvl = XmlConvert.ToInt32(reader.GetAttribute("breakLvl"));
                    }
                    else if (reader.Name.CompareTo("box_prob") == 0)
                    {
                        tempBoxProb = float.Parse(reader.GetAttribute("prob"));
                        qualityS = reader.GetAttribute("type");
                        tempBoxVal = XmlConvert.ToInt32(reader.GetAttribute("val"));
                        if (qualityS.CompareTo("S") == 0)
                            bQuality = BOX_QUALITY.S;
                        else if (qualityS.CompareTo("M") == 0)
                            bQuality = BOX_QUALITY.M;
                        else if (qualityS.CompareTo("L") == 0)
                            bQuality = BOX_QUALITY.L;
                        else if (qualityS.CompareTo("XL") == 0)
                            bQuality = BOX_QUALITY.XL;
                        else
                            Debug.LogError("Error reading quality value");
                        //_tempDistr.Add(new QualityDistribution(bQuality, float.Parse(reader.GetAttribute("prob"))));
                    }
                    else if (reader.Name.CompareTo("item") == 0)
                    {
                        //while (reader.Read() && reader.IsStartElement()/*reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Whitespace*/)
                        //{
                        _tempSizePait.Add(new ItemSizePair(reader.GetAttribute("id"), float.Parse(reader.GetAttribute("prob"))));
                        //}
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name.CompareTo("data_entry") == 0)
                    {
                        //Debug.Log("Adding lists..... " + _tempDistr.Count + " / " + _tempSizePait.Count);
                        //Add Entry tp Table and clear temp vars
                        _shopItemList.Add(new ShopItemTableEntry(tempStage, tempLvl, _tempDistr));
                        _tempDistr = new List<QualityDistribution>();

                    }
                    else if (reader.Name.CompareTo("item_pool") == 0)
                    {
                        Debug.Log("Adding box item list..... " + bQuality + "/" + tempBoxProb + " / " + _tempSizePait.Count);
                        _tempDistr.Add(new QualityDistribution(bQuality, tempBoxProb, tempBoxVal, _tempSizePait));
                        _tempSizePait = new List<ItemSizePair>();
                    }
                    else
                        Debug.Log("End element:    " + reader.Name);

                    break;
            }
        }
        Debug.Log("Shop items loaded: " + _shopItemList.Count);
        for (int i = 0; i < _shopItemList.Count; ++i)
        {
            Debug.Log("entry" + i + " / " + _shopItemList[i].BreakStage + " / " + _shopItemList[i].BreakLvl + " item count first box" + _shopItemList[i].BoxQualityDistr[0].ItemList.Count);
        }
    }

    /*
    public void CreateInventoryItemsFile()
    {
        XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _dataRelativePath);

        writer.WriteStartDocument();
        //writer.WriteStartElement("item_data");
        //writer.WriteEndElement();
        writer.Close();

    }*/

    /// <summary>
    /// 
    /// </summary>
    public void LoadInventoryItems()
    {
        XmlReader reader = null;
       // Debug.Log("Path is: " + Application.persistentDataPath + _invFileName);
        if (File.Exists(Path.Combine(Application.persistentDataPath,_invFileName)))
        {
            reader = XmlReader.Create(Path.Combine(Application.persistentDataPath,_invFileName));// XmlReader.Create(new StringReader(_inventoryDataTA.text));
        }
        else
        {
            /*private void createSaveData()
            {
            TextAsset textAsset = Resources.Load("Saved_Data/SavedData") as TextAsset;
            XmlDocument xmldoc = new XmlDocument ();
            xmldoc.LoadXml(textAsset.text);
            xmldoc.Save(Application.persistentDataPath + "\SavedData.xml");
            }*/
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(_inventoryDataTA.text);
            xmldoc.Save(Path.Combine(Application.persistentDataPath,_invFileName));
            reader = XmlReader.Create(Path.Combine(Application.persistentDataPath ,_invFileName));
            /*
            XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _invFileName);

            writer.WriteStartDocument();
            writer.WriteStartElement("item_data");
            writer.WriteEndElement();
            writer.Close();
            reader = XmlReader.Create(Application.persistentDataPath + _invFileName);
             * */
        }

        //if no items saved, create empty file
        /*if (reader == null)
        {
            XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _dataRelativePath);

            writer.WriteStartDocument();
            writer.Close();
        }*/
        EquipmentItem aux;
        if (_inventoryItems == null)
            _inventoryItems = new List<EquipmentItem>();
        else
            _inventoryItems.Clear();

        while (reader != null && reader.Read() && reader.IsStartElement())
        {
            //switch (reader.NodeType)
            //{
            //  case XmlNodeType.Element:
            if (reader.Name.CompareTo("item") == 0)
            {
                Debug.Log("itm: " + reader.GetAttribute("id"));
                aux = _gameItemList.Find((itm) => (itm.IdName == reader.GetAttribute("id"))) as EquipmentItem;
                //Debug.Log("inventory item found on data" + aux.IdName);
                aux.Equipped = reader.GetAttribute("equipped").CompareTo("True") == 0;
                aux.New = reader.GetAttribute("new").CompareTo("True") == 0;
                aux.Count = XmlConvert.ToInt32(reader.GetAttribute("count"));
                _inventoryItems.Add(aux);
            }
            //      break;
            //}
        }
        reader.Close();
        Debug.Log("Loaded " + _inventoryItems.Count + " inventory items");

    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveInventoryItems()
    {
        Debug.Log("_________Saving inventory items");
        XmlWriter writer = XmlWriter.Create(Path.Combine(Application.persistentDataPath,_invFileName));

        writer.WriteStartDocument();
        writer.WriteStartElement("item_data");
        for (int i = 0; i < _inventoryItems.Count; ++i)
        {
            writer.WriteStartElement("item");
            writer.WriteAttributeString("id", _inventoryItems[i].IdName);
            writer.WriteAttributeString("equipped", _inventoryItems[i].Equipped.ToString());
            writer.WriteAttributeString("new", _inventoryItems[i].New.ToString());
            writer.WriteAttributeString("count", _inventoryItems[i].Count.ToString());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Close();
    }


    /// <summary>
    /// 
    /// </summary>
    public void LoadLevelData()
    {
        Debug.Log("_________________Load level data");
        XmlReader reader = null;
        if (File.Exists(Path.Combine(Application.persistentDataPath,_levelFileName)))
        {
            reader = XmlReader.Create(Path.Combine(Application.persistentDataPath,_levelFileName));// XmlReader.Create(new StringReader(_inventoryDataTA.text));
        }
        else
        {
            Debug.Log("Creating new coz no file exists");
            /////
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(_levelDataTA.text);
            xmldoc.Save(Path.Combine(Application.persistentDataPath,_levelFileName));
            reader = XmlReader.Create(Path.Combine(Application.persistentDataPath,_levelFileName));
            ////
            /*XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _levelFileName);

            writer.WriteStartDocument();
            writer.WriteStartElement("level_data");
            writer.WriteEndElement();
            writer.Close();
            reader = XmlReader.Create(Application.persistentDataPath + _levelFileName);*/
        }
        int stageIndex = -1;
        string auxSt = "";
        //if no items saved, create empty file
        /*if (reader == null)
        {
            XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _dataRelativePath);

            writer.WriteStartDocument();
            writer.Close();
        }*/


        while (reader != null && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name.CompareTo("stage") == 0)
                    {
                        stageIndex = XmlConvert.ToInt32(reader.GetAttribute("id"));
                        auxSt = reader.GetAttribute("state");
                        if (auxSt.CompareTo("completed") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.COMPLETED;
                        else if (auxSt.CompareTo("unlocked") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.UNLOCKED;
                        else if (auxSt.CompareTo("locked") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.LOCKED;
                        else if (auxSt.CompareTo("unavailable") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.UNAVAILABLE;
                        else
                            Debug.LogError("wrong level state data" + reader.Name);

                        while (reader != null && reader.Read())
                        {
                            if (reader.IsStartElement() && reader.Name.CompareTo("level") == 0)
                            {
                                //Debug.Log("Loading lvl " + stageIndex + " / " + reader.GetAttribute("id"));
                                //Score
                                GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].MaxScore = XmlConvert.ToInt32((reader.GetAttribute("score")));
                                //State
                                auxSt = reader.GetAttribute("state");
                                if (auxSt.CompareTo("failed") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.FAILED;
                                else if (auxSt.CompareTo("completed") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
                                else if (auxSt.CompareTo("unlocked") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.UNLOCKED;
                                else if (auxSt.CompareTo("locked") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.LOCKED;
                                else if (auxSt.CompareTo("unavailable") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.UNAVAILABLE;
                                else
                                    Debug.LogError("wrong level state data" + reader.Name);

                                //Rank
                                auxSt = reader.GetAttribute("rank");
                                if (auxSt.CompareTo("S") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.S;
                                else if (auxSt.CompareTo("A") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.A;
                                else if (auxSt.CompareTo("B") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.B;
                                else if (auxSt.CompareTo("C") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.C;
                                else if (auxSt.CompareTo("D") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.D;
                                else if (auxSt.CompareTo("E") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.E;
                                else if (auxSt.CompareTo("F") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.F;
                                else
                                    Debug.LogError("wrong level rank data" + reader.Name);
                            }
                            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.CompareTo("stage") == 0)
                                break;
                        }
                    }
                    break;
            }
        }
        reader.Close();
    }

    /// <summary>
    /// 
    /// </summary>
    /*public void SaveLevelData()
    {
        XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _levelFileName);
        Level.AVAILABILITY_STATE auxSt;

        writer.WriteStartDocument();
        writer.WriteStartElement("level_data");
        for (int i = 0; i < GameMgr.Instance._StageList.Count; ++i)
        {
            writer.WriteStartElement("stage");
            writer.WriteAttributeString("id", i.ToString());
            for (int j = 0; i < GameMgr.Instance._StageList[i].GetLevelList().Count; ++j)
            {
                writer.WriteStartElement("level");
                writer.WriteAttributeString("id", j.ToString());
                writer.WriteAttributeString("score", GameMgr.Instance._StageList[i].GetLevelList()[j].GetMaxScore().ToString());
                auxSt = GameMgr.Instance._StageList[i].GetLevelList()[j].GetState();
                switch (auxSt)
                {
                    case Level.AVAILABILITY_STATE.COMPLETED:
                        writer.WriteAttributeString("state", "completed");
                        break;

                    case Level.AVAILABILITY_STATE.UNLOCKED:
                        writer.WriteAttributeString("state", "unlocked");
                        break;

                    case Level.AVAILABILITY_STATE.LOCKED:
                        writer.WriteAttributeString("state", "locked");
                        break;

                    case Level.AVAILABILITY_STATE.UNAVAILABLE:
                        writer.WriteAttributeString("state", "unavailable");
                        break;
                }
                writer.WriteEndElement(); //</level>
            }
        }
        writer.WriteEndElement(); 
    }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stageIndex"></param>
    /// <param name="lvlIndex"></param>
    public void SaveLevelData(int stageIndex, int lvlIndex)
    {
        //Debug.Log("Save level data________" + stageIndex + " / " + lvlIndex);
        XmlDocument doc = new XmlDocument();
        doc.Load(Path.Combine(Application.persistentDataPath,_levelFileName));

        XmlNode root = doc.DocumentElement;
        //Debug.Log("R00t: " + root.Name);
        //XmlElement levelDataNode = (XmlElement)root.SelectSingleNode("level_data");
        //Debug.Log("Leveldatanode: " + levelDataNode);
        //Current stage
        XmlNodeList stages = ((XmlElement)root).GetElementsByTagName("stage");
        //Debug.Log("stages: " + stages);

        //stage data
        foreach (XmlAttribute attr in stages[stageIndex].Attributes)
        {
            if (attr.Name.Equals("state"))
            {
                switch (GameMgr.Instance._StageList[stageIndex].GetAvState())
                {
                    case Stage.STAGE_STATE.COMPLETED:
                        attr.Value = "completed";
                        break;

                    case Stage.STAGE_STATE.LOCKED:
                        attr.Value = "locked";
                        break;

                    case Stage.STAGE_STATE.UNAVAILABLE:
                        attr.Value = "unavailable";
                        break;

                    case Stage.STAGE_STATE.UNLOCKED:
                        attr.Value = "unlocked";
                        break;
                }
            }
        }
                           
        //go to stage-level and rewrite attribute data
        foreach (XmlAttribute attr in stages[stageIndex].ChildNodes[lvlIndex].Attributes)
        {
            Debug.Log("Score &&& State attr " + attr.Name);
            //Score
            if (attr.Name.Equals("score"))
            {
                Debug.Log("push score");
                attr.Value = GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].MaxScore.ToString();
            }
            //State
            else if (attr.Name.Equals("state"))
            {
                if (GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].GetAvState() == Level.AVAILABILITY_STATE.FAILED)
                    attr.Value = "failed";
                else
                    attr.Value = "completed";

            }
            //Rank
            else if (attr.Name.Equals("rank"))
            {
                switch (GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].Rank)
                {
                    case Level.RANK.S:
                        attr.Value = "S";
                        break;
                    case Level.RANK.A:
                        attr.Value = "A";
                        break;
                    case Level.RANK.B:
                        attr.Value = "B";
                        break;
                    case Level.RANK.C:
                        attr.Value = "C";
                        break;
                    case Level.RANK.D:
                        attr.Value = "D";
                        break;
                    case Level.RANK.E:
                        attr.Value = "E";
                        break;
                    case Level.RANK.F:
                        attr.Value = "F";
                        break;
                }
            }
        }
        //unlock next level if level succeeded
        if (GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].AvailabilitySt != Level.AVAILABILITY_STATE.FAILED 
            && lvlIndex < GameMgr.Instance._StageList[stageIndex].GetLevelList().Count - 1 && GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex+1].AvailabilitySt == Level.AVAILABILITY_STATE.LOCKED)
        {
            //go to stage-level and rewrite attribute data
            foreach (XmlAttribute attr in stages[stageIndex].ChildNodes[lvlIndex + 1].Attributes)
            {
                if (attr.Name.Equals("state"))
                    attr.Value = "unlocked";
            }
            GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex + 1].AvailabilitySt = Level.AVAILABILITY_STATE.UNLOCKED;
            GameMgr.Instance.LevelUnlocked = true;
        }
        //stage completed if last level completed
        else if (GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].AvailabilitySt != Level.AVAILABILITY_STATE.FAILED && lvlIndex == GameMgr.Instance._StageList[stageIndex].GetLevelList().Count - 1 && stageIndex < stages.Count - 1)
        {
            GameMgr.Instance.LevelUnlocked = true;
            //unlock next stage
            foreach (XmlAttribute attr in stages[stageIndex + 1].Attributes)
            {
                if (attr.Name.Equals("state"))
                    attr.Value = "unlocked";
            }
            GameMgr.Instance._StageList[stageIndex + 1].State = Stage.STAGE_STATE.UNLOCKED;
            //lock next stage levels but first which is unlocked - FILE
            for (int i = 0; i < stages[stageIndex + 1].ChildNodes.Count; ++i)
            {
                foreach (XmlAttribute attr in stages[stageIndex + 1].ChildNodes[i].Attributes)
                {
                    if (attr.Name.Equals("state"))
                    {
                        if (i == 0)
                            attr.Value = "unlocked";
                        else
                            attr.Value = "locked";
                    }
                }
            }
            //lock next stage levels but first which is unlocked - DATA
            for (int i = 0; i < GameMgr.Instance._StageList[stageIndex + 1].GetLevelList().Count; ++i)
            {
                if (i == 0)
                    GameMgr.Instance._StageList[stageIndex + 1].GetLevelList()[i].AvailabilitySt = Level.AVAILABILITY_STATE.UNLOCKED;
                else
                    GameMgr.Instance._StageList[stageIndex + 1].GetLevelList()[i].AvailabilitySt = Level.AVAILABILITY_STATE.LOCKED;

            }
            //switch stage state from unavailable to locked. 
            if (stageIndex < stages.Count - 2)
            {
                //FILE
                foreach (XmlAttribute attr in stages[stageIndex + 2].Attributes)
                {
                    if (attr.Name.Equals("state"))
                        attr.Value = "locked";
                }
                //DATA
                GameMgr.Instance._StageList[stageIndex + 2].State = Stage.STAGE_STATE.LOCKED;

                //set stage+2 levels from unavailable to locked
                for (int i = 0; i < stages[stageIndex + 2].ChildNodes.Count; ++i)
                {
                    foreach (XmlAttribute attr in stages[stageIndex + 2].ChildNodes[i].Attributes)
                    {
                        if (attr.Name.Equals("state"))
                            attr.Value = "locked";
                        
                    }
                }
                foreach (Level lvl in GameMgr.Instance._StageList[stageIndex + 2].GetLevelList())
                    lvl.AvailabilitySt = Level.AVAILABILITY_STATE.LOCKED;
            }


        }
        doc.Save(Path.Combine(Application.persistentDataPath,_levelFileName));

        /*foreach (XmlElement stage in levelDataNode)
        {
            if  (XmlConvert.ToInt32(stage.GetAttribute("id")) == stageIndex)// == GameMgr.Instance.CurrentPlayer.Id)
            {
                for (int i= 0; i< stage.ChildNodes.Count;++i)
                //foreach (XmlNode level in stage)
                {
                    if  (XmlConvert.ToInt32(stage.geGetAttribute("id")) == lvlIndex)// == GameMgr.Instance.CurrentPlayer.Id)
                    {
                        foreach (XmlAttribute attr in level.Attributes)
                        {
                            if (attr.Name.Equals("DifficultyReached"))
                            {
         
                                
                               
                                

                                break;
                            }
                        }
                        break;
                    }
                }
                break;
            
        }*/
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveGold()
    {
        Debug.Log("Saving gold__________");
        PlayerPrefs.SetInt("Gold", GameMgr.Instance.Gold);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveData()
    {
        PlayerPrefs.SetInt("Gold", GameMgr.Instance.Gold);
        PlayerPrefs.SetString("Lang", LocalizationService.Instance.Localization);
        PlayerPrefs.SetInt("Vib", _vibration);
        PlayerPrefs.SetInt("RstShp", GameMgr.Instance.ResetShop ? 1 : 0);
        PlayerPrefs.SetInt("Stg", GameMgr.Instance.StageIndex);
        PlayerPrefs.SetInt("Lvl", GameMgr.Instance.LevelIndex);
        //TODO: review if saving inventory items is needed
        SaveInventoryItems();
        //SaveLevelData(GameMgr.Instance.StageIndex, GameMgr.Instance.LevelIndex);  //level data should be already saved
        //TODO: levels score data
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadData()
    {
        GameMgr.Instance.Gold = PlayerPrefs.GetInt("Gold");
        Debug.Log("Loading language..." + PlayerPrefs.GetString("Lang"));
        //LocalizationService.Instance.Localization = PlayerPrefs.GetString("Lang");
        if (PlayerPrefs.GetString("Lang") == "")
            LocalizationService.Instance.Localization = "English";
        else
            LocalizationService.Instance.Localization = PlayerPrefs.GetString("Lang");

        _vibration = PlayerPrefs.GetInt("Vib");
        GameMgr.Instance.ResetShop = PlayerPrefs.GetInt("RstShp") != 0;
        GameMgr.Instance.StageIndex = PlayerPrefs.GetInt("Stg");
        GameMgr.Instance.LevelIndex = PlayerPrefs.GetInt("Lvl");
        LoadItemData();
        LoadInventoryItems();
        LoadLevelData();
        LoadShopData();
        LoadShopState();
    }

    /// <summary>
    /// Last visited shop state
    /// </summary>
    public void LoadShopState()
    {
        _shopBoxesItemList = new List<string>();
        _shopBoxesItemList.Add(PlayerPrefs.GetString("box_0"));
        _shopBoxesItemList.Add(PlayerPrefs.GetString("box_1"));
        _shopBoxesItemList.Add(PlayerPrefs.GetString("box_2"));

        _shopBoxesSizeList = new List<int>();
        _shopBoxesSizeList.Add(PlayerPrefs.GetInt("bsize_0"));
        _shopBoxesSizeList.Add(PlayerPrefs.GetInt("bsize_1"));
        _shopBoxesSizeList.Add(PlayerPrefs.GetInt("bsize_2"));

        _shopBoxesPriceList = new List<int>();
        _shopBoxesPriceList.Add(PlayerPrefs.GetInt("bprice_0"));
        _shopBoxesPriceList.Add(PlayerPrefs.GetInt("bprice_1"));
        _shopBoxesPriceList.Add(PlayerPrefs.GetInt("bprice_2"));

        _shopBoxesInteractableList = new List<int>();
        _shopBoxesInteractableList.Add(PlayerPrefs.GetInt("binteractable_0"));
        _shopBoxesInteractableList.Add(PlayerPrefs.GetInt("binteractable_1"));
        _shopBoxesInteractableList.Add(PlayerPrefs.GetInt("binteractable_2"));
    }
    /// <summary>
    /// 
    /// </summary>
    public void SaveShopState()
    {
        PlayerPrefs.SetString("box_0", _shopBoxesItemList[0]);
        PlayerPrefs.SetString("box_1", _shopBoxesItemList[1]);
        PlayerPrefs.SetString("box_2", _shopBoxesItemList[2]);

        PlayerPrefs.SetInt("bsize_0", _shopBoxesSizeList[0]);
        PlayerPrefs.SetInt("bsize_1", _shopBoxesSizeList[1]);
        PlayerPrefs.SetInt("bsize_2", _shopBoxesSizeList[2]);

        PlayerPrefs.SetInt("bprice_0", _shopBoxesPriceList[0]);
        PlayerPrefs.SetInt("bprice_1", _shopBoxesPriceList[1]);
        PlayerPrefs.SetInt("bprice_2", _shopBoxesPriceList[2]);

        PlayerPrefs.SetInt("binteractable_0", _shopBoxesInteractableList[0]);
        PlayerPrefs.SetInt("binteractable_1", _shopBoxesInteractableList[1]);
        PlayerPrefs.SetInt("binteractable_2", _shopBoxesInteractableList[2]);
    }
    /// <summary>
    /// 
    /// </summary>
    public void RemoveData()
    {
        Debug.Log("Delete");
        if (File.Exists(Path.Combine(Application.persistentDataPath,_invFileName)))
            File.Delete(Path.Combine(Application.persistentDataPath,_invFileName));
        if (File.Exists(Path.Combine(Application.persistentDataPath,_levelFileName)))
            File.Delete(Path.Combine(Application.persistentDataPath,_levelFileName));
    }

    /// <summary>
    /// 
    /// </summary>
    /*public void UnlockLevelData()
    {
        for (int i = 0; i < GameMgr.Instance._StageList.Count; ++i)
        {
            if (GameMgr.Instance._StageList[i].State == Stage.STAGE_STATE.LOCKED || GameMgr.Instance._StageList[i].State == Stage.STAGE_STATE.UNAVAILABLE)
                GameMgr.Instance._StageList[i].State = Stage.STAGE_STATE.UNLOCKED;

            for (int j = 0; j < GameMgr.Instance._StageList[i].GetLevelList().Count; ++j)
            {
                if (GameMgr.Instance._StageList[i].GetLevelList()[j].AvailabilitySt == Level.AVAILABILITY_STATE.UNAVAILABLE || GameMgr.Instance._StageList[i].GetLevelList()[j].AvailabilitySt == Level.AVAILABILITY_STATE.LOCKED)
                    GameMgr.Instance._StageList[i].GetLevelList()[j].AvailabilitySt = Level.AVAILABILITY_STATE.UNLOCKED;
                SaveLevelData(i, j);
            }

        }

    }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stage"></param>
    /// <param name="lvl"></param>
    /// <returns></returns>
    public List<ShopItemTableEntry> GetShopItemList()
    {
        return _shopItemList;

    }

    public List<EquipmentItem> GetGameItems()
    {
        return _gameItemList;
    }
    public List<GoldItem> GetGoldItems()
    {
        return _goldItemList;
    }
    public List<EquipmentItem> GetInventoryItems()
    {
        return _inventoryItems;
    }




    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    /// <param name="saveToFile"></param>
    public void AddInventoryItem(EquipmentItem eI, bool saveToFile = true)
    {
        Debug.Log("Adding to data inventory item... " + eI.IdName + ",s aving to File= " + saveToFile);
        if (_inventoryItems.Contains(eI))
            ++_inventoryItems.Find((e) => (e.IdName.CompareTo(eI.IdName) == 0)).Count;
        else
        {
            eI.New = true;
            _inventoryItems.Add(eI);
        }

        if (saveToFile)
            SaveInventoryItems();
    }
    #endregion


    #region Private Methods

    #endregion


    #region Properties
    public bool DataLoaded { get { return _dataLoaded; } private set { _dataLoaded = true; } }
    //public int Gold { get { return _gold; } set { _gold = value; } }
    public int Vibration { get { return _vibration; } set { _vibration = value; } }
    public List<string> ShopBoxesItemList { get { return _shopBoxesItemList; }  set { _shopBoxesItemList = value; } }
    public List<int> ShopBoxesSizeList {  get { return _shopBoxesSizeList; } set { _shopBoxesSizeList = value; } }
    public List<int> ShopBoxesPriceList {  get { return _shopBoxesPriceList; } set { _shopBoxesPriceList = value; } }
    public List<int> ShopBoxesInteractableList { get { return _shopBoxesInteractableList; } set { _shopBoxesInteractableList = value; } }
    #endregion

    #region Private Serialized Fields

    #endregion

    #region Private Non-serialized Fields
    //TODO: read from file
    [SerializeField]
    private List<EquipmentItem> _gameItemList;
    private List<GoldItem> _goldItemList;
    [SerializeField]
    private List<ShopItemTableEntry> _shopItemList;

    [SerializeField]
    private List<EquipmentItem> _inventoryItems;

    [SerializeField]
    private TextAsset _itemDataTA;  //XML file containing game items data (as Text Asset)
    [SerializeField]
    private TextAsset _shopDataTA;  //XML file containing shop data : items displayed pool, prob list..
    [SerializeField]
    private TextAsset _inventoryDataTA; //TODO: load and write from persistent data path
    [SerializeField]
    private TextAsset _levelDataTA;

    [SerializeField]
    private string _levelFileName;
    [SerializeField]
    private string _invFileName;

    private bool _readingGoldItem;
    private List<ShopItem> _availableShopItemList;
    private List<QualityDistribution> _tempDistr;
    private List<ItemSizePair> _tempSizePait;

    private int /*_gold,*/ _vibration;

    private bool _dataLoaded;

    private List<string> _shopBoxesItemList;
    private List<int> _shopBoxesSizeList;
    private List<int> _shopBoxesPriceList;
    public List<int> _shopBoxesInteractableList;
    #endregion
}
