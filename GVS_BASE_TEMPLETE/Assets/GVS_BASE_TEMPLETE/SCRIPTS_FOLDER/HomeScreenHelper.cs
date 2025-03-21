using UnityEngine;

public class HomeScreenHelper : MonoBehaviour
{
    public void ShowBannerAD() 
    {
        ADSManager.Instance.ShowBannerADS();
    }

    public void HideBannerAD()
    {
        ADSManager.Instance.HideBannerADS();
    }

    public void ShowInterstitalAD()
    {
        ADSManager.Instance.ShowADSInterstitial();
    }

    public void ShowRewardedAD()
    {
        ADSManager.Instance.ShowRewardedVideoAds();
    }

    public void ShowRewardedInterstitalAD()
    {
        ADSManager.Instance.ShowRewardedInterstialADS();
    }

    public void ShowDEBUG_POPUP()
    {
        ADSManager.Instance.ShowADSInspector();
    }
}
