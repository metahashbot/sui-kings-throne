using System;
using ARPG.Character.Base;
using ARPG.Equipment;
using Global.Audio;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_停止音效_StopAudio : BasePlayerAnimationEventCallback
	{

		[InfoBox("如果是ID停止，则会把所有匹配音频名的音源全都停掉。")]
		[LabelText("√:按ID停止  | 口:按音轨停止")]
		public bool StopByID = true;
		[FormerlySerializedAs("AudioClipName"), ShowIf(nameof(StopByID))]
		[LabelText("需要停止的【音频ID】")]
		public string AudioIDToStop;

		[HideIf(nameof(StopByID))]
		[LabelText("需要停止的音轨们")]
		public AudioTrackTypeEnum StopTrack;



		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			if (StopByID)
			{
				GeneralAudioManager.Instance.StopAudioBy(AudioIDToStop);
			}
			else
			{
				GeneralAudioManager.Instance.StopAudioBy(StopTrack);
			}

			return base.ExecuteBySkill(behaviour, skillRef, createNewWhenExist);
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			if (StopByID)
			{
				GeneralAudioManager.Instance.StopAudioBy(AudioIDToStop);
			}
			else
			{
				GeneralAudioManager.Instance.StopAudioBy(StopTrack);
			}

			return base.ExecuteByWeapon(behaviour, weaponHandler, createNewWhenExist);
		}

		public override string GetElementNameInList()
		{
			if (StopByID)
			{
				return $"{GetBaseCustomName()} 停止音效：_{(AudioIDToStop )}_";
			}
			else
			{
				return $"{GetBaseCustomName()} 停止音轨：_{(StopTrack.ToString())}_";
			}
		}
	}
}