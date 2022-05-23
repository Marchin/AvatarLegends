using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignSelectionPopup : Popup {
    [SerializeField] private ButtonList _campaignsList = default;
    [SerializeField] private Button _addCampaign = default;
    [SerializeField] private Button _closeButton = default;
    private UserData userData => ApplicationManager.Instance.Data.User;
    private Dictionary<string, Campaign> campaigns => userData.Campaigns;

    private void Awake() {
        _closeButton.onClick.AddListener(PopupManager.Instance.Back);
        _addCampaign.onClick.AddListener(async () => {
            var addCampaignPopup = await PopupManager.Instance.GetOrLoadPopup<AddCampaignPopup>(restore: false);
            addCampaignPopup.Populate(
                entry => {
                    campaigns.Add(entry.Name, entry as Campaign);
                    RefreshCampaignButtons();
                },
                campaigns.Keys,
                null
            );
        });

        RefreshCampaignButtons();
    }

    private void RefreshCampaignButtons() {
        List<ButtonData> buttons = new List<ButtonData>(campaigns.Count);

        foreach (var campaign in campaigns) {
            buttons.Add(new ButtonData {
                Text = campaign.Key,
                Callback = () => {
                    userData.SelectedCampaignName = campaign.Key;
                    var selectedCampaign = userData.SelectedCampaign;
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