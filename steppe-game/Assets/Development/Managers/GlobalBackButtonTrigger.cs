using UnityEngine;
using UnityEngine.UI;

namespace Development.Managers
{
    public sealed class GlobalBackButtonTrigger : MonoBehaviour
    {
        private Button _globalBackButton;

        // private void Awake() =>
        //     _globalBackButton = GameObject.FindWithTag("GlobalBackButton").GetComponent<Button>();

        public void SetButtonActiveOn() => _globalBackButton.gameObject.SetActive(true);
        public void SetButtonActiveOff() => _globalBackButton.gameObject.SetActive(false);
        public void InvokeOnClick() => _globalBackButton.onClick.Invoke();
    }
}