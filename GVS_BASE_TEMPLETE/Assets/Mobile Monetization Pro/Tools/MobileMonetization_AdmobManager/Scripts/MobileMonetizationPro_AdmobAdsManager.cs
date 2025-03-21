using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using UnityEngine.Advertisements;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_AdmobAdsManager : MonoBehaviour
    {
        public bool DebugAdInspector = true;
        public Button AdInspectorButton;

        public Button ShowBannerAdButton;
        public Image ImageToUseToDisplayNativeAd;

        [Serializable]
        public class FunctionInfo
        {
            public bool ShowRewardedInterstial = false;
            public Button RewardedButton;
            public MonoBehaviour script;
            public string scriptName;
            public List<string> functionNames;
            public int selectedFunctionIndex;
        }

        public Button[] ActionButtonsToInvokeInterstitalAds;

        public List<Button> rewardedButtons = new List<Button>();

        public List<FunctionInfo> functions = new List<FunctionInfo>();

        FunctionInfo functionInfo;

        private void OnValidate()
        {
            foreach (var function in functions)
            {
                function.functionNames = GetFunctionNames(function.script);
            }
        }
        public List<Button> GetRewardedButtons()
        {
            foreach (var functionInfo in functions)
            {
                rewardedButtons.Add(functionInfo.RewardedButton);
            }
            return rewardedButtons;
        }
        public void OnButtonClick()
        {
            if (functionInfo != null)
            {
                // Call the selected function when the button is clicked
                string selectedFunctionName = functionInfo.functionNames[functionInfo.selectedFunctionIndex];
                MethodInfo method = functionInfo.script.GetType().GetMethod(selectedFunctionName);
                if (method != null)
                {
                    method.Invoke(functionInfo.script, null);
                    functionInfo = null;
                }
            }
        }
        private List<string> GetFunctionNames(MonoBehaviour script)
        {
            List<string> functionNames = new List<string>();
            if (script != null)
            {
                Type type = script.GetType();
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo method in methods)
                {
                    functionNames.Add(method.Name);
                }
            }
            return functionNames;
        }
        private void Start()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.ImageToUseToDisplayNativeAd = ImageToUseToDisplayNativeAd;

                    if (MobileMonetizationPro_AdmobAdsInitializer.instance.ShowBannerAdsInStart == false)
                    {
                        if (ShowBannerAdButton != null)
                        {
                            ShowBannerAdButton.onClick.AddListener(() =>
                            {
                                MobileMonetizationPro_AdmobAdsInitializer.instance.LoadBanner();
                            });
                        }

                    }

                    if (DebugAdInspector == true)
                    {
                        AdInspectorButton.onClick.AddListener(() => MobileMonetizationPro_AdmobAdsInitializer.instance.OpenInspector());
                    }
                }


                for (int i = 0; i < ActionButtonsToInvokeInterstitalAds.Length; i++)
                {
                    if (ActionButtonsToInvokeInterstitalAds[i] != null)
                    {
                        ActionButtonsToInvokeInterstitalAds[i].onClick.AddListener(() =>
                        {
                            // Call a function when the button is clicked
                            ShowInterstitial();
                        });

                    }

                }
            }



            List<Button> rewardedButtons = GetRewardedButtons();

            // Now you can work with the `rewardedButtons` list
            foreach (Button rewardedButton in rewardedButtons)
            {
                // Do something with each rewarded button
                // For example, you can add a click listener
                if (rewardedButton != null)
                {
                    rewardedButton.onClick.AddListener(() => ShowRewarded(rewardedButton));
                }

            }
        }
        public void ShowInterstitial()
        {

            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
                {
                    if (MobileMonetizationPro_AdmobAdsInitializer.instance.EnableTimedInterstitalAds == false)
                    {
                        MobileMonetizationPro_AdmobAdsInitializer.instance.ShowInterstitialAd();
                    }
                    else
                    {

                        if (MobileMonetizationPro_AdmobAdsInitializer.instance.CanShowAdsNow == true)
                        {
                            MobileMonetizationPro_AdmobAdsInitializer.instance.ShowInterstitialAd();
                        }
                    }
                }
            }
        }

        public void ShowRewarded(Button clickedButton)
        {
            if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
            {
                functionInfo = functions.Find(info => info.RewardedButton == clickedButton);
                if(functionInfo.ShowRewardedInterstial == false)
                {
                    Debug.Log("Showing Rewarded Video Ad");
                    MobileMonetizationPro_AdmobAdsInitializer.instance.ShowRewardedAd();
                }
                else
                {
                    Debug.Log("Showing Rewarded Interstitial Video Ad");
                    MobileMonetizationPro_AdmobAdsInitializer.instance.ShowRewardedInterstitialAd();
                }          
            }
        }
        public void ResetAndReloadFullAds()
        {
            if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance.ResetInterstitalAdTimerOnRewardedAd == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.CanShowAdsNow = false;
                    MobileMonetizationPro_AdmobAdsInitializer.instance.Timer = 0f;
                }

                if (MobileMonetizationPro_AdmobAdsInitializer.instance.EnableTimedInterstitalAds == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.CanShowAdsNow = false;
                    MobileMonetizationPro_AdmobAdsInitializer.instance.Timer = 0f;
                }
                MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdCompleted = false;
                MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdSkipped = false;
                MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdUnknown = false;

                MobileMonetizationPro_AdmobAdsInitializer.instance.LoadInterstitial();
                MobileMonetizationPro_AdmobAdsInitializer.instance.LoadRewarded();
                MobileMonetizationPro_AdmobAdsInitializer.instance.LoadRewardedInterstitialAd();
            }
        }
        //// Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
        //public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        //{
        //    if (adUnitId.Equals(AdsInitializerUAds.instance.RewardedAdUnitID) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        //    {
        //        ResetAndReloadFullAds();
        //        if (AdsInitializerUAds.instance != null)
        //        {
        //            OnButtonClick();
        //        }
        //    }
        //    else if (adUnitId.Equals(AdsInitializerUAds.instance.InterstitalAdUnitID) && showCompletionState.Equals(UnityAdsShowCompletionState.SKIPPED))
        //    {
        //        ResetAndReloadFullAds();
        //    }
        //    else if (adUnitId.Equals(AdsInitializerUAds.instance.InterstitalAdUnitID) && showCompletionState.Equals(UnityAdsShowCompletionState.UNKNOWN))
        //    {
        //        ResetAndReloadFullAds();
        //    }

        //}
        public void CheckForAdCompletion()
        {
            if(MobileMonetizationPro_AdmobAdsInitializer.instance != null)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdCompleted == true)
                {
                    ResetAndReloadFullAds();
                    if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
                    {
                        OnButtonClick();
                    }
                }
                else if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdSkipped == true)
                {
                    ResetAndReloadFullAds();
                }
                else if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdUnknown == true)
                {
                    ResetAndReloadFullAds();
                }
            }
          
        }
    }
}