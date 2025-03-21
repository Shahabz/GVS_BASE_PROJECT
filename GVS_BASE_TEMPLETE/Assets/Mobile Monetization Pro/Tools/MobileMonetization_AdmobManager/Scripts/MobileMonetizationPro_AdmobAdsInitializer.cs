using UnityEngine;
using TMPro;
using GoogleMobileAds.Api;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Ump.Api;
using System.Collections.Generic;
using GoogleMobileAds.Common;
using System.Collections;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_AdmobAdsInitializer : MonoBehaviour
    {
        public static MobileMonetizationPro_AdmobAdsInitializer instance;

        public bool EnableGdprConsentMessage = true;

        //public TextMeshProUGUI T;

        [Header("Ad unit Id's")]
        public string AndroidBannerId = "ca-app-pub-3940256099942544/6300978111";
        public string AndroidInterstitalId = "ca-app-pub-3940256099942544/1033173712";
        public string AndroidRewardedId = "ca-app-pub-3940256099942544/5224354917";
        public string AndroidNativeId = "ca-app-pub-3940256099942544/2247696110";
        public string AndroidAppOpenId = "ca-app-pub-3940256099942544/9257395921";
        public string AndroidRewardedInterstitialID = "ca-app-pub-3940256099942544/5354046379";
        
        //public string AndroidRewardedInterstitalId = "ca-app-pub-3940256099942544/5354046379";

        public string IOSBannerId = "ca-app-pub-3940256099942544/2934735716";
        public string IOSInterstitalId = "ca-app-pub-3940256099942544/44114689102";
        public string IOSRewardedId = "ca-app-pub-3940256099942544/1712485313";
        public string IOSNativeId = "ca-app-pub-3940256099942544/3986624511";
        public string IOSAppOpenId = "ca-app-pub-3940256099942544/9257395921";
        public string IOSRewardedInterstitialID = "ca-app-pub-3940256099942544/6978759866";
        //public string IOSRewardedInterstitalId = "ca-app-pub-3940256099942544/5354046379";

        BannerView bannerView;
        InterstitialAd interstitialAd;
        RewardedAd rewardedAd;
        NativeAd nativeAd;

        string bannerId;
        string interId;
        string rewardedId;
        string nativeId;
        string appopenId;
        string rewardedinterstitalId;
        //string rewardedinterstitalId;

        [Header("Ads Settings")]
        public bool ShowBannerAdsInStart = true;
        public AdPosition ChooseBannerPosition = AdPosition.Bottom;
        public AdSize BannerAdSize = AdSize.Banner;
        public bool EnableTimedInterstitalAds = true;
        public int InterstitialAdIntervalSeconds = 10;

        public bool ResetInterstitalAdTimerOnRewardedAd = true;

        [Header("AppOpen Ads Settings")]
        public int AppOpensToCheckBeforeShowingAppOpenAd = 3;
        public float DelayShowAppOpenAd = 2f;

        [HideInInspector]
        public bool CanShowAdsNow = false;

        [HideInInspector]
        public float Timer = 0;

        [HideInInspector]
        public bool IsAdSkipped = false;
        [HideInInspector]
        public bool IsAdCompleted = false;
        [HideInInspector]
        public bool IsAdUnknown = false;

        MobileMonetizationPro_AdmobAdsManager AdsManagerAdmobAdsScript;

        [HideInInspector]
        public Image ImageToUseToDisplayNativeAd;

        [HideInInspector]
        public bool IsBannerStartShowing = false;

        [HideInInspector]
        public bool IsAdsInitializationCompleted = false;

        AppOpenAd AppOpenAdV;
        //private RewardedInterstitialAd _rewardedInterstitialAd;

        private int openCount = 0;

        private RewardedInterstitialAd _rewardedInterstitialAd;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                PlayerPrefs.SetInt("Admob_IsAppOpened", 0);
            }
            else
            {
                // If an instance already exists, destroy this duplicate
                Destroy(gameObject);
            }


#if UNITY_ANDROID
        bannerId = AndroidBannerId;
        interId = AndroidInterstitalId;
        rewardedId = AndroidRewardedId;
        nativeId = AndroidNativeId;
        appopenId = AndroidAppOpenId;
        rewardedinterstitalId = AndroidRewardedInterstitialID;
            //rewardedinterstitalId = AndroidRewardedInterstitalId;
#elif UNITY_IPHONE
            bannerId = IOSBannerId;
            interId = IOSInterstitalId;
            rewardedId = IOSRewardedId;
            nativeId = IOSNativeId;
            appopenId = IOSAppOpenId;
            rewardedinterstitalId = IOSRewardedInterstitialID;
