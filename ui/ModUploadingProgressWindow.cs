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

        public void Reset()
        {
            lastvalue = 0;
        }
        private float lastvalue;
    }
    private float progress = 0f;
    private Image bar;
    private Text percent;
    private bool uploading = false;
    internal ulong fileId;
    protected override void Init()
    {
        percent = new GameObject("Percent", typeof(Text)).GetComponent<Text>();
        
        RectTransform percentTransform = percent.GetComponent<RectTransform>();
        percentTransform.SetParent(ContentTransform);
        percentTransform.localScale = Vector3.one;
        percentTransform.localPosition = new(130, -100);
        percentTransform.sizeDelta = new(180, 30);
        OT.InitializeCommonText(percent);
        percent.alignment = TextAnchor.MiddleCenter;
        percent.resizeTextMaxSize = 14;
        percent.resizeTextMinSize = 6;
        percent.resizeTextForBestFit = true;
    }

    private UploadProgress uploadProgress = new();
    public static UploadProgress ShowWindow()
    {
        Instance.uploading = true;
        ScrollWindow.showWindow(WindowId);
        return Instance.uploadProgress;
    }
    public override void OnNormalEnable()
    {
        base.OnNormalEnable();
        progress = 0f;
        fileId = 0;
        percent.color = Color.white;
        uploadProgress.Reset();
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
        Instance.percent.color = Color.green;
        if (Instance.fileId > 0)
        {
            Application.OpenURL("steam://url/CommunityFilePage/" + Instance.fileId);
        }
    }

    public static void ErrorUpload(Exception obj)
    {
        Instance.uploading = false;
        Instance.percent.text = LM.Get("NML_"+obj.Message);
        Instance.percent.color = Color.red;
    }
}