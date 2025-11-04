using Godot;
using System.Threading.Tasks;
using System;

public partial class DamageOverlay : TextureRect
{
    [Export] public float FlashDuration = 0.3f;
    [Export] public float MaxAlpha = 0.4f;

    private bool isFlashing = false;

    public async void Flash()
    {
        if (isFlashing)
            return;

        isFlashing = true;

        Color baseColor = Modulate;
        float halfDuration = FlashDuration / 2f;

        float startTime = Time.GetTicksMsec();
        while ((Time.GetTicksMsec() - startTime) / 1000f < halfDuration)
        {
            float elapsed = (Time.GetTicksMsec() - startTime) / 1000f;
            float alpha = Mathf.Lerp(0, MaxAlpha, elapsed / halfDuration);
            Modulate = new Color(baseColor.R, baseColor.G, baseColor.B, alpha);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        startTime = Time.GetTicksMsec();
        while ((Time.GetTicksMsec() - startTime) / 1000f < halfDuration)
        {
            float elapsed = (Time.GetTicksMsec() - startTime) / 1000f;
            float alpha = Mathf.Lerp(MaxAlpha, 0, elapsed / halfDuration);
            Modulate = new Color(baseColor.R, baseColor.G, baseColor.B, alpha);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        Modulate = new Color(baseColor.R, baseColor.G, baseColor.B, 0);
        isFlashing = false;
    }
}
