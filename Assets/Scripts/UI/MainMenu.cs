using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MainMenu : MonoBehaviour {
    [SerializeField] private ButtonList _buttonList = default;

    private async void Start() {
        UserDataManager.Instance.OnAuthChanged += RefreshButtons;

        await UniTask.WaitUntil(() => ApplicationManager.Instance.Initialized);

        RefreshButtons();
    }

    private void RefreshButtons() {
        List<ButtonData> buttons = new List<ButtonData>();

        buttons.Add(new ButtonData {
            Text = "Campaigns",
            Callback = () => _ =  PopupManager.Instance.GetOrLoadPopup<CampaignSelectionPopup>()
        });

        if (UserDataManager.Instance.IsUserLoggedIn) {
            buttons.Add(new ButtonData("Log Out", () => {
                MessagePopup.ShowConfirmationPopup(
                    "Are you sure you want to log out from Google Drive?",
                    UserDataManager.Instance.LogOut
                );
            }));
        } else {
            buttons.Add(new ButtonData("Log In", async () => {
                await UserDataManager.Instance.Login();
            }));
        }

        buttons.Add(new ButtonData {
            Text = "Quit",
            Callback = async () => {
                var loadingWheel = ApplicationManager.Instance.ShowLoadingScreen.Subscribe();
                await UserDataManager.Instance.SaveAllDataAsync();
                loadingWheel.Finish();
                UnityUtils.Quit();
            }
        });

        _buttonList.Populate(buttons);
    }
}
