using System.Collections;
using System.Collections.Generic;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UIP_BlurPanel : UI_UIBasePanel
{
	public static UIP_BlurPanel Instance;

	[SerializeField, Required, LabelText("Image 全屏面板"), FoldoutGroup("配置", true)]
	protected Image _selfFullscreenPanel;
	
	[SerializeField,Required,LabelText("RF _ 关联模糊RenderFeature"), FoldoutGroup("配置",true)]
	public UniversalRendererData _rf_BlurRenderFeature;
	
	protected const float _alphaMax = 255f / 255f;

	protected override void Awake()
	{
		Instance = this;
		base.Awake();

	}
	
	
	public void EnableFullScreenImage(float alpha)
	{
		_rootGO.SetActive(true);
		alpha = Mathf.Clamp(alpha, 0f, _alphaMax);
		_selfFullscreenPanel.color = new Color(1,1,1,alpha);
		_selfFullscreenPanel.gameObject.SetActive(true);
	}

	public void DisableFullScreenImage()
	{
		_rootGO.SetActive(false);
		_selfFullscreenPanel.gameObject.SetActive(false);
	}
	
	
	


}
