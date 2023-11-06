using NeoModLoader.api;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class ModUploadingProgressWindow : AbstractWindow<ModUploadingProgressWindow>
{
    
    public class UploadProgress : IProgress<float>
    {
        public void Report(float value)
        {
            if (this.lastvalue >= value)
            {
                return;
            }
            this.lastvalue = value;
            Instance.progress = value;
        }

        private float lastvalue;
    }
    private float progress = 0f;
    private Image bar;
    private Text percent;
    private bool uploading = false;
    protected override void Init()
    {
        percent = new GameObject("Percent", typeof(Text)).GetComponent<Text>();
        
        RectTransform percentTransform = percent.GetComponent<RectTransform>();
        percentTransform.SetParent(ContentTransform);
        percentTransform.localScale = Vector3.one;
        percentTransform.localPosition = new(130, -50);
        percentTransform.sizeDelta = new(100, 30);
        OT.InitializeCommonText(percent);
    }

    public static UploadProgress ShowWindow()
    {
        Instance.uploading = true;
        ScrollWindow.showWindow(WindowId);
        return new UploadProgress();
    }
    public override void OnNormalEnable()
    {
        base.OnNormalEnable();
        this.progress = 0f;
    }

    public override void OnNormalDisable()
    {
        base.OnNormalDisable();
        uploading = false;
    }

    private void Update()
    {
        if(!Initialized || !IsOpened || !uploading) return;
        
        percent.text = $"{progress * 100}%";
    }
    public static void FinishUpload()
    {
        Instance.uploading = false;
        Instance.percent.text = LM.Get("ModUploadFinish");
    }

    public static void ErrorUpload(Exception obj)
    {
        
    }
}