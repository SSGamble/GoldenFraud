using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源管理
/// </summary>
public class ResourcesManager {

    // 存储已经加载过的头像图集
    private static Dictionary<string, Sprite> nameSpriteDic = new Dictionary<string, Sprite>();

    /// <summary>
    /// 获取头像
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
	public static Sprite GetSprite(string iconName) {
        if (nameSpriteDic.ContainsKey(iconName)) { // 如果字典里已经存在，就直接拿出来
            return nameSpriteDic[iconName];
        }
        else {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Icon/headIcon"); // 加载图集
            string[] nameArr = iconName.Split('_'); // 以 _ 分割 icon_name 字段，headIcon_56
            Sprite sprite = sprites[int.Parse(nameArr[1])];
            nameSpriteDic.Add(iconName, sprite);
            return sprite; // 返回头像
        }
    }

    /// <summary>
    /// 加载牌的图片
    /// </summary>
    /// <param name="cardName"></param>
    /// <returns></returns>
    public static Sprite LoadCardSprite(string cardName) {
        if (nameSpriteDic.ContainsKey(cardName)) { // 如果字典里已经存在，就直接拿出来
            return nameSpriteDic[cardName];
        }
        else {
            Sprite sprite = Resources.Load<Sprite>("Poke/" + cardName); 
            nameSpriteDic.Add(cardName, sprite);
            return sprite; 
        }
    }
}
