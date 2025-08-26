using HintsCore;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MEC;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;
using UnityEngine;

namespace ProjectMER.Events.Handlers.Internal;

public class ToolGunEventsHandler : CustomEventsHandler
{
	private static CoroutineHandle _toolGunCoroutine;

	public override void OnServerRoundStarted()
	{
		Timing.KillCoroutines(_toolGunCoroutine);
		_toolGunCoroutine = Timing.RunCoroutine(ToolGunGUI());
	}

	private static void SendUI(Player player, string text, float duration)
	{
		PlayerDisplay display = PlayerDisplay.Get(player);
		DisplayBlock block = new DisplayBlock(new (0, -540), new (Constants.CanvasSafeWidth, Constants.CanvasSafeHeight));
		MessageBlock message = new MessageBlock(text, Color.white);
		block.Contents.Add(message);
		display.AddBlock(block);
		Timing.CallDelayed(duration, () => display.RemoveBlock(block));
	}

	private static IEnumerator<float> ToolGunGUI()
	{
		while (true)
		{
			yield return Timing.WaitForSeconds(0.1f);

			foreach (Player player in Player.List)
			{
				if (!player.CurrentItem.IsToolGun(out ToolGunItem _) && !ToolGunHandler.TryGetSelectedMapObject(player, out MapEditorObject _))
					continue;

				string hud;
				try
				{
					hud = ToolGunUI.GetHintHUD(player);
				}
				catch (Exception e)
				{
					Logger.Error(e);
					hud = "ERROR: Check server console";
				}

				SendUI(player, hud, 0.1f);
			}
		}
	}

	public override void OnPlayerDryFiringWeapon(PlayerDryFiringWeaponEventArgs ev)
	{
		if (!ev.Weapon.IsToolGun(out ToolGunItem toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.Shot(ev.Player);
	}

	public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
	{
		if (!ev.Weapon.IsToolGun(out ToolGunItem toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.SelectedObjectToSpawn--;
	}

	public override void OnPlayerDroppingItem(PlayerDroppingItemEventArgs ev)
	{
		if (!ev.Item.IsToolGun(out ToolGunItem toolGun))
			return;

		ev.IsAllowed = false;
		if (!ev.Throw)
		{
			ToolGunItem.Remove(ev.Player);
			return;
		}
		toolGun.SelectedObjectToSpawn++;
	}
}