#endif

        }
        void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                UnityEngine.Debug.LogError(consentError);
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                if (formError != null)
                {
                // Consent gathering failed.
                UnityEngine.Debug.LogError(consentError);
                    return;
                }

            // Consent has been gathered.
            if (ConsentInformation.CanRequestAds())
                {
                //AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
                MobileAds.RaiseAdEventsOnUnityMainThread = true;
                    MobileAds.Initialize(initStatus =>
                    {
                        IsAdsInitializationCompleted = true;
                        if (ShowBannerAdsInStart == true)
                        {
                            LoadBanner();
                        }
                        LoadAppOpenAd();
                        LoadInterstitial();
                        LoadRewarded();
                        RequestNativeAd();
                        LoadRewardedInterstitialAd();
                        if (PlayerPrefs.GetInt("Admob_IsAppOpened") == 0)
                        {
                            openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                            openCount++;
                            PlayerPrefs.SetInt("AdmobAd_AppOpenCount", openCount);
                            PlayerPrefs.Save();
                            PlayerPrefs.SetInt("Admob_IsAppOpened", 1);
                        }

                        if (openCount >= AppOpensToCheckBeforeShowingAppOpenAd)
                        {
                            StartCoroutine(ShowAppOpenAdWithDelay());
                            PlayerPrefs.SetInt("AdmobAd_AppOpenCount", 0);
                            openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                        }

                    });
                }
            });

        }
        //private void OnDestroy()
        //{
        //    AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
        //}
        public void LoadAppOpenAd()
        {
            //// Clean up the old ad before loading a new one.
            //if (AppOpenAdV != null)
            //{
            //    AppOpenAdV.Destroy();
            //    AppOpenAdV = null;
            //}

            //Debug.Log("Loading the app open ad.");

            //// Create our request used to load the ad.
            //var adRequest = new AdRequest.Builder().Build();

            //// send the request to load the ad.
            //AppOpenAd.Load(appopenId, Screen_Orientation, adRequest,
            //    (AppOpenAd ad, LoadAdError error) =>
            //    {
            //    // if error is not null, the load request failed.
            //    if (error != null || ad == null)
            //        {
            //            Debug.LogError("app open ad failed to load an ad " +
            //                           "with error : " + error);
            //            return;
            //        }

            //        Debug.Log("App open ad loaded with response : "
            //                  + ad.GetResponseInfo());

            //        AppOpenAdV = ad;
            //        RegisterEventHandlers(ad);
            //    });

            // Clean up the old ad before loading a new one.
            if (AppOpenAdV != null)
            {
                AppOpenAdV.Destroy();
                AppOpenAdV = null;
            }

            Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(appopenId, adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogError("app open ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("App open ad loaded with response : "
                              + ad.GetResponseInfo());

                    AppOpenAdV = ad;
                    RegisterEventHandlers(ad);
                });
        }
        //private void OnAppStateChanged(AppState state)
        //{
        //    Debug.Log("App State changed to : " + state);

        //    // if the app is Foregrounded and the ad is available, show it.
        //    if (state == AppState.Foreground)
        //    {
        //        StartCoroutine(ShowAppOpenAdWithDelay());
        //    }
        //}
        IEnumerator ShowAppOpenAdWithDelay()
        {
            yield return new WaitForSeconds(DelayShowAppOpenAd);
            ShowAppOpenAd();
        }

        public void ShowAppOpenAd()
        {
            if (AppOpenAdV != null && AppOpenAdV.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                AppOpenAdV.Show();
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
            }
        }
        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("App open ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("App open ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("App open ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");
                LoadAppOpenAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App open ad failed to open full screen content " +
                               "with error : " + error);
                LoadAppOpenAd();

            };
        }
        private void OnEnable()
        {
            if (EnableGdprConsentMessage == true)
            {
                // Create a ConsentRequestParameters object.
                ConsentRequestParameters request = new ConsentRequestParameters();

                // Check the current consent information status.
                ConsentInformation.Update(request, OnConsentInfoUpdated);
            }
            else
            {

                MobileAds.RaiseAdEventsOnUnityMainThread = true;
                MobileAds.Initialize(initStatus =>
                {
                    IsAdsInitializationCompleted = true;
                    if (ShowBannerAdsInStart == true)
                    {
                        LoadBanner();
                    }
                    LoadAppOpenAd();
                    LoadInterstitial();
                    LoadRewarded();
                    RequestNativeAd();
                    LoadRewardedInterstitialAd();
                    if (PlayerPrefs.GetInt("Admob_IsAppOpened") == 0)
                    {
                        openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                        openCount++;
                        PlayerPrefs.SetInt("AdmobAd_AppOpenCount", openCount);
                        PlayerPrefs.Save();
                        PlayerPrefs.SetInt("Admob_IsAppOpened", 1);
                    }

                    if (openCount >= AppOpensToCheckBeforeShowingAppOpenAd)
                    {
                        StartCoroutine(ShowAppOpenAdWithDelay());
                        PlayerPrefs.SetInt("AdmobAd_AppOpenCount", 0);
                        openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                    }

                });
            }
        }
        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            //T.text = GetDeviceID();

        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsAdsInitializationCompleted == true)
            {
                if (IsBannerStartShowing == true || ShowBannerAdsInStart == true)
                {
                    LoadBanner();
                }
                LoadInterstitial();
                LoadRewarded();
                RequestNativeAd();
                LoadRewardedInterstitialAd();
                if (PlayerPrefs.GetInt("Admob_IsAppOpened") == 0)
                {
                    openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                    openCount++;
                    PlayerPrefs.SetInt("AdmobAd_AppOpenCount", openCount);
                    PlayerPrefs.Save();
                    PlayerPrefs.SetInt("Admob_IsAppOpened", 1);
                }

                if (openCount >= AppOpensToCheckBeforeShowingAppOpenAd)
                {
                    StartCoroutine(ShowAppOpenAdWithDelay());
                    PlayerPrefs.SetInt("AdmobAd_AppOpenCount", 0);
                    openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                }
            }

        }
        private void Update()
        {
            if (PlayerPrefs.GetInt("AdsRemovedSuccessfully") == 0)
            {
                if (Timer >= InterstitialAdIntervalSeconds)
                {
                    Timer = 0;
                    CanShowAdsNow = true;
                }
                else
                {
                    if (EnableTimedInterstitalAds == true)
                    {
                        Timer += Time.deltaTime;
                        if (PlayerPrefs.GetInt("AdsRemoved") == 1)
                        {
                            if (PlayerPrefs.GetInt("AdsRemovedSuccessfully") == 0)
                            {
                                DestroyBannerAd();
                                PlayerPrefs.SetInt("AdsRemovedSuccessfully", 1);
                            }
                        }
                    }
                }
            }
        }
        public void CheckForAdmobManagerScript()
        {
            if (AdsManagerAdmobAdsScript == null)
            {
                if (FindObjectOfType<MobileMonetizationPro_AdmobAdsManager>() != null)
                {
                    AdsManagerAdmobAdsScript = FindObjectOfType<MobileMonetizationPro_AdmobAdsManager>();
                }
                if (AdsManagerAdmobAdsScript != null)
                {
                    AdsManagerAdmobAdsScript.CheckForAdCompletion();
                }
            }
            else
            {
                AdsManagerAdmobAdsScript.CheckForAdCompletion();
            }
        }

        #region Banner

        public void LoadBanner()
        {
            // Previous Code If you want to use than uncomment from line 443 to 463
            //if (PlayerPrefs.GetInt("AdsRemoved") == 0 && IsAdsInitializationCompleted == true)
            //{
            //    //create a banner
            //    CreateBannerView();

            //    //listen to banner events
            //    ListenToBannerEvents();

            //    //load the banner
            //    if (bannerView == null)
            //    {
            //        CreateBannerView();
            //    }

            //    var adRequest = new AdRequest();
            //    adRequest.Keywords.Add("unity-admob-sample");

            //    print("Loading banner Ad !!");
            //    bannerView.LoadAd(adRequest);//show the banner on the screen
            //    IsBannerStartShowing = true;
            //}

            // New Code If you want to use previous code than comment from line 466 to 493
            if (PlayerPrefs.GetInt("AdsRemoved") == 0 && IsAdsInitializationCompleted == true)
            {
                if (bannerView == null)
                {
                    //create a banner
                    CreateBannerView();

                    //listen to banner events
                    ListenToBannerEvents();

                    //load the banner
                    if (bannerView == null)
                    {
                        CreateBannerView();
                    }

                    var adRequest = new AdRequest();
                    adRequest.Keywords.Add("unity-admob-sample");

                    print("Loading banner Ad !!");
                    bannerView.LoadAd(adRequest);//show the banner on the screen
                    IsBannerStartShowing = true;
                }
                else
                {
                    bannerView.Show();
                }
            }
        }

        void CreateBannerView()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (bannerView != null)
                {
                    DestroyBannerAd();
                }
                bannerView = new BannerView(bannerId, BannerAdSize, ChooseBannerPosition);
            }
        }
        void ListenToBannerEvents()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + bannerView.GetResponseInfo());
            };
                // Raised when an ad fails to load into the banner view.
                bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
                {
                    Debug.LogError("Banner view failed to load an ad with error : "
                        + error);
                };
                // Raised when the ad is estimated to have earned money.
                bannerView.OnAdPaid += (AdValue adValue) =>
                {
                    Debug.Log("Banner view paid {0} {1}." +
                        adValue.Value +
                        adValue.CurrencyCode);
                };
                // Raised when an impression is recorded for an ad.
                bannerView.OnAdImpressionRecorded += () =>
                {
                    Debug.Log("Banner view recorded an impression.");
                };
                // Raised when a click is recorded for an ad.
                bannerView.OnAdClicked += () =>
                {
                    Debug.Log("Banner view was clicked.");
                };
                // Raised when an ad opened full screen content.
                bannerView.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("Banner view full screen content opened.");
                };
                // Raised when the ad closed full screen content.
                bannerView.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Banner view full screen content closed.");
                };
            }
        }
        public void DestroyBannerAd()
        {
            if (bannerView != null)
            {
                print("Destroying banner Ad");
                bannerView.Destroy();
                bannerView = null;
            }
        }
        #endregion

        #region Interstitial

        public void LoadInterstitial()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0 && IsAdsInitializationCompleted == true)
            {
                if (interstitialAd != null)
                {
                    interstitialAd.Destroy();
                    interstitialAd = null;
                }
                var adRequest = new AdRequest();
                adRequest.Keywords.Add("unity-admob-sample");

                InterstitialAd.Load(interId, adRequest, (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        print("Interstitial ad failed to load" + error);
                        return;
                    }

                    print("Interstitial ad loaded !!" + ad.GetResponseInfo());

                    interstitialAd = ad;
                    InterstitialEvent(interstitialAd);
                });
            }
        }
        public void ShowInterstitialAd()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (interstitialAd != null && interstitialAd.CanShowAd())
                {
                    interstitialAd.Show();
                }
                else
                {
                    print("Intersititial ad not ready!!");
                }
            }
        }
        public void InterstitialEvent(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log("Interstitial ad paid {0} {1}." +
                    adValue.Value +
                    adValue.CurrencyCode);
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
                IsAdSkipped = true;
                CheckForAdmobManagerScript();

            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                IsAdSkipped = true;
                CheckForAdmobManagerScript();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        #endregion

        #region Rewarded

        public void LoadRewarded()
        {
            if (IsAdsInitializationCompleted == true)
            {
                if (rewardedAd != null)
                {
                    rewardedAd.Destroy();
                    rewardedAd = null;
                }
                var adRequest = new AdRequest();
                adRequest.Keywords.Add("unity-admob-sample");

                RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        print("Rewarded failed to load" + error);
                        return;
                    }

                    print("Rewarded ad loaded !!");
                    rewardedAd = ad;
                    RewardedAdEvents(rewardedAd);
                });
            }
        }
        public void ShowRewardedAd()
        {
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    print("Give reward to player !!");
                });
            }
            else
            {
                print("Rewarded ad not ready");
            }
        }
        public void RewardedAdEvents(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log("Rewarded ad paid {0} {1}." +
                    adValue.Value +
                    adValue.CurrencyCode);
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                IsAdCompleted = true;
                CheckForAdmobManagerScript();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        #endregion


        #region Native

        public void RequestNativeAd()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                AdLoader adLoader = new AdLoader.Builder(nativeId).ForNativeAd().Build();

                adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
                adLoader.OnAdFailedToLoad += this.HandleNativeAdFailedToLoad;

                adLoader.LoadAd(new AdRequest());
            }
        }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                print("Native ad loaded");
                this.nativeAd = e.nativeAd;

                Texture2D iconTexture = this.nativeAd.GetIconTexture();
                Sprite sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.one * .5f);

                ImageToUseToDisplayNativeAd.sprite = sprite;
            }

        }

        private void HandleNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            print("Native ad failed to load" + e.ToString());

        }
        #endregion

        #region RewardedInterstital
        public void LoadRewardedInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedInterstitialAd != null)
            {
                _rewardedInterstitialAd.Destroy();
                _rewardedInterstitialAd = null;
            }

            Debug.Log("Loading the rewarded interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");

            // send the request to load the ad.
            RewardedInterstitialAd.Load(rewardedinterstitalId, adRequest,
                (RewardedInterstitialAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogError("rewarded interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedInterstitialAd = ad;

                    // Register to ad events to extend functionality.
                    RegisterEventHandlers(ad);
                });
        }
        public void ShowRewardedInterstitialAd()
        {
            const string rewardMsg =
                "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";

            if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
            {
                _rewardedInterstitialAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                });
            }
        }
        private void RegisterEventHandlers(RewardedInterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                IsAdCompleted = true;
                CheckForAdmobManagerScript();
                Debug.Log("Rewarded interstitial ad full screen content closed.");
                LoadRewardedInterstitialAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded interstitial ad failed to open " +
                               "full screen content with error : " + error);
                LoadRewardedInterstitialAd();
            };
        }
        #endregion
        //public static string GetDeviceID()
        //{
        //    //Get Android ID
        //    AndroidJavaClass clsunity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //    AndroidJavaObject objActivity = clsunity.GetStatic<AndroidJavaObject>("currentActivity");
        //    AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
        //    AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");

        //    string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");

        //    // Get bytes of Android ID
        //    System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        //    byte[] bytes = ue.GetBytes(android_id);

        //    // Encrypt bytes with md5
        //    System.Security.Cryptography.MD5CryptoServiceProvider mD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        //    byte[] hashbytes = mD5.ComputeHash(bytes);

        //    // Convert the encrypted bytes back to a string (base 16)
        //    string hashString = "";

        //    for (int i = 0; i < hashbytes.Length; i++)
        //    {
        //        hashString += System.Convert.ToString(hashbytes[i], 16).PadLeft(2, '0');
        //    }
        //    string device_id = hashString.PadLeft(32, '0');
        //    return device_id;
        //}
        public void OpenInspector()
        {
            MobileAds.OpenAdInspector(error =>
            {
                // Error will be set if there was an issue and the inspector was not displayed.
            });
        }
        //public void LoadRewardedInterstitialAd()
        //{
        //    // Clean up the old ad before loading a new one.
        //    if (_rewardedInterstitialAd != null)
        //    {
        //        _rewardedInterstitialAd.Destroy();
        //        _rewardedInterstitialAd = null;
        //    }

        //    Debug.Log("Loading the rewarded interstitial ad.");

        //    // create our request used to load the ad.
        //    var adRequest = new AdRequest();
        //    adRequest.Keywords.Add("unity-admob-sample");

        //    // send the request to load the ad.
        //    RewardedInterstitialAd.Load(rewardedinterstitalId, adRequest,
        //        (RewardedInterstitialAd ad, LoadAdError error) =>
        //        {
        //          // if error is not null, the load request failed.
        //          if (error != null || ad == null)
        //            {
        //                Debug.LogError("rewarded interstitial ad failed to load an ad " +
        //                               "with error : " + error);
        //                return;
        //            }

        //            Debug.Log("Rewarded interstitial ad loaded with response : "
        //                      + ad.GetResponseInfo());

        //            _rewardedInterstitialAd = ad;
        //        });
        //}
        //public void ShowRewardedInterstitialAd()
        //{
        //    const string rewardMsg =
        //        "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";

        //    if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        //    {
        //        _rewardedInterstitialAd.Show((Reward reward) =>
        //        {
        //            // TODO: Reward the user.
        //            Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
        //            _rewardedInterstitialAd.Destroy();
        //        });
        //    }
        //}
        //private void RegisterEventHandlers(RewardedInterstitialAd ad)
        //{
        //    // Raised when the ad is estimated to have earned money.
        //    ad.OnAdPaid += (AdValue adValue) =>
        //    {
        //        Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
        //            adValue.Value,
        //            adValue.CurrencyCode));
        //    };
        //    // Raised when an impression is recorded for an ad.
        //    ad.OnAdImpressionRecorded += () =>
        //    {
        //        Debug.Log("Rewarded interstitial ad recorded an impression.");
        //    };
        //    // Raised when a click is recorded for an ad.
        //    ad.OnAdClicked += () =>
        //    {
        //        Debug.Log("Rewarded interstitial ad was clicked.");
        //    };
        //    // Raised when an ad opened full screen content.
        //    ad.OnAdFullScreenContentOpened += () =>
        //    {
        //        Debug.Log("Rewarded interstitial ad full screen content opened.");
        //    };
        //    // Raised when the ad closed full screen content.
        //    ad.OnAdFullScreenContentClosed += () =>
        //    {
        //        Debug.Log("Rewarded interstitial ad full screen content closed.");
        //        LoadRewardedInterstitialAd();

        //    };
        //    // Raised when the ad failed to open full screen content.
        //    ad.OnAdFullScreenContentFailed += (AdError error) =>
        //    {
        //        Debug.LogError("Rewarded interstitial ad failed to open " +
        //                       "full screen content with error : " + error);
        //        LoadRewardedInterstitialAd();
        //    };
        //}
    }
}