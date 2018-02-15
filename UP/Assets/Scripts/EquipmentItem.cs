/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : ShopItem {

    public EquipmentItem(string id, string spriteId, int value, SLOT_TYPE sT, List<MOD_TYPE> mtList, List<float> modValList, int count = 1) : base(id,spriteId,value)
    {
        _slotType = sT;
        _modTypeList = mtList;
        _modValueList = modValList;
        _count = count;
    }
    public EquipmentItem(string id, string spriteId, int value) : base(id, spriteId, value)
    {

    }

	#region Public Data
    public enum SLOT_TYPE { SHAKER_A = 0, SHAKER_B, COLLECTOR_A , COLLECTOR_B, STRIKER_A, STRIKER_B }
    public enum MOD_TYPE { SACK_SIZE = 0, RELOAD_SPEED, COLLECTOR_SPEED, STRIKER_HIT_SIZE, ACURRACY,STRIKER_SPEED, GOLD_FIND_PROB,ITEM_FIND_PROB, FALL_SPEED, ADDITIONA_MOD }
	#endregion


	#region Behaviour Methods

	#endregion

	#region Public Methods

	#endregion


	#region Private Methods

	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    public SLOT_TYPE SlotType { get { return _slotType; } set { _slotType = value; } }
    public List<MOD_TYPE> ModTypeList { get { return _modTypeList; } set { _modTypeList = value; } }
    public List<float> ModValueList { get { return _modValueList; } set { _modValueList = value; } }
    public bool Equipped { get { return _equipped; } set { _equipped = value; } }
    public bool New { get { return _new; } set { _new = value; } }
    public int Count { get { return _count; } set { _count = value; } }
	#endregion

	#region Private Non-serialized Fields
    private SLOT_TYPE _slotType;
    private List<MOD_TYPE> _modTypeList;
    private List<float> _modValueList;
    private bool _equipped;
    private bool _new;  //to check which items are seen in inventory for 1st time
    private int _count;
	#endregion
}
