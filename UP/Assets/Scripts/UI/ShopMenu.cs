/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: 2017/09/19
 * @Brief: Shop menu logic
 * @Description: Interactive shop with boxes which can be purchased, 
 * containing random rewards.
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenu : MonoBehaviour {

	#region Public Data
    public const int _smallBoxPrice = 35;
    public const int _mediumBoxPrice = 100;
    public const int _largeBoxPrice = 225;
    public const int _extraLargeBoxPrice = 500;
    
    public enum SHOP_ST { IDLE = 0, BOX_POPUP, OPENING_BOX, BOX_OPENED}
    public enum KEEPER_ST {  IDLE = 0, ANIM }
    #endregion

    #region Behaviour Methods
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.C))
        {
            _boxTapPs.gameObject.SetActive(true);
            _boxTapPs.Stop();
            _boxTapPs.Play();
        }*/

        if (_state == SHOP_ST.OPENING_BOX)
        {
            _frameTimer += Time.deltaTime;
            if (_frameTimer >= _boxFrameTime)
            {
                Debug.Log("loading.. ." + _availableShopItemList[_currentIndex].IdSpriteName);
                _state = SHOP_ST.BOX_OPENED;
                _boxReward.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load(_availableShopItemList[_currentIndex].IdSpriteName, typeof(Sprite)) as Sprite;
                _boxReward.SetActive(true);
                _delayTimer = 0f;
                //SFX
                if (_availableShopItemList[_currentIndex].IdSpriteName.CompareTo("Empty") == 0)
                    AudioController.Play("aud_item_fail");
                else
                {
                    if (DataMgr.Instance.GetGoldItems().Find((itm) => (itm.IdSpriteName.CompareTo(_availableShopItemList[_currentIndex].IdSpriteName) == 0)) != null)
                    {
                        AudioController.Play("ding");
                        //Deprecated since UIparticles comp added, they get autoplayed..
                        _goldItemPs.gameObject.SetActive(true);
                        _goldItemPs.Stop();
                        _goldItemPs.Play();
                        //_goldItemPs.gameObject.SetActive(true);
                    }
                    else
                    {
                        AudioController.Play("aud_box_success_01");
                        //Deprecated since UIparticles comp added, they get autoplayed.. 
                        _equipmentItemPs.gameObject.SetActive(true);
                        _equipmentItemPs.Stop();
                        _equipmentItemPs.Play();
                        //_equipmentItemPs.gameObject.SetActive(true);
                    }
                }
            }

        }
        else if (_state == SHOP_ST.BOX_OPENED && _delayTimer < _rewardShowMinTime)
            _delayTimer += Time.deltaTime;
        /*else if (_state == SHOP_ST.BOX_OPENED)
            _state = SHOP_ST.IDLE;*/

        //Shop keeper anim
            _keeperTimer += Time.deltaTime;
        switch (_keeperSt)
        {
            case KEEPER_ST.IDLE:
                if (_keeperTimer >= _cooldownAnimTime)
                {
                    _keeperTimer = 0f;
                    _shopKeeperImg.sprite = _animSpriteList[0];
                    _keeperAnimIndex = 0;
                    _keeperSt = KEEPER_ST.ANIM;
                }
                break;

            case KEEPER_ST.ANIM:
                
                if (_keeperTimer >= _animationTime)
                {
                    _keeperSt = KEEPER_ST.IDLE;
                    _keeperTimer = 0f;
                    _shopKeeperImg.sprite = _animSpriteList[0];
                }
                else
                {
                    _keeperFrameTimer += Time.deltaTime;
                    if (_keeperFrameTimer >= _keeperFrameTime)
                    {
                        _keeperAnimIndex = (_keeperAnimIndex + 1) % _animSpriteList.Count;
                        _keeperFrameTimer = 0f;
                        _shopKeeperImg.sprite = _animSpriteList[_keeperAnimIndex];
                    }
                }
                    break;
        }
        
    }
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void InitShop()
    {
        //(0) Edge case: fresh game start, no levels played
        if (/*GameMgr.Instance.StageIndex == 0 && GameMgr.Instance.LevelIndex == 0 && */GameMgr.Instance._StageList[0].GetLevelList()[0].GetAvState() == Level.AVAILABILITY_STATE.UNLOCKED)
        {
            Debug.Log("SHOP 0"+GameMgr.Instance.LevelIndex);
           _shopPriceList = new List<int>();
            _auxQualityList = new List<DataMgr.BOX_QUALITY>();
            //Default placeholder boxes
            for (int i = 0; i < 3; ++i)
            {
                //Price
                _itemSpawnPtList[i].GetComponentInChildren<Text>().text = "25";
                _shopPriceList.Add(25);

                _auxQualityList.Add((DataMgr.BOX_QUALITY)DataMgr.Instance.ShopBoxesSizeList[0]);
                //Set boxes sprite + state
                _boxBtnList[i].image.sprite = _boxSizeSpriteList[DataMgr.Instance.ShopBoxesSizeList[0]];
                _boxBtnList[i].interactable = true;
            }
        }
        //(1) Setup Shop items
        else if (GameMgr.Instance.ResetShop)
        {
            Debug.Log("SHOP 1");
            CreateBoxes();
            GameMgr.Instance.ResetShop = false;
        }
        else
        {
            Debug.Log("SHOP 2");
            //Check if last shop state is loaded
            if (_availableShopItemList == null)
            {
                _availableShopItemList = new List<ShopItem>();
                _shopPriceList = new List<int>();
                _auxQualityList = new List<DataMgr.BOX_QUALITY>();
                List<EquipmentItem> itemList = DataMgr.Instance.GetGameItems();
                List<GoldItem> goldItemList = DataMgr.Instance.GetGoldItems();

                DataMgr.Instance.LoadShopState();

                for (int i = 0; i < 3; ++i)
                {
                    //Item
                    if (itemList.Find((itm) => (itm.IdName == DataMgr.Instance.ShopBoxesItemList[i])) != null)
                        _availableShopItemList.Add(itemList.Find((itm) => (itm.IdName == DataMgr.Instance.ShopBoxesItemList[i])));
                    else
                        _availableShopItemList.Add(goldItemList.Find((itm) => (itm.IdName == DataMgr.Instance.ShopBoxesItemList[i])));

                    //Price
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = DataMgr.Instance.ShopBoxesPriceList[i].ToString("0");
                    _shopPriceList.Add(_availableShopItemList[i].Value);

                    //Quality
                    int index = itemList.FindIndex((itm) => (itm.IdName == DataMgr.Instance.ShopBoxesItemList[i]));
                    _auxQualityList.Add((DataMgr.BOX_QUALITY)DataMgr.Instance.ShopBoxesSizeList[i]);
                    //Set boxes sprite + state
                    _boxBtnList[i].image.sprite = _boxSizeSpriteList[DataMgr.Instance.ShopBoxesSizeList[i]];
                    _boxBtnList[i].interactable = DataMgr.Instance.ShopBoxesInteractableList[i] != 0;


                }

            }
        }

        //(2) set spawned box prices
        //SetupPrices();

        //(3) Setup itemBought panel
        _itemBoughtImg.gameObject.SetActive(false);
        _itemBoughtText.text = "";
        _itemNameText.text = "";

        //(4) Setup gold
        _goldText.text = GameMgr.Instance.Gold.ToString();

        //(5) Init variables
        _boxTapCounter = 0;
        _itemBought = false;    //flag used to decide if saving gold/inventory is needed
        _boxPopUp.SetActive(false);
        _boxReward.SetActive(false);
        _equipmentItemPs.gameObject.SetActive(false);
        _goldItemPs.gameObject.SetActive(false);
        _boxTapPs.gameObject.SetActive(false);
        _state = SHOP_ST.IDLE;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Back()
    {
        if (_state != SHOP_ST.IDLE /*&& _state != SHOP_ST.BOX_OPENED*/)
            return;

        gameObject.SetActive(false);
        //Save data
        if (GameMgr.Instance.ShowInvFb)
        {
            DataMgr.Instance.SaveInventoryItems();
            //TODO: check if game screen or selection and enable invenotry feedback
        }
        if (_itemBought)
            DataMgr.Instance.SaveGold();
        DataMgr.Instance.SaveShopState();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemIndex"></param>
    public void AttemptToBuy(int itemIndex)
    {
        if (_state != SHOP_ST.IDLE)
            return;

        int currentBoxPrice = -1;

        _currentIndex = itemIndex;
        //Get selected box price
        /*switch (_auxQualityList[itemIndex])
        {
            case DataMgr.BOX_QUALITY.S:
                currentBoxPrice = _smallBoxPrice;
                break;

            case DataMgr.BOX_QUALITY.M:
                currentBoxPrice = _mediumBoxPrice;
                break;

            case DataMgr.BOX_QUALITY.L:
                currentBoxPrice = _largeBoxPrice;
                break;

            case DataMgr.BOX_QUALITY.XL:
                currentBoxPrice = _extraLargeBoxPrice;
                break;
        }*/
        if (GameMgr.Instance.Gold >= _shopPriceList[itemIndex])
            BuyItem(_currentIndex);
        else
        {
            //No gold feedback
            if (!LeanTween.isTweening(_goldText.gameObject))
                LeanTween.scale(_goldText.gameObject, Vector3.one*1.1f, .5f).setEase(_goldFeedbackAC).setLoopPingPong(1);
            AudioController.Play("aud_no_money");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /*public void HideBoughtItem()
    {
        //EquipmentItem aux = null;

        _showingBoughtItem = false;
        _itemBoughtImg.gameObject.SetActive(false);
        _itemBoughtText.text = "";
        //Update gold if it was a gold item
        if (DataMgr.Instance.GetGoldItems().Find((e) => (e.IdName.CompareTo(_currentBoughtItem.IdName) == 0)) != null)
        {
            Debug.Log("Current bought item is gold item, adding gold...");
            GameMgr.Instance.AddGold(_currentBoughtItem.Value);
            //TODO; GOLD tween count
            _goldText.text = GameMgr.Instance.Gold.ToString();
            //TODO: SFX money + tween money counting
        }
        else  //add item to inventory
        {
            DataMgr.Instance.AddInventoryItem(DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(_availableShopItemList[_currentIndex].IdName) == 0)));

            //DataMgr.Instance.AddInventoryItem(aux);
            GameMgr.Instance.ShowInvFb = true;
        }
    }*/

    /// <summary>
    /// 
    /// </summary>
    public void BoxTap()
    {
        if (_boxTapCounter >= _boxTapList[(int)_currentBoxQuality])
            return;

        ++_boxTapCounter;
        //Feedback Anim
        if (LeanTween.isTweening(_tapBox))
        {
            LeanTween.cancel(_tapBox);
            _tapBox.transform.rotation = Quaternion.identity;
            
        }
        //Feedback + OpenBox or just feedback
        if (_boxTapCounter >= _boxTapList[(int)_currentBoxQuality])
            LeanTween.rotate(_tapBox, Vector3.forward * 20f, .1f).setLoopPingPong(1).setOnComplete(() => { OpenBox(); });
        else
            LeanTween.rotate(_tapBox, Vector3.forward * 20f, .1f).setLoopPingPong(1);

        _boxTapMat.mainTexture = _boxTapPsTxList[(int)_currentBoxQuality];
        ParticleSystem.MainModule main = _boxTapPs.main;
        main.startSize = new ParticleSystem.MinMaxCurve(_boxTapPsSizeList[(int)_currentBoxQuality].x, _boxTapPsSizeList[(int)_currentBoxQuality].y);

        _boxTapPs.gameObject.SetActive(true);
        _boxTapPs.Stop();
        _boxTapPs.Play();   //Particles
        //_boxTapPs.gameObject.SetActive(false);
        //_boxTapPs.gameObject.SetActive(true);
        
        AudioController.Play("aud_tear_01");
       
    }
    /*
    public void TTTT()
    {
        _boxTapMat.mainTexture = _boxTapPsTxList[(int)_currentBoxQuality];
        ParticleSystem.MainModule main = _boxTapPs.main;
        main.startSize = new ParticleSystem.MinMaxCurve(_boxTapPsSizeList[(int)_currentBoxQuality].x, _boxTapPsSizeList[(int)_currentBoxQuality].y);

        _boxTapPs.Play();   //Particles
        AudioController.Play("aud_tear_01");
    }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void ShowItemOnFrame()
    {
        if (_delayTimer < _rewardShowMinTime)
            return;

        //Hide popup
        _boxPopUp.SetActive(false);
        _boxReward.SetActive(false);
        _boxTapCounter = 0;
        //Reveal box item on image frame
        if (_availableShopItemList[_currentIndex].IdSpriteName.CompareTo("Empty") != 0)
        {
            _itemBoughtImg.sprite = Resources.Load(_availableShopItemList[_currentIndex].IdSpriteName, typeof(Sprite)) as Sprite;
            _itemBoughtImg.gameObject.SetActive(true);
            SetItemText(_availableShopItemList[_currentIndex]);
            //_okButton.gameObject.SetActive(true);
            //_showingBoughtItem = true;

            //Gold item
            if (DataMgr.Instance.GetGoldItems().Find((e) => (e.IdName.CompareTo(_currentBoughtItem.IdName) == 0)) != null)
            {
                Debug.Log("Current bought item is gold item, adding gold...");
                GameMgr.Instance.AddGold(_currentBoughtItem.Value);
                //TODO; GOLD tween count
                _goldText.text = GameMgr.Instance.Gold.ToString();
                //TODO: SFX money + tween money counting
            }
            else  //add item to inventory
            {
                DataMgr.Instance.AddInventoryItem(DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(_availableShopItemList[_currentIndex].IdName) == 0)));

                //DataMgr.Instance.AddInventoryItem(aux);
                GameMgr.Instance.ShowInvFb = true;
            }
        }
        _state = SHOP_ST.IDLE;
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /*private void GetAvailableShopItems()
    {
        bool skip = false;
        int globalLvl = -1;

        //TODO :cler properly
        List<DataMgr.ItemSizePair> auxList = null;
        List<DataMgr.ItemSizePair> auxList2 = new List<DataMgr.ItemSizePair>();
        List<DataMgr.QualityDistribution> auxDistr = null;
        
        List<EquipmentItem> auxGameItems = null;
        List<GoldItem> auxGoldItems = null;
        List<ShopItem> auxItemListWithReqs = new List<ShopItem>();

        //TODO: fi magic numbers
        globalLvl = GameMgr.Instance.StageIndex * 9 + GameMgr.Instance.LevelIndex;
        Debug.Log("Global lvl to find in table" + globalLvl);
        if (_availableShopItemList == null)
            _availableShopItemList = new List<ShopItem>();
        else
            _availableShopItemList.Clear();
        if (_auxQualityList == null)
            _auxQualityList = new List<DataMgr.BOX_QUALITY>();
        else
            _auxQualityList.Clear();

        //Get items pool depending on last level completed
        _shopItemList = DataMgr.Instance.GetShopItemList();
        Debug.Log("lists: " + _shopItemList[0].BoxQualityDistr.Count + " / " + _shopItemList[0].ItemSizePairList.Count);

        for (int i = 0; i < _shopItemList.Count && !skip; ++i)
        {
            if (_shopItemList[i].BreakLvl >= globalLvl)
            {
                //entry found
                skip = true;
                Debug.Log("Found entry!");
                auxList = _shopItemList[i].BoxQualityDistr;
                auxDistr = _shopItemList[i].BoxQualityDistr;

                //(1) create box size shuffle bag according to given distribution
                foreach (DataMgr.QualityDistribution qD in auxDistr)
                {
                    for (int k = 0; k < Mathf.FloorToInt(qD.Ratio * 100f); ++k)
                        _auxQualityList.Add(qD.Quality);
                    
                }
                Debug.Log("elemtns in shuffle bag: " + _auxQualityList.Count+" which are S: "+_auxQualityList.FindAll((rr) =>(rr == DataMgr.BOX_QUALITY.S)).Count);
                Shuffle(_auxQualityList);
                _auxQualityList = _auxQualityList.GetRange(0, 3); //get three random elements (we have alredy shuffled list)

                foreach (DataMgr.BOX_QUALITY bq in _auxQualityList)
                    Debug.Log("BQ: " + bq);
                //(2) get items baed on 
                auxGameItems = DataMgr.Instance.GetGameItems();
                auxGoldItems = DataMgr.Instance.GetGoldItems();

                //TODO: find all items which meets reqs for each size on auzQualityList and get a randome one, iterating over qualityList one by one
                for (int j = 0; j < _auxQualityList.Count; ++j)
                {
                    if (auxItemListWithReqs != null)
                        auxItemListWithReqs.Clear();

                    auxList2.Clear();
                    //add ALL items which meet reqs of p.quality from shoptable entry
                    foreach (DataMgr.ItemSizePair ipp in auxList)
                        Debug.Log("Item in ipp" + ipp.ItemId+ ipp.MinQuality);
                    auxList2.AddRange(auxList.FindAll((kp) => (kp.MinQuality <= _auxQualityList[j])));
                    Debug.Log("for quality "+_auxQualityList[j].ToString()+" we have "+auxList2.Count+" elements");
                    for (int jj = 0; jj < auxList2.Count; ++jj)
                        Debug.Log("----------->" + auxList2[jj].ItemId);
                    Shuffle(auxList2);
                    if (auxGameItems.Find((itm) => (itm.IdName == auxList2[0].ItemId)) != null)
                        _availableShopItemList.Add(auxGameItems.Find((itm) => (itm.IdName == auxList2[0].ItemId)));
                    else
                        _availableShopItemList.Add(auxGoldItems.Find((itm) => (itm.IdName == auxList2[0].ItemId)));
                    // auxItemListWithReqs.AddRange(auxGameItems.FindAll((itm) => (itm.IdName == auxList[j].ItemId) && (_auxQualityList[j]>=auxList[j].MinQuality)));

                    // auxItemListWithReqs.Add(
                    // _availableShopItemList.Add(auxGameItems.Find((itm) => (itm.IdName == p.ItemId) && (_auxQualityList.Find((d) => (d >= p.MinQuality)) != null)));
                }
            }
        }
        //Debug.Log("_availableShopItemList.Count" + _availableShopItemList.Count);
        for (int i = 0; i < _availableShopItemList.Count; i++)
            Debug.Log("Avaliable: " + _availableShopItemList[i].IdName);
    }
    */


    /// <summary>
    /// Create a set of three boxes generating random items based on shop data table
    /// </summary>
    private void CreateBoxes()
    {
        string auxId = "";
        List<EquipmentItem> itemList = DataMgr.Instance.GetGameItems();
        List<GoldItem> goldItemList = DataMgr.Instance.GetGoldItems();

        //(0)Cleanup lists
        if (_availableShopItemList == null)
            _availableShopItemList = new List<ShopItem>();
        else _availableShopItemList.Clear();
        if (DataMgr.Instance.ShopBoxesItemList == null)
            DataMgr.Instance.ShopBoxesItemList = new List<string>();
        else
            DataMgr.Instance.ShopBoxesItemList.Clear();

        if (_qDistList == null)
            _qDistList = new List<DataMgr.QualityDistribution>();
        else
            _qDistList.Clear();

        if (_shopPriceList == null)
            _shopPriceList = new List<int>();
        else
            _shopPriceList.Clear();
        if (DataMgr.Instance.ShopBoxesPriceList == null)
            DataMgr.Instance.ShopBoxesPriceList = new List<int>();
        else
            DataMgr.Instance.ShopBoxesPriceList.Clear();

        if (_auxQualityList == null)
            _auxQualityList = new List<DataMgr.BOX_QUALITY>();
        else
            _auxQualityList.Clear();
        if (DataMgr.Instance.ShopBoxesSizeList == null)
            DataMgr.Instance.ShopBoxesSizeList = new List<int>();
        else
            DataMgr.Instance.ShopBoxesSizeList.Clear();

        if (DataMgr.Instance.ShopBoxesInteractableList == null)
            DataMgr.Instance.ShopBoxesInteractableList = new List<int>();
        else
            DataMgr.Instance.ShopBoxesInteractableList.Clear();

            //(1)Get entry based on last stage/level completed
            foreach (DataMgr.ShopItemTableEntry e in DataMgr.Instance.GetShopItemList())
            Debug.Log("e: " + e.BreakStage + "/" + e.BreakLvl);
        DataMgr.ShopItemTableEntry shopEntry = DataMgr.Instance.GetShopItemList().Find((e) =>(e.BreakStage == GameMgr.Instance.StageIndex) && (e.BreakLvl >= GameMgr.Instance.LevelIndex));

        //Debug
        if (shopEntry == null)
            Debug.LogError("No entry found!");

        //(2)Create Box Shuffle Bag
        foreach (DataMgr.QualityDistribution qD in shopEntry.BoxQualityDistr)
        {
            for (int k = 0; k < Mathf.FloorToInt(qD.Ratio * 100f); ++k)
                _qDistList.Add(qD);

        }
        //(3)Select boxes
        for (int i=0; i< 3; ++i)    //TODO: magic numbers
        {
            DataMgr.QualityDistribution selectedBox = _qDistList[0];
            _auxQualityList.Add(selectedBox.Quality);
            DataMgr.Instance.ShopBoxesSizeList.Add((int)selectedBox.Quality);
            Shuffle(_qDistList);    //reroll for next iteration
                        
            //Cleanup
            if (_boxItemIndexList == null)
                _boxItemIndexList = new List<int>();
            else
                _boxItemIndexList.Clear();

            //(4)Create Item Shuffle Bag storing their indexes
            for (int j = 0; j < selectedBox.ItemList.Count; ++j)
            {
                for (int k = 0; k < Mathf.FloorToInt(selectedBox.ItemList[j].Prob * 100f); ++k)
                    _boxItemIndexList.Add(j);
            }
            Shuffle(_boxItemIndexList);
            auxId = selectedBox.ItemList[_boxItemIndexList[0]].ItemId;
            _shopPriceList.Add(selectedBox.Value);
            
            //(5)Add selected item to shop list
            if (itemList.Find((itm) => (itm.IdName == auxId)) != null)
                _availableShopItemList.Add(itemList.Find((itm) => (itm.IdName == auxId)));
            else
                _availableShopItemList.Add(goldItemList.Find((itm) => (itm.IdName == auxId)));
            DataMgr.Instance.ShopBoxesItemList.Add(auxId);
            
            //(6)Set box price
            _itemSpawnPtList[i].GetComponentInChildren<Text>().text = selectedBox.Value.ToString("0");
            DataMgr.Instance.ShopBoxesPriceList.Add(selectedBox.Value);
        }

        //Set boxes sprites
        for (int i = 0; i < _boxBtnList.Count; ++i)
        {
            _boxBtnList[i].image.sprite = _boxSizeSpriteList[(int)_auxQualityList[i]];
            _boxBtnList[i].interactable = true;
            DataMgr.Instance.ShopBoxesInteractableList.Add(1);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /*private void SetupPrices()
    {
        for (int i=0; i< _shopItemList.Count; ++i)
       // for (int i = 0; i < _auxQualityList.Count; ++i)
        {
            switch (_auxQualityList[i])
            {
                case DataMgr.BOX_QUALITY.S:
                    
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _availableShopItemList[i]._smallBoxPrice.ToString();
                    break;

                case DataMgr.BOX_QUALITY.M:
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _mediumBoxPrice.ToString();
                    break;

                case DataMgr.BOX_QUALITY.L:
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _largeBoxPrice.ToString();
                    break;

                case DataMgr.BOX_QUALITY.XL:
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _extraLargeBoxPrice.ToString();
                    break;
            }
            _itemSpawnPtList[i].GetComponentInChildren<Button>().image.sprite = _boxSizeSpriteList[(int)_auxQualityList[i]];
        }
    }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    private void BuyItem(int index)
    {
        _itemBought = true;
        //This box no longer available
        _itemSpawnPtList[index].GetComponentInChildren<Button>().interactable = false;
        DataMgr.Instance.ShopBoxesInteractableList[index] = 0;
        _currentBoughtItem = _availableShopItemList[index];
        _currentBoxQuality = _auxQualityList[index];
        //Remove gold
        /*switch (_currentBoxQuality)
        {
            case DataMgr.BOX_QUALITY.S:
                GameMgr.Instance.AddGold(- _smallBoxPrice);
                break;

            case DataMgr.BOX_QUALITY.M:
                GameMgr.Instance.AddGold(-_mediumBoxPrice);
                break;

            case DataMgr.BOX_QUALITY.L:
                GameMgr.Instance.AddGold(-_largeBoxPrice);
                break;

            case DataMgr.BOX_QUALITY.XL:
                GameMgr.Instance.AddGold(-_extraLargeBoxPrice);
                break;
        }*/
        GameMgr.Instance.AddGold(-_shopPriceList[index]);
        //TODO: GOLD Tween COUNT
        _goldText.text = GameMgr.Instance.Gold.ToString("0");
        //TODO SFX money spent
        _tapBox.GetComponent<Image>().sprite = _boxSizeSpriteList[(int)_auxQualityList[index]];
        _boxPopUp.SetActive(true);
        _state = SHOP_ST.BOX_POPUP;
        //ShowItemOnFrame(index);

        AudioController.Play("aud_money_01");
    }

    /// <summary>
    /// 
    /// </summary>
    private void OpenBox()
    {
        _state = SHOP_ST.OPENING_BOX;
        _frameTimer = 0f;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ei"></param>
    private void SetItemText(ShopItem sItem)
    {
        Debug.Log("Item : " + sItem.IdName);
        _itemNameText.text = LocalizationService.Instance.GetTextByKey("loc_" + sItem.IdName);
        if (DataMgr.Instance.GetGoldItems().Find((e) => (e.IdName.CompareTo(sItem.IdName)==0)) != null)
        {
            _itemBoughtText.text = LocalizationService.Instance.GetTextByKey("loc_gold") + " + " + sItem.Value;
        }
        else
        {
            _itemBoughtText.text = "";
            for (int i = 0; i < ((EquipmentItem)sItem).ModTypeList.Count; ++i)
            {
                switch (((EquipmentItem)sItem).ModTypeList[i])
                {
                    case EquipmentItem.MOD_TYPE.ACURRACY:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_accuracy"); //"Acurracy + ";
                        break;

                    /*case EquipmentItem.MOD_TYPE.ADDITIONA_MOD:
                        _itemBoughtText.text += "Additional Mod%%%%%";
                        break;*/

                    case EquipmentItem.MOD_TYPE.COLLECTOR_SPEED:

                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_speedCol");// + " " + (level + 1).ToString(); "Collector speed + ";
                        break;

                    case EquipmentItem.MOD_TYPE.FALL_SPEED:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_speedFruitFall");// "Fruit fall speed - ";
                        break;
                    case EquipmentItem.MOD_TYPE.GOLD_FIND_PROB:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_goldChance");
                        break;
                    case EquipmentItem.MOD_TYPE.ITEM_FIND_PROB:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_equipChance"); //"Gold item spawn chance + ";
                        break;

                    case EquipmentItem.MOD_TYPE.RELOAD_SPEED:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_reloadSpeed"); //"Collector sack reload speed + ";
                        break;

                    case EquipmentItem.MOD_TYPE.SACK_SIZE:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_sackSize"); //"Sack size = ";
                        break;

                    case EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_strikerArea"); //"Striker hit area + ";
                        break;

                    case EquipmentItem.MOD_TYPE.STRIKER_SPEED:
                        _itemBoughtText.text += LocalizationService.Instance.GetTextByKey("loc_strikerSpeed"); //"Striker speed + ";
                        break;
                }
                //Sign
                if (((EquipmentItem)sItem).ModValueList[i] >= 0f)
                {
                    //if (((EquipmentItem)sItem).ModTypeList[i] != EquipmentItem.MOD_TYPE.FALL_SPEED && ((EquipmentItem)sItem).ModTypeList[i] != EquipmentItem.MOD_TYPE.RELOAD_SPEED)
                        _itemBoughtText.text += " + ";
                    //else
                        //_itemBoughtText.text += " - ";
                }
                else
                {
                    //if (((EquipmentItem)sItem).ModTypeList[i] != EquipmentItem.MOD_TYPE.FALL_SPEED && ((EquipmentItem)sItem).ModTypeList[i] != EquipmentItem.MOD_TYPE.RELOAD_SPEED)
                        _itemBoughtText.text += " - ";
                    //else
                        //_itemBoughtText.text += " + ";
                }
                //Format
                if ((((EquipmentItem)sItem).ModTypeList[i]) != EquipmentItem.MOD_TYPE.SACK_SIZE)
                    _itemBoughtText.text += Mathf.Abs((((EquipmentItem)sItem).ModValueList[i]) * 100f).ToString("0.00") + "%";
                else
                    _itemBoughtText.text += Mathf.Abs(((EquipmentItem)sItem).ModValueList[i]).ToString("0");
                if (i != ((EquipmentItem)sItem).ModTypeList.Count - 1)
                    _itemBoughtText.text += '\n';
            }
        }
        
    }
    /// <summary>
    /// Fisher-Yates shuffle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private List<GameObject> _itemSpawnPtList;
    [SerializeField]
    private List<Sprite> _boxSizeSpriteList;
    [SerializeField]
    private Image _itemBoughtImg;
    
    [SerializeField]
    private Text _goldText;
    [SerializeField]
    private Text _itemBoughtText;

    [SerializeField]
    private GameObject _boxPopUp;
    [SerializeField]
    private GameObject _boxReward;

    [SerializeField]
    private GameObject _tapBox;
    [SerializeField]
    private List<int> _boxTapList;  //tap count needed to open different quality boxes

    [SerializeField]
    private float _boxFrameTime;    //for opening animation

    [SerializeField]
    private Image _shopKeeperImg;
    [SerializeField]
    private List<Sprite> _animSpriteList;
    [SerializeField]
    private float _cooldownAnimTime;
    [SerializeField]
    private float _animationTime;
    [SerializeField]
    private float _keeperFrameTime;
    [SerializeField]
    private List<Button> _boxBtnList;

    [SerializeField]
    private float _rewardShowMinTime;

    [SerializeField]
    private Text _itemNameText;

    [SerializeField]
    private AnimationCurve _goldFeedbackAC;

    [SerializeField]
    private ParticleSystem _equipmentItemPs, _goldItemPs;
    [SerializeField]
    private ParticleSystem _boxTapPs;
    [SerializeField]
    private Material _boxTapMat;
    [SerializeField]
    private List<Texture> _boxTapPsTxList;   //ordered by Quality type
    [SerializeField]
    private List<Vector2> _boxTapPsSizeList;    //[min, max]
    #endregion

    #region Private Non-serialized Fields
    private SHOP_ST _state;
    private List<DataMgr.ShopItemTableEntry> _shopItemList;
    private List<ShopItem> _availableShopItemList;
    private List<DataMgr.BOX_QUALITY> _auxQualityList;
    private static System.Random rng = new System.Random();

    private ShopItem _currentBoughtItem;
    private DataMgr.BOX_QUALITY _currentBoxQuality;
    private int _boxTapCounter;
    private float _frameTimer;
    private int _currentIndex;

    private float _keeperTimer, _keeperFrameTimer;
    private KEEPER_ST _keeperSt;
    private int _keeperAnimIndex;

    private List<DataMgr.QualityDistribution> _qDistList;
    private List<int> _boxItemIndexList;
    private List<int> _shopPriceList;

    private bool _itemBought;
    private float _delayTimer;
    #endregion
}
