using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Protocol.Dto;

public class RankPanel : MonoBehaviour {

    private Transform parent;
    private Button btnClose;
    public GameObject itemPre;
    private List<GameObject> listGo = new List<GameObject>();

    private void Awake() {
        EventCenter.AddListener(EventType.ShowRankPanel, ShowRankPanel);
        EventCenter.AddListener<RankListDto>(EventType.SendRankListDto, GetRankListDto);
        parent = transform.Find("List/ScrollRect/Parent");
        btnClose = transform.Find("btnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(OnCloseButtonClick);
    }

    private void OnDestroy() {
        EventCenter.RemoveListener(EventType.ShowRankPanel, ShowRankPanel);
        EventCenter.RemoveListener<RankListDto>(EventType.SendRankListDto, GetRankListDto);
    }

    private void OnCloseButtonClick() {
        transform.DOScale(Vector3.zero, 0.3f);
    }

    private void ShowRankPanel() {
        transform.DOScale(Vector3.one, 0.3f);
    }

    /// <summary>
    /// 得到排行榜传输模型，进行解析
    /// </summary>
    /// <param name="dto"></param>
    private void GetRankListDto(RankListDto dto) {
        if (dto == null) {
            return;
        }

        // 先销毁之前的 排行榜 List
        foreach (var go in listGo) {
            Destroy(go);
        }

        listGo.Clear(); // 清空 排行榜 List 列表

        for (int i = 0; i < dto.rankList.Count; i++) {
            if (dto.rankList[i].name == Models.GameModel.userDto.name) { // 如果是自己
                GameObject go = Instantiate(itemPre, parent);
                go.transform.Find("Index/txtIndex").GetComponent<Text>().text = (i + 1).ToString(); // 排名
                go.transform.Find("Index/txtIndex").GetComponent<Text>().color = Color.red; // 排名字体变成红色
                go.transform.Find("txtName").GetComponent<Text>().text = "我"; // 将用户名改成我
                go.transform.Find("txtName").GetComponent<Text>().color = Color.red; // 用户名字体变成红色
                go.transform.Find("txtCoin").GetComponent<Text>().text = Models.GameModel.userDto.coin.ToString(); // 金币数
                go.transform.Find("txtCoin").GetComponent<Text>().color = Color.red; // 金币字体变成红色

                listGo.Add(go);
            }
            else {
                GameObject go = Instantiate(itemPre, parent);
                go.transform.Find("Index/txtIndex").GetComponent<Text>().text = (i + 1).ToString(); // 排名
                go.transform.Find("txtName").GetComponent<Text>().text = dto.rankList[i].name; // 用户名
                go.transform.Find("txtCoin").GetComponent<Text>().text = dto.rankList[i].coin.ToString(); // 金币数

                listGo.Add(go);
            }
        }
    }
}
