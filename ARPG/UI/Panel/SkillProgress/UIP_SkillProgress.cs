using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.UI.Panel.SkillProgress
{
    public class UIP_SkillProgress : UI_UIBasePanel
    {

        [LabelText("slider-主体滑条"), Required, SerializeField,TitleGroup("===Widget===")]
        private UnityEngine.UI.Slider _slider_Main;
        


        public override void StartInitializeByUIM()
        {
            base.StartInitializeByUIM();
            GlobalActionBus.GetGlobalActionBus()
                .RegisterAction(ActionBus_ActionTypeEnum.G_Skill_SkillRequireProgress_技能要求读条进度, _ABC_UpdateProgress);
        }


        private void _ABC_UpdateProgress(DS_ActionBusArguGroup ds)
        {
            _lastUpdateTime = BaseGameReferenceService.CurrentTimeInSecond;
            if (!IsPanelCurrentSelfActive)
            {
                ShowThisPanel();
            }

            float n = ds.FloatArgu1.Value;
            _slider_Main.value = n;




        }


        private float _AutoHideInterval = 0.35f;

        private float _lastUpdateTime;


        public override void UpdateTick(float currentTime, int currentFrameCount, float deltaTime)
        {
            base.UpdateTick(currentTime, currentFrameCount, deltaTime);
            
            //当这么多帧数之后没有再接受到活跃信息，就隐藏这个面板
            if(currentTime > (_lastUpdateTime + _AutoHideInterval) && IsPanelCurrentSelfActive)
            {
                HideThisPanel();
            }
        }


    }
}
