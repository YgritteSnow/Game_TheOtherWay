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
		public float GetData(int cacheIdx, int spectrumIdx)
		{
			return spectrumCache[cacheIdx, spectrumIdx];
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
		public static bool Analysis(ref float[] data, int count, out float[] result, out int[] classify)
		{
			result = new float[count];
			classify = new int[data.Length];
			bool[] flag = new bool[count];

			float d_min, d_max;
			FindMinMax(ref data, out d_min, out d_max);
			for(int i = 0; i < result.Length; ++i)
			{
				result[i] = (d_max - d_min) * (i * 2 + 1) / 6;
				flag[i] = true;
			}

			for(int i = 0; i < classify.Length; ++i)
			{
				classify[i] = -1;
			}

			int max_loop = 100; // max loop times
			bool flag_change = true; // if is still changing
			while (--max_loop > 0 && flag_change)
			{
				flag_change = false;
				// 为每个元素分类
				for (int data_idx = 0; data_idx < data.Length; ++data_idx)
				{
					int min_idx = 0;
					float min_distance = 10000;
					for(int cluster_idx = 0; cluster_idx < result.Length; ++cluster_idx)
					{
						float new_distance = Mathf.Abs(data[data_idx] - result[cluster_idx]);
						if(new_distance < min_distance)
						{
							min_distance = new_distance;
							min_idx = cluster_idx;
						}
					}
					if (min_idx != classify[data_idx])
					{
						//if(max_loop <= 98)Debug.Log("Changed:" + max_loop + ", " + data_idx + " - " + classify[data_idx] + ":" + min_idx);
						classify[data_idx] = min_idx;
						flag_change = true;
					}
				}

				// 重新计算中心
				int[] result_count = new int[result.Length];
				for (int cluster_idx = 0; cluster_idx < result.Length; ++cluster_idx)
				{
					result[cluster_idx] = 0;
					result_count[cluster_idx] = 0;
				}
				for (int data_idx = 0; data_idx < classify.Length; ++data_idx)
				{
					result[classify[data_idx]] += data[data_idx];
					result_count[classify[data_idx]]++;
				}
				for (int cluster_idx = 0; cluster_idx < result.Length; ++cluster_idx)
				{
					if (result_count[cluster_idx] != 0)
					{
						result[cluster_idx] /= result_count[cluster_idx];
					}
				}
			}
			Debug.Log("Result:" + max_loop + ", " + flag_change);
			return !flag_change;
		}

		static void FindMinMax(ref float[] data, out float d_min, out float d_max)
		{
			d_min = 10000;
			d_max = -10000;
			foreach (float v in data)
			{
				d_max = Mathf.Max(d_max, v);
				d_min = Mathf.Min(d_min, v);
			}
		}
	}
	#endregion
}
