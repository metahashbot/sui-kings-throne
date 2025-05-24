using ARPG.Character.Config;
using RPGCore;
using RPGCore.Interface;
using RPGCore.Skill;
using Sirenix.OdinInspector;
namespace ARPG.Character.Base
{
	public class ARPGPlayerCharacterDataModel : BaseARPGDataModel
	{
		protected SOFE_ARPGCharacterInitConfig _fullConfigRef;

		[ShowInInspector,LabelText("运行时技能容器"),FoldoutGroup("运行时",true)]
		public RPSkill_SkillHolder SelfSkillHolderInstance { get; protected set; }
		
		
		
		
		
		
		
		public ARPGPlayerCharacterDataModel(RolePlay_BaseBehaviour behaviourReference) : base(behaviourReference)
		{
			SelfSkillHolderInstance = new RPSkill_SkillHolder(behaviourReference as I_RP_ObjectCanReleaseSkill);
		}
		public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.UpdateTick(currentTime, currentFrameCount, delta);
			SelfSkillHolderInstance.UpdateTick(currentTime, currentFrameCount, delta);
			
		}

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			SelfSkillHolderInstance.FixedUpdateTick(currentTime,currentFrame, delta);
		}
	}
}