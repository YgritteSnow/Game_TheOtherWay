using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WAudioAnalysis
{
	#region Offline AudioData Cache
	public struct AudioRawDataCache
	{
		public AudioRawDataCache(AudioClip clip)
		{
			float[] samples = new float[clip.samples * clip.channels];
			Debug.Log("samples=" + clip.samples
				+ "\n channels=" + clip.channels
				+ "\n frequency=" + clip.frequency
				+ "\n length=" + clip.length
				+ "\n loadType=" + clip.loadType
				+ "\n loadState=" + clip.loadState
				+ "\n preloadAudioData=" + clip.preloadAudioData
				);
			clip.GetData(samples, 0);
			rawData = samples;
		}
		public float[] rawData;
	}
	public struct AudioSpectrumDataCache
	{
		public AudioSpectrumDataCache(float duation, float step, int spectrumSize)
		{
			int sampleCount = Mathf.FloorToInt(duation / step);
			spectrumCache = new float[sampleCount, spectrumSize];
			this.spectrumSize = spectrumSize;
			this.size = 0;
			this.capacity = sampleCount;
			this.duation = duation;
			this.step = step;
		}
		public void AddData(float[] data, float time)
		{
			if(time < step * size || time >= duation)
			{
				return;
			}
			int new_size = Mathf.FloorToInt(time / step);
			new_size = Mathf.Min(new_size, capacity - 1);

			for(int size_idx = size; size_idx <= new_size; ++size_idx)
			{
				for (int idx = 0; idx < spectrumSize; ++idx)
				{
					spectrumCache[size_idx, idx] = data[idx];
				}
			}
			size = new_size + 1;
		}
		public float[,] spectrumCache;
		public int size;
		private int capacity;
		private int spectrumSize;

		private float duation;
		private float step;
	}
	#endregion
	#region CacheAndSmooth
	public struct CacheAndSmooth
	{
		public CacheAndSmooth(int count)
		{
			result = 0;
			smoothCount = count;
			smoothedAmpList = new float[smoothCount];
			smoothWindow = new float[smoothCount];
			smoothIdx = 0;

			float total = 0;
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				float x = (idx + 0.5f) / smoothCount;
				smoothWindow[idx] = x > 0.5f ? 4 * (1 - x) : 4 * x;
				total += smoothWindow[idx];
			}
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				smoothWindow[idx] /= total;
			}
		}
		public float AddData(float data)
		{
			smoothedAmpList[smoothIdx] = data;
			smoothIdx = (smoothIdx + 1) % smoothCount;
			result = 0;
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				result += smoothedAmpList[(smoothIdx + 1 + idx) % smoothCount] * smoothWindow[idx];
			}
			return result;
		}
		public float result;
		private int smoothCount;
		private float[] smoothedAmpList;
		private float[] smoothWindow;
		private int smoothIdx;
	}
	#endregion
	#region CacheAndSmoothWithCheck
	public struct CacheAndSmoothWithCheck
	{
		public CacheAndSmoothWithCheck(int smoothCount, float checkRatio)
		{
			this.smoothCount = smoothCount;
			this.checkRatio = checkRatio;

			result = 0;
			inited = false;
			smoothedAmpList = new float[smoothCount];
			smoothWindow = new float[smoothCount];
			smoothIdx = 0;

			float total = 0;
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				float x = (idx + 0.5f) / smoothCount;
				smoothWindow[idx] = x > 0.5f ? 4 * (1 - x) : 4 * x;
				total += smoothWindow[idx];
			}
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				smoothWindow[idx] /= total;
			}
		}
		public bool AddData(float data)
		{
			if (smoothIdx == smoothCount - 1)
			{
				inited = true;
			}
			bool res = false;
			if (inited && (data - result) / data >= checkRatio)
			{
				res = true;
			}
			smoothedAmpList[smoothIdx] = data;
			smoothIdx = (smoothIdx + 1) % smoothCount;
			result = 0;
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				result += smoothedAmpList[(smoothIdx + 1 + idx) % smoothCount] * smoothWindow[idx];
			}
			return res;
		}
		public float result;
		private int smoothCount;
		private bool inited;
		private float checkRatio;
		private float[] smoothedAmpList;
		private float[] smoothWindow;
		private int smoothIdx;
	}
	#endregion

	#region Filter
	public struct Filter
	{
		// 仅保留某一段中的最大值点
		private static void FilterWindow_isMax(ref float[] spectrum, int idx, int window_width)
		{
			int idx_start = System.Math.Max(0, idx - window_width);
			int idx_end = System.Math.Min(spectrum.Length, idx + window_width);
			int last_max_idx = idx_start;
			for (int i = idx_start + 1; i < idx_end; ++i)
			{
				if (spectrum[i] > spectrum[last_max_idx])
				{
					spectrum[last_max_idx] = 0;
					last_max_idx = i;
				}
				else
				{
					spectrum[i] = 0;
				}
			}
		}
		public static void FilterMax(ref float[] spectrum, int window_width)
		{
			for (int idx = 0; idx != spectrum.Length; ++idx)
			{
				FilterWindow_isMax(ref spectrum, idx, window_width);
			}
		}
	}
	#endregion
	#region Cluster
	public class Cluster
	{
		public Cluster() { }
	}
	#endregion
}
