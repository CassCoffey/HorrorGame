using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NSpeex;
using System;
using System.IO;
using System.Linq;

public static class SpeexEncoderWrapper
{
	public static SpeexEncoder Encoder = new SpeexEncoder(BandMode.Narrow);
	public static SpeexDecoder Decoder = new SpeexDecoder(BandMode.Narrow, false);
	
	private static int encodedBytes = 0;
	private static int samplesEncoded = 0;
	
	
	public static byte[] EncodeMyAudio(short[] raw, out int numEncoded)
	{
		Encoder.Quality = 8;						//change this to change your quality. 1 = worse quality, but less data
		var encodedData = new byte[raw.Length];
		samplesEncoded = raw.Length - raw.Length % Encoder.FrameSize;
		encodedBytes = Encoder.Encode(raw, 0, samplesEncoded, encodedData, 0, encodedData.Length);
		numEncoded = encodedBytes;
		return encodedData;
	}
	
	public static short[] DecodeMyAudio(byte[] encoded, int numEncoded)
	{
		short[] outData = new short[encoded.Length];
		Decoder.Decode(encoded, 0, numEncoded, outData, 0, false);
		return outData;
	}
	
	public static short[] ConvertToShort(byte[] raw)
	{
		short[] data = new short[ raw.Length / 2 ];
	    int sampleIndex = 0;
	
	    for ( int index = 0; index < raw.Length; index += 2, sampleIndex++ )
	    {
	        data[ sampleIndex ] = BitConverter.ToInt16( raw, index );
	    }
		
		return data;
	}
	
}
