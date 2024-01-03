using Photon.Pun;
using UnityEngine;

public class ParentView : MonoBehaviour, IPunObservable
{
	private int parentID;
	public int ParentID
	{
		get => parentID;
		set
		{
			parentID = value;
			SetParent(value);
		}
	}

	private void SetParent(int newID)
	{
		print("Switching Parent");
		
		if (newID == 0)
			transform.SetParent(null);
		else
		{
			transform.SetParent(PhotonView.Find(newID).transform);
			transform.localPosition = Vector3.up * .1f;
			transform.localEulerAngles = Vector3.zero;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
			stream.SendNext(ParentID);

		else if (stream.IsReading)
			SetParent((int)stream.ReceiveNext());
	}
}
