using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ApplicationManager : MonoBehaviourSingleton<ApplicationManager> {
    [SerializeField] private GameObject _loadingScreen = default;
    [SerializeField] private GameObject _loadingWheel = default;
    [SerializeField] private GameObject _inputLock = default;
    public OperationBySubscription ShowLoadingScreen { get; private set; }
    public OperationBySubscription ShowLoadingWheel { get; private set; }
    public OperationBySubscription DisableBackButton { get; private set; }
    public OperationBySubscription LockScreen { get; private set; }
    private OperationBySubscription.Subscription _loadingWheelSubscription;
    public AppData Data { get; private set; }
    public bool Initialized { get; private set; }
    
    private async void Start() {
        ShowLoadingScreen = new OperationBySubscription(
            onStart: () => {
                _loadingScreen.SetActive(true);
                _loadingWheelSubscription = ShowLoadingWheel.Subscribe();
            },
            onAllFinished: () => {
                _loadingScreen.SetActive(false);
                _loadingWheelSubscription.Finish();
            }
        );

        ShowLoadingWheel = new OperationBySubscription(
            onStart: () => _loadingWheel.SetActive(true),
            onAllFinished: () => _loadingWheel.SetActive(false)
        );

        DisableBackButton = new OperationBySubscription(null, null);

        LockScreen = new OperationBySubscription(
            onStart: () => _inputLock.SetActive(true),
            onAllFinished: () => _inputLock.SetActive(false)
        );

        await Addressables.InitializeAsync();

        var data = await Addressables.LoadAssetAsync<TextAsset>("Data");
        Data = JsonConvert.DeserializeObject<AppData>(data.text) ?? new AppData();

        UserDataManager.Instance.Sync().Forget();

        Initialized = true;
    }

    private void Update() {
        if (!DisableBackButton.IsRunning && Input.GetKeyDown(KeyCode.Escape)) {
            PopupManager.Instance.Back();
        }
    }
}