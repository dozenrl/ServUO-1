#region Header
//               _,-'/-'/
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2023  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #                                       #
#endregion

// ServUO Compatibility Fixes for Vita-Nex Core
// This file provides compatibility between RunUO-based Vita-Nex Core and ServUO

#region References
using System;
using Server;
using Server.Items;
using Server.Engines.Craft;
#endregion

namespace Server.Items
{
	// BaseTool is the old RunUO interface name, ITool is the new ServUO name
	// Create a type alias for backwards compatibility
	using BaseTool = ITool;
}
