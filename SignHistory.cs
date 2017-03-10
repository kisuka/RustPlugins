using Oxide.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
	[Info("SignHistory", "Kisuka", "1.0.0")]
	[Description("Creates a changelog for signs.")]

  class SignHistory : RustPlugin
	{
		private const string AdminPerm = "signhistory.admin";
		private Dictionary<string, Sign> signs = new Dictionary<string, Sign>();

		#region Data
			class Sign
			{
				public string Owner;
				public List<string> Changes = new List<string>();
			}
			private void LoadData() => signs = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, Sign>>("SignHistoryList");
			private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("SignHistoryList", signs);
		#endregion

		#region Commands
			[ChatCommand("history")]
			void cmdHistory(BasePlayer player, string command, string[] args)
			{
				if (!permission.UserHasPermission(player.UserIDString, AdminPerm)) {
					SendReply(player, "You do not have permission.");
					return;
				}
				
				RaycastHit hit;
				if (player == null || !Physics.Raycast(player.eyes.HeadRay(), out hit, 2.0f)) return;

				var sign = hit.transform.GetComponentInParent<Signage>();
				if (sign == null) return;

				var entityID = getEntityID(sign);

				if (!signs.ContainsKey(entityID)) {
					SendReply(player, "No history found.");
					return;
				}
				
				SendReply(player, "Owner: "+signs[entityID].Owner);
				SendReply(player, "Changes:");
				foreach (var change in signs[entityID].Changes) {
					SendReply(player, change);
				}
				return;
			}
		#endregion

		void Init()
		{
			if (!permission.PermissionExists(AdminPerm))
				permission.RegisterPermission(AdminPerm, this);

			LoadData();
		}

		void Unload()
		{
			SaveData();
		}

		void OnSignUpdated(Signage sign, BasePlayer player, string text)
		{
		    logSignChange(sign, player);
		}

		void logSignChange(Signage sign, BasePlayer player)
		{
			var entityID = getEntityID(sign);
			
			if (!signs.ContainsKey(entityID)) {
				var owner = BasePlayer.FindByID(sign.OwnerID);
				
				signs.Add(entityID, new Sign() {
						Owner = owner.displayName +" (" +sign.OwnerID+")",
						Changes = new List<string>()
				});
			}
			
			if (signs.ContainsKey(entityID)) {
				signs[entityID].Changes.Add(DateTime.Now + " : " + player.displayName + " (" + player.userID + ")");
			}

			SaveData();
		}

		public string getEntityID(BaseEntity entity)
		{
			return $"({entity.transform.localPosition.x};{entity.transform.localPosition.y};{entity.transform.localPosition.z})";
		}
	}
}