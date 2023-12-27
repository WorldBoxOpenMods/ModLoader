using NeoModLoader.api;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

internal class ModUploadingProgressWindow : AbstractWindow<ModUploadingProgressWindow>
{
    private Image bar;
    internal ulong fileId;
    private Text percent;
    private float progress = 0f;
    private float real_progress = 0f;

    private float start_time;
    private bool uploading = false;

    private UploadProgress uploadProgress = new();

    private void Update()
    {
        if (!Initialized || !IsOpened || !uploading) return;

        if (progress < 0.9f)
        {
            progress += Math.Max(0, real_progress / (Time.time - start_time) * Time.deltaTime);
        }
        else
        {
            progress = Math.Max(progress, Mathf.Lerp(progress, real_progress, Time.deltaTime * 0.1f));
        }

        UpdateDisplay();
    }

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

        var bar_bg = new GameObject("Bar", typeof(Image), typeof(Mask)).GetComponent<Image>();
        bar_bg.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        bar_bg.type = Image.Type.Sliced;
        bar_bg.color = Color.gray;
        RectTransform bar_bg_transform;
        (bar_bg_transform = (RectTransform)bar_bg.transform).SetParent(ContentTransform);
        bar_bg_transform.localScale = Vector3.one;
        bar_bg_transform.localPosition = new(130, -123);
        bar_bg_transform.sizeDelta = new(190, 20);

        bar = new GameObject("Image", typeof(Image)).GetComponent<Image>();
        RectTransform bar_transform;
        (bar_transform = (RectTransform)bar.transform).SetParent(bar_bg_transform);
        bar_transform.localScale = Vector3.one;
        bar_transform.sizeDelta = new(190, 20);
        bar_transform.localPosition = new(-bar_transform.sizeDelta.x / 2, 0);
        bar_transform.pivot = new(0, 0.5f);

        bar.color = Color.green;
    }

    public static UploadProgress ShowWindow()
    {
        Instance.uploading = true;
        Instance.uploadProgress.Reset();
        ScrollWindow.showWindow(WindowId);
        Instance.start_time = Time.time;
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

    private void UpdateDisplay()
    {
        bar.transform.localScale = new Vector3(progress, 1, 1);
        percent.text = $"{(int)(progress * 100)}%";
    }

    public static void FinishUpload()
    {
        Instance.uploading = false;

        Instance.progress = 1;
        Instance.UpdateDisplay();

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
        Instance.percent.text = LM.Get("NML_" + obj.Message);
        Instance.percent.color = Color.red;
    }

    public class UploadProgress : IProgress<float>
    {
        public void Report(float value)
        {
            Instance.real_progress = value;
            if (Instance.progress >= value)
            {
                return;
            }

            Instance.progress = value;
        }

        public void Reset()
        {
            Instance.progress = 0;
            Instance.real_progress = 0;
        }
    }
}