using ARPG.Manager;
using Global;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Global.BaseGameReferenceService;

public class UIFW_IgnoreBattleFirebase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameReferenceService_ARPG.Instance.InputActionInstance.BattleGeneral.FireBase.Disable();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameRunningState != GameRunningStateTypeEnum.Paused_暂停)
        {
            GameReferenceService_ARPG.Instance.InputActionInstance.BattleGeneral.FireBase.Enable();
        }
    }
}
