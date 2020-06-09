﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_IOS
namespace SkillzSDK.Internal.API.iOS
{
	internal sealed class BridgedAPI : IBridgedAPI, IRandom, ISyncDelegateInitializer
	{
		public IRandom Random
		{
			get
			{
				return this;
			}
		}

		bool IAsyncAPI.IsMatchInProgress
		{
			get
			{
				return (Application.platform == RuntimePlatform.IPhonePlayer) && (InteropMethods._tournamentIsInProgress() != 0);
			}
		}

		public float SkillzMusicVolume
		{
			get
			{
				return InteropMethods._getSkillzMusicVolume();
			}
			set
			{
				InteropMethods._setSkillzMusicVolume(value);
			}
		}

		public float SoundEffectsVolume
		{
			get
			{
				return InteropMethods._getSFXVolume();
			}
			set
			{
				InteropMethods._setSFXVolume(value);
			}
		}

		bool ISyncAPI.IsMatchCompleted
		{
			get
			{
				return InteropMethods._isMatchCompleted();
			}
		}

		private Match _matchInfo;

		public void Initialize(int gameID, Environment environment, Orientation orientation)
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer && !SystemInfo.deviceModel.ToLower().Contains("ipad"))
			{
				Debug.LogWarning("Trying to initialize Skillz on a platform other than iPhone");
				return;
			}

			var environmentString = environment == Environment.Sandbox
				? "SkillzSandbox"
				: "SkillzProduction";

