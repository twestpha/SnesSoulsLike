using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Units/Weapon Data", order = 2)]
[System.Serializable]
public class WeaponData : ScriptableObject {
    public GameObject weaponPrefab;
}
