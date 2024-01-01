using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
	private void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
		DontDestroyOnLoad(this.gameObject);
	}

	private void Start()
	{
		if (PhotonNetwork.IsConnected)
			PhotonNetwork.JoinRandomRoom();
		else {
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.GameVersion = "1";
		}
	}

    public override void OnJoinRandomFailed(short returnCode, string message) {
		PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = 2});
    }

    public override void OnJoinedRoom()
	{
		if (PhotonNetwork.IsMasterClient) {
			PhotonNetwork.LoadLevel("Game");
		}
	}

    public override void OnConnectedToMaster()
	{
		print("Connected To Master!");
		PhotonNetwork.JoinRandomRoom();
	}
}
