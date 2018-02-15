/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldItem : ShopItem {

    public GoldItem(string id, string spriteId, int value) : base(id, spriteId, value)
    {
        /*IdName = id;
        IdSpriteName = spriteId;
        Value = value;*/
    }


	#region Public Data
    public enum ITEM_TYPE { AMULET = 0, RING, PICTURE }
	#endregion


	#region Public Methods

	#endregion


	#region Private Methods

	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private ITEM_TYPE _type;


	#endregion

	#region Private Non-serialized Fields
    
	#endregion
}