			InteropMethods._skillzInitForGameIdAndEnvironment(gameID.ToString(), environmentString);
		}

		public void LaunchSkillz()
		{
			InteropMethods._launchSkillz();
		}

		public Hashtable GetMatchRules()
		{
			var matchRules = Marshal.PtrToStringAnsi(InteropMethods._getMatchRules());
			Dictionary<string, object> matchInfoDict = DeserializeJSONToDictionary(matchRules);
			return new Hashtable(matchInfoDict);
		}

		public Match GetMatchInfo()
		{
			if (_matchInfo == null)
			{
				string matchInfo = Marshal.PtrToStringAnsi(InteropMethods._getMatchInfo());
				Dictionary<string, object> matchInfoDict = DeserializeJSONToDictionary(matchInfo);
				_matchInfo = new Match(matchInfoDict);
			}

			return _matchInfo;
		}

		public void AbortMatch()
		{
			InteropMethods._notifyPlayerAbortWithCompletion();
			_matchInfo = null;
		}

		public void UpdatePlayersCurrentScore(string score)
		{
			InteropMethods._updatePlayersCurrentStringScore(score);
		}

		public void UpdatePlayersCurrentScore(int score)
		{
			InteropMethods._updatePlayersCurrentIntScore(score);
		}

		public void UpdatePlayersCurrentScore(float score)
		{
			InteropMethods._updatePlayersCurrentScore(score);
		}

		public void ReportFinalScore(string score)
		{
			InteropMethods._displayTournamentResultsWithStringScore(score);
			_matchInfo = null;
		}

		public void ReportFinalScore(int score)
		{
			InteropMethods._displayTournamentResultsWithScore(score);
			_matchInfo = null;
		}

		public void ReportFinalScore(float score)
		{
			InteropMethods._displayTournamentResultsWithFloatScore(score);
			_matchInfo = null;
		}

		public string SDKVersionShort()
		{
			return Marshal.PtrToStringAnsi(InteropMethods._SDKShortVersion());
		}

		public Player GetPlayer()
		{
			var playerJson = Marshal.PtrToStringAnsi(InteropMethods._player());
			Dictionary<string, object> playerDict = DeserializeJSONToDictionary(playerJson);
			return new Player(playerDict);
		}

		public void AddMetadataForMatchInProgress(string metadataJson, bool forMatchInProgress)
		{
			InteropMethods._addMetadataForMatchInProgress(metadataJson, forMatchInProgress);
		}

		public void SetSkillzBackgroundMusic(string fileName)
		{
			Debug.Log("SkillzAudio Api.cs setSkillzBackgroundMusic with file name: " + fileName);
			InteropMethods._setSkillzBackgroundMusic(fileName);
		}

		int ISyncAPI.GetConnectedPlayerCount()
		{
			return InteropMethods._connectedPlayerCount();
		}

		ulong ISyncAPI.GetCurrentOpponentPlayerId()
		{
			return InteropMethods._currentOpponentPlayerId();
		}

		ulong ISyncAPI.GetCurrentPlayerId()
		{
			return InteropMethods._currentPlayerId();
		}

		double ISyncAPI.GetServerTime()
		{
			return InteropMethods._getServerTime();
		}

		long ISyncAPI.GetTimeLeftForReconnection(ulong playerId)
		{
			return InteropMethods._reconnectTimeLeftForPlayer(playerId);
		}

		void ISyncAPI.SendData(byte[] data)
		{
			using (UnmanagedArray ua = new UnmanagedArray(data))
			{
				InteropMethods._sendData(ua.IntPtr, ua.Length);
			}
		}

		float IRandom.Value()
		{
			if (((IAsyncAPI)this).IsMatchInProgress)
			{
				return InteropMethods._getRandomFloat();
			}

			return UnityEngine.Random.value;
		}

		void ISyncDelegateInitializer.Initialize(SkillzSyncDelegate syncDelegate)
		{
			SkillzSyncProxy.Initialize(syncDelegate);
		}

		private static Dictionary<string, object> DeserializeJSONToDictionary(string jsonString)
		{
			return MiniJSON.Json.Deserialize(jsonString) as Dictionary<string, object>;
		}

		/// <summary>
		/// Proxy for marshaling callbacks from the sync delegate instance
		/// in the Skillz iOS SDK.
		/// </summary>
		private static class SkillzSyncProxy
		{
			private static SkillzSyncDelegate _syncDelegate;

			public static void Initialize(SkillzSyncDelegate syncDelegate)
			{
				_syncDelegate = syncDelegate;
				AssignSyncDelegateFunctions();
			}

			[MonoPInvokeCallback(typeof(IntFP))]
			public static void onOpponentHasLostConnection(ulong playerId)
			{
				_syncDelegate.OnOpponentHasLostConnection(playerId);
			}

			[MonoPInvokeCallback(typeof(IntFP))]
			public static void onOpponentHasReconnected(ulong playerId)
			{
				_syncDelegate.OnOpponentHasReconnected(playerId);
			}

			[MonoPInvokeCallback(typeof(IntFP))]
			public static void onOpponentHasLeftMatch(ulong playerId)
			{
				_syncDelegate.OnOpponentHasLeftMatch(playerId);
			}

			[MonoPInvokeCallback(typeof(VoidFP))]
			public static void onCurrentPlayerHasLostConnection()
			{
				_syncDelegate.OnCurrentPlayerHasLostConnection();
			}

			[MonoPInvokeCallback(typeof(VoidFP))]
			public static void onCurrentPlayerHasReconnected()
			{
				_syncDelegate.OnCurrentPlayerHasReconnected();
			}

			[MonoPInvokeCallback(typeof(VoidFP))]
			public static void onCurrentPlayerHasLeftMatch()
			{
				_syncDelegate.OnCurrentPlayerHasLeftMatch();
			}

			[MonoPInvokeCallback(typeof(IntPtrIntFP))]
			public static void onDidReceiveData(IntPtr value, ulong length)
			{
				var managedArray = new byte[length];
				Marshal.Copy(value, managedArray, 0, (int)length);

				_syncDelegate.OnDidReceiveData(managedArray);
			}

			[MonoPInvokeCallback(typeof(VoidFP))]
			public static void onMatchCompleted()
			{
				_syncDelegate.OnMatchCompleted();
			}

			private static void AssignSyncDelegateFunctions()
			{
				Debug.Log("Assign Sync Delegate Functions");

				var onMatchCompletedFP = new VoidFP(onMatchCompleted);
				IntPtr onMatchCompletedIP = Marshal.GetFunctionPointerForDelegate(onMatchCompletedFP);
				InteropMethods._assignOnMatchCompletedFunc(onMatchCompletedIP);

				var onDidReceiveDataFP = new IntPtrIntFP(onDidReceiveData);
				IntPtr onDidReceiveDataIP = Marshal.GetFunctionPointerForDelegate(onDidReceiveDataFP);
				InteropMethods._assignOnDidReceiveDataFunc(onDidReceiveDataIP);

				var onOpponentHasLostConnectionFP = new IntFP(onOpponentHasLostConnection);
				IntPtr onOpponentHasLostConnectionIP = Marshal.GetFunctionPointerForDelegate(onOpponentHasLostConnectionFP);
				InteropMethods._assignOnOpponentHasLostConnectionFunc(onOpponentHasLostConnectionIP);

				var OnOpponentHasReconnectedFP = new IntFP(onOpponentHasReconnected);
				IntPtr onOpponentHasReconnectedIP = Marshal.GetFunctionPointerForDelegate(OnOpponentHasReconnectedFP);
				InteropMethods._assignOnOpponentHasReconnectedFunc(onOpponentHasReconnectedIP);

				var onOpponentHasLeftMatchFP = new IntFP(onOpponentHasLeftMatch);
				IntPtr onOpponentHasLeftMatchIP = Marshal.GetFunctionPointerForDelegate(onOpponentHasLeftMatchFP);
				InteropMethods._assignOnOpponentHasLeftMatchFunc(onOpponentHasLeftMatchIP);

				var onCurrentPlayerHasLostConnectionFP = new VoidFP(onCurrentPlayerHasLostConnection);
				IntPtr onCurrentPlayerHasLostConnectionIP = Marshal.GetFunctionPointerForDelegate(onCurrentPlayerHasLostConnectionFP);
				InteropMethods._assignOnCurrentPlayerHasLostConnectionFunc(onCurrentPlayerHasLostConnectionIP);

				var onCurrentPlayerHasReconnectedFP = new VoidFP(onCurrentPlayerHasReconnected);
				IntPtr onCurrentPlayerHasReconnectedIP = Marshal.GetFunctionPointerForDelegate(onCurrentPlayerHasReconnectedFP);
				InteropMethods._assignOnCurrentPlayerHasReconnectedFunc(onCurrentPlayerHasReconnectedIP);

				var onCurrentPlayerHasLeftMatchFP = new VoidFP(onCurrentPlayerHasLeftMatch);
				IntPtr onCurrentPlayerHasLeftMatchIP = Marshal.GetFunctionPointerForDelegate(onCurrentPlayerHasLeftMatchFP);
				InteropMethods._assignOnCurrentPlayerHasLeftMatchFunc(onCurrentPlayerHasLeftMatchIP);
			}
		}
	}
}
#endif
