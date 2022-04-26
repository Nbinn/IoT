using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Dashboard
{
    public class Manager : MonoBehaviour
    {

        [SerializeField]
        private CanvasGroup Layer1;
        [SerializeField]
        private Text BrokerText;
        [SerializeField]
        private Text UsernameText;
        [SerializeField]
        private Text PasswordText;
        [SerializeField]
        private Button ConnButton;
        [SerializeField]
        private Text ErrMsg;
        [SerializeField]
        private CanvasGroup Layer2;
        [SerializeField]
        private Text TempText;
        [SerializeField]
        private Text HumiText;
        [SerializeField]
        private Toggle LEDToggle;
        [SerializeField]
        private Toggle PumpToggle;
        [SerializeField]
        private Image temperatureBar;
        [SerializeField]
        private float maxTemperature;
        [SerializeField]
        private ImgsFillDynamic Gauge;
        
        private Tween twenFade;


        // Start is called before the first frame update
        void Start()
        {
            Layer1.interactable = true;
            Layer2.interactable = false;
            Layer1.blocksRaycasts = true;
            Layer2.blocksRaycasts = false;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Update_Status(Status_Data _status_data)
        {
            foreach (data_ss _data in _status_data.data_ss)
            {
                switch (_data.ss_name)
                {
                    case "temp":
                        TempText.text = "TEMP: " + _data.ss_value + " " + _data.ss_unit;
                        temperatureBar.fillAmount = (float.Parse(_data.ss_value)+30)/maxTemperature;
                        break;
                    case "humi":
                        HumiText.text = "HUMI: " + _data.ss_value + " " + _data.ss_unit;
                        this.Gauge.SetValue(float.Parse(_data.ss_value)/100f, false, 1f);
                        break;
                }
            }

            
        }

        public void DisplayErrorMessage()
        {
            ErrMsg.text = "CONNECTION FAILED!";
        }

        public string GetBrokerText()
        {
            return BrokerText.text;
        }

        public string GetUsernameText()
        {
            return UsernameText.text;
        }

        public string GetPasswordText()
        {
            return PasswordText.text;
        }

        public void Fade(CanvasGroup _canvas, float endValue, float duration, TweenCallback onFinish)
        {
            if (twenFade != null)
            {
                twenFade.Kill(false);
            }

            twenFade = _canvas.DOFade(endValue, duration);
            twenFade.onComplete += onFinish;
        }

        public void FadeIn(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 1f, duration, () =>
            {
                _canvas.interactable = true;
                _canvas.blocksRaycasts = true;
            });
        }

        public void FadeOut(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 0f, duration, () =>
            {
                _canvas.interactable = false;
                _canvas.blocksRaycasts = false;
            });
        }



        IEnumerator _IESwitchLayer()
        {
            if (Layer1.interactable == true)
            {
                FadeOut(Layer1, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(Layer2, 0.25f);
            }
            else
            {
                FadeOut(Layer2, 0.25f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(Layer1, 0.25f);
            }
        }

        public void SwitchLayer()
        {
            ErrMsg.text = "";
            StartCoroutine(_IESwitchLayer());
        }

        public bool getLEDToggle()
        {
            return LEDToggle.isOn;
        }

        public bool getPumpToggle()
        {
            return PumpToggle.isOn;
        }

        public void ClearErrorMessage()
        {
            ErrMsg.text = "";
        }
    }
}

