/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Brief: Store level layout reference points
 * @Description: This class provides the elemental references which configure the scene layout
 * like the net object, left and right area range..
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLayout : MonoBehaviour {

	#region Public Data

	#endregion


	#region Public Methods
    public List<GameObject> GetNetList()
    {
        return _netObjectList;
    }

    public Transform GetLeftBotspawnLimit()
    {
        return _leftBotSpawnLimit;
    }

    public Transform GetRightTopSpawnLimit()
    {
        return _rightTopSpawnLimit;
    }

    public Transform GetLeftCollectorLimit()
    {
        return _leftCollectorLimit;
    }

    public Transform GetRightCollectorLimit()
    {
        return _rightCollectorLimit;
    }

    public Transform GetNetHeightRef()
    {
        return _netHeightRef;
    }

    public Transform GetFloorHeightRef()
    {
        return _floorHeightRef;
    }

    public GameObject GetBackground()
    {
        return _background;
    }

    public FruitTree GetFruitTree()
    {
        return _fruitTree;
    }

    public Transform GetMinLeftFingerRef()
    {
        return _minLeftFingerRef;
    }

    public Transform GetMaxLeftFingerRef()
    {
        return _maxLeftFingerRef;
    }

    public Transform GetMinRightFingerRef()
    {
        return _minRightFingerRef;
    }

    public Transform GetMaxRightFingerRef()
    {
        return _maxRightFingerRef;
    }

    public Vector2 GetCamShakerPos()
    {
        return _camIntroShakerPos.position;
    }

    public Vector2 GetCamInitPos()
    {
        return _camIntroStartPos.position;
    }

    public Transform GetCollectorStartPos()
    {
        return _collectorMonkeyStartPos;
    }

    public Transform GetStrikerStartPos()
    {
        _strikerMonkeyStartPos.position = new Vector3(_strikerMonkeyStartPos.position.x, _floorHeightRef.position.y, _strikerMonkeyStartPos.position.z);
        return _strikerMonkeyStartPos;
    }

    /// <summary>
    /// Return the List of Transform references used to place guards
    /// </summary>
    /// <param name="distribution"></param>
    /// <returns></returns>
    public List<Transform> GetGuardPositionList(Level.GUARD_POSITION distribution)
    {
        List<Transform> returnList = null;

        switch (distribution)
        {
            case Level.GUARD_POSITION.RIGHT:
                returnList = _guardSpawnPositionsRight;
                break;

            case Level.GUARD_POSITION.LEFT:
                returnList = _guardSpawnPositionsLeft;
                break;

            case Level.GUARD_POSITION.ALL:
                returnList = new List<Transform>(_guardSpawnPositionsLeft.Count + _guardSpawnPositionsRight.Count);
                returnList.AddRange(_guardSpawnPositionsLeft);
                returnList.AddRange(_guardSpawnPositionsRight);
                break;
        }
        return returnList;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rightList"></param>
    /// <returns></returns>
    public List<Transform> GetGuardPositionList(bool rightList)
    {
        if (rightList)
            return _guardSpawnPositionsRight;
        return _guardSpawnPositionsLeft;
    }
	#endregion


	#region Private Methods

	#endregion


	#region Properties
    /*public GameObject NetObject { get { return _netObject; } private set { _netObject = value; } }
    public Transform LeftBotSpawnLimit { get { return _leftBotSpawnLimit; } set { _leftBotSpawnLimit = value; } }
    public Transform RighttBotSpawnLimit { get { return _rightTopSpawnLimit; } set { _rightTopSpawnLimit = value; } }
    public Transform LeftCollectorLimit { get { return _leftCollectorLimit; } set { _leftCollectorLimit = value; } }
    public Transform RightCollectorLimit { get { return _rightCollectorLimit; } set { _rightCollectorLimit = value; } }*/
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private List<GameObject> _netObjectList;
    [SerializeField]
    private Transform _leftBotSpawnLimit, _rightTopSpawnLimit;  //defines fruit spawn area
    [SerializeField]
    private Transform _leftCollectorLimit, _rightCollectorLimit;
    [SerializeField]
    private Transform _collectorMonkeyStartPos, _strikerMonkeyStartPos;
    [SerializeField]
    private Transform _netHeightRef, _floorHeightRef;
    [SerializeField]
    private GameObject _background;
    [SerializeField]
    private FruitTree _fruitTree;
    [SerializeField]
    private List<Transform> _guardSpawnPositionsLeft, _guardSpawnPositionsRight;
    [SerializeField]
    private Transform _minLeftFingerRef, _maxLeftFingerRef, _minRightFingerRef, _maxRightFingerRef;
    [SerializeField]
    private Transform _camIntroStartPos, _camIntroShakerPos;
    #endregion

    #region Private Non-serialized Fields

    #endregion
}
