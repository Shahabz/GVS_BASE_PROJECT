    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using TMPro;

public class SceneLoader : MonoBehaviour
    {
        public float delayLoading;
        public string sceneToLoad = "";
        public Color bgColor;
        public string titlestring;
        public AnimNum animID;
        [Space(50)]
        public Animator _anim;
        public Animation loader;
        public TextMeshProUGUI percText, titleText;
        public Image bgImg;

        string loaderClip;
        AsyncOperation asyncLoad = null;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            loaderClip = loader.clip.name;
            titleText.text = titlestring;
            bgImg.color = bgColor;
            _anim.Play(AnimString);
        }
        string AnimString
        {
            get
            {
                return string.Format("anim{0}", (int)animID);
            }
        }
        private void Start()
        {
            StartCoroutine(LoadLevel());
        }
        bool animDone = false;
        private IEnumerator LoadLevel()
        {
            yield return new WaitForSeconds(delayLoading);
            loader.Play(loaderClip);
            loader[loaderClip].speed = 0.0f;
            loader[loaderClip].normalizedTime = 0.0f;
            asyncLoad = SceneManager.LoadSceneAsync(1); // SceneManager.LoadSceneAsync(sceneToLoad);
        while (!asyncLoad.isDone)
            {
                percText.text = string.Format("{0:0.0}%", asyncLoad.progress * 100f);
                loader[loaderClip].normalizedTime = asyncLoad.progress;
                yield return new WaitForEndOfFrame();
            }
            asyncLoad = null;
            loader[loaderClip].normalizedTime = 1.0f;
            yield return new WaitForEndOfFrame();
            if (animDone)
                Destroy(this.gameObject);
        }
        public void DoneAnim()
        {
            animDone = true;
        }
    }
    public enum AnimNum
    {
        Anim1 = 1,
        Anim2,
        Anim3
    }
