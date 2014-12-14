using UnityEngine;
using System.Collections;
using NSpeex;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NetworkView), typeof(AudioSource))]
public class VoiceChatManagerScript : MonoBehaviour 
{
	public KeyCode PushToTalkKey;
	public int DataSendSize = 1000; //this can be touchy, 1000 seemed to work for me
	public int MaxTimeTalking = 30; 
	
    private AudioClip clip;
	private AudioClip receivedClip;
    private int Frequency = 16000;
	private bool isMicConnected;
	private int lastPosition = 0;
	private List<float> floatAudio = new List<float>();
	
	void Start()
	{
	}
	
	void Update()
	{
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			return;	//if you try to send RPC's without being connected to something, Unity cries.
		}
		if(Input.GetKeyDown(PushToTalkKey) && Microphone.devices.Count() > 0)
		{
			lastPosition = 0;
			clip = Microphone.Start(null, false, MaxTimeTalking, Frequency);	//null is your default device
																				//you can get the others from the Microphone class
		}
		if(Input.GetKeyUp(PushToTalkKey) && Microphone.devices.Count() > 0)
		{
			Microphone.End(null);	//same as above, just make sure these match up
		}
		
		if(Microphone.IsRecording(null))	//same as above
		{
			var position = Microphone.GetPosition(null);	 //same as above
			float[] data = new float[position - lastPosition];
			clip.GetData(data, lastPosition);
			floatAudio.AddRange(data.ToList());
			if(floatAudio.Count > DataSendSize)
			{
				int numEncoded = 0;
				var results = SpeexEncoderWrapper.EncodeMyAudio(CompressToShortArray(floatAudio.ToArray()), out numEncoded);
				var compressed = ByteCompressor.Compress(results);
				networkView.RPC("RPCSendChatData", RPCMode.Others, compressed, numEncoded);
				floatAudio.Clear();
			}
			lastPosition = position;
		}
	}
	
	[RPC]
	private void RPCSendChatData(byte[] data, int encodedBytes)
	{
		var decompressed = ByteCompressor.Decompress(data);
		var floats = DecompressToFloatArray(SpeexEncoderWrapper.DecodeMyAudio(decompressed, encodedBytes));
		receivedClip = AudioClip.Create("Received", floats.Length, 1, Frequency, false, false);
		receivedClip.SetData(floats, 0);
		audio.PlayOneShot(receivedClip);
	}	
	
	public short[] CompressToShortArray(float[] rawAudio)
	{
		var resultData = new short[rawAudio.Length];
		
		for(var i = 0; i < rawAudio.Length; i++)
		{
			resultData[i] = CompressToShort(rawAudio[i]);
		}
		return resultData;
	}
	
	public short CompressToShort(float data)
	{
		return (short)Mathf.Round(data * (float)short.MaxValue);
	}
	
	public float[] DecompressToFloatArray(short[] data)
	{
		var resultData = new float[data.Length];
		
		for(var i = 0; i < data.Length; i++)
		{
			resultData[i] = DecompressToFloat(data[i]);
		}
		return resultData;
	}
	
	public float DecompressToFloat(short data)
	{
		return (float)data/(float)short.MaxValue;
	}
	
}