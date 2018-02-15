/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopItem {

    public ShopItem(string id, string spriteId, int val)
    {

        _idName = id;
        _idSpriteName = spriteId;
        _value = val;
    }

	#region Public Data

	#endregion


	#region Behaviour Methods

	#endregion

	#region Public Methods

	#endregion


	#region Private Methods

	#endregion


	#region Properties
    public string IdName { get { return _idName; } set { _idName = value; } }
    public string IdSpriteName { get { return _idSpriteName; } set { _idSpriteName = value; } }
    public int Value { get { return _value; } set { _value = value; } }
    public Sprite _Sprite { get { return _sprite; } set { _sprite = value; } }
	#endregion

	#region Private Serialized Fields

	#endregion

	#region Private Non-serialized Fields
    private string _idName;
    private string _idSpriteName;
    private int _value;
    private Sprite _sprite;
	#endregion
}
