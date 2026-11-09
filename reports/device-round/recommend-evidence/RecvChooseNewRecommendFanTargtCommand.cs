using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GameModule;
using HappyMahjong.CSReplay;
using HappyMahjong.Common;
using HuanLeGameSo;
using IFix;
using UnityEngine;
using strange.extensions.command.impl;

namespace HappyMahjong.Game;

public class RecvChooseNewRecommendFanTargtCommand : EventCommand
{
	[CompilerGenerated]
	private sealed class _003CDelaySetShowEffectPara_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NewFanRecommendDataModel dataModel;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDelaySetShowEffectPara_003Ed__5(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = Yielders.GetWaitForSeconds(0.5f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (dataModel != null)
				{
					dataModel.needShowSelectedEffect = false;
				}
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[Inject]
	public IProcessModel ProcessModel { get; set; }

	public override void Execute()
	{
		if (WrappersManagerImpl.IsPatched(36372))
		{
			WrappersManagerImpl.GetPatch(36372).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("RecvChooseNewRecommendFanTargtCommand", "NewFanRecommend");
		TNewRecommendFanTarget tNewRecommendFanTarget = base.evt.data as TNewRecommendFanTarget;
		List<FanRecommendDetail> list = new List<FanRecommendDetail>();
		for (int i = 0; i < tNewRecommendFanTarget.nTargetNum; i++)
		{
			list.Add(FanRecommendDetail.BuildVOData(tNewRecommendFanTarget.astTargetList[i]));
		}
		NewFanRecommendDataModel newFanRecommendDataModel = SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleFanRecommend) as NewFanRecommendDataModel;
		newFanRecommendDataModel.SetNewFanRecommendDetailData(list);
		newFanRecommendDataModel.UIMode = tNewRecommendFanTarget.iUiMode;
		if (CSReplayRecordService.sIsReplaying())
		{
			return;
		}
		Transform transform = SingletonBase<PopUpManager>.GetInstance().GetUIRoot().transform;
		BubbleEventHelper bubbleEventHelper = new BubbleEventHelper();
		if (list.Count == 0)
		{
			newFanRecommendDataModel.recvCloseRecommendCommand = true;
			bubbleEventHelper.BubbleDispatch(transform, UIViewEvent.CloseRecommendPanel);
			bubbleEventHelper.BubbleDispatch(transform, UIViewEvent.TryShowOrHideRecommendFanLabel, false);
			base.dispatcher.Dispatch(UIViewEvent.UpdateNewFanRecommendIndicatorInHand, null);
			return;
		}
		if (newFanRecommendDataModel.needShowSelectedEffect)
		{
			new BubbleEventHelper().BubbleDispatch(transform, UIViewEvent.UpdateNewFanRecommendPieces, newFanRecommendDataModel.curFanRecommendDetails);
			new BubbleEventHelper().BubbleDispatch(transform, UIViewEvent.UpdateRecommendLabel, newFanRecommendDataModel.CurSelectedRecommendDetail);
			if (!newFanRecommendDataModel.disableFanRecommend)
			{
				base.dispatcher.Dispatch(UIViewEvent.UpdateNewFanRecommendIndicatorInHand, newFanRecommendDataModel.CurSelectedRecommendDetail);
			}
			newFanRecommendDataModel.needShowSelectedEffect = true;
			transform.GetComponent<GameUIRoot>().StartCoroutine(DelaySetShowEffectPara(newFanRecommendDataModel));
		}
		newFanRecommendDataModel.hasUnprocessedData = true;
		ProcedureVO procedureVO = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.UpdateQiPaiTagInHand, newFanRecommendDataModel.CurSelectedRecommendDetail, 0);
		Log.Info(Color.cyan, "|||| Set Active FanRecommendDeatil " + newFanRecommendDataModel.CurSelectedRecommendDetail.ToString(), "Test");
		ProcessModel.PushProcedure(procedureVO);
		base.dispatcher.Dispatch(GameEvent.TrySetChangeNMJTile, newFanRecommendDataModel.CurSelectedRecommendDetail);
	}

	[IteratorStateMachine(typeof(_003CDelaySetShowEffectPara_003Ed__5))]
	private IEnumerator DelaySetShowEffectPara(NewFanRecommendDataModel dataModel)
	{
		if (WrappersManagerImpl.IsPatched(36376))
		{
			return WrappersManagerImpl.GetPatch(36376).__Gen_Wrap_2894(this, dataModel);
		}
		return new _003CDelaySetShowEffectPara_003Ed__5(0)
		{
			dataModel = dataModel
		};
	}

	public void _003C_003EiFixBaseProxy_Execute()
	{
		base.Execute();
	}
}
