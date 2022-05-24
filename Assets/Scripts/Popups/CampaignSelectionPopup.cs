using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignSelectionPopup : Popup {
    [SerializeField] private ButtonList _campaignsList = default;
    [SerializeField] private Button _addCampaign = default;
    [SerializeField] private Button _closeButton = default;
    private AppData Data => ApplicationManager.Instance.Data;
    private UserData UserData => Data.User;
    private Dictionary<string, Campaign> Campaigns => UserData.Campaigns;

    private void Awake() {
        _closeButton.onClick.AddListener(PopupManager.Instance.Back);
        _addCampaign.onClick.AddListener(async () => {
            var addCampaignPopup = await PopupManager.Instance.GetOrLoadPopup<AddCampaignPopup>(restore: false);
            addCampaignPopup.Populate(
                entry => {
                    Campaigns.Add(entry.Name, entry as Campaign);
                    RefreshCampaignButtons();
                },
                Campaigns.Keys,
                null
            );
        });

        RefreshCampaignButtons();
    }

    private void RefreshCampaignButtons() {
        List<ButtonData> buttons = new List<ButtonData>(Campaigns.Count);

        foreach (var campaign in Campaigns) {
            buttons.Add(new ButtonData {
                Text = campaign.Key,
                Callback = () => {
                    PopupManager.Instance.Back();
                    Data.ClearCache();
                    UserData.SelectedCampaignName = campaign.Key;
                    var selectedCampaign = UserData.SelectedCampaign;
                    selectedCampaign.CurrentSession = selectedCampaign.LastSession;
                    _ = PopupManager.Instance.GetOrLoadPopup<CampaignViewPopup>(restore: false);
                }
            });
        }
        _campaignsList.Populate(buttons);
    }

    public override object GetRestorationData() {
        return null;
    }

    public override void Restore(object data) {
    }
}